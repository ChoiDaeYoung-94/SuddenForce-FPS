using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace AD
{
    /// <summary>
    /// Scene 전환을 관리
    /// 목표 씬을 additive로 로드한 후, 현재 씬을 언로드하는 방식
    /// </summary>
    public class SceneManager : ISubManager
    {
        private GameConstants.Scene _targetScene;
        private CancellationTokenSource _ctsSceneChange;

        public async UniTask InitAsync()
        {
            _ctsSceneChange = new CancellationTokenSource();
            
            await UniTask.Yield();
        }

        public void Release()
        {
            _ctsSceneChange?.Cancel();
            _ctsSceneChange?.Dispose();
            _ctsSceneChange = null;
        }

        public void ChangeScene(GameConstants.Scene targetScene)
        {
            DebugLogger.Log("targetScene으로 전환");

            Managers.PopupManager.PopupSceneLoading();
            //Managers.SoundManager.PauseBGM();

            _targetScene = targetScene;

            ChangeSceneAsync(_ctsSceneChange.Token).Forget();
        }

        private async UniTask ChangeSceneAsync(CancellationToken token)
        {
            UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            AsyncOperation loadAsyncOp =
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_targetScene.ToString(),
                    UnityEngine.SceneManagement.LoadSceneMode.Additive);
            loadAsyncOp.allowSceneActivation = false;

            while (!loadAsyncOp.isDone && !token.IsCancellationRequested)
            {
                DebugLogger.Log($"{loadAsyncOp.progress} - load progress");

                if (loadAsyncOp.progress >= 0.9f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1d), cancellationToken: token);
                    loadAsyncOp.allowSceneActivation = true;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            UnityEngine.SceneManagement.Scene targetScene =
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(_targetScene.ToString());
            if (targetScene.IsValid())
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(targetScene);
            }

            GameConstants.Scene curScene = (GameConstants.Scene)Enum.Parse(typeof(GameConstants.Scene), currentScene.name);
            Managers.PoolManager.ClearScenePools(curScene);
            await GetIScene(curScene).ReleaseAsync();
            AsyncOperation unloadAsyncOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentScene);
            while (!unloadAsyncOp.isDone && !token.IsCancellationRequested)
            {
                DebugLogger.Log($"{unloadAsyncOp.progress} - unload progress");
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            await Resources.UnloadUnusedAssets().ToUniTask(cancellationToken: token);
            await UniTask.RunOnThreadPool(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });

            await Managers.PoolManager.InitPoolsForScene(_targetScene);
            await GetIScene(_targetScene).InitAsync();
            Managers.PopupManager.ClosePopupSceneLoading();
            //Managers.SoundManager.UnpauseBGM();
        }

        private IScene GetIScene(GameConstants.Scene scene)
            => scene switch
            {
                GameConstants.Scene.Login => (LoginScene.Instance),
                _ => (null),
            };
    }
}