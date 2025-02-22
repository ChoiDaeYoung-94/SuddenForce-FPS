using UnityEngine;

namespace AD
{
    /// <summary>
    /// 미리 정의하고 사용할 것들
    /// </summary>
    public class Define : MonoBehaviour
    {
        /// <summary>
        /// Pool에서 가져온 객체 기본으로 담아두는 go.name
        /// </summary>
        public static string _activePool = "ActivePool";

        /// <summary>
        /// 사용중인 Scene
        /// </summary>
        public enum Scenes
        {
            NextScene,
            Login,
            Main,
            Lobby,
            Room,
            DesertHouse
        }

        public enum Maps
        {
            DesertHouse
        }

        /// <summary>
        /// IAPItems 목록
        /// </summary>
        public enum IAPItems
        {
            PRODUCT_NO_ADS
        }
    }
}