using System.Collections.Generic;
using UnityEngine;

namespace Script.Data
{
    public class DataSerializer : MonoBehaviour
    {
        [SerializeField] private List<Level> levels;
        [SerializeField] private List<Tuple<TowerType,int>> towerPrice;
        [SerializeField] private List<Wave> waves;

        private static DataSerializer instance;

        private void Awake()
        {
            if (instance && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static List<Level> Levels => instance.levels;
        public static int GetPriceOfTower(TowerType type) => instance.towerPrice.GetTuple(type);

        public static Wave GetWave(int waveCount) => instance.waves[waveCount - 1];
    }
}

