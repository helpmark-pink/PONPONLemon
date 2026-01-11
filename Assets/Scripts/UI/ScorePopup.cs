using UnityEngine;
using UnityEngine.UI;

namespace PONPONLemon.UI
{
    public class ScorePopup : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("アニメーション設定")]
        [SerializeField] private float moveUpDistance = 80f;
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private float fadeStartTime = 0.5f;
        
        public void Initialize(int score, Color color)
        {
            if (scoreText != null)
            {
                scoreText.text = "+" + score.ToString();
                scoreText.color = color;
            }
            
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            StartCoroutine(PlayAnimation());
        }
        
        private System.Collections.IEnumerator PlayAnimation()
        {
            // 初期状態
            transform.localScale = Vector3.zero;
            Vector3 startPos = transform.localPosition;
            
            // スケールイン
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.2f;
                transform.localScale = Vector3.one * EaseOutBack(t);
                yield return null;
            }
            transform.localScale = Vector3.one;
            
            // 上に移動 + フェードアウト
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 上に移動
                transform.localPosition = startPos + Vector3.up * (moveUpDistance * t);
                
                // フェードアウト
                if (t > fadeStartTime / duration && canvasGroup != null)
                {
                    float fadeT = (t - fadeStartTime / duration) / (1f - fadeStartTime / duration);
                    canvasGroup.alpha = 1f - fadeT;
                }
                
                yield return null;
            }
            
            Destroy(gameObject);
        }
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
