using System;
using System.Collections.Generic;
using System.Linq;
using Script.Data;
using Script.Data.Enum;
using Script.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Script.Entities.Tower
{
    public class Tower : MonoBehaviour
    {
        public static readonly Dictionary<GameObject, Tower> Towers = new();
        
        
        [SerializeField] private float range;
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private CircleCollider2D towerCollider;
        [SerializeField] private CircleCollider2D rangeCollider;
        [SerializeField] private SpriteRenderer rangePreview;
        [SerializeField] private TMP_Text sellCost;
        [SerializeField] private Canvas upgradeCanvas;
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private TowerType towerType;

        protected readonly Dictionary<Bonus, List<float>> Bonus = new();
        
        public readonly UnityEvent OnSell = new();

        public float Range => range * Bonus[Data.Enum.Bonus.Range].Last();

        public int PriceOnSell => DataSerializer.GetSellPriceOfTower(towerType);

        public TowerType Type => towerType;

        private void Awake()
        {
            ClearBoost();
            
            gameObject.layer = LayerMask.NameToLayer("Tower");
            Towers.Add(gameObject, this);
        }

        protected virtual void Start()
        {
            towerCollider.isTrigger = true;
            rangeCollider.isTrigger = true;
            rangePreview.enabled = false;
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            RangeChange();
            
            sellCost.text = PriceOnSell.ToString();
            
            LevelManager.OnClick.AddListener(CatchOtherClick);
            Booster.ClearBoost.AddListener(ClearBoost);
            
            Booster.ResetBoost();
        }
        
        protected virtual void OnDestroy()
        {
            Towers.Remove(gameObject);
            LevelManager.OnClick.RemoveListener(CatchOtherClick);
            Booster.ClearBoost.RemoveListener(ClearBoost);
        }

        protected void SetRangeLayer(int layer) => rangeCollider.gameObject.layer = layer;

        private void RangeChange()
        {
            rangeCollider.radius = Range;
            rangePreview.transform.localScale = new Vector3(Range*2, Range*2, 1);
        }
        
        public void OnClick()
        {
            if (!upgradePanel)
                return;
            
            upgradePanel.SetActive(true);
            rangePreview.enabled = true;
            upgradeCanvas.sortingOrder = 1001;
            LevelManager.OnClick.Invoke(this);
        }
        
        public void Upgrade(int type)
        {
            LevelManager.Instance.UpgradeTower(this, (TowerType)Enum.ToObject(typeof(TowerType), type));
        }

        public virtual void Sell()
        {
            towerCollider.isTrigger = false;
            LevelManager.Instance.SellTower(this);
            OnSell.Invoke();
        }

        private void CatchOtherClick(MonoBehaviour clicker)
        {
            if (clicker == this || !upgradePanel)
                return;

            if (upgradePanel.activeSelf)
                LevelManager.Instance.DontShowPlacementChoice();
            
            upgradePanel.SetActive(false);
            rangePreview.enabled = false;
            upgradeCanvas.sortingOrder = 1000;
        }

        private void ClearBoost()
        {
            Bonus.Clear();
            foreach (Bonus bonus in Enum.GetValues(typeof(Bonus)))
                Bonus.Add(bonus, new() {1});
            RangeChange();
        }

        public void AddBonus(Booster booster)
        {
            var newBonus = booster.GivenBonus;
            foreach (var b in newBonus)
            {
                Bonus[b.Key].Add(b.Value);
                Bonus[b.Key].Sort();

                if (b.Key == Data.Enum.Bonus.Range)
                    RangeChange();
            }
        }
    }
}

