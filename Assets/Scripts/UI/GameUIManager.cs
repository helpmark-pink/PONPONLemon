using UnityEngine;
using UnityEngine.UI;
using PONPONLemon.Core;
using PONPONLemon.Managers;

namespace PONPONLemon.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("スコア表示")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Image scoreFrame;
        
        [Header("タイマー表示")]
        [SerializeField] private Text timerText;
        [SerializeField] private Image timerGaugeFill;
        [SerializeField] private Image timerFrame;
        
        [Header("コンボ表示")]
        [SerializeField] private GameObject comboPanel;
        [SerializeField] private Text comboText;
        [SerializeField] private Text comboLabel;
        
        [Header("フィーバー表示")]
        [SerializeField] private GameObject feverGaugePanel;
        [SerializeField] private Image feverGaugeFill;
        [SerializeField] private GameObject feverActivePanel;
        [SerializeField] private Text feverTimerText;
        
        [Header("カウントダウン")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private Text countdownText;
        
        [Header("ポーズ")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRetryButton;
        [SerializeField] private Button pauseTitleButton;
        
        [Header("リザルト")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultScoreText;
        [SerializeField] private Text resultHighScoreText;
        [SerializeField] private GameObject newRecordIcon;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button titleButton;
        
        [Header("スコアポップアップ")]
        [SerializeField] private GameObject scorePopupPrefab;
        [SerializeField] private Transform scorePopupParent;
        
        [Header("アニメーション設定")]
        [SerializeField] private float countdownInterval = 1f;
        
        private int countdownValue = 3;
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            SetupButtons();
        }
        
        private void OnDestroy()
        {
            RemoveEventListeners();
        }
        
        private void InitializeUI()
        {
            if (comboPanel != null) comboPanel.SetActive(false);
            if (feverActivePanel != null) feverActivePanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
            if (countdownPanel != null) countdownPanel.SetActive(false);
            if (newRecordIcon != null) newRecordIcon.SetActive(false);
            
            UpdateScoreDisplay(0);
            UpdateTimerDisplay(GameConstants.GAME_TIME);
            UpdateFeverGauge(0);
        }
        
        private void SetupEventListeners()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
            
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
                ScoreManager.Instance.OnScorePopup += ShowScorePopup;
            }
            
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimeChanged += UpdateTimerDisplay;
            }
            
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.OnComboChanged += UpdateComboDisplay;
                ComboManager.Instance.OnComboReset += HideCombo;
            }
            
            if (FeverManager.Instance != null)
            {
                FeverManager.Instance.OnFeverCountChanged += OnFeverCountChanged;
                FeverManager.Instance.OnFeverStart += OnFeverStart;
                FeverManager.Instance.OnFeverEnd += OnFeverEnd;
                FeverManager.Instance.OnFeverTimeChanged += UpdateFeverTimer;
            }
        }
        
        private void RemoveEventListeners()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
            
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
                ScoreManager.Instance.OnScorePopup -= ShowScorePopup;
            }
            
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimeChanged -= UpdateTimerDisplay;
            }
            
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.OnComboChanged -= UpdateComboDisplay;
                ComboManager.Instance.OnComboReset -= HideCombo;
            }
            
            if (FeverManager.Instance != null)
            {
                FeverManager.Instance.OnFeverCountChanged -= OnFeverCountChanged;
                FeverManager.Instance.OnFeverStart -= OnFeverStart;
                FeverManager.Instance.OnFeverEnd -= OnFeverEnd;
                FeverManager.Instance.OnFeverTimeChanged -= UpdateFeverTimer;
            }
        }
        
        private void SetupButtons()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            
            if (pauseRetryButton != null)
                pauseRetryButton.onClick.AddListener(OnRetryButtonClicked);
            
            if (pauseTitleButton != null)
                pauseTitleButton.onClick.AddListener(OnTitleButtonClicked);
            
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryButtonClicked);
            
            if (titleButton != null)
                titleButton.onClick.AddListener(OnTitleButtonClicked);
        }
        
        #region UI更新メソッド
        
        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString("N0");
            }
        }
        
        private void UpdateTimerDisplay(float time)
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(time);
                timerText.text = seconds.ToString();
            }
            
            if (timerGaugeFill != null && TimerManager.Instance != null)
            {
                timerGaugeFill.fillAmount = TimerManager.Instance.TimeRatio;
            }
        }
        
        private void UpdateComboDisplay(int combo)
        {
            if (combo <= 0)
            {
                HideCombo();
                return;
            }
            
            if (comboPanel != null)
            {
                comboPanel.SetActive(true);
            }
            
            if (comboText != null)
            {
                comboText.text = combo.ToString();
            }
            
            // コンボアニメーション
            if (comboPanel != null)
            {
                comboPanel.transform.localScale = Vector3.one * 1.2f;
                StartCoroutine(ScaleAnimation(comboPanel.transform, Vector3.one, 0.2f));
            }
        }
        
        private void HideCombo()
        {
            if (comboPanel != null)
            {
                comboPanel.SetActive(false);
            }
        }
        
        private void UpdateFeverGauge(int count)
        {
            if (feverGaugeFill != null && FeverManager.Instance != null)
            {
                feverGaugeFill.fillAmount = FeverManager.Instance.FeverRatio;
            }
        }
        
        private void OnFeverCountChanged(int count)
        {
            UpdateFeverGauge(count);
        }
        
        private void OnFeverStart()
        {
            if (feverGaugePanel != null) feverGaugePanel.SetActive(false);
            if (feverActivePanel != null) feverActivePanel.SetActive(true);
        }
        
        private void OnFeverEnd()
        {
            if (feverGaugePanel != null) feverGaugePanel.SetActive(true);
            if (feverActivePanel != null) feverActivePanel.SetActive(false);
        }
        
        private void UpdateFeverTimer(float time)
        {
            if (feverTimerText != null)
            {
                feverTimerText.text = Mathf.CeilToInt(time).ToString();
            }
        }
        
        private void ShowScorePopup(int score, Vector3 worldPosition)
        {
            if (scorePopupPrefab == null || scorePopupParent == null) return;
            
            GameObject popup = Instantiate(scorePopupPrefab, scorePopupParent);
            
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            popup.transform.position = screenPos;
            
            Text popupText = popup.GetComponentInChildren<Text>();
            if (popupText != null)
            {
                popupText.text = "+" + score.ToString();
            }
            
            StartCoroutine(ScorePopupAnimation(popup));
        }
        
        private System.Collections.IEnumerator ScorePopupAnimation(GameObject popup)
        {
            popup.transform.localScale = Vector3.zero;
            
            // スケールイン
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.2f;
                popup.transform.localScale = Vector3.one * EaseOutBack(t);
                yield return null;
            }
            popup.transform.localScale = Vector3.one;
            
            // 上に移動 + フェードアウト
            Vector3 startPos = popup.transform.localPosition;
            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = popup.AddComponent<CanvasGroup>();
            
            elapsed = 0f;
            while (elapsed < 0.8f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.8f;
                popup.transform.localPosition = startPos + Vector3.up * (50f * t);
                if (t > 0.5f)
                {
                    canvasGroup.alpha = 1f - ((t - 0.5f) / 0.5f);
                }
                yield return null;
            }
            
            Destroy(popup);
        }
        
        #endregion
        
        #region ゲーム状態変更
        
        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Countdown:
                    StartCountdown();
                    break;
                case GameState.Playing:
                    HideCountdown();
                    HidePausePanel();
                    break;
                case GameState.Paused:
                    ShowPausePanel();
                    break;
                case GameState.Result:
                    ShowResultPanel();
                    break;
            }
        }
        
        #endregion
        
        #region カウントダウン
        
        private void StartCountdown()
        {
            countdownValue = 3;
            if (countdownPanel != null) countdownPanel.SetActive(true);
            UpdateCountdownDisplay();
            InvokeRepeating(nameof(CountdownTick), countdownInterval, countdownInterval);
        }
        
        private void CountdownTick()
        {
            countdownValue--;
            
            if (countdownValue <= 0)
            {
                CancelInvoke(nameof(CountdownTick));
                HideCountdown();
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGame();
                }
            }
            else
            {
                UpdateCountdownDisplay();
            }
        }
        
        private void UpdateCountdownDisplay()
        {
            if (countdownText != null)
            {
                countdownText.text = countdownValue.ToString();
                countdownText.transform.localScale = Vector3.one * 1.5f;
                StartCoroutine(ScaleAnimation(countdownText.transform, Vector3.one, 0.3f));
            }
        }
        
        private void HideCountdown()
        {
            if (countdownPanel != null) countdownPanel.SetActive(false);
        }
        
        #endregion
        
        #region ポーズ
        
        private void ShowPausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                pausePanel.transform.localScale = Vector3.zero;
                StartCoroutine(ScaleAnimationUnscaled(pausePanel.transform, Vector3.one, 0.3f));
            }
        }
        
        private void HidePausePanel()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }
        
        #endregion
        
        #region リザルト
        
        private void ShowResultPanel()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
                resultPanel.transform.localScale = Vector3.zero;
                StartCoroutine(ScaleAnimation(resultPanel.transform, Vector3.one, 0.5f));
            }
            
            if (resultScoreText != null && ScoreManager.Instance != null)
            {
                resultScoreText.text = ScoreManager.Instance.CurrentScore.ToString("N0");
            }
            
            if (resultHighScoreText != null && ScoreManager.Instance != null)
            {
                resultHighScoreText.text = ScoreManager.Instance.HighScore.ToString("N0");
            }
            
            if (newRecordIcon != null && ScoreManager.Instance != null)
            {
                newRecordIcon.SetActive(ScoreManager.Instance.IsNewRecord);
            }
        }
        
        #endregion
        
        #region ボタンイベント
        
        private void OnPauseButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }
        
        private void OnResumeButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }
        
        private void OnRetryButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RetryGame();
            }
        }
        
        private void OnTitleButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToTitle();
            }
        }
        
        #endregion
        
        #region アニメーションヘルパー
        
        private System.Collections.IEnumerator ScaleAnimation(Transform target, Vector3 to, float duration)
        {
            Vector3 from = target.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.localScale = Vector3.Lerp(from, to, EaseOutBack(t));
                yield return null;
            }
            
            target.localScale = to;
        }
        
        private System.Collections.IEnumerator ScaleAnimationUnscaled(Transform target, Vector3 to, float duration)
        {
            Vector3 from = target.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.localScale = Vector3.Lerp(from, to, EaseOutBack(t));
                yield return null;
            }
            
            target.localScale = to;
        }
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        #endregion
    }
}
