using UnityEngine;
using System;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }
        public bool IsNewRecord { get; private set; }
        
        public event Action<int> OnScoreChanged;
        public event Action<int> OnScoreAdded;
        public event Action OnNewHighScore;
        
        private void Start()
        {
            LoadHighScore();
        }
        
        public void ResetScore()
        {
            CurrentScore = 0;
            IsNewRecord = false;
            OnScoreChanged?.Invoke(CurrentScore);
        }
        
        public void AddScore(int tsumCount, int chainCount, int combo, bool isFever)
        {
            int baseScore = tsumCount * GameConstants.BASE_SCORE_PER_TSUM;
            
            int chainBonus = 0;
            if (chainCount > GameConstants.MIN_CHAIN_COUNT)
            {
                chainBonus = (chainCount - GameConstants.MIN_CHAIN_COUNT) * GameConstants.CHAIN_BONUS_PER_TSUM * tsumCount;
            }
            
            float comboMultiplier = GameConstants.GetComboMultiplier(combo);
            float feverMultiplier = isFever ? GameConstants.FEVER_SCORE_MULTIPLIER : 1f;
            
            int totalScore = Mathf.RoundToInt((baseScore + chainBonus) * comboMultiplier * feverMultiplier);
            
            CurrentScore += totalScore;
            
            OnScoreAdded?.Invoke(totalScore);
            OnScoreChanged?.Invoke(CurrentScore);
            
            CheckHighScore();
        }
        
        private void CheckHighScore()
        {
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                
                if (!IsNewRecord)
                {
                    IsNewRecord = true;
                    OnNewHighScore?.Invoke();
                }
            }
        }
        
        public void SaveHighScore()
        {
            PlayerPrefs.SetInt(GameConstants.KEY_HIGH_SCORE, HighScore);
            PlayerPrefs.Save();
        }
        
        private void LoadHighScore()
        {
            HighScore = PlayerPrefs.GetInt(GameConstants.KEY_HIGH_SCORE, 0);
        }
        
        public int CalculateScorePreview(int tsumCount, int chainCount, int combo, bool isFever)
        {
            int baseScore = tsumCount * GameConstants.BASE_SCORE_PER_TSUM;
            int chainBonus = 0;
            if (chainCount > GameConstants.MIN_CHAIN_COUNT)
            {
                chainBonus = (chainCount - GameConstants.MIN_CHAIN_COUNT) * GameConstants.CHAIN_BONUS_PER_TSUM * tsumCount;
            }
            float comboMultiplier = GameConstants.GetComboMultiplier(combo);
            float feverMultiplier = isFever ? GameConstants.FEVER_SCORE_MULTIPLIER : 1f;
            
            return Mathf.RoundToInt((baseScore + chainBonus) * comboMultiplier * feverMultiplier);
        }
    }
}