using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using Script.Data;
using Script.Data.Enum;
using Script.Manager;
using Script.UI;
using TMPro;
using UnityEngine;

namespace Script.Entities.Tower
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Defender : Tower
    {
        [Header("Stats")]
        [SerializeField] private int damage = 1;
        [SerializeField] private float fireRate = 1;
        [SerializeField] private float rotationSpeed = 1;
        [SerializeField] private DamageType damageType = DamageType.Classic;
        [SerializeField] private AttackStyle attackStyle = AttackStyle.First;
        
        [Header("UI")]
        [SerializeField] private GameObject shootEffectPrefab;
        [SerializeField] private GameObject mesh;
        [SerializeField] private TMP_Text attackStyleText;
        
        
        private readonly List<Monster.Monster> monsterInRange = new();

        private float reload;

        private bool Ready => reload <= 0 && target && attackStyle != AttackStyle.None && IsLookingAtTarget();

        private Monster.Monster target;

        private int Damage => (int)(damage * Bonus[Data.Enum.Bonus.Damage].Last());
        private float FireRate => fireRate * Bonus[Data.Enum.Bonus.FireRate].Last();
        private float RotationSpeed => rotationSpeed * Bonus[Data.Enum.Bonus.RotationSpeed].Last();

        public AttackStyle AttackStyle
        {
            get => attackStyle;
            set
            {
                attackStyle = value;
                attackStyleText.text = attackStyle.ToString();
            }
        }


        
        protected override void Start()
        {
            base.Start();
            
            attackStyleText.text = attackStyle.ToString();
            
            SetRangeLayer(LayerMask.NameToLayer("DefenderRange"));
        }

        

        private void FixedUpdate()
        {
            reload -= LevelManager.FixedDeltaTime;
            
            if (!target)
                return;
            if (target.WillDie)
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
            if(other.gameObject.layer != LayerMask.NameToLayer("Monster"))
                return;
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
            if(other.gameObject.layer != LayerMask.NameToLayer("Monster"))
                return;
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

        public void SwitchAttackStyle() => AttackStyle = attackStyle.Next();

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

            mesh.transform.rotation = Quaternion.Lerp(mesh.transform.rotation, targetRotation, RotationSpeed * LevelManager.FixedDeltaTime);
        }

        private Monster.Monster SearchTarget()
        {
            monsterInRange.RemoveAll(monster => monster.WillDie);
            if (monsterInRange.Count > 0)
            {
                return attackStyle switch
                {
                    AttackStyle.First => monsterInRange.FirstInPosition(),
                    AttackStyle.Last => monsterInRange.LastInPosition(),
                    AttackStyle.Strongest => monsterInRange.Strongest(),
                    AttackStyle.Weakest => monsterInRange.Weakest(),
                    AttackStyle.Spawner => monsterInRange.Spawner(),
                    _ => monsterInRange.First()
                };
            } 
            return null;
        }

        private void Attack()
        {
            if (target.WillDie)
                return;
            if (shootEffectPrefab)
            {
                var go = LeanPool.Spawn(shootEffectPrefab);
                if (ShootEffect.ShootEffects.TryGetValue(go, out var script))
                {
                    var newDamage = new Damage { Amount = Damage, Type = damageType };
                    script.SetTrajectory(transform.position, target.transform.position);
                    target.TakePreviousDamage(newDamage);
                    StartCoroutine(TakeDamageAfterTime(script.TimeTravel, target, newDamage));
                }
                else
                    Debug.LogWarning($"ShootEffect script for {go.name} not found");
            }
            else
                target.TakeDamage(new Damage {Amount = Damage, Type = damageType}, true);
            
            reload = 1 / FireRate;
        }

        private IEnumerator TakeDamageAfterTime(float time, Monster.Monster monster, Damage newDamage)
        {
            yield return LevelManager.WaitForSecond(time);
            monster.TakeDamage(newDamage);
        }

        public void SetAttackStyle(AttackStyle newAttackStyle) => attackStyle = newAttackStyle;
    }
}
