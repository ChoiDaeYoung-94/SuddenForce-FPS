using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace AD
{
    /// <summary>
    /// 로그인 관리 클래스 (Google Play)
    /// </summary>
    public class Login : MonoBehaviour
    {
        [Header("--- UI Elements ---")]
        [SerializeField] private GameObject _loading;
        [SerializeField] private TMPro.TMP_Text _loadingText;
        [SerializeField] private GameObject _retry;

        private void Awake()
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
            GoMainScene();
#elif UNITY_ANDROID
            LoginWithGoogle();
#endif
        }

        #region Login with google

        private void LoginWithGoogle()
        {
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        }

        private void ProcessAuthentication(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                // Continue with Play Games Services
                Debug.Log("Success LoginGoogle");
                GoMainScene();
            }
            else
            {
                // Disable your integration with Play Games Services or show a login button
                // to ask users to sign-in. Clicking it should call
                // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);

                Debug.Log($"Failed LoginGoogle{status}");
                _loadingText.text = $"Failed LoginWithGoogle...";
            }
        }

        #endregion

        private void GoMainScene()
        {
            AD.Managers.SceneM.NextScene(AD.GameConstants.Scene.Main);
        }

        #endregion

        public void ClickedOK() => AD.Managers.SoundM.UI_Ok();
    }
}