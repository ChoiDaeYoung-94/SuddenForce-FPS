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
    public class SceneManager : MonoBehaviour
    {
        private AD.GameConstants.Scene _targetScene;
        private CancellationTokenSource _ctsSceneChange;

        private void Awake()
        {
            _ctsSceneChange = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _ctsSceneChange?.Cancel();
            _ctsSceneChange?.Dispose();
            _ctsSceneChange = null;
        }

        public void ChangeScene(AD.GameConstants.Scene targetScene)
        {
            AD.DebugLogger.Log("SceneManager", "targetScene으로 전환");

            AD.Managers.PopupM.PopupSceneLoading();
            AD.Managers.SoundM.PauseBGM();

            _targetScene = targetScene;

            ChangeSceneAsync(_ctsSceneChange.Token).Forget();
        }

        private async UniTask ChangeSceneAsync(CancellationToken token)
        {
            UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            AsyncOperation loadAsyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_targetScene.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Additive);
            loadAsyncOp.allowSceneActivation = false;

            while (!loadAsyncOp.isDone && !token.IsCancellationRequested)
            {
                AD.DebugLogger.Log("SceneManager", $"{loadAsyncOp.progress} - load progress");

                if (loadAsyncOp.progress >= 0.9f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1d), cancellationToken: token);
                    loadAsyncOp.allowSceneActivation = true;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            UnityEngine.SceneManagement.Scene targetScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(_targetScene.ToString());
            if (targetScene.IsValid())
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(targetScene);
            }

            AsyncOperation unloadAsyncOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentScene);
            while (!unloadAsyncOp.isDone && !token.IsCancellationRequested)
            {
                AD.DebugLogger.Log("SceneManager", $"{unloadAsyncOp.progress} - unload progress");
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            await Resources.UnloadUnusedAssets().ToUniTask(cancellationToken: token);

            AD.Managers.PopupM.ClosePopupSceneLoading();
            AD.Managers.SoundM.UnpauseBGM();
        }
    }
}