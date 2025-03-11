using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using Script.Entities.Monster;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

namespace Script.Manager
{
        public class MonsterGenerator : MonoBehaviour
    {
        #region SerialiseField

        [SerializeField,Tooltip("First is the prefab of monster\nSecond is the min wave he can spawn\nThird is the amount of it in one wave")] 
        private List<Script.Data.Tuple<GameObject,int,float>> monsters = new();
        [SerializeField] private float timeBetweenEnemy = 1;
        [SerializeField] private List<SplineContainer> trajectories = new();

        #endregion

        #region Private

        private int currentWave = 1;

        private readonly List<Monster> currentMonsters = new();

        private Coroutine spawningMonster;

        #endregion

        public static MonsterGenerator Instance;

        public readonly UnityEvent<Monster> OnMonsterFinish = new();
        public readonly UnityEvent<Monster> OnMonsterDie = new();
        public readonly UnityEvent OnWaveFinish = new();
        
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

        public void StartWave() => spawningMonster = StartCoroutine(RtnStartWave());

        private IEnumerator RtnStartWave()
        {
            currentMonsters.Clear();
            Dictionary<GameObject, int> monstersAmount = new();
            foreach (var monsterStats in monsters)
                if(currentWave >= monsterStats.second)
                    monstersAmount.Add(monsterStats.first, Math.Max(1, (int)(monsterStats.third * currentWave)));

            
            foreach (var monster in monstersAmount)
            {
                int round = 0;
                while (monster.Value > round)
                {
                    float tmpWait = timeBetweenEnemy;
                    while (tmpWait >= 0)
                    {
                        yield return null;
                        tmpWait -= LevelManager.DeltaTime;
                    }
                    var chosenSpline = trajectories[round % trajectories.Count];
                    var go = LeanPool.Spawn(monster.Key, chosenSpline.EvaluatePosition(0), Quaternion.identity);
                    var script = go.GetComponent<Monster>();
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
