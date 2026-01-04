using UnityEngine;
using System;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class FeverManager : MonoBehaviour
    {
        public int FeverGauge { get; private set; }
        public float FeverTimeRemaining { get; private set; }
        public bool IsFeverActive { get; private set; }
        public float GaugeRatio => (float)FeverGauge / GameConstants.FEVER_REQUIRED_COUNT;
        public float FeverTimeRatio => FeverTimeRemaining / GameConstants.FEVER_DURATION;
        public int FeverCount { get; private set; }
        
        public event Action<int> OnGaugeChanged;
        public event Action OnFeverStart;
        public event Action OnFeverEnd;
        public event Action<float> OnFeverTimeChanged;
        public event Action OnGaugeFull;
        
        private bool isEnabled = true;
        
        public void ResetFever()
        {
            FeverGauge = 0;
            FeverTimeRemaining = 0;
            IsFeverActive = false;
            FeverCount = 0;
            OnGaugeChanged?.Invoke(FeverGauge);
        }
        
        public void AddGauge(int amount)
        {
            if (!isEnabled) return;
            
            FeverGauge = Mathf.Min(FeverGauge + amount, GameConstants.FEVER_REQUIRED_COUNT);
            OnGaugeChanged?.Invoke(FeverGauge);
            
            if (FeverGauge >= GameConstants.FEVER_REQUIRED_COUNT && !IsFeverActive)
            {
                OnGaugeFull?.Invoke();
                StartFever();
            }
        }
        
        public void StartFever()
        {
            if (IsFeverActive) return;
            
            IsFeverActive = true;
            FeverTimeRemaining = GameConstants.FEVER_DURATION;
            FeverGauge = 0;
            FeverCount++;
            
            OnFeverStart?.Invoke();
            OnGaugeChanged?.Invoke(FeverGauge);
        }
        
        private void EndFever()
        {
            IsFeverActive = false;
            FeverTimeRemaining = 0;
            OnFeverEnd?.Invoke();
        }
        
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }
        
        private void Update()
        {
            if (!isEnabled || !IsFeverActive) return;
            
            FeverTimeRemaining -= Time.deltaTime;
            OnFeverTimeChanged?.Invoke(FeverTimeRatio);
            
            if (FeverTimeRemaining <= 0)
                EndFever();
        }
    }
}
