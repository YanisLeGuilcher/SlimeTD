using System.Collections.Generic;
using System.IO;
using Script.Data;
using UnityEngine;

namespace Script.Manager
{
    public class SaveManager : MonoBehaviour
    {
        
        [SerializeField] private List<Level> levels;

        public static void SaveLevel(Level level, LevelData data)
        {
            var dataPath = Application.dataPath + "/../Levels";
            if(!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            data.Save(level);
        }

        public static void DeleteSave(Level level)
        {
            File.Delete(Application.dataPath + "/../Levels/" + level.name);
        }


        public static bool LevelUseSave(Level level) => File.Exists(Application.dataPath + "/../Levels/" + level.name);
        public static LevelData GetSave(Level level) => LevelData.LoadData(level);
    }
}

