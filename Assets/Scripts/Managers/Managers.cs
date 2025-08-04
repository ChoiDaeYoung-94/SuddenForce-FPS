using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework.Internal;
using UnityEngine;

namespace AD
{
    public interface ISubManager
    {
        UniTask InitAsync();
        void Release();
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

        private List<ISubManager> _subManagers = new();
        public static ResourceManager ResourceManager { get; } = new();
        public static SceneManager SceneManager { get; } = new();
        public static TableManager TableManager { get; } = new();
        public static UpdateManager UpdateManager { get; } = new();
        public static PoolManager PoolManager => PoolManager.Instance;
        public static PopupManager PopupManager => PopupManager.Instance;
        public static SoundManager SoundManager => SoundManager.Instance;
        public static UIManager UIManager => UIManager.Instance;

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            InitAsync().Forget();
        }

        private async UniTask InitAsync()
        {
            _subManagers.Add(ResourceManager);
            _subManagers.Add(SceneManager);
            _subManagers.Add(TableManager);
            _subManagers.Add(UpdateManager);
            _subManagers.Add(PoolManager);
            _subManagers.Add(PopupManager);
            _subManagers.Add(SoundManager);
            _subManagers.Add(UIManager);
            
            foreach (var manager in _subManagers)
            {
                await manager.InitAsync();
            }

            SceneManager.ChangeScene(GameConstants.Scene.Login);
        }

        private void OnDestroy()
        {
            foreach (var manager in _subManagers)
            {
                manager?.Release();
            }

            _instance = null;
        }
    }
}