using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Script.Data
{
    [Serializable]
    public struct LevelData
    {
        public int 
            lifePoint,
            money,
            waveCount;

        public List<TowerData> towers;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine(lifePoint.ToString());
            sb.AppendLine(money.ToString());
            sb.AppendLine(waveCount.ToString());

            foreach (var tower in towers)
                sb.AppendLine(tower.ToString());

            return sb.ToString();
        }
        
        public void Save(Level level) 
        {
            var dataPath = Application.dataPath + "/../Levels/" + level.name;
            if(File.Exists(dataPath))
                File.Delete(dataPath);
            File.WriteAllBytes(dataPath, ToString().ToBase38());
        }

        public static LevelData LoadData(Level level)
        {
            LevelData data = new();
            var dataPath = Application.dataPath + "/../Levels/" + level.name;

            string content = File.ReadAllBytes(dataPath).FromBase38();
            
            var lines = content.Split("\n");

            data.lifePoint = int.Parse(lines[0]);
            data.money = int.Parse(lines[1]);
            data.waveCount = int.Parse(lines[2]);
            
            lines = lines.Skip(3).ToArray();
            

            data.towers = new();

            foreach (var line in lines)
                if(line.Length != 0)
                    data.towers.Add(TowerData.Parse(line));

            return data;
        }
    }
}

