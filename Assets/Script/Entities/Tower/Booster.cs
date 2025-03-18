using System.Collections.Generic;
using Script.Data;
using Script.Data.Enum;
using UnityEngine;

namespace Script.Entities.Tower
{
    public class Booster : Tower
    {
        [SerializeField] private List<Tuple<Bonus,float>> bonus = new();

        public Dictionary<Bonus, float> GivenBonus => bonus.ToDictionary();

        protected override void Start()
        {
            base.Start();

            var colliders = Physics2D.OverlapCircleAll(transform.position, Range, 1 << LayerMask.NameToLayer("Tower"));

            foreach (var towerCollider in colliders)
            {
                if (Towers.TryGetValue(towerCollider.gameObject, out var script))
                    script.AddBonus(this);
                else
                    Debug.LogWarning($"Can not found script of {towerCollider.name}");
            }
        }
    }
}

