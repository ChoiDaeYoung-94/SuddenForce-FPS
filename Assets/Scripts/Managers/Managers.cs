using System.Collections.Generic;
using UnityEngine;

namespace AD
{
    public interface ISubManager
    {
        void Init();
    }
    
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
        public static Managers Instance => _instance;
        
        private List<ISubManager> _subManagers = new List<ISubManager>();
        public static PoolManager PoolManager => new PoolManager();
        public static ResourceManager ResourceManager => new ResourceManager();
        public static SceneManager SceneManager => new SceneManager();
        public static UpdateManager UpdateManager => new UpdateManager();
        public static PopupManager PopupManager => PopupManager.Instance;
        public static SoundManager SoundManager => SoundManager.Instance;

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
            _subManagers.Add(PoolManager);
            _subManagers.Add(ResourceManager);
            _subManagers.Add(SceneManager);
            _subManagers.Add(UpdateManager);
            _subManagers.Add(PopupManager);
            _subManagers.Add(SoundManager);
            
            foreach (var manager in _subManagers)
                manager.Init();
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
