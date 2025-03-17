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

        private Vector3 targetPoint;
        public float TimeTravel => Vector3.Distance(targetPoint,transform.position) / speed;
        private void Awake() => ShootEffects.Add(gameObject, this);
        
        private void OnDestroy() => ShootEffects.Remove(gameObject);

        public void SetTrajectory(Vector3 start, Vector3 target)
        {
            targetPoint = target;
            transform.position = start;
            var direction = target - transform.position;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            StartCoroutine(Travel());
        }

        private IEnumerator Travel()
        {
            while (Vector3.Distance(transform.position, targetPoint) > .01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * LevelManager.DeltaTime);
                yield return null;
            }
            LeanPool.Despawn(gameObject);
        }
    }
}

