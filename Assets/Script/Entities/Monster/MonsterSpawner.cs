using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using Script.Manager;
using UnityEngine;


namespace Script.Entities.Monster
{
    [RequireComponent(typeof(Collider2D))]
    public class MonsterSpawner : Monster
    {
        [SerializeField] private List<GameObject> monsterToSpawn;
        [SerializeField] private float timeBetweenSummon = 10;

        private readonly int summonHash = Animator.StringToHash("Summon");
        private float summonDuration;
        
        protected override void Start()
        {
            base.Start();
            

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;

            foreach (AnimationClip clip in controller.animationClips)
            {
                if (Animator.StringToHash(clip.name) != summonHash)
                    continue;
                
                summonDuration = clip.length;
                break;
            }
            
            StartCoroutine(SummonMonster());
        }

        private IEnumerator SummonMonster()
        {
            while (Alive)
            {
                yield return LevelManager.WaitForSecond(timeBetweenSummon);
                
                if (Dead)
                    break;
                
                animator.Play(summonHash);
                
                yield return LevelManager.WaitForSecond(summonDuration);
                
                if (Dead)
                    break;
                
                int round = 0;
                
                foreach (var drop in monsterToSpawn)
                {
                    var go = LeanPool.Spawn(drop, SplineContainer.EvaluatePosition(0), Quaternion.identity);
                    if (!Monsters.TryGetValue(go, out var script))
                    {
                        Debug.LogWarning($"Monster script for {go.name} not found");
                        continue;
                    }
                    float newProgress = Progress - 0.005f * round;
                    round++;
                
                    script.SetSpline(SplineContainer, newProgress);
                    MonsterGenerator.Instance.AddMonster(script);
                }
            }
        }

    }
}

