using UnityEngine;

namespace Script.Data
{
    [CreateAssetMenu(fileName = "Level", menuName = "Data/Level")]
    public class Level : ScriptableObject
    {
        public string sceneName;
        public string title;
        public Sprite preview;
    }
}

