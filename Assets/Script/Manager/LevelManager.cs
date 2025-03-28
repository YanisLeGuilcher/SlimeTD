using System.Collections;
using System.Collections.Generic;
using Script.Data;
using Script.Data.Enum;
using Script.Entities.Monster;
using Script.Entities.Tower;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Script.Manager
{
    public class LevelManager : MonoBehaviour
    {
        #region Static
        
        public static readonly UnityEvent<MonoBehaviour> OnClick = new();
        
        public static LevelManager Instance;
        
        public static int Speed => Instance.paused ? 0 : Instance.cacheSpeed;
        public static float FixedDeltaTime => Speed * Time.fixedDeltaTime;
        public static float DeltaTime => Speed * Time.deltaTime;
        private int cacheSpeed = 1;
        public static bool PlayerLoose => Instance.currentLifePoint <= 0;
        
        #endregion

        #region SerializeField

        [Header("UI")]
        [SerializeField] private TMP_Text waveCount;
        [SerializeField] private TMP_Text lifePoint;
        [SerializeField] private TMP_Text money;
        [SerializeField] private GameObject pausePanel;
        
        [Header("Scripts")]
        [SerializeField] private MonsterGenerator monsterGenerator;
        
        [Header("Buttons")]
        [SerializeField] private GameObject startWave;
        [SerializeField] private GameObject selectSpeed;
        [SerializeField] private RectTransform placementChoice;
        [SerializeField] private List<Tuple<TowerType, TMP_Text>> towersCanBePlace = new();

        [Header("Data")] 
        [SerializeField] private LayerMask layerBlock;
        [SerializeField] private int startMoney = 400;
        [SerializeField] private float moneyEarnByWave = 100;
        [SerializeField] private float moneyEarnByWaveFactor = .01f;
        [SerializeField] private Level level;

        #endregion

        #region Private

        private int 
            currentLifePoint = 100,
            currentMoney;
        
        private Camera mainCamera;

        private bool
            waveProcessing,
            paused,
            dontShowPlacementChoice;

        private readonly List<Tower> towers = new();

        #endregion
        
        #region UnityEventFunction

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Debug.LogWarning($"Multiple Instance of LevelManager: {Instance}");
                Destroy(this);
                return;
            }

            currentMoney = startMoney;

            Instance = this;
            UpdateWaveCount();
            UpdateLifePoint();
            UpdateMoney();
            
            monsterGenerator.OnMonsterDie.AddListener(MonsterDie);
            monsterGenerator.OnMonsterFinish.AddListener(MonsterFinish);
            monsterGenerator.OnWaveFinish.AddListener(WaveFinish);
            OnClick.AddListener(CatchOverClick);
        }

        private void OnDestroy()
        {
            monsterGenerator.OnMonsterDie.RemoveListener(MonsterDie);
            monsterGenerator.OnMonsterFinish.RemoveListener(MonsterFinish);
            monsterGenerator.OnWaveFinish.RemoveListener(WaveFinish);
        }

        private void Start()
        {
            mainCamera = Camera.main;
            if(mainCamera == null)
                Debug.LogWarning("No camera found");

            if (!SaveManager.LevelUseSave(level))
                Save();
            else
            {
                LevelData data = SaveManager.GetSave(level);

                currentLifePoint = data.lifePoint;
                currentMoney = data.money;
                monsterGenerator.CurrentWave = data.waveCount;
            
                UpdateWaveCount();
                UpdateLifePoint();
                UpdateMoney();
            
                foreach (var tower in data.towers)
                    SpawnTower(tower.type, tower.position, tower.attackStyle, true);
            }

            foreach (var tower in towersCanBePlace)
                tower.second.text = DataSerializer.GetPriceOfTower(tower.first).ToString();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                ShowChoiceOfPlacement(false);
                OnClick.Invoke(this);
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverUIExcludingSortingLayer(LayerMask.NameToLayer("UiIgnore")))
                    return;

                if (ChoicePlacementDisplayed)
                {
                    ShowChoiceOfPlacement(false);
                    return;
                }
                
                OnClick.Invoke(this);
                
                Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.OverlapCircle(mousePosition, 0.85f, layerBlock);
                if (!hit)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        transform as RectTransform,
                        Input.mousePosition,
                        mainCamera,
                        out var localPoint
                    );
                    GiveChoiceOfPlacement(localPoint);
                }
                else
                    ShowChoiceOfPlacement(false);
            }

            dontShowPlacementChoice = false;
        }

        #endregion

        #region Tower

        private bool ChoicePlacementDisplayed => placementChoice.gameObject.activeSelf;
        
        private void GiveChoiceOfPlacement(Vector2 position)
        {
            ShowChoiceOfPlacement();
            placementChoice.anchoredPosition = position;
        }

        private void ShowChoiceOfPlacement(bool enable = true)
        {
            if(enable && dontShowPlacementChoice)
                return;
            if(placementChoice.gameObject.activeSelf != enable)
                placementChoice.gameObject.SetActive(enable);
        }
        
        public void SpawnTower(TowerType type, Vector3 position, AttackStyle attackStyle = AttackStyle.First, bool skipPrice = false)
        {
            if (!skipPrice)
            {
                int price = DataSerializer.GetPriceOfTower(type);
                if(currentMoney < price)
                    return;
            
                currentMoney -= price;
                money.text = currentMoney.ToString();
            }
            
            
            var go = Instantiate(PrefabFactory.Instance[type], position, Quaternion.identity);
            placementChoice.gameObject.SetActive(false);

            if (!Tower.Towers.TryGetValue(go, out var script))
            {
                Debug.LogWarning($"Can't find script for {go.name}");
                return;
            }
            
            towers.Add(script);

            if (script is Defender defender)
                defender.SetAttackStyle(attackStyle);
        }

        public void UpgradeTower(Tower oldOne, TowerType newOne)
        {
            int price = DataSerializer.GetPriceOfTower(newOne);
            if(currentMoney < price)
                return;
            
            currentMoney -= price;
            money.text = currentMoney.ToString();
            
            var newTower = Instantiate(PrefabFactory.Instance[newOne], oldOne.transform.position, Quaternion.identity);

            towers.Remove(oldOne);
            
            if(Tower.Towers.TryGetValue(newTower, out var script))
                towers.Add(script);
            else
                Debug.LogWarning($"Can not found script of {newTower.name}");
            
            Destroy(oldOne.gameObject);
        }
        
        public void SellTower(Tower defender)
        {
            currentMoney += defender.PriceOnSell;
            money.text = Instance.currentMoney.ToString();

            towers.Remove(defender);
            
            Destroy(defender.gameObject);
        }
        

        #endregion
        
        #region Button
        
        public void StartWave()
        {
            if(waveProcessing)
                return;
            monsterGenerator.StartWave();
            startWave.SetActive(false);
            selectSpeed.SetActive(true);
            waveProcessing = true;
            
            Save();
        }

        public void SetSpeed(int amount) => cacheSpeed = amount;
        
        public void ResumeGame() => ResumeGame(Speed == 0);
        
        public void ResumeGame(bool enable)
        {
            if (enable)
            {
                paused = false;
                pausePanel.SetActive(false);
            }
            else
            {
                cacheSpeed = Speed;
                paused = true;
                pausePanel.SetActive(true);
            }
        }

        public void Retry()
        {
            SaveManager.DeleteSave(level);
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }


        public void GoToMainMenu()
        {
            if (!waveProcessing)
                Save();
            SceneManager.LoadSceneAsync(0);
        }

        #endregion

        private void CatchOverClick(MonoBehaviour clicker)
        {
            if(clicker == this)
                return;
            ShowChoiceOfPlacement(false);
        }
        
        public void UpdateWaveCount() => waveCount.text = monsterGenerator.CurrentWave.ToString();
        public void UpdateMoney() => money.text = currentMoney.ToString();
        public void UpdateLifePoint() => lifePoint.text = currentLifePoint.ToString();
        
        private void MonsterDie(Monster monster)
        {
            currentMoney += monster.Money;
            money.text = currentMoney.ToString();
        }
        
        private void MonsterFinish(Monster monster)
        {
            currentLifePoint -= monster.Damage;
            if (currentLifePoint <= 0)
            {
                currentLifePoint = 0;
                pausePanel.SetActive(true);
                startWave.SetActive(false);
                selectSpeed.SetActive(false);
            }
            lifePoint.text = currentLifePoint.ToString();
        }


        public void DontShowPlacementChoice()
        {
            dontShowPlacementChoice = true;
            if(placementChoice.gameObject.activeSelf)
                placementChoice.gameObject.SetActive(false);
        }
        
        private void WaveFinish()
        {
            if(!waveProcessing)
                return;
            waveProcessing = false;
            UpdateWaveCount();
            
            startWave.SetActive(true);
            selectSpeed.SetActive(false);
            currentMoney += (int)(moneyEarnByWave * (moneyEarnByWaveFactor * monsterGenerator.CurrentWave + 1));
            money.text = currentMoney.ToString();
            
            Save();
        }

        public static IEnumerator WaitForSecond(float seconds)
        {
            float currentWait = 0;
            while (currentWait < seconds)
            {
                yield return null;
                currentWait += DeltaTime;
            }
        }

        private void Save()
        {
            SaveManager.SaveLevel(level,
                new LevelData
                {
                    lifePoint = currentLifePoint,
                    money = currentMoney,
                    waveCount = monsterGenerator.CurrentWave,
                    towers = towers.ToData()
                });
        }
        
    }
}

