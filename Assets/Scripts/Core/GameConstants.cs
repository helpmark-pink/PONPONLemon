using UnityEngine;

namespace PONPONLemon.Core
{
    public static class GameConstants
    {
        public const float GAME_TIME = 60f;
        public const int MIN_CHAIN_COUNT = 3;
        public const float COMBO_TIME_LIMIT = 2f;
        
        public const int FEVER_REQUIRED_COUNT = 30;
        public const float FEVER_DURATION = 10f;
        public const float FEVER_SCORE_MULTIPLIER = 2f;
        
        public const int BASE_SCORE_PER_TSUM = 10;
        public const int CHAIN_BONUS_PER_TSUM = 5;
        
        public static readonly float[] COMBO_MULTIPLIERS = { 1f, 1f, 1f, 1f, 1f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 3f };
        
        public const int TSUM_TYPES = 5;
        public const int GRID_WIDTH = 7;
        public const int GRID_HEIGHT = 9;
        public const float TSUM_SIZE = 120f;
        public const float TSUM_SPACING = 10f;
        
        public const float TSUM_DROP_SPEED = 800f;
        public const float TSUM_POP_DURATION = 0.3f;
        public const float TSUM_SPAWN_DELAY = 0.05f;
        
        public const string SCENE_TITLE = "TitleScene";
        public const string SCENE_HOWTOPLAY = "HowToPlayScene";
        public const string SCENE_GAME = "GameScene";
        
        public const string KEY_HIGH_SCORE = "HighScore";
        public const string KEY_BGM_VOLUME = "BGMVolume";
        public const string KEY_SE_VOLUME = "SEVolume";
        
        public static float GetComboMultiplier(int combo)
        {
            if (combo < 0) return 1f;
            if (combo >= COMBO_MULTIPLIERS.Length) return COMBO_MULTIPLIERS[COMBO_MULTIPLIERS.Length - 1];
            return COMBO_MULTIPLIERS[combo];
        }
    }
}