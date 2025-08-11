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

    [System.Serializable]
    public class SoundTableData : ITableData
    {
        public string Type; // "BGM" 또는 "SFX"
        public string Name;

        public string GetKey()
        {
            return $"{Type}/{Name}";
        }
    }

    [System.Serializable]
    public class UITableData : ITableData
    {
        public GameConstants.Scene Scene;
        public string UIName;
        public UIBase.UIType Type;
        public UIBase.LoadType LoadType;

        public string GetKey()
        {
            return $"{Scene}/{UIName}";
        }
    }
}