using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PONPONLemon.Core
{
    /// <summary>
    /// LeanTweenが無い場合の簡易Tweenクラス
    /// LeanTweenをインポートした場合は、このクラスを削除してください
    /// </summary>
    public static class SimpleTween
    {
        public static Coroutine Scale(MonoBehaviour owner, GameObject target, Vector3 to, float duration, System.Action onComplete = null)
        {
            return owner.StartCoroutine(ScaleCoroutine(target.transform, to, duration, onComplete));
        }
        
        public static Coroutine Move(MonoBehaviour owner, RectTransform target, Vector2 to, float duration, System.Action onComplete = null)
        {
            return owner.StartCoroutine(MoveCoroutine(target, to, duration, onComplete));
        }
        
        public static Coroutine Alpha(MonoBehaviour owner, CanvasGroup target, float to, float duration, float delay = 0f, System.Action onComplete = null)
        {
            return owner.StartCoroutine(AlphaCoroutine(target, to, duration, delay, onComplete));
        }
        
        private static IEnumerator ScaleCoroutine(Transform target, Vector3 to, float duration, System.Action onComplete)
        {
            Vector3 from = target.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutBack(t);
                target.localScale = Vector3.Lerp(from, to, t);
                yield return null;
            }
            
            target.localScale = to;
            onComplete?.Invoke();
        }
        
        private static IEnumerator MoveCoroutine(RectTransform target, Vector2 to, float duration, System.Action onComplete)
        {
            Vector2 from = target.anchoredPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutBounce(t);
                target.anchoredPosition = Vector2.Lerp(from, to, t);
                yield return null;
            }
            
            target.anchoredPosition = to;
            onComplete?.Invoke();
        }
        
        private static IEnumerator AlphaCoroutine(CanvasGroup target, float to, float duration, float delay, System.Action onComplete)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            
            float from = target.alpha;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            
            target.alpha = to;
            onComplete?.Invoke();
        }
        
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        private static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            
            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / d1;
                return n1 * t * t + 0.984375f;
            }
        }
    }
}
