using UnityEngine;

namespace AD
{
    /// <summary>
    /// Manager 스크립트 관리
    /// </summary>
    public class Managers : MonoBehaviour
    {
        /// <summary>
        /// Singleton - 객체 오직 1
        /// Manager관련 script 모두 등록
        /// </summary>
        private static Managers _instance;
        public static Managers Instance { get { return _instance; } }

        private PoolManager _poolM = new PoolManager();
        public static PoolManager PoolM { get { return _instance._poolM; } }

        [SerializeField] private PopupManager _popupM = null;
        public static PopupManager PopupM { get { return _instance._popupM; } }

        private ResourceManager _resourceM = new ResourceManager();
        public static ResourceManager ResourceM { get { return _instance._resourceM; } }

        [SerializeField] private SceneManager _sceneM = null;
        public static SceneManager SceneM { get { return _instance._sceneM; } }

        [SerializeField] private SoundManager _soundM = null;
        public static SoundManager SoundM { get { return _instance._soundM; } }

        [SerializeField] private UpdateManager _updateM = null;
        public static UpdateManager UpdateM { get { return _instance._updateM; } }

        [SerializeField] private GameObject _networkRunnerObject = null;

        [Header("--- Managers data ---")]
        [Tooltip("Pool에 사용할 GameObject")]
        public GameObject[] PoolGameObjects = null;
        [Tooltip("Pool에 사용할 UI")]
        public GameObject[] PoolUIs = null;

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            PoolM.Init();
            SoundM.Init();
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public static void CreateNetworkRunner()
        {
            Instantiate(_instance._networkRunnerObject);
        }
    }
}
