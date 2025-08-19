using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace AD
{
    /// <summary>
    /// 로그인 관리 클래스 (Google Play, PlayFab)
    /// </summary>
    public class LoginCanvas : UIBase
    {
        [Header("--- UI Elements ---")]
        [SerializeField] private GameObject _loading;
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private GameObject _retry;

        protected override void Awake()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
#endif
        }

        private void Start()
        {
            _loadingText.text = "LogIn...";
            TryStartLogin();
        }

        #region Connection Check

        private void TryStartLogin()
        {
            if (!IsInternetAvailable())
            {
                ShowRetryPanel();
                return;
            }

            StartLogin();
        }

        private bool IsInternetAvailable() => Application.internetReachability != NetworkReachability.NotReachable;

        private void ShowRetryPanel()
        {
            _loading.SetActive(false);
            _retry.SetActive(true);
        }

        public void RetryConnection()
        {
            _retry.SetActive(false);
            _loading.SetActive(true);
            TryStartLogin();
        }

        #endregion

        #region Login Process

        private void StartLogin()
        {
            _retry.SetActive(false);
            _loading.SetActive(true);

#if UNITY_EDITOR
            LoginPlayFabInEditorAsync().Forget();
#elif UNITY_ANDROID
            LoginWithGoogle();
#else
            LoginPlayFabInEditorAsync().Forget();
#endif
        }

        #region Login with Google (Android)

        private void LoginWithGoogle()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
#endif
        }

#if UNITY_ANDROID
        private void ProcessAuthentication(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                _loadingText.text = "Google OK, PlayFab...";
                LoginPlayFabWithGoogleAsync().Forget();
            }
            else
            {
                _loadingText.text = $"Failed Google Sign-In ({status})";
                ShowRetryPanel();
            }
        }

        private async UniTask LoginPlayFabWithGoogleAsync()
        {
            try
            {
                // v2 권장: RequestServerSideAccess 로 서버 인증 코드 요청
                // (스코프가 필요없다면 오버로드로 간단히 코드만 받을 수도 있습니다)
                var tcs = new UniTaskCompletionSource<string>();

                // 간단 오버로드: 코드 문자열만 콜백으로 수신 (README에 예시) 
                PlayGamesPlatform.Instance.RequestServerSideAccess(
                    /* forceRefreshToken: */ false,
                    code => tcs.TrySetResult(code)
                );

                var serverAuthCode = await tcs.Task;
                if (string.IsNullOrEmpty(serverAuthCode))
                {
                    _loadingText.text = "No ServerAuthCode. Check Play Console Web Client ID.";
                    ShowRetryPanel();
                    return;
                }

                await Managers.PlayFabManager.LoginWithGoogleAuthCodeAsync(serverAuthCode);
                await GoLobbyScene();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _loadingText.text = "PlayFab login failed.";
                ShowRetryPanel();
            }
        }
#endif
        #endregion

        #region Editor (CustomID)

        /// <summary>
        /// [Editor 전용] PlayFab CustomID 로그인 흐름
        /// 본 타이틀은 운영 정책상 Client/LoginWithCustomId 경로의 신규 계정 생성(createAccount)을 차단합니다.
        /// 따라서 에디터에서는 항상 createAccount:false 로 로그인하는 것을 기본으로 합니다.
        ///
        /// 실패(계정 미존재) 시 대응 순서:
        /// 1) Game Manager ▸ Title Settings ▸ API Features 에서
        ///    'Disable player creation using Client/LoginWithCustomId'를 잠시 해제 후,
        ///    에디터에서 createAccount:true 로 한 번만 로그인해 테스트 계정을 생성 → 다시 차단(체크 ON) 복구
        /// 2) DEV 전용 타이틀을 별도로 두고, DEV에서는 CustomId 신규 생성 허용 / PROD에서는 차단
        /// 3) Android 실단에서 먼저 GPGS v2 → PlayFab(LoginWithGoogleAccount)로 로그인하여 계정을 생성한 뒤,
        ///    에디터에서는 동일 사용자로 createAccount:false 로만 로그인
        ///
        /// 왜 Custom/Device ID 신규 생성을 막나요?
        /// - 저신뢰 식별자(재설치/복제/위조 용이)로 인한 계정 증식, 밴 회피, 보상 파밍 등 어뷰징 대응
        /// - Google/Apple 같은 OAuth만 신규 생성 허용 시 복구 용이, 구매/정책 준수 및 보안 강화
        /// </summary>
        private async UniTask LoginPlayFabInEditorAsync()
        {
            try
            {
                string customId = $"EDITOR_{SystemInfo.deviceUniqueIdentifier}";
                await Managers.PlayFabManager.LoginWithCustomIdAsync(customId, createAccount: false);
                await GoLobbyScene();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _loadingText.text =
                    "PlayFab 로그인 실패(에디터).\n" +
                    "테스트 계정이 아직 없다면 아래 중 하나를 수행해 주세요:\n" +
                    "1) 타이틀에서 '클라이언트 계정 생성 허용'을 잠시 켠 뒤 한 번 로그인해 계정 생성 후 다시 끄기\n" +
                    "2) DEV용 타이틀(계정 생성 허용)을 별도로 사용";
                ShowRetryPanel();
            }
        }

        #endregion

        private async UniTask GoLobbyScene()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5d));
            AD.Managers.SceneManager.ChangeScene(AD.GameConstants.Scene.Lobby);
        }

        #endregion
    }
}
