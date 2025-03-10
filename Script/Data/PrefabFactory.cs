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

        public GameObject this[DamageRank rank] => uiDamagePrefab.GetTuple(rank);
        public GameObject this[TowerType type] => towerPrefab.GetTuple(type);
    }
}

