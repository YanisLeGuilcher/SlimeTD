using System.IO;
using Script.Data;
using UnityEngine;

namespace Script.Manager
{
    public static class SaveManager
    {
#if UNITY_EDITOR
        public static readonly string DataPath = Application.persistentDataPath + "/../";
#else
        public static readonly string DataPath = Application.persistentDataPath + "/";
#endif

        public static void SaveLevel(Level level, LevelData data)
        {
            var dataPath = DataPath + "Levels";
            if(!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            data.Save(level);
        }

        public static void DeleteSave(Level level) => File.Delete(DataPath + "Levels/" + level.name);


        public static bool LevelUseSave(Level level) => File.Exists(DataPath + "Levels/" + level.name);
        public static LevelData GetSave(Level level) => LevelData.LoadData(level);
    }
}

