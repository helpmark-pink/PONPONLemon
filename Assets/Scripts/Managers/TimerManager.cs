using UnityEngine;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class TimerManager : MonoBehaviour
    {
        public static TimerManager Instance { get; private set; }
        
        [Header("タイマー設定")]
        [SerializeField] private float gameTime = GameConstants.GAME_TIME;
        
        private float remainingTime;
        private bool isRunning = false;
        
        public float RemainingTime => remainingTime;
        public float GameTime => gameTime;
        public float TimeRatio => remainingTime / gameTime;
        
        // イベント
        public System.Action<float> OnTimeChanged;
        public System.Action OnTimeUp;
        
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
        
        private void Start()
        {
            ResetTimer();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        
        private void Update()
        {
            if (!isRunning) return;
            
            remainingTime -= Time.deltaTime;
            OnTimeChanged?.Invoke(remainingTime);
            
            if (remainingTime <= 0)
            {
                remainingTime = 0;
                isRunning = false;
                OnTimeUp?.Invoke();
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EndGame();
                }
            }
        }
        
        public void ResetTimer()
        {
            remainingTime = gameTime;
            isRunning = false;
            OnTimeChanged?.Invoke(remainingTime);
        }
        
        public void StartTimer()
        {
            isRunning = true;
        }
        
        public void StopTimer()
        {
            isRunning = false;
        }
        
        public void AddTime(float seconds)
        {
            remainingTime += seconds;
            if (remainingTime > gameTime) remainingTime = gameTime;
            OnTimeChanged?.Invoke(remainingTime);
        }
        
        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                case GameState.Fever:
                    StartTimer();
                    break;
                case GameState.Paused:
                case GameState.Result:
                    StopTimer();
                    break;
            }
        }
    }
}
