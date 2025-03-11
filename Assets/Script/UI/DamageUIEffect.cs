using System.Collections;
using System.Collections.Generic;
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
        public static readonly Dictionary<GameObject,DamageUIEffect> DamageUIEffects = new();
        
        [SerializeField] private TMP_Text text;

        [SerializeField] private float translationSpeed = 1;
        [SerializeField] private float duration = 1;
        [SerializeField] private float fadeDuration = 1;

        private float currentDuration;
        private float currentFadeDuration;

        private Color baseColor;
    
        public void SetAmount(BigInteger amount) => text.text = amount >= 1000 ? $"{amount/1000}K" : $"{amount}";

        private void Awake()
        {
            baseColor = text.color;
            DamageUIEffects.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            DamageUIEffects.Remove(gameObject);
        }

        private void OnEnable()
        {
            text.color = baseColor;
            currentDuration = duration;
            currentFadeDuration = fadeDuration;
            StartCoroutine(Fade());
        }

        private IEnumerator Fade()
        {
            Vector3 translation = Random.onUnitSphere.normalized * translationSpeed;
            while (currentDuration > 0)
            {
                transform.Translate(translation * Time.deltaTime);
                yield return null;
                currentDuration -= Time.deltaTime;
            }
            float maxFadeDuration = currentFadeDuration;
            while (currentFadeDuration > 0)
            {
                var tmp = text.color;
                tmp.a = currentFadeDuration / maxFadeDuration;
                text.color = tmp;
                transform.Translate(translation * Time.deltaTime);
                yield return null;
                currentFadeDuration -= Time.deltaTime;
            }
            var endColor = text.color;
            endColor.a = 0;
            text.color = endColor;
            yield return null;
            LeanPool.Despawn(gameObject);
        }
    }
}

