using UnityEngine;
using UnityEngine.UI;
using PONPONLemon.Core;

namespace PONPONLemon.Tsum
{
    public class TsumItem : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private TsumType tsumType;
        [SerializeField] private Image tsumImage;
        [SerializeField] private Image glowImage;
        
        [Header("状態")]
        [SerializeField] private TsumState currentState = TsumState.Idle;
        
        private Vector2 targetPosition;
        private bool isSelected = false;
        private int gridX;
        private int gridY;
        
        public TsumType Type => tsumType;
        public TsumState State => currentState;
        public bool IsSelected => isSelected;
        public int GridX => gridX;
        public int GridY => gridY;
        
        // イベント
        public System.Action<TsumItem> OnTsumSelected;
        public System.Action<TsumItem> OnTsumDeselected;
        public System.Action<TsumItem> OnTsumPopped;
        
        public void Initialize(TsumType type, Sprite sprite, int x, int y)
        {
            tsumType = type;
            gridX = x;
            gridY = y;
            
            if (tsumImage != null && sprite != null)
            {
                tsumImage.sprite = sprite;
            }
            
            SetState(TsumState.Idle);
            SetSelected(false);
        }
        
        public void SetGridPosition(int x, int y)
        {
            gridX = x;
            gridY = y;
        }
        
        public void SetState(TsumState state)
        {
            currentState = state;
            
            switch (state)
            {
                case TsumState.Idle:
                    break;
                case TsumState.Selected:
                    break;
                case TsumState.Dropping:
                    break;
                case TsumState.Popping:
                    Pop();
                    break;
            }
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (glowImage != null)
            {
                glowImage.enabled = selected;
            }
            
            // 選択時のスケールアニメーション
            float targetScale = selected ? 1.1f : 1f;
            StartCoroutine(ScaleAnimation(Vector3.one * targetScale, 0.1f));
            
            if (selected)
            {
                SetState(TsumState.Selected);
                OnTsumSelected?.Invoke(this);
            }
            else
            {
                SetState(TsumState.Idle);
                OnTsumDeselected?.Invoke(this);
            }
        }
        
        public void MoveTo(Vector2 position, float duration = 0.2f)
        {
            targetPosition = position;
            SetState(TsumState.Dropping);
            
            StartCoroutine(MoveAnimation(position, duration, true));
        }
        
        public void DropTo(Vector2 position, float speed)
        {
            targetPosition = position;
            SetState(TsumState.Dropping);
            
            RectTransform rectTransform = GetComponent<RectTransform>();
            float distance = Vector2.Distance(rectTransform.anchoredPosition, position);
            float duration = distance / speed;
            
            StartCoroutine(MoveAnimation(position, duration, true));
        }
        
        public void Pop()
        {
            StartCoroutine(PopAnimation());
        }
        
        private System.Collections.IEnumerator PopAnimation()
        {
            // ポップアニメーション - 大きくなって消える
            float popDuration = GameConstants.TSUM_POP_DURATION;
            
            // まず大きく
            yield return ScaleAnimationCoroutine(Vector3.one * 1.3f, popDuration * 0.3f);
            
            // 縮んで消える
            yield return ScaleAnimationCoroutine(Vector3.zero, popDuration * 0.7f);
            
            OnTsumPopped?.Invoke(this);
            Destroy(gameObject);
        }
        
        public bool IsAdjacent(TsumItem other)
        {
            if (other == null) return false;
            
            int dx = Mathf.Abs(gridX - other.gridX);
            int dy = Mathf.Abs(gridY - other.gridY);
            
            // 縦横斜め隣接をチェック
            return dx <= 1 && dy <= 1 && !(dx == 0 && dy == 0);
        }
        
        public bool IsSameType(TsumItem other)
        {
            return other != null && tsumType == other.Type;
        }
        
        #region アニメーションヘルパー
        
        private System.Collections.IEnumerator ScaleAnimation(Vector3 to, float duration)
        {
            yield return ScaleAnimationCoroutine(to, duration);
        }
        
        private System.Collections.IEnumerator ScaleAnimationCoroutine(Vector3 to, float duration)
        {
            Vector3 from = transform.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(from, to, EaseOutQuad(t));
                yield return null;
            }
            
            transform.localScale = to;
        }
        
        private System.Collections.IEnumerator MoveAnimation(Vector2 to, float duration, bool bounce)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 from = rectTransform.anchoredPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = bounce ? EaseOutBounce(t) : EaseOutQuad(t);
                rectTransform.anchoredPosition = Vector2.Lerp(from, to, easedT);
                yield return null;
            }
            
            rectTransform.anchoredPosition = to;
            SetState(TsumState.Idle);
        }
        
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
        
        private float EaseOutBounce(float t)
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
        
        private float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }
        
        #endregion
    }
}
