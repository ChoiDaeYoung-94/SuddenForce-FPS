using System.Collections.Generic;

namespace AD
{
    /// <summary>
    /// 게임에서 사용될 상수를 정의하는 클래스
    /// </summary>
    public static class GameConstants
    {
        public enum Scene
        {
            Login,
            Lobby,
            Room
        }

        public enum GameScene
        {
            DesertHouse
        }

        #region Resources Path

        /// <summary>
        /// Prefabs 폴더의 경우 각 씬별로 사용되는 prefab을 나열
        /// -> Load시 GetPath 후 + 로 씬이름도 붙여서 사용
        /// </summary>
        public enum ResourceCategory
        {
            Root,
            Managers,
            UI,
            Popup,
            Prefabs
        }

        private static readonly Dictionary<ResourceCategory, string> ResourcesPathMap = new()
        {
            { ResourceCategory.Root, "" },
            { ResourceCategory.Managers, "Managers/" },
            { ResourceCategory.UI, "Prefabs/UI/" },
            { ResourceCategory.Popup, "Popup/" },
            { ResourceCategory.Prefabs, "Prefabs/" },
        };

        public static string GetPath(ResourceCategory category)
        {
            return ResourcesPathMap.TryGetValue(category, out var path) ? path : "";
        }

        #endregion
    }
}