
using System.Collections.Generic;
using UnityEngine;

namespace Script.Data
{
    public class DataSerializer : MonoBehaviour
    {
        [SerializeField] private List<Level> levels;

        private static DataSerializer instance;

        private void Awake()
        {
            if (instance && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static List<Level> Levels => instance.levels;
    }
}

