using Script.Data;
using Script.UI;
using UnityEngine;

namespace Script.Manager
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject levelsPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject successPanel;
        [SerializeField] private GameObject returnButton;


        [SerializeField] private Transform levelsContainer;

        private void Start()
        {
            var levels = DataSerializer.Levels;
            foreach (var level in levels)
                Instantiate(PrefabFactory.LevelUI, levelsContainer).GetComponent<LevelButton>().SetLevel(level);
        }


        public void PlayButton()
        {
            startPanel.SetActive(false);
            levelsPanel.SetActive(true);
            shopPanel.SetActive(false);
            successPanel.SetActive(false);
            returnButton.SetActive(true);
        }
        
        public void ShopButton()
        {
            startPanel.SetActive(false);
            levelsPanel.SetActive(false);
            shopPanel.SetActive(true);
            successPanel.SetActive(false);
            returnButton.SetActive(true);
        }
        
        public void SuccessButton()
        {
            startPanel.SetActive(false);
            levelsPanel.SetActive(false);
            shopPanel.SetActive(false);
            successPanel.SetActive(true);
            returnButton.SetActive(true);
        }

        public void Return()
        {
            startPanel.SetActive(true);
            levelsPanel.SetActive(false);
            shopPanel.SetActive(false);
            successPanel.SetActive(false);
            returnButton.SetActive(false);
        }

        public void ExitButton() => Application.Quit();
    }
}

