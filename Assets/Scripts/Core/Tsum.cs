using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using PONPONLemon.Core;

namespace PONPONLemon.Tsum
{
    public class Tsum : MonoBehaviour
    {
        [SerializeField] private Image tsumImage;
        [SerializeField] private Image highlightImage;
        
        public TsumType Type { get; private set; }
        public TsumState State { get; private set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public bool IsSelected { get; private set; }
        
        private Vector3 targetPosition;
        private bool isMoving;
        private RectTransform rectTransform;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (highlightImage != null)
                highlightImage.gameObject.SetActive(false);
        }
        
        public void Initialize(TsumType type, Sprite sprite, int gridX, int gridY)
        {
            Type = type;
            GridX = gridX;
            GridY = gridY;
            State = TsumState.Idle;
            IsSelected = false;
            
            if (tsumImage != null)
                tsumImage.sprite = sprite;
            
            gameObject.name = $"Tsum_{type}_{gridX}_{gridY}";
        }
        
        public void Select()
        {
            if (State != TsumState.Idle) return;
            
            IsSelected = true;
            State = TsumState.Selected;
            
            if (highlightImage != null)
                highlightImage.gameObject.SetActive(true);
            
            transform.localScale = Vector3.one * 1.1f;
        }
        
        public void Deselect()
        {
            IsSelected = false;
            State = TsumState.Idle;
            
            if (highlightImage != null)
                highlightImage.gameObject.SetActive(false);
            
            transform.localScale = Vector3.one;
        }
        
        public void MoveTo(Vector3 position, bool instant = false)
        {
            targetPosition = position;
            
            if (instant)
            {
                rectTransform.anchoredPosition = position;
                isMoving = false;
            }
            else
            {
                isMoving = true;
                State = TsumState.Dropping;
            }
        }
        
        public void Pop(Action onComplete = null)
        {
            State = TsumState.Popping;
            StartCoroutine(PopAnimation(onComplete));
        }
        
        private IEnumerator PopAnimation(Action onComplete)
        {
            float elapsed = 0;
            float duration = GameConstants.TSUM_POP_DURATION;
            Vector3 startScale = transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = 1 - t;
                transform.localScale = startScale * Mathf.Max(0, scale);
                yield return null;
            }
            
            onComplete?.Invoke();
            Destroy(gameObject);
        }
        
        public void DestroyImmediate()
        {
            Destroy(gameObject);
        }
        
        private void Update()
        {
            if (isMoving)
            {
                Vector2 currentPos = rectTransform.anchoredPosition;
                Vector2 newPos = Vector2.MoveTowards(currentPos, targetPosition,
                    GameConstants.TSUM_DROP_SPEED * Time.deltaTime);
                
                rectTransform.anchoredPosition = newPos;
                
                if (Vector2.Distance(newPos, targetPosition) < 0.1f)
                {
                    rectTransform.anchoredPosition = targetPosition;
                    isMoving = false;
                    State = TsumState.Idle;
                }
            }
        }
        
        public bool IsAdjacent(Tsum other)
        {
            if (other == null) return false;
            int dx = Mathf.Abs(GridX - other.GridX);
            int dy = Mathf.Abs(GridY - other.GridY);
            return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
        }
        
        public bool IsSameType(Tsum other)
        {
            return other != null && Type == other.Type;
        }
    }
}