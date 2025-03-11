using Script.Data;
using Script.Entities.Defender;
using Script.Entities.Monster;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Script.Manager
{
    public class LevelManager : MonoBehaviour
    {
        public static readonly UnityEvent OnClick = new();
        
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


        [SerializeField] private PlayerInput input;

        private int currentLifePoint = 100;
        private int currentMoney = 400;
        private int currentWaveCount = 1;

        
        public bool Paused { get; private set; }

        public int Speed => Paused ? 0 : cacheSpeed;
        public static float DeltaTime => Instance.Speed * Time.deltaTime;
        private int cacheSpeed = 1;

        public static LevelManager Instance;

        private Camera mainCamera;

        private bool waveProcessing;

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
            waveCount.text = currentWaveCount.ToString();
            
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
                RemoveChoiceOfPlacement();
                OnClick.Invoke();
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                
                OnClick.Invoke();
                
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
                    RemoveChoiceOfPlacement();
            }
        }

        public void UpgradeTower(Defender oldOne, TowerType newOne)
        {
            Debug.Log(newOne);
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

        private void GiveChoiceOfPlacement(Vector2 position)
        {
            placementChoice.gameObject.SetActive(true);
            placementChoice.anchoredPosition = position;
        }
        
        private void RemoveChoiceOfPlacement()
        {
            if(placementChoice.gameObject.activeSelf)
                placementChoice.gameObject.SetActive(false);
        }

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
            currentWaveCount++;
            waveCount.text = currentWaveCount.ToString();
            
            startWave.SetActive(true);
            selectSpeed.SetActive(false);
            currentMoney += (int)(moneyEarnByWave * (moneyEarnByWaveFactor * currentWaveCount + 1));
            money.text = currentMoney.ToString();
        }

        public void ResumeGame() => ResumeGame(Speed == 0);
        
        public void ResumeGame(bool enable)
        {
            if (enable)
            {
                Paused = false;
                pausePanel.SetActive(false);
            }
            else
            {
                cacheSpeed = Speed;
                Paused = true;
                pausePanel.SetActive(true);
            }
        }
        
        public void Retry() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);


        public void GoToMainMenu() => SceneManager.LoadSceneAsync(0);
    }
}

