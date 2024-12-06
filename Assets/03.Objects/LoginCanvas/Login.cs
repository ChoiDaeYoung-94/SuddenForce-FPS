#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

using PlayFab;
using PlayFab.ClientModels;

public class Login : MonoBehaviour
{
    [Header("--- 세팅 ---")]
    [SerializeField, Tooltip("GO - Loading")]
    GameObject _go_Loading = null;
    [SerializeField, Tooltip("GO - Retry")]
    GameObject _go_Retry = null;
    [SerializeField, Tooltip("TMP - Load")]
    TMPro.TMP_Text _TMP_load = null;
    [SerializeField, Tooltip("GO - NickName")]
    GameObject _go_NickName = null;
    [SerializeField, Tooltip("TMP - NickName")]
    TMPro.TMP_Text _TMP_NickName = null;
    [SerializeField, Tooltip("GO - WarningRule")]
    GameObject _go_WarningRule = null;
    [SerializeField, Tooltip("GO - WarningNAE")]
    GameObject _go_WarningNAE = null;

    [Header("--- 참고용 ---")]
    Coroutine _co_SaveData = null;
    Coroutine _co_Login = null;

    private State _state = null;

    private void Awake()
    {
        PlayGamesPlatform.Activate();
    }

    private void Start()
    {
        LoginStep();
    }

    public void SetState(State state)
    {
        _state = state;
        _state.Handle();
    }

    #region Functions
    /// <summary>
    /// login step 진행
    /// </summary>
    public void LoginStep()
    {
        if (_state == null)
            _state = new CheckConnection(this);

        _state.Handle();
    }

