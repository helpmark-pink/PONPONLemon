using UnityEngine;
using UnityEngine.SceneManagement;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("状態")]
        [SerializeField] private GameState currentState = GameState.Ready;
        
        public GameState CurrentState => currentState;
        
        // イベント
        public System.Action<GameState> OnGameStateChanged;
        public System.Action OnGameStart;
        public System.Action OnGameEnd;
        public System.Action OnGamePause;
        public System.Action OnGameResume;
        
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
            StartCountdown();
        }
        
        public void SetGameState(GameState newState)
        {
            if (currentState == newState) return;
            
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
            
            switch (currentState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    OnGameStart?.Invoke();
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    OnGamePause?.Invoke();
                    break;
                case GameState.Result:
                    Time.timeScale = 1f;
                    OnGameEnd?.Invoke();
                    break;
            }
        }
        
        public void StartCountdown()
        {
            SetGameState(GameState.Countdown);
        }
        
        public void StartGame()
        {
            SetGameState(GameState.Playing);
        }
        
        public void PauseGame()
        {
            if (currentState == GameState.Playing || currentState == GameState.Fever)
            {
                SetGameState(GameState.Paused);
            }
        }
        
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }
        
        public void EndGame()
        {
            SetGameState(GameState.Result);
        }
        
        public void StartFever()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Fever);
            }
        }
        
        public void EndFever()
        {
            if (currentState == GameState.Fever)
            {
                SetGameState(GameState.Playing);
            }
        }
        
        public void RetryGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameConstants.SCENE_GAME);
        }
        
        public void GoToTitle()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameConstants.SCENE_TITLE);
        }
        
        public bool IsPlaying()
        {
            return currentState == GameState.Playing || currentState == GameState.Fever;
        }
    }
}
