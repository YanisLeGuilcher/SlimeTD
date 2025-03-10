using Script.Data;
using Script.Entities.Monster;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Script.Manager
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Player Detail")]
        [SerializeField] private TMP_Text waveCount;
        [SerializeField] private TMP_Text lifePoint;
        [SerializeField] private TMP_Text money;
        [Header("Scripts")]
        [SerializeField] private MonsterGenerator monsterGenerator;
        
        [Header("Buttons")]
        [SerializeField] private GameObject startWave;
        [SerializeField] private GameObject selectSpeed;
        [SerializeField] private RectTransform placementChoice;

        [Header("Data")] 
        [SerializeField] private LayerMask layerAllow;
        [SerializeField] private LayerMask layerBlock;


        private int _currentLifePoint = 100;
        private int _currentMoney = 100;
        private int _currentWaveCount = 1;

        public int Speed { get; private set; } = 1;

        public static LevelManager Instance;

        private Camera _mainCamera;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Debug.LogWarning($"Multiple Instance of LevelManager: {Instance}");
                Destroy(this);
                return;
            }

            Instance = this;
            lifePoint.text = _currentLifePoint.ToString();
            money.text = _currentMoney.ToString();
            waveCount.text = _currentWaveCount.ToString();
            
            monsterGenerator.OnMonsterDie.AddListener(MonsterDie);
            monsterGenerator.OnMonsterFinish.AddListener(MonsterFinish);
            monsterGenerator.OnWaveFinish.AddListener(WaveFinish);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            if(_mainCamera == null)
                Debug.LogWarning("No camera found");
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
                RemoveChoiceOfPlacement();
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                
                Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, layerBlock);

                if (layerAllow.Contains(hit.collider.gameObject.layer))
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        transform as RectTransform,
                        Input.mousePosition,
                        _mainCamera,
                        out var localPoint
                    );
                    GiveChoiceOfPlacement(localPoint);
                }
                else
                    RemoveChoiceOfPlacement();
            }
        }

        public void SpawnTower(TowerType type, Vector3 position)
        {
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
            monsterGenerator.StartWave();
            startWave.SetActive(false);
            selectSpeed.SetActive(true);
        }

        public void SetSpeed(int amount) => Speed = amount;


        private void MonsterDie(Monster monster)
        {
            _currentMoney += monster.Money;
            money.text = _currentMoney.ToString();
        }
        
        private void MonsterFinish(Monster monster)
        {
            _currentLifePoint -= monster.Damage;
            lifePoint.text = _currentLifePoint.ToString();
        }
        
        private void WaveFinish()
        {
            _currentWaveCount++;
            waveCount.text = _currentWaveCount.ToString();
            
            startWave.SetActive(true);
            selectSpeed.SetActive(false);
        }
    }
}

