using System.Collections;
using System.Numerics;
using Lean.Pool;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace Script.UI
{
    public class DamageUIEffect : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        [SerializeField] private float translationSpeed = 1;
        [SerializeField] private float duration = 1;
        [SerializeField] private float fadeDuration = 1;

        private float _currentDuration;
        private float _currentFadeDuration;

        private Color _baseColor;
    
        public void SetAmount(BigInteger amount) => text.text = amount >= 1000 ? $"{amount/1000}K" : $"{amount}";

        private void Awake()
        {
            _baseColor = text.color;
        }

        private void OnEnable()
        {
            text.color = _baseColor;
            _currentDuration = duration;
            _currentFadeDuration = fadeDuration;
            StartCoroutine(Fade());
        }

        private IEnumerator Fade()
        {
            Vector3 translation = Random.onUnitSphere.normalized * translationSpeed;
            while (_currentDuration > 0)
            {
                transform.Translate(translation * Time.deltaTime);
                yield return null;
                _currentDuration -= Time.deltaTime;
            }
            float maxFadeDuration = _currentFadeDuration;
            while (_currentFadeDuration > 0)
            {
                var tmp = text.color;
                tmp.a = _currentFadeDuration / maxFadeDuration;
                text.color = tmp;
                transform.Translate(translation * Time.deltaTime);
                yield return null;
                _currentFadeDuration -= Time.deltaTime;
            }
            var endColor = text.color;
            endColor.a = 0;
            text.color = endColor;
            yield return null;
            LeanPool.Despawn(gameObject);
        }
    }
}

