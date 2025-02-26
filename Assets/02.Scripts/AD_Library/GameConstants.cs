namespace AD
{
    /// <summary>
    /// 게임에서 사용될 상수를 정의하는 클래스
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Pool에서 가져온 객체 기본으로 담아두는 GameObject의 이름
        /// </summary>
        public const string ActivePool = "ActivePool";

        /// <summary>
        /// 사용 중인 Scene 목록
        /// </summary>
        public enum Scene
        {
            NextScene,
            Login,
            Main,
            Lobby,
            Room,
            DesertHouse
        }

        /// <summary>
        /// 사용 중인 Map 목록
        /// </summary>
        public enum Map
        {
            DesertHouse
        }
    }
}