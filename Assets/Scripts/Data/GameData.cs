namespace AD.GameData
{
    [System.Serializable]
    public class PoolTableData : ITableData
    {
        public GameConstants.Scene Scene;
        public string PrefabName;
        public int Count;
        public GameConstants.PoolType Type;
        public string GetKey()
        {
            return $"{Scene}/{PrefabName}";
        }
    }
}