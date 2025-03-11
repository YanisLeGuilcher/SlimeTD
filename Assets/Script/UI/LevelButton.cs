using System.Collections.Generic;
using Script.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Script.UI
{
    public class LevelButton : MonoBehaviour
    {
        public static readonly Dictionary<GameObject,LevelButton> LevelButtons = new ();
        
        [SerializeField] private Image preview;
        [SerializeField] private TMP_Text title;
        
        private Level level;

        private void Awake()
        {
            LevelButtons.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            LevelButtons.Remove(gameObject);
        }

        public void SetLevel(Level newLevel)
        {
            level = newLevel;
            preview.sprite = level.preview;
            title.text = level.title;
        }


        public void StartLevel() => SceneManager.LoadSceneAsync(level.sceneName);
    }
}

