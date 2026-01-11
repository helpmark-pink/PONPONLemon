using UnityEngine;
using UnityEngine.UI;
using PONPONLemon.Managers;

namespace PONPONLemon.Effects
{
    public class FeverEffect : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject feverTextObject;
        [SerializeField] private Image backgroundOverlay;
        [SerializeField] private ParticleSystem feverParticles;
        
        [Header("色設定")]
        [SerializeField] private Color feverOverlayColor = new Color(1f, 0.8f, 0f, 0.2f);
        [SerializeField] private float pulseSpeed = 2f;
        
        private bool isFeverActive = false;
        private float pulseTimer = 0f;
        
        private void Start()
        {
            if (FeverManager.Instance != null)
            {
                FeverManager.Instance.OnFeverStart += OnFeverStart;
                FeverManager.Instance.OnFeverEnd += OnFeverEnd;
            }
            
            HideEffect();
        }
        
        private void OnDestroy()
        {
            if (FeverManager.Instance != null)
            {
                FeverManager.Instance.OnFeverStart -= OnFeverStart;
                FeverManager.Instance.OnFeverEnd -= OnFeverEnd;
            }
        }
        
        private void Update()
        {
            if (!isFeverActive) return;
            
            // パルスアニメーション
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
            
            if (backgroundOverlay != null)
            {
                Color color = feverOverlayColor;
                color.a = feverOverlayColor.a * (0.5f + pulse * 0.5f);
                backgroundOverlay.color = color;
            }
        }
        
        private void OnFeverStart()
        {
            isFeverActive = true;
            ShowEffect();
        }
        
        private void OnFeverEnd()
        {
            isFeverActive = false;
            HideEffect();
        }
        
        private void ShowEffect()
        {
            if (feverTextObject != null)
            {
                feverTextObject.SetActive(true);
                StartCoroutine(FeverTextAnimation());
            }
            
            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(true);
                StartCoroutine(FadeInOverlay());
            }
            
            if (feverParticles != null)
            {
                feverParticles.Play();
            }
        }
        
        private void HideEffect()
        {
            if (feverTextObject != null)
            {
                feverTextObject.SetActive(false);
            }
            
            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(false);
            }
            
            if (feverParticles != null)
            {
                feverParticles.Stop();
            }
        }
        
        private System.Collections.IEnumerator FeverTextAnimation()
        {
            // 大きくなって登場
            feverTextObject.transform.localScale = Vector3.zero;
            
            float elapsed = 0f;
            float duration = 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = EaseOutBack(t) * 1.2f;
                feverTextObject.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            
            // 少し縮む
            elapsed = 0f;
            duration = 0.2f;
            Vector3 from = feverTextObject.transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                feverTextObject.transform.localScale = Vector3.Lerp(from, Vector3.one, t);
                yield return null;
            }
            
            feverTextObject.transform.localScale = Vector3.one;
        }
        
        private System.Collections.IEnumerator FadeInOverlay()
        {
            backgroundOverlay.color = new Color(feverOverlayColor.r, feverOverlayColor.g, feverOverlayColor.b, 0f);
            
            float elapsed = 0f;
            float duration = 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Color color = feverOverlayColor;
                color.a = feverOverlayColor.a * t;
                backgroundOverlay.color = color;
                yield return null;
            }
        }
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
