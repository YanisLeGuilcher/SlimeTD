using System.Collections.Generic;
using Script.Data;
using Script.Data.Enum;
using UnityEngine;
using UnityEngine.Events;

namespace Script.Entities.Tower
{
    public class Booster : Tower
    {
        public static readonly UnityEvent AttributeBoost = new();
        public static readonly UnityEvent ClearBoost = new();

        public static void ResetBoost()
        {
            ClearBoost.Invoke();
            AttributeBoost.Invoke();
        }
        
        [SerializeField] private List<Tuple<Bonus,float>> bonus = new();
        [SerializeField] private LayerMask boostedMask;

        public Dictionary<Bonus, float> GivenBonus => bonus.ToDictionary();


        protected override void Start()
        {
            SetRangeLayer(LayerMask.NameToLayer("BoosterRange"));
            AttributeBoost.AddListener(RefreshBoost);
            
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            AttributeBoost.RemoveListener(RefreshBoost);
            ResetBoost();
        }

        private void RefreshBoost()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, Range, boostedMask);
            
            foreach (var towerCollider in colliders)
            {
                if(towerCollider.gameObject == gameObject)
                    continue;
                if (Towers.TryGetValue(towerCollider.gameObject, out var script))
                    script.AddBonus(this);
                else
                    Debug.LogWarning($"Can not found script of {towerCollider.name}");
            }
        }
    }
}

