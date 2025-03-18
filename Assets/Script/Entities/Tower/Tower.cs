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
        [SerializeField] private CircleCollider2D rangeCollider;
        [SerializeField] private SpriteRenderer rangePreview;
        [SerializeField] private TMP_Text sellCost;
        [SerializeField] private Canvas upgradeCanvas;
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private TowerType towerType;

        protected readonly Dictionary<Bonus, List<float>> Bonus = new();


        public readonly UnityEvent OnSell = new();

        public float Range => 
            range * Bonus.GetValueOrDefault(Data.Enum.Bonus.Range, new List<float> { 1f })
                .Last();

        public int PriceOnSell => (int)(DataSerializer.GetPriceOfTower(towerType) * .7f);
        
        protected virtual void Start()
        {
            Towers.Add(gameObject, this);
            gameObject.layer = LayerMask.NameToLayer("Tower");
            rangeCollider.gameObject.layer = LayerMask.NameToLayer("TowerRange");
            rangeCollider.radius = range;
            rangeCollider.isTrigger = true;
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            
            rangePreview.transform.localScale = new Vector3(range*2, range*2, 1);
            rangePreview.enabled = false;
            
            sellCost.text = PriceOnSell.ToString();
            
            
            LevelManager.OnClick.AddListener(CatchOtherClick);
        }
        
        private void OnDestroy()
        {
            Towers.Remove(gameObject);
            LevelManager.OnClick.RemoveListener(CatchOtherClick);
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
            LevelManager.Instance.SellTower(this);
            OnSell.Invoke();
        }

        private void CatchOtherClick(MonoBehaviour clicker)
        {
            if (clicker == this || !upgradePanel)
                return;
            
            upgradePanel.SetActive(false);
            rangePreview.enabled = false;
            upgradeCanvas.sortingOrder = 1000;
        }

        public void AddBonus(Booster booster)
        {
            var newBonus = booster.GivenBonus;
            foreach (var b in newBonus)
            {
                Bonus[b.Key].Add(b.Value);
                Bonus[b.Key].Sort();
                booster.OnSell.AddListener(() => Bonus[b.Key].Remove(b.Value));
            }
        }
    }
}

