using UnityEngine;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class ComboManager : MonoBehaviour
    {
        public static ComboManager Instance { get; private set; }
        
        [Header("コンボ設定")]
        [SerializeField] private float comboTimeLimit = GameConstants.COMBO_TIME_LIMIT;
        
        private int currentCombo = 0;
        private float comboTimer = 0f;
        private bool isComboActive = false;
        
        public int CurrentCombo => currentCombo;
        public float ComboTimer => comboTimer;
        public float ComboTimeLimit => comboTimeLimit;
        public bool IsComboActive => isComboActive;
        
        // イベント
        public System.Action<int> OnComboChanged;
        public System.Action OnComboReset;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            if (!isComboActive) return;
            
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
            {
                return;
            }
            
            comboTimer -= Time.deltaTime;
            
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
        
        public void AddCombo()
        {
            currentCombo++;
            comboTimer = comboTimeLimit;
            isComboActive = true;
            OnComboChanged?.Invoke(currentCombo);
        }
        
        public void ResetCombo()
        {
            if (currentCombo > 0)
            {
                currentCombo = 0;
                isComboActive = false;
                OnComboReset?.Invoke();
                OnComboChanged?.Invoke(currentCombo);
            }
        }
        
        public float GetCurrentMultiplier()
        {
            return GameConstants.GetComboMultiplier(currentCombo);
        }
    }
}
