using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using Script.Data;
using Script.Entities.Monster;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

namespace Script.Manager
{
    public class MonsterGenerator : MonoBehaviour
    {
        
        public static MonsterGenerator Instance;

        #region SerialiseField
        
        [SerializeField] private List<SplineContainer> trajectories = new();

        #endregion

        #region Private

        private int currentWave = 1;

        private readonly List<Monster> currentMonsters = new();

        private Coroutine spawningMonster;

        #endregion


        #region Event

        public readonly UnityEvent<Monster> OnMonsterFinish = new();
        public readonly UnityEvent<Monster> OnMonsterDie = new();
        public readonly UnityEvent OnWaveFinish = new();

        #endregion

        #region UnityFuncEvent

        private void Awake()
        {
            if(Instance)
                Debug.LogWarning($"Instance of MonsterGenerator not unset: {Instance.name}");
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance && Instance == this)
                Instance = null;
        }

        #endregion
        
        

        public void StartWave() => spawningMonster = StartCoroutine(RtnStartWave());

        private IEnumerator RtnStartWave()
        {
            currentMonsters.Clear();

            var waveParts = DataSerializer.GetWave(currentWave);

            
            foreach (var wavePart in waveParts.waveParts)
            {
                int round = 0;
                while (wavePart.numberOfMonster > round)
                {
                    float tmpWait = wavePart.timeBetweenMonster;
                    while (tmpWait >= 0)
                    {
                        yield return null;
                        tmpWait -= LevelManager.DeltaTime;
                    }
                    var chosenSpline = trajectories[round % trajectories.Count];
                    var go = LeanPool.Spawn(wavePart.monster, chosenSpline.EvaluatePosition(0), Quaternion.identity);
                    if (!Monster.Monsters.TryGetValue(go, out var script))
                    {
                        Debug.LogWarning($"Monster script for {go.name} not found");
                        continue;
                    }
                    script.SetSpline(chosenSpline);
                    AddMonster(script);
                    round++;
                }
            }
            spawningMonster = null;
            currentWave++;
        }

        public void AddMonster(Monster monster)
        {
            monster.Die.AddListener(() => MonsterDie(monster));
            monster.Finish.AddListener(() => MonsterFinish(monster));
            currentMonsters.Add(monster);
        }
        
        private void MonsterDie(Monster monster)
        {
            OnMonsterDie.Invoke(monster);
            currentMonsters.Remove(monster);
            LeanPool.Despawn(monster, monster.DeathAnimationTime);
            if(currentMonsters.Count == 0 && spawningMonster == null)
                OnWaveFinish.Invoke();
        }

        private void MonsterFinish(Monster monster)
        {
            currentMonsters.Remove(monster);
            OnMonsterFinish.Invoke(monster);
            LeanPool.Despawn(monster);
            if(currentMonsters.Count == 0 && spawningMonster == null)
                OnWaveFinish.Invoke();
        }
    

#if UNITY_EDITOR
        [ContextMenu("Launch Wave")]
        private void LaunchWave() => StartWave();
#endif
    }
}
