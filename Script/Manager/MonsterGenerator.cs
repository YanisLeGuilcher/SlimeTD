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
        [SerializeField] private float moneyEarnByWave = 100;
        [SerializeField] private float moneyEarnByWaveFactor = 1.01f;
        [SerializeField] private List<SplineContainer> trajectories = new();

        #endregion

        #region Private

        private int _currentWave = 1;

        private readonly List<Monster> _currentMonsters = new();

        private Coroutine _spawningMonster;

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

        public void StartWave() => _spawningMonster = StartCoroutine(RtnStartWave());

        private IEnumerator RtnStartWave()
        {
            _currentMonsters.Clear();
            Dictionary<GameObject, int> monstersAmount = new();
            foreach (var monsterStats in monsters)
                if(_currentWave >= monsterStats.second)
                    monstersAmount.Add(monsterStats.first, Math.Max(1, (int)(monsterStats.third * _currentWave)));

            
            foreach (var monster in monstersAmount)
            {
                int round = 0;
                while (monster.Value > round)
                {
                    float tmpWait = timeBetweenEnemy;
                    while (tmpWait >= 0)
                    {
                        yield return null;
                        tmpWait -= Time.deltaTime * LevelManager.Instance.Speed;
                    }
                    var chosenSpline = trajectories[round % trajectories.Count];
                    var go = LeanPool.Spawn(monster.Key, chosenSpline.EvaluatePosition(0), Quaternion.identity);
                    var script = go.GetComponent<Monster>();
                    script.SetSpline(chosenSpline);
                    script.Die.AddListener(() => MonsterDie(script));
                    script.Finish.AddListener(() => MonsterFinish(script));
                    _currentMonsters.Add(script);
                    round++;
                    
                    
                }
            }

            _spawningMonster = null;
            _currentWave++;
        }

        public void AddMonster(Monster monster)
        {
            monster.Die.AddListener(() => MonsterDie(monster));
            monster.Finish.AddListener(() => MonsterFinish(monster));
        }
        
        private void MonsterDie(Monster monster)
        {
            OnMonsterDie.Invoke(monster);
            _currentMonsters.Remove(monster);
            LeanPool.Despawn(monster, monster.DeathAnimationTime);
            if(_currentMonsters.Count == 0 && _spawningMonster == null)
                OnWaveFinish.Invoke();
        }

        private void MonsterFinish(Monster monster)
        {
            _currentMonsters.Remove(monster);
            OnMonsterFinish.Invoke(monster);
            LeanPool.Despawn(monster);
            if(_currentMonsters.Count == 0 && _spawningMonster == null)
                OnWaveFinish.Invoke();
        }
    

#if UNITY_EDITOR
        [ContextMenu("Launch Wave")]
        private void LaunchWave() => StartWave();
#endif
    }
}
