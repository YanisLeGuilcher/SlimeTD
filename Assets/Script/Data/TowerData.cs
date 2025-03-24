using System;
using Script.Data.Enum;
using UnityEngine;

namespace Script.Data
{
    [Serializable]
    public struct TowerData
    {
        public TowerType type;
        public Vector3 position;
        public AttackStyle attackStyle;

        public override string ToString() => $"{type} {position.x} {position.y} {position.z} {attackStyle}";

        public static TowerData Parse(string data)
        {
            TowerData tower = new();
            string[] values = data.Split(" ");
            if(!System.Enum.TryParse(values[0], out tower.type))
                Debug.LogWarning($"{values[0]} can't be parse to TowerType");
            tower.position = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));

            if (values.Length < 5)
                return tower;
            
            if(!System.Enum.TryParse(values[4], out tower.attackStyle))
                Debug.LogWarning($"{values[4]} can't be parse to AttackStyle");
            
            return tower;
        }
    }
}

