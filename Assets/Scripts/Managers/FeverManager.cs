using UnityEngine;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class FeverManager : MonoBehaviour
    {
        public static FeverManager Instance { get; private set; }
        
        [Header("フィーバー設定")]
        [SerializeField] private int requiredCount = GameConstants.FEVER_REQUIRED_COUNT;
        [SerializeField] private float feverDuration = GameConstants.FEVER_DURATION;
        
        private int currentCount = 0;
        private float feverTimer = 0f;
        private bool isFeverActive = false;
        
        public int CurrentCount => currentCount;
        public int RequiredCount => requiredCount;
        public float FeverTimer => feverTimer;
        public float FeverDuration => feverDuration;
        public bool IsFeverActive => isFeverActive;
        public float FeverRatio => (float)currentCount / requiredCount;
        public float FeverTimeRatio => feverTimer / feverDuration;
        
        // イベント
        public System.Action<int> OnFeverCountChanged;
        public System.Action OnFeverStart;
        public System.Action OnFeverEnd;
        public System.Action<float> OnFeverTimeChanged;
        
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
            if (!isFeverActive) return;
            
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
            {
                return;
            }
            
            feverTimer -= Time.deltaTime;
            OnFeverTimeChanged?.Invoke(feverTimer);
            
            if (feverTimer <= 0)
            {
                EndFever();
            }
        }
        
        public void AddCount(int count)
        {
            if (isFeverActive) return;
            
            currentCount += count;
            OnFeverCountChanged?.Invoke(currentCount);
            
            if (currentCount >= requiredCount)
            {
                StartFever();
            }
        }
        
        public void StartFever()
        {
            isFeverActive = true;
            feverTimer = feverDuration;
            currentCount = 0;
            
            OnFeverStart?.Invoke();
            OnFeverCountChanged?.Invoke(currentCount);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartFever();
            }
        }
        
        public void EndFever()
        {
            isFeverActive = false;
            feverTimer = 0f;
            
            OnFeverEnd?.Invoke();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndFever();
            }
        }
        
        public void ResetFever()
        {
            isFeverActive = false;
            feverTimer = 0f;
            currentCount = 0;
            OnFeverCountChanged?.Invoke(currentCount);
        }
    }
}