    #region Google Login
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            // Continue with Play Games Services
            Debug.Log("Success LoginGoogle");
            SetState(new LoginPlayFab(this));
        }
        else
        {
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);

            Debug.Log($"Failed LoginGoogle{status}");
        }
    }
    #endregion

    #region PlayFab Login
    void OnLoginWithPlayFabSuccess(LoginResult result)
    {
        Debug.Log("Success LoginWithPlayFab");
        _TMP_load.text = "Success!!";

        AD.Managers.DataM.StrID = result.PlayFabId;

        GetPlayerProfile(AD.Managers.DataM.StrID);
    }

    void GetPlayerProfile(string playFabId)
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true
            }
        },
        result =>
        {
            if (string.IsNullOrEmpty(result.PlayerProfile.DisplayName))
                _go_NickName.SetActive(true);
            else
                SetState(new CheckData(this));
        },
        error =>
        {
            //Debug.LogError(error.GenerateErrorReport());
            Debug.Log("Failed to get profile");
        });
    }

    void OnLoginWithPlayFabFailure(PlayFabError error)
    {
        Debug.Log($"Failed LoginWithPlayFab -> SignUpWithPlayFab -> {error}");

        SignUpWithPlayFab();
    }

    void SignUpWithPlayFab()
    {
        string id = $"{Social.localUser.id}@AeDeong.com";

        Debug.Log(Social.localUser.id);
        Debug.Log(Social.localUser.userName);

        var request = new RegisterPlayFabUserRequest { Email = id, Password = "AeDeong", RequireBothUsernameAndEmail = false };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterWithPlayFabSuccess, OnRegisterWithPlayFabFailure);
    }

    void OnRegisterWithPlayFabSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Success SignUpWithPlayFab");
        _TMP_load.text = "Success!!";

        AD.Managers.DataM.StrID = result.PlayFabId;

        // nickname 설정
        _go_NickName.SetActive(true);
    }

    void OnRegisterWithPlayFabFailure(PlayFabError error)
    {
        Debug.LogWarning($"Failed SignUpWithPlayFab -> {error}");
        _TMP_load.text = $"Failed SignUpWithPlayFab... :'( \n{error}";
    }

    /// <summary>
    /// Panel_NickName -> Btn_Confirm
    /// </summary>
    public void CheckNickName()
    {
        string str_temp = _TMP_NickName.text;

        if (string.IsNullOrEmpty(str_temp) || str_temp.Contains(" ") || str_temp.Length < 3 || str_temp.Length > 20)
            _go_WarningRule.SetActive(true);
        else
            SetState(new UpdateDisplayName(this, str_temp));
    }
    #endregion

    private void GoMainScene()
    {
        AD.Managers.SceneM.NextScene(AD.Define.Scenes.Main);
    }

    public void ClickedOK() => AD.Managers.SoundM.UI_Ok();
    #endregion

    #region Coroutines
    IEnumerator SaveNickName()
    {
        while (AD.Managers.ServerM.isInprogress)
            yield return null;

        StopSaveNickNameCoroutine();

        SetState(new CheckData(this));
    }

    void StopSaveNickNameCoroutine()
    {
        if (_co_SaveData != null)
        {
            StopCoroutine(_co_SaveData);
            _co_SaveData = null;
        }
    }

    IEnumerator InitPlayerData()
    {
        while (AD.Managers.ServerM.isInprogress)
            yield return null;

        StopInitPlayerDataCoroutine();

        GoMainScene();
    }

    void StopInitPlayerDataCoroutine()
    {
        if (_co_Login != null)
        {
            StopCoroutine(_co_Login);
            _co_Login = null;
        }
    }
    #endregion

    #region Login Step State Class
    /// <summary>
    /// 여러 상태를 정의하기 위해 abstract 사용
    /// 아래 State를 상속 받는 class들의 순서대로 상태 진입
    /// </summary>
    public abstract class State
    {
        protected Login _loginStep;

        public State(Login loginStep)
        {
            _loginStep = loginStep;
        }

        public abstract void Handle();
    }

    class CheckConnection : State
    {
        public CheckConnection(Login loginStep) : base(loginStep) { }

        /// <summary>
        /// 인터넷 연결 확인 후 로그인 진입
        /// * Retry 버튼 클릭시 연결 재시도
        /// </summary>
        public override void Handle()
        {
            _loginStep._TMP_load.text = "LogIn...";

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _loginStep._go_Loading.SetActive(false);
                _loginStep._go_Retry.SetActive(true);
            }
            else
                _loginStep.SetState(new StartLogin(_loginStep));
        }
    }

    class StartLogin : State
    {
        public StartLogin(Login loginStep) : base(loginStep) { }

        /// <summary>
        /// login 진입
        /// </summary>
        public override void Handle()
        {
            _loginStep._go_Retry.SetActive(false);
            _loginStep._go_Loading.SetActive(true);

#if UNITY_EDITOR
            _loginStep.SetState(new LoginPlayFabTestAccount(_loginStep));
#elif UNITY_ANDROID
            _loginStep.SetState(new LoginGoogle(_loginStep));
#endif
        }
    }

    class LoginPlayFabTestAccount : State
    {
        public LoginPlayFabTestAccount(Login loginStep) : base(loginStep) { }

        public override void Handle()
        {
            var request = new LoginWithEmailAddressRequest { Email = "testAccount@AeDeong.com", Password = "TestAccount" };
            PlayFabClientAPI.LoginWithEmailAddress(request,
                (success) =>
                {
                    AD.Managers.DataM.StrID = success.PlayFabId;
                    _loginStep.SetState(new CheckData(_loginStep));
                },
                (failed) =>
                // SignUpWithTestAccount
                {
                    var request = new RegisterPlayFabUserRequest { Email = "testAccount@AeDeong.com", Password = "TestAccount", RequireBothUsernameAndEmail = false };
                    PlayFabClientAPI.RegisterPlayFabUser(request,
                        (success) =>
                        {
                            AD.Managers.DataM.StrID = success.PlayFabId;
                            _loginStep.SetState(new UpdateDisplayName(_loginStep, "testAccount"));
                        },
                        (failed) => Debug.Log("Failed SignUpWithTestAccount  " + failed.ErrorMessage));
                });
        }
    }

    class LoginPlayFab : State
    {
        public LoginPlayFab(Login loginStep) : base(loginStep) { }

        public override void Handle()
        {
            string id = $"{Social.localUser.id}@AeDeong.com";

            var request = new LoginWithEmailAddressRequest { Email = id, Password = "AeDeong" };
            PlayFabClientAPI.LoginWithEmailAddress(request, _loginStep.OnLoginWithPlayFabSuccess, _loginStep.OnLoginWithPlayFabFailure);
        }
    }

    class LoginGoogle : State
    {
        public LoginGoogle(Login loginStep) : base(loginStep) { }

        public override void Handle()
        {
            PlayGamesPlatform.Instance.Authenticate(_loginStep.ProcessAuthentication);
        }
    }

    class UpdateDisplayName : State
    {
        string displayName = string.Empty;

        public UpdateDisplayName(Login loginStep, string displayName) : base(loginStep) { this.displayName = displayName; }

        public override void Handle()
        {
            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            },
            result =>
            {
                _loginStep._go_NickName.SetActive(false);
                _loginStep._go_WarningRule.SetActive(false);
                _loginStep._go_WarningNAE.SetActive(false);

                AD.Managers.ServerM.SetData(new Dictionary<string, string> { { "NickName", displayName } }, GetAllData: false, Update: false);

                _loginStep._TMP_load.text = "Save NickName...";

                // SaveNickName 후 CheckData 진입
                _loginStep._co_SaveData = _loginStep.StartCoroutine(_loginStep.SaveNickName());
            },
            error =>
            {
                //Debug.LogError(error.GenerateErrorReport());
                _loginStep._go_WarningNAE.SetActive(true);
            }); ;
        }
    }

    class CheckData : State
    {
        public CheckData(Login loginStep) : base(loginStep) { }

        /// <summary>
        /// Login 마지막 Step
        /// </summary>
        public override void Handle()
        {
            _loginStep._TMP_load.text = "Check Data...";

            //AD.Managers.DataM.UpdatePlayerData();

            _loginStep._co_Login = _loginStep.StartCoroutine(_loginStep.InitPlayerData());
        }
    }
    #endregion

#if UNITY_EDITOR
    [CustomEditor(typeof(Login))]
    public class customEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Google Login with PlayFab", MessageType.Info);

            base.OnInspectorGUI();
        }
    }
#endif
}