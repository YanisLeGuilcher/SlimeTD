
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using Script.Data;
using Script.Manager;
using Script.UI;
using TMPro;
using UnityEngine;

namespace Script.Entities.Defender
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Defender : MonoBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private float damage = 1;
        [SerializeField] private float fireRate = 1;
        [SerializeField] private float rotationSpeed = 1;
        [SerializeField] private int priceOnSell = 200;
        [SerializeField] private DamageType damageType = DamageType.Classic;
        [SerializeField] private CircleCollider2D rangeCollider;
        [SerializeField] private SpriteRenderer rangePreview;
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private AttackStyle attackStyle = AttackStyle.First;
        [SerializeField] private TMP_Text sellCost;
        [SerializeField] private GameObject mesh;
        [SerializeField] private GameObject shootEffectPrefab;

        [SerializeField] private Canvas upgradeCanvas;
        [SerializeField] private GameObject upgradePanel;


        private readonly List<Monster.Monster> monsterInRange = new();

        private float reload;

        private bool Ready => reload <= 0 && target && attackStyle != AttackStyle.None && IsLookingAtTarget();

        private Monster.Monster target;


        public int PriceOnSell => priceOnSell;
        
        private void Start()
        {
            rangeCollider.gameObject.layer = LayerMask.NameToLayer("TowerRange");
            gameObject.layer = LayerMask.NameToLayer("Tower");
            rangeCollider.radius = range;
            rangePreview.transform.localScale = new Vector3(range*2, range*2, 1);
            rangePreview.enabled = false;
            rangeCollider.isTrigger = true;
            rigidBody.bodyType = RigidbodyType2D.Kinematic;

            sellCost.text = priceOnSell.ToString();
            
            LevelManager.OnClick.AddListener(CatchOtherClick);
        }

        private void OnDestroy()
        {
            LevelManager.OnClick.RemoveListener(CatchOtherClick);
        }

        private void FixedUpdate()
        {
            reload -= LevelManager.FixedDeltaTime;
            
            if (!target)
                return;
            if (target.Dead)
            {
                monsterInRange.Remove(target);
                target = SearchTarget();
                if (!target)
                    return;
            }
            
            LookAtTarget();
            if(!Ready)
                return;
            
            Attack();
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!Monster.Monster.Monsters.TryGetValue(other.gameObject, out var script))
            {
                Debug.LogWarning($"Monster script for {other.name} not found");
                return;
            }
            if(monsterInRange.Contains(script))
                return;
            monsterInRange.Add(script);
            script.Die.AddListener(() => monsterInRange.Remove(script));
            script.Finish.AddListener(() => monsterInRange.Remove(script));

            target = SearchTarget();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!Monster.Monster.Monsters.TryGetValue(other.gameObject, out var script))
            {
                Debug.LogWarning($"Monster script for {other.name} not found");
                return;
            }
            monsterInRange.Remove(script);
            script.Die.RemoveListener(() => monsterInRange.Remove(script));
            script.Finish.RemoveListener(() => monsterInRange.Remove(script));
            
            target = SearchTarget();
        }


        public void OnClick()
        {
            if (!upgradePanel)
                return;
            
            upgradePanel.SetActive(true);
            upgradeCanvas.sortingOrder = 600;
        }


        public void Upgrade(int type)
        {
            LevelManager.Instance.UpgradeTower(this, (TowerType)Enum.ToObject(typeof(TowerType), type));
        }

        public void Sell() => LevelManager.Instance.SellTower(this);

        private void CatchOtherClick()
        {
            if (!upgradePanel)
                return;
            
            upgradePanel.SetActive(false);
            upgradeCanvas.sortingOrder = 500;
        }

        private bool IsLookingAtTarget()
        {
            if (!target) 
                return false;

            Vector3 direction = target.transform.position - mesh.transform.position;

            float angleDifference = Mathf.DeltaAngle(mesh.transform.eulerAngles.z, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            return Mathf.Abs(angleDifference) < 5f;
        }

        private void LookAtTarget()
        {
            Vector3 direction = target.transform.position - mesh.transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            mesh.transform.rotation = Quaternion.Lerp(mesh.transform.rotation, targetRotation, rotationSpeed * LevelManager.FixedDeltaTime);
        }

        private Monster.Monster SearchTarget()
        {
            if (monsterInRange.Count > 0)
            {
                return attackStyle switch
                {
                    AttackStyle.First => monsterInRange.FirstInPosition(),
                    AttackStyle.Last => monsterInRange.LastInPosition(),
                    AttackStyle.Strongest => monsterInRange.Strongest(),
                    AttackStyle.Weakest => monsterInRange.Weakest(),
                    AttackStyle.Looser => monsterInRange.Weakest(),
                    _ => monsterInRange.First()
                };
            } 
            return null;
        }

        private void Attack()
        {
            if(target.Dead)
                return;
            if (shootEffectPrefab)
            {
                var go = LeanPool.Spawn(shootEffectPrefab);
                if (ShootEffect.ShootEffects.TryGetValue(go, out var script))
                {
                    script.SetTrajectory(transform.position, target.transform.position);
                    StartCoroutine(TakeDamageAfterTime(script.TimeTravel, target));
                }
                else
                    Debug.LogWarning($"ShootEffect script for {go.name} not found");
            }
            else
                target.TakeDamage(new Damage {Amount = damage, Type = damageType});
            
            reload = 1 / fireRate;
        }

        private IEnumerator TakeDamageAfterTime(float time, Monster.Monster monster)
        {
            yield return LevelManager.WaitForSecond(time);
            monster.TakeDamage(new Damage {Amount = damage, Type = damageType});
        }
        
    }
}
