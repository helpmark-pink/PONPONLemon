using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using PONPONLemon.Core;
using PONPONLemon.Tsum;
using PONPONLemon.UI;

namespace PONPONLemon.Managers
{
    public enum GameState
    {
        Ready,
        Playing,
        Paused,
        Result
    }
    
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Managers")]
        [SerializeField] private TsumManager tsumManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private TimerManager timerManager;
        [SerializeField] private ComboManager comboManager;
        [SerializeField] private FeverManager feverManager;
        
        [Header("UI")]
        [SerializeField] private GameUIManager uiManager;
        
        public GameState CurrentState { get; private set; }
        
        public int TotalTsumsCleared { get; private set; }
        public int MaxChain { get; private set; }
        
        public event Action<GameState> OnGameStateChanged;
        public event Action OnGameStart;
        public event Action OnGameEnd;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            if (tsumManager == null) tsumManager = FindFirstObjectByType<TsumManager>();
            if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();
            if (timerManager == null) timerManager = FindFirstObjectByType<TimerManager>();
            if (comboManager == null) comboManager = FindFirstObjectByType<ComboManager>();
            if (feverManager == null) feverManager = FindFirstObjectByType<FeverManager>();
            if (uiManager == null) uiManager = FindFirstObjectByType<GameUIManager>();
            
            if (tsumManager != null)
            {
                tsumManager.OnChainCompleted += OnChainCompleted;
                tsumManager.OnChainCancelled += OnChainCancelled;
            }
            
            if (timerManager != null)
            {
                timerManager.OnTimeUp += OnTimeUp;
            }
            
            ResetGame();
            StartCoroutine(StartGameAfterDelay(1f));
        }
        
        public void ResetGame()
        {
            CurrentState = GameState.Ready;
            TotalTsumsCleared = 0;
            MaxChain = 0;
            
            scoreManager?.ResetScore();
            timerManager?.Initialize();
            comboManager?.ResetCombo();
            feverManager?.ResetFever();
            tsumManager?.InitializeGrid();
            tsumManager?.SetInputEnabled(false);
            
            OnGameStateChanged?.Invoke(CurrentState);
        }
        
        private IEnumerator StartGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartGame();
        }
        
        public void StartGame()
        {
            if (CurrentState != GameState.Ready) return;
            
            CurrentState = GameState.Playing;
            
            timerManager?.StartTimer();
            tsumManager?.SetInputEnabled(true);
            comboManager?.SetEnabled(true);
            feverManager?.SetEnabled(true);
            
            OnGameStart?.Invoke();
            OnGameStateChanged?.Invoke(CurrentState);
        }
        
        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;
            
            CurrentState = GameState.Paused;
            
            timerManager?.PauseTimer();
            tsumManager?.SetInputEnabled(false);
            comboManager?.SetEnabled(false);
            feverManager?.SetEnabled(false);
            
            uiManager?.ShowPausePanel();
            OnGameStateChanged?.Invoke(CurrentState);
        }
        
        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;
            
            CurrentState = GameState.Playing;
            
            timerManager?.ResumeTimer();
            tsumManager?.SetInputEnabled(true);
            comboManager?.SetEnabled(true);
            feverManager?.SetEnabled(true);
            
            uiManager?.HidePausePanel();
            OnGameStateChanged?.Invoke(CurrentState);
        }
        
        private void EndGame()
        {
            if (CurrentState == GameState.Result) return;
            
            CurrentState = GameState.Result;
            
            tsumManager?.SetInputEnabled(false);
            comboManager?.SetEnabled(false);
            feverManager?.SetEnabled(false);
            
            scoreManager?.SaveHighScore();
            
            OnGameEnd?.Invoke();
            OnGameStateChanged?.Invoke(CurrentState);
            
            StartCoroutine(ShowResultAfterDelay(1f));
        }
        
        private IEnumerator ShowResultAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            uiManager?.ShowResultPanel(
                scoreManager?.CurrentScore ?? 0,
                scoreManager?.HighScore ?? 0,
                scoreManager?.IsNewRecord ?? false,
                comboManager?.MaxCombo ?? 0,
                TotalTsumsCleared,
                feverManager?.FeverCount ?? 0
            );
        }
        
        private void OnChainCompleted(int tsumCount, int chainCount)
        {
            TotalTsumsCleared += tsumCount;
            if (chainCount > MaxChain) MaxChain = chainCount;
            
            comboManager?.AddCombo();
            feverManager?.AddGauge(tsumCount);
            
            bool isFever = feverManager?.IsFeverActive ?? false;
            int combo = comboManager?.CurrentCombo ?? 0;
            scoreManager?.AddScore(tsumCount, chainCount, combo, isFever);
            
            int addedScore = scoreManager?.CalculateScorePreview(tsumCount, chainCount, combo, isFever) ?? 0;
            uiManager?.ShowScorePopup(addedScore, chainCount);
        }
        
        private void OnChainCancelled()
        {
        }
        
        private void OnTimeUp()
        {
            EndGame();
        }
        
        public void Retry()
        {
            SceneManager.LoadScene(GameConstants.SCENE_GAME);
        }
        
        public void GoToTitle()
        {
            SceneManager.LoadScene(GameConstants.SCENE_TITLE);
        }
        
        private void OnDestroy()
        {
            if (tsumManager != null)
            {
                tsumManager.OnChainCompleted -= OnChainCompleted;
                tsumManager.OnChainCancelled -= OnChainCancelled;
            }
            
            if (timerManager != null)
            {
                timerManager.OnTimeUp -= OnTimeUp;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
