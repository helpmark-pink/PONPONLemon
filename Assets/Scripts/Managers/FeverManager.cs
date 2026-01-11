using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class FeverManager : MonoBehaviour
    {
        [Header("Fever Settings")]
        [SerializeField] private float feverDuration = 10f;
        [SerializeField] private float feverGaugeMax = 100f;
        [SerializeField] private float gaugeIncreasePerTsum = 5f;
        
        [Header("UI")]
        [SerializeField] private Slider feverGaugeSlider;
        [SerializeField] private GameObject feverEffect;
        
        private float currentGauge = 0f;
        private bool isFeverActive = false;
        private bool isEnabled = true;
        private int feverCount = 0;
        
        public bool IsFeverActive => isFeverActive;
        public float FeverGaugePercent => currentGauge / feverGaugeMax;
        public float GaugeRatio => currentGauge / feverGaugeMax;
        public int FeverCount => feverCount;
        
        public System.Action OnFeverStart;
        public System.Action OnFeverEnd;
        public System.Action<int> OnGaugeChanged;
        public System.Action<float> OnFeverTimeChanged;
        
        private void Start()
        {
            UpdateUI();
            if (feverEffect != null)
                feverEffect.SetActive(false);
        }
        
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }
        
        public void AddGauge(int amount)
        {
            AddFeverGauge(amount);
        }
        
        public void AddFeverGauge(int tsumCount)
        {
            if (isFeverActive || !isEnabled) return;
            
            currentGauge += gaugeIncreasePerTsum * tsumCount;
            currentGauge = Mathf.Min(currentGauge, feverGaugeMax);
            UpdateUI();
            OnGaugeChanged?.Invoke((int)currentGauge);
            
            if (currentGauge >= feverGaugeMax)
            {
                StartFever();
            }
        }
        
        public void StartFever()
        {
            if (isFeverActive) return;
            
            isFeverActive = true;
            feverCount++;
            currentGauge = feverGaugeMax;
            UpdateUI();
            
            if (feverEffect != null)
                feverEffect.SetActive(true);
            
            OnFeverStart?.Invoke();
            StartCoroutine(FeverTimer());
        }
        
        private IEnumerator FeverTimer()
        {
            float elapsed = 0;
            while (elapsed < feverDuration)
            {
                elapsed += Time.deltaTime;
                currentGauge = feverGaugeMax * (1 - elapsed / feverDuration);
                UpdateUI();
                OnFeverTimeChanged?.Invoke(1 - elapsed / feverDuration);
                yield return null;
            }
            
            EndFever();
        }
        
        private void EndFever()
        {
            isFeverActive = false;
            currentGauge = 0;
            UpdateUI();
            OnGaugeChanged?.Invoke(0);
            
            if (feverEffect != null)
                feverEffect.SetActive(false);
            
            OnFeverEnd?.Invoke();
        }
        
        private void UpdateUI()
        {
            if (feverGaugeSlider != null)
                feverGaugeSlider.value = currentGauge / feverGaugeMax;
        }
        
        public void ResetFever()
        {
            StopAllCoroutines();
            isFeverActive = false;
            currentGauge = 0;
            feverCount = 0;
            UpdateUI();
            
            if (feverEffect != null)
                feverEffect.SetActive(false);
        }
    }
}
