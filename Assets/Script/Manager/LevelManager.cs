using System.Collections;
using Script.Data;
using Script.Entities.Defender;
using Script.Entities.Monster;
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

        [Header("Data")] 
        [SerializeField] private LayerMask layerAllow;
        [SerializeField] private LayerMask layerBlock;
        [SerializeField] private float moneyEarnByWave = 100;
        [SerializeField] private float moneyEarnByWaveFactor = .01f;

        #endregion

        #region Private

        private int 
            currentLifePoint = 100,
            currentMoney = 400;
        
        private Camera mainCamera;

        private bool 
            waveProcessing,
            paused;

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

            Instance = this;
            lifePoint.text = currentLifePoint.ToString();
            money.text = currentMoney.ToString();
            UpdateWaveCount();
            
            monsterGenerator.OnMonsterDie.AddListener(MonsterDie);
            monsterGenerator.OnMonsterFinish.AddListener(MonsterFinish);
            monsterGenerator.OnWaveFinish.AddListener(WaveFinish);
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
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                if (ChoicePlacementDisplayed)
                {
                    ShowChoiceOfPlacement(false);
                    return;
                }
                
                OnClick.Invoke(this);
                
                Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, layerBlock);
                if (layerAllow.Contains(hit.collider.gameObject.layer))
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
            if(placementChoice.gameObject.activeSelf != enable)
                placementChoice.gameObject.SetActive(enable);
        }
        
        public void SpawnTower(TowerType type, Vector3 position)
        {
            int price = DataSerializer.GetPriceOfTower(type);
            if(currentMoney < price)
                return;
            
            currentMoney -= price;
            money.text = currentMoney.ToString();
            
            Instantiate(PrefabFactory.Instance[type], position, Quaternion.identity);
            placementChoice.gameObject.SetActive(false);
        }

        public void UpgradeTower(Defender oldOne, TowerType newOne)
        {
            int price = DataSerializer.GetPriceOfTower(newOne);
            if(currentMoney < price)
                return;
            
            currentMoney -= price;
            money.text = currentMoney.ToString();
            
            Instantiate(PrefabFactory.Instance[newOne], oldOne.transform.position, Quaternion.identity);
            
            Destroy(oldOne.gameObject);
        }
        
        public void SellTower(Defender defender)
        {
            currentMoney += defender.PriceOnSell;
            money.text = Instance.currentMoney.ToString();
            
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

        public void Retry() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);


        public void GoToMainMenu() => SceneManager.LoadSceneAsync(0);

        #endregion
        
        public void UpdateWaveCount() => waveCount.text = monsterGenerator.CurrentWave.ToString();
        
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

        
    }
}

