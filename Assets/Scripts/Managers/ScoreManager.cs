using UnityEngine;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }
        
        [Header("スコア")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int highScore = 0;
        
        public int CurrentScore => currentScore;
        public int HighScore => highScore;
        public bool IsNewRecord { get; private set; }
        
        // イベント
        public System.Action<int> OnScoreChanged;
        public System.Action<int, Vector3> OnScorePopup;
        
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
            LoadHighScore();
            ResetScore();
        }
        
        public void ResetScore()
        {
            currentScore = 0;
            IsNewRecord = false;
            OnScoreChanged?.Invoke(currentScore);
        }
        
        public void AddScore(int chainCount, int combo, Vector3 position, bool isFever = false)
        {
            if (chainCount < GameConstants.MIN_CHAIN_COUNT) return;
            
            // 基本スコア計算
            int baseScore = chainCount * GameConstants.BASE_SCORE_PER_TSUM;
            int chainBonus = (chainCount - GameConstants.MIN_CHAIN_COUNT) * GameConstants.CHAIN_BONUS_PER_TSUM;
            
            // コンボ倍率
            float comboMultiplier = GameConstants.GetComboMultiplier(combo);
            
            // フィーバー倍率
            float feverMultiplier = isFever ? GameConstants.FEVER_SCORE_MULTIPLIER : 1f;
            
            // 最終スコア
            int addedScore = Mathf.RoundToInt((baseScore + chainBonus) * comboMultiplier * feverMultiplier);
            
            currentScore += addedScore;
            OnScoreChanged?.Invoke(currentScore);
            OnScorePopup?.Invoke(addedScore, position);
            
            // ハイスコア更新チェック
            if (currentScore > highScore)
            {
                highScore = currentScore;
                IsNewRecord = true;
                SaveHighScore();
            }
        }
        
        public void AddScoreSimple(int score)
        {
            currentScore += score;
            OnScoreChanged?.Invoke(currentScore);
            
            if (currentScore > highScore)
            {
                highScore = currentScore;
                IsNewRecord = true;
                SaveHighScore();
            }
        }
        
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt(GameConstants.KEY_HIGH_SCORE, 0);
        }
        
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(GameConstants.KEY_HIGH_SCORE, highScore);
            PlayerPrefs.Save();
        }
    }
}
