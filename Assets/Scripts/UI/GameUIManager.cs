using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PONPONLemon.Core;
using PONPONLemon.Managers;

namespace PONPONLemon.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("Score")]
        [SerializeField] private Text scoreText;
        [SerializeField] private RectTransform scorePopupParent;
        [SerializeField] private GameObject scorePopupPrefab;
        
        [Header("Timer")]
        [SerializeField] private Text timerText;
        [SerializeField] private Image timerFillImage;
        
        [Header("Combo")]
        [SerializeField] private GameObject comboPanel;
        [SerializeField] private Text comboCountText;
        [SerializeField] private Text comboMultiplierText;
        [SerializeField] private Image comboTimerFill;
        
        [Header("Fever")]
        [SerializeField] private Image feverGaugeFill;
        [SerializeField] private GameObject feverTextObject;
        [SerializeField] private GameObject feverEffectObject;
        
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject resultPanel;
        
        [Header("Result UI")]
        [SerializeField] private Text resultScoreText;
        [SerializeField] private Text resultHighScoreText;
        [SerializeField] private GameObject newRecordIcon;
        [SerializeField] private Text resultMaxComboText;
        [SerializeField] private Text resultTsumsClearedText;
        [SerializeField] private Text resultFeverCountText;
        
        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button titleButton;
        
        private ScoreManager scoreManager;
        private TimerManager timerManager;
        private ComboManager comboManager;
        private FeverManager feverManager;
        private GameManager gameManager;
        
        private void Start()
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
            timerManager = FindFirstObjectByType<TimerManager>();
            comboManager = FindFirstObjectByType<ComboManager>();
            feverManager = FindFirstObjectByType<FeverManager>();
            gameManager = GameManager.Instance;
            
            RegisterEvents();
            SetupButtons();
            InitializeUI();
        }
        
        private void RegisterEvents()
        {
            if (scoreManager != null)
                scoreManager.OnScoreChanged += UpdateScoreUI;
            if (timerManager != null)
                timerManager.OnTimeChanged += UpdateTimerUI;
            if (comboManager != null)
            {
                comboManager.OnComboChanged += UpdateComboUI;
                comboManager.OnComboTimerChanged += UpdateComboTimerUI;
            }
            if (feverManager != null)
            {
                feverManager.OnGaugeChanged += UpdateFeverGaugeUI;
                feverManager.OnFeverStart += OnFeverStart;
                feverManager.OnFeverEnd += OnFeverEnd;
                feverManager.OnFeverTimeChanged += UpdateFeverTimeUI;
            }
        }
        
        private void SetupButtons()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);
            if (titleButton != null)
                titleButton.onClick.AddListener(OnTitleClicked);
        }
        
        private void InitializeUI()
        {
            UpdateScoreUI(0);
            UpdateTimerUI(GameConstants.GAME_TIME);
            UpdateComboUI(0);
            UpdateFeverGaugeUI(0);
            
            if (comboPanel != null) comboPanel.SetActive(false);
            if (feverTextObject != null) feverTextObject.SetActive(false);
            if (feverEffectObject != null) feverEffectObject.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
        }
        
        private void UpdateScoreUI(int score)
        {
            if (scoreText != null)
                scoreText.text = score.ToString("N0");
        }
        
        private void UpdateTimerUI(float time)
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(time).ToString();
            if (timerFillImage != null && timerManager != null)
                timerFillImage.fillAmount = timerManager.TimeRatio;
        }
        
        private void UpdateComboUI(int combo)
        {
            if (comboPanel != null)
                comboPanel.SetActive(combo > 0);
            if (comboCountText != null)
                comboCountText.text = combo.ToString();
            if (comboMultiplierText != null && comboManager != null)
                comboMultiplierText.text = comboManager.GetMultiplierText();
        }
        
        private void UpdateComboTimerUI(float ratio)
        {
            if (comboTimerFill != null)
                comboTimerFill.fillAmount = ratio;
        }
        
        private void UpdateFeverGaugeUI(int gauge)
        {
            if (feverGaugeFill != null && feverManager != null)
                feverGaugeFill.fillAmount = feverManager.GaugeRatio;
        }
        
        private void UpdateFeverTimeUI(float ratio)
        {
            if (feverGaugeFill != null)
                feverGaugeFill.fillAmount = ratio;
        }
        
        private void OnFeverStart()
        {
            if (feverTextObject != null)
            {
                feverTextObject.SetActive(true);
                feverTextObject.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateScale(feverTextObject.transform, Vector3.one, 0.5f));
            }
            if (feverEffectObject != null)
                feverEffectObject.SetActive(true);
        }
        
        private void OnFeverEnd()
        {
            if (feverTextObject != null)
            {
                StartCoroutine(AnimateScale(feverTextObject.transform, Vector3.zero, 0.3f, () =>
                    feverTextObject.SetActive(false)));
            }
            if (feverEffectObject != null)
                feverEffectObject.SetActive(false);
        }
        
        private IEnumerator AnimateScale(Transform t, Vector3 to, float duration, System.Action onComplete = null)
        {
            Vector3 from = t.localScale;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            t.localScale = to;
            onComplete?.Invoke();
        }
        
        public void ShowScorePopup(int score, int chain)
        {
            if (scorePopupPrefab == null || scorePopupParent == null) return;
            
            GameObject popup = Instantiate(scorePopupPrefab, scorePopupParent);
            Text text = popup.GetComponentInChildren<Text>();
            
            if (text != null)
            {
                text.text = $"+{score}";
                if (chain > 3)
                    text.text += $"\n{chain}チェーン!";
            }
            
            StartCoroutine(AnimatePopup(popup));
        }
        
        private IEnumerator AnimatePopup(GameObject popup)
        {
            popup.transform.localScale = Vector3.zero;
            Vector3 startPos = popup.transform.localPosition;
            CanvasGroup cg = popup.GetComponent<CanvasGroup>();
            
            float elapsed = 0;
            float duration = 1f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                if (t < 0.2f)
                    popup.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / 0.2f);
                
                popup.transform.localPosition = startPos + Vector3.up * (100 * t);
                
                if (cg != null && t > 0.5f)
                    cg.alpha = 1 - ((t - 0.5f) / 0.5f);
                
                yield return null;
            }
            Destroy(popup);
        }
        
        public void ShowPausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                pausePanel.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateScale(pausePanel.transform, Vector3.one, 0.3f));
            }
        }
        
        public void HidePausePanel()
        {
            if (pausePanel != null)
            {
                StartCoroutine(AnimateScale(pausePanel.transform, Vector3.zero, 0.2f, () =>
                    pausePanel.SetActive(false)));
            }
        }
        
        public void ShowResultPanel(int score, int highScore, bool isNewRecord,
            int maxCombo, int tsumsCleared, int feverCount)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
                resultPanel.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateScale(resultPanel.transform, Vector3.one, 0.5f));
            }
            
            if (resultScoreText != null)
                resultScoreText.text = score.ToString("N0");
            if (resultHighScoreText != null)
                resultHighScoreText.text = highScore.ToString("N0");
            if (newRecordIcon != null)
                newRecordIcon.SetActive(isNewRecord);
            if (resultMaxComboText != null)
                resultMaxComboText.text = maxCombo.ToString();
            if (resultTsumsClearedText != null)
                resultTsumsClearedText.text = tsumsCleared.ToString();
            if (resultFeverCountText != null)
                resultFeverCountText.text = feverCount.ToString();
        }
        
        private void OnPauseClicked() => gameManager?.PauseGame();
        private void OnResumeClicked() => gameManager?.ResumeGame();
        private void OnRetryClicked() => gameManager?.Retry();
        private void OnTitleClicked() => gameManager?.GoToTitle();
        
        private void OnDestroy()
        {
            if (scoreManager != null)
                scoreManager.OnScoreChanged -= UpdateScoreUI;
            if (timerManager != null)
                timerManager.OnTimeChanged -= UpdateTimerUI;
            if (comboManager != null)
            {
                comboManager.OnComboChanged -= UpdateComboUI;
                comboManager.OnComboTimerChanged -= UpdateComboTimerUI;
            }
            if (feverManager != null)
            {
                feverManager.OnGaugeChanged -= UpdateFeverGaugeUI;
                feverManager.OnFeverStart -= OnFeverStart;
                feverManager.OnFeverEnd -= OnFeverEnd;
                feverManager.OnFeverTimeChanged -= UpdateFeverTimeUI;
            }
        }
    }
}
