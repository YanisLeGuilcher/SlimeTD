using System.Collections.Generic;
using System.Numerics;
using Lean.Pool;
using Script.Data;
using Script.Manager;
using Script.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Script.Entities.Monster
{
    [RequireComponent(typeof(Collider2D))]
    public class Monster : MonoBehaviour
    {
        public static readonly Dictionary<GameObject,Monster> Monsters = new();
        
        
        [SerializeField] private float life = 1;
        [SerializeField] private int damageOnPass = 1;
        [SerializeField] private int moneyEarn = 1;
        [SerializeField] private float speed = 0.5f;
        [SerializeField] private int rank = 1;
        [SerializeField] private List<Tuple<DamageType,float>> weaknessTolerance;
        [SerializeField] private List<GameObject> dropOnDeath;
        [SerializeField] protected Animator animator;
        [SerializeField] private new Collider2D collider;
        
        public float Progress { get; private set; }
        public readonly UnityEvent Die = new();
        public readonly UnityEvent Finish = new();

        public int Rank => rank;
        public int Damage => damageOnPass;
        public int Money => moneyEarn;
        public bool Dead => currentLife <= 0;
        public bool Alive => currentLife > 0;

        public float DeathAnimationTime
        {
            get
            {
                foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    if (clip.name == "Death")
                        return clip.length;
                return 0f;
            }
        }
        protected SplineContainer SplineContainer;

        private readonly int deathHash = Animator.StringToHash("Death");
        private readonly int hurtHash = Animator.StringToHash("Hurt");

        private float currentLife;

        private void Awake() => Monsters.Add(gameObject,this);

        protected virtual void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Monster");
            collider.isTrigger = true;
        }

        private void OnDestroy()
        {
            Monsters.Remove(gameObject);
        }

        private void OnEnable()
        {
            currentLife = life;
            Die.RemoveAllListeners();
            Finish.RemoveAllListeners();
        }

        public void SetSpline(SplineContainer spline, float startProgress = 0)
        {
            SplineContainer = spline;
            Progress = startProgress;
        }

        private void FixedUpdate()
        {
            if (Dead) 
                return;

            animator.speed = LevelManager.Speed;
            
            Progress += speed / SplineContainer.Spline.GetLength() * LevelManager.FixedDeltaTime;
            if (Progress >= 1)
                Finish.Invoke();
            else
            {
                Vector3 positionWithoutOffset = SplineContainer.Spline.EvaluatePosition(Progress);
                Vector3 newPosition = positionWithoutOffset + SplineContainer.transform.position;
                
                TurnMesh((newPosition - transform.position).x > 0.01f);
                
                transform.position = newPosition;
            }
            
        }

        
        
        public void TakeDamage(Damage damage)
        {
            if(Dead)
                return;
            
            var weak = weaknessTolerance.GetTupleOrDefault(damage.Type, 1);
 
            damage.Amount *= weak;
            var damageRank = weak switch
            {
                0 => DamageRank.None,
                < 1 => DamageRank.Reduce,
                > 1 => DamageRank.Critical,
                _ => DamageRank.Classic
            };
            ShowDamage(damageRank, new BigInteger(damage.Amount));

            currentLife -= damage.Amount;
            if (Dead)
                Death();
            else
                animator.Play(hurtHash);
        }

        private void ShowDamage(DamageRank damageRank, BigInteger amount)
        {
            var go = LeanPool.Spawn(PrefabFactory.Instance[damageRank]);
            go.transform.position = transform.position;
            if (DamageUIEffect.DamageUIEffects.TryGetValue(go, out var script))
                script.SetAmount(amount);
            else
                Debug.LogWarning($"DamageUIEffect script for {go.name} not found");
        }

        private void Death()
        {
            int round = 0;
            
            foreach (var drop in dropOnDeath)
            {
                var go = LeanPool.Spawn(drop, SplineContainer.EvaluatePosition(0), Quaternion.identity);
                if (!Monsters.TryGetValue(go, out var script))
                {
                    Debug.LogWarning($"Monster script for {go.name} not found");
                    return;
                }

                float newProgress = Progress - 0.005f * round;
                round++;
                
                script.SetSpline(SplineContainer, newProgress);
                MonsterGenerator.Instance.AddMonster(script);
            }
            animator.Play(deathHash);
            Die.Invoke();
        }

        private void TurnMesh(bool right)
        {
            transform.rotation = right ? Quaternion.identity : Quaternion.Euler(new Vector3(0,180,0));
        }

#if UNITY_EDITOR
        [ContextMenu("Kill")]
        private void Kill()
        {
            TakeDamage(new Damage{Amount = 1000000000, Type = DamageType.IgnoreWeakness});
        }
        
        [ContextMenu("Hurt (one damage)")]
        private void Hurt()
        {
            TakeDamage(new Damage{Amount = 1, Type = DamageType.IgnoreWeakness});
        }
#endif
    }
}

