
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

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
            GoMainScene();
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

    private void GoMainScene()
    {
        AD.Managers.SceneM.NextScene(AD.GameConstants.Scene.Main);
    }

    public void ClickedOK() => AD.Managers.SoundM.UI_Ok();
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

#if UNITY_ANDROID
            _loginStep.SetState(new LoginGoogle(_loginStep));
#endif
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
    #endregion
}