using Script.Data;
using Script.Data.Enum;
using Script.Manager;
using UnityEngine;

namespace Script.UI
{
    public class TowerPlacementButton : MonoBehaviour
    {
        [SerializeField] private TowerType type;

        public void PlaceTower()
        {
            LevelManager.Instance.SpawnTower(type, transform.parent.position);
        }
    }
}

