using UnityEngine;
using System;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class ComboManager : MonoBehaviour
    {
        public int CurrentCombo { get; private set; }
        public int MaxCombo { get; private set; }
        public float ComboTimeRemaining { get; private set; }
        public float ComboMultiplier => GameConstants.GetComboMultiplier(CurrentCombo);
        public bool IsComboActive => CurrentCombo > 0 && ComboTimeRemaining > 0;
        
        public event Action<int> OnComboChanged;
        public event Action<int> OnComboIncreased;
        public event Action OnComboReset;
        public event Action<float> OnComboTimerChanged;
        
        private bool isEnabled = true;
        
        public void ResetCombo()
        {
            CurrentCombo = 0;
            MaxCombo = 0;
            ComboTimeRemaining = 0;
            OnComboChanged?.Invoke(CurrentCombo);
            OnComboReset?.Invoke();
        }
        
        public void AddCombo()
        {
            if (!isEnabled) return;
            
            CurrentCombo++;
            ComboTimeRemaining = GameConstants.COMBO_TIME_LIMIT;
            
            if (CurrentCombo > MaxCombo)
                MaxCombo = CurrentCombo;
            
            OnComboIncreased?.Invoke(CurrentCombo);
            OnComboChanged?.Invoke(CurrentCombo);
        }
        
        public void BreakCombo()
        {
            if (CurrentCombo > 0)
            {
                CurrentCombo = 0;
                ComboTimeRemaining = 0;
                OnComboReset?.Invoke();
                OnComboChanged?.Invoke(CurrentCombo);
            }
        }
        
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            if (!enabled)
                ComboTimeRemaining = GameConstants.COMBO_TIME_LIMIT;
        }
        
        private void Update()
        {
            if (!isEnabled || CurrentCombo <= 0) return;
            
            if (ComboTimeRemaining > 0)
            {
                ComboTimeRemaining -= Time.deltaTime;
                OnComboTimerChanged?.Invoke(ComboTimeRemaining / GameConstants.COMBO_TIME_LIMIT);
                
                if (ComboTimeRemaining <= 0)
                    BreakCombo();
            }
        }
        
        public string GetMultiplierText()
        {
            float mult = ComboMultiplier;
            return $"x{mult:F1}";
        }
    }
}

