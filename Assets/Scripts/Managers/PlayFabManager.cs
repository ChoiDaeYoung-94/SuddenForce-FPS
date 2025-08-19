using System;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace AD
{
    public class PlayFabManager : ISubManager
    {
        public async UniTask InitAsync()
        {
            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                var cfg = Resources.Load<PlayFabConfig>("PlayFabConfig");
                if (cfg != null)
                {
                    PlayFabSettings.staticSettings.TitleId = cfg.GetTitleId();
                }
                else
                {
                    DebugLogger.LogLoadError("PlayFabConfig");
                }
            }
            await UniTask.Yield();
        }

        public void Release()
        {
            
        }

        private static void EnsureTitleId()
        {
            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
                throw new Exception("PlayFab TitleId is not set.");
        }

        /// <summary>GPGS v2의 ServerAuthCode로 PlayFab 로그인</summary>
        public async UniTask<LoginResult> LoginWithGoogleAuthCodeAsync(string serverAuthCode, bool createAccount = true)
        {
            EnsureTitleId();

            var tcs = new UniTaskCompletionSource<LoginResult>();
            var req = new LoginWithGoogleAccountRequest
            {
                TitleId = PlayFabSettings.staticSettings.TitleId,
                ServerAuthCode = serverAuthCode,
                CreateAccount = createAccount
            };

            PlayFabClientAPI.LoginWithGoogleAccount(req,
                r => tcs.TrySetResult(r),
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport())));

            return await tcs.Task;
        }

        /// <summary>에디터/테스트용 CustomID 로그인</summary>
        public async UniTask<LoginResult> LoginWithCustomIdAsync(string customId, bool createAccount = true)
        {
            EnsureTitleId();

            var tcs = new UniTaskCompletionSource<LoginResult>();
            var req = new LoginWithCustomIDRequest
            {
                TitleId = PlayFabSettings.staticSettings.TitleId,
                CustomId = customId,
                CreateAccount = createAccount
            };

            PlayFabClientAPI.LoginWithCustomID(req,
                r => tcs.TrySetResult(r),
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport())));

            return await tcs.Task;
        }
    }
}