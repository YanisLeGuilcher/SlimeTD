using System.Collections.Generic;
using System.Linq;
using Script.Data;
using Script.Entities.Monster;
using Script.Manager;
using UnityEngine;

namespace Script.Defender
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Defender : MonoBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private float damage = 1;
        [SerializeField] private float fireRate = 1;
        [SerializeField] private float rotationSpeed = 1;
        [SerializeField] private DamageType damageType = DamageType.Classic;
        [SerializeField] private CircleCollider2D rangeCollider;
        [SerializeField] private SpriteRenderer rangePreview;
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private AttackStyle attackStyle = AttackStyle.First;


        private readonly List<Monster> _monsterInRange = new();

        private float _reload;

        private bool Ready => _reload <= 0 && _target && attackStyle != AttackStyle.None && IsLookingAtTarget();

        private Monster _target;

        private void Awake()
        {
            rangeCollider.gameObject.layer = LayerMask.NameToLayer("TowerRange");
            gameObject.layer = LayerMask.NameToLayer("Tower");
            rangeCollider.radius = range;
            rangePreview.transform.localScale = new Vector3(range*2, range*2, 1);
            rangePreview.enabled = false;
            rangeCollider.isTrigger = true;
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Update()
        {
            _reload -= Time.deltaTime * LevelManager.Instance.Speed;
            
            if (!_target)
                return;
            if (_target.Dead)
            {
                _monsterInRange.Remove(_target);
                _target = SearchTarget();
                if (!_target)
                    return;
            }
            
            LookAtTarget();
            if(!Ready)
                return;
            
            Attack();
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            var script = other.GetComponent<Monster>();
            if(_monsterInRange.Contains(script))
                return;
            _monsterInRange.Add(script);
            script.Die.AddListener(() => _monsterInRange.Remove(script));
            script.Finish.AddListener(() => _monsterInRange.Remove(script));

            _target = SearchTarget();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var script = other.GetComponent<Monster>();
            _monsterInRange.Remove(script);
            script.Die.RemoveListener(() => _monsterInRange.Remove(script));
            script.Finish.RemoveListener(() => _monsterInRange.Remove(script));
            
            _target = SearchTarget();
        }

        private bool IsLookingAtTarget()
        {
            if (!_target) 
                return false;

            Vector3 direction = _target.transform.position - transform.position;

            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.z, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            return Mathf.Abs(angleDifference) < 5f;
        }

        private void LookAtTarget()
        {
            Vector3 direction = _target.transform.position - transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * LevelManager.Instance.Speed);
        }

        private Monster SearchTarget()
        {
            if (_monsterInRange.Count > 0)
            {
                return attackStyle switch
                {
                    AttackStyle.First => _monsterInRange.FirstInPosition(),
                    AttackStyle.Last => _monsterInRange.LastInPosition(),
                    AttackStyle.Strongest => _monsterInRange.Strongest(),
                    AttackStyle.Weakest => _monsterInRange.Weakest(),
                    AttackStyle.Looser => _monsterInRange.Weakest(),
                    _ => _monsterInRange.First()
                };
            } 
            return null;
        }

        private void Attack()
        {
            _target.TakeDamage(new Damage {Amount = damage, Type = damageType});
            _reload = 1 / fireRate;
        }
    }
}
