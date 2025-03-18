using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using Script.Data;
using Script.Data.Enum;
using Script.Manager;
using UnityEngine;


namespace Script.Entities.Monster
{
    [RequireComponent(typeof(Collider2D))]
    public class MonsterSpawner : Monster
    {
        [SerializeField] private List<MonsterType> monsterToSpawn;
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
                animator.Play(summonHash);
                
                yield return LevelManager.WaitForSecond(summonDuration);
                
                if (Dead)
                    break;

                float spawnedProgress = Progress - .005f;
                
                foreach (var drop in monsterToSpawn)
                {
                    var prefab = PrefabFactory.Instance[drop];
                    var go = LeanPool.Spawn(prefab, SplineContainer.EvaluatePosition(0), Quaternion.identity);
                    if (!Monsters.TryGetValue(go, out var script))
                    {
                        Debug.LogWarning($"Monster script for {drop} not found");
                        continue;
                    }
                
                    script.SetSpline(SplineContainer, spawnedProgress);
                    MonsterGenerator.Instance.AddMonster(script);
                    spawnedProgress -= .005f;
                }
                
                yield return LevelManager.WaitForSecond(timeBetweenSummon);
            }
        }

    }
}

