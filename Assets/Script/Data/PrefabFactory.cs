using System.Collections.Generic;
using UnityEngine;

namespace Script.Data
{
    public class PrefabFactory : MonoBehaviour
    {
        public static PrefabFactory Instance;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            if(transform.IsRoot())
                DontDestroyOnLoad(gameObject);
        }


        [SerializeField] private List<Tuple<DamageRank, GameObject>> uiDamagePrefab;
        [SerializeField] private List<Tuple<TowerType, GameObject>> towerPrefab;
        [SerializeField] private GameObject levelUI;
        [SerializeField] private List<Tuple<MonsterType, GameObject>> monsterPrefabs;


        public GameObject this[DamageRank rank] => uiDamagePrefab.GetTuple(rank);
        public GameObject this[TowerType type] => towerPrefab.GetTuple(type);
        public GameObject this[MonsterType type] => monsterPrefabs.GetTuple(type);

        public static GameObject LevelUI => Instance.levelUI;
    }
}

