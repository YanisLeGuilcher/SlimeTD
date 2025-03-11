using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using Script.Manager;
using UnityEngine;

namespace Script.UI
{
    public class ShootEffect : MonoBehaviour
    {
        public static readonly Dictionary<GameObject, ShootEffect> ShootEffects = new();

        [SerializeField] private float speed;
        private void Awake()
        {
            ShootEffects.Add(gameObject, this);
        }
        
        private void OnDestroy()
        {
            ShootEffects.Remove(gameObject);
        }

        public void SetTrajectory(Vector3 start, Vector3 target)
        {
            transform.position = start;
            Vector3 direction = target - transform.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            StartCoroutine(Travel(target));
        }

        private IEnumerator Travel(Vector3 target)
        {
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * LevelManager.DeltaTime);
                yield return null;
            }
            LeanPool.Despawn(gameObject);
        }
    }
}

