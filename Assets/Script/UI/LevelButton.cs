using Script.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Script.UI
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Image preview;
        [SerializeField] private TMP_Text title;
        
        private Level level;

        public void SetLevel(Level newLevel)
        {
            level = newLevel;
            preview.sprite = level.preview;
            title.text = level.title;
        }


        public void StartLevel() => SceneManager.LoadSceneAsync(level.sceneName);
    }
}

