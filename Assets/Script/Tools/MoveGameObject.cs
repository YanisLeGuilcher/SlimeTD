using Script.Manager;
using UnityEngine;

namespace Script.Tools
{
    public class MoveGameObject : MonoBehaviour
    {
        [SerializeField] private Vector3 translation;
        [SerializeField] private Vector3 rotation;
        private void Update()
        {
            transform.Translate(translation * LevelManager.DeltaTime);
            transform.Rotate(rotation * LevelManager.DeltaTime);
        }
    }
}

