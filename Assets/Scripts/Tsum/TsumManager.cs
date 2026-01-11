using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using PONPONLemon.Core;
using PONPONLemon.Managers;

namespace PONPONLemon.Tsum
{
    public class TsumManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("ツム設定")]
        [SerializeField] private GameObject tsumPrefab;
        [SerializeField] private RectTransform playArea;
        [SerializeField] private Sprite[] tsumSprites;
        
        [Header("グリッド設定")]
        [SerializeField] private int gridWidth = GameConstants.GRID_WIDTH;
        [SerializeField] private int gridHeight = GameConstants.GRID_HEIGHT;
        [SerializeField] private float tsumSize = GameConstants.TSUM_SIZE;
        [SerializeField] private float tsumSpacing = GameConstants.TSUM_SPACING;
        
        [Header("チェーンライン")]
        [SerializeField] private LineRenderer chainLine;
        [SerializeField] private Color chainLineColor = Color.yellow;
        
        private TsumItem[,] grid;
        private List<TsumItem> selectedChain = new List<TsumItem>();
        private bool isDragging = false;
        private Camera mainCamera;
        
        // イベント
        public System.Action<int, Vector3> OnChainCompleted;
        
        private void Start()
        {
            mainCamera = Camera.main;
            InitializeGrid();
            
            // ゲーム開始時にツムを配置
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += OnGameStart;
            }
            
            if (chainLine != null)
            {
                chainLine.startColor = chainLineColor;
                chainLine.endColor = chainLineColor;
                chainLine.startWidth = 10f;
                chainLine.endWidth = 10f;
                chainLine.positionCount = 0;
            }
        }
        
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= OnGameStart;
            }
        }
        
        private void OnGameStart()
        {
            FillGrid();
        }
        
        private void InitializeGrid()
        {
            grid = new TsumItem[gridWidth, gridHeight];
        }
        
        private void FillGrid()
        {
            StartCoroutine(FillGridCoroutine());
        }
        
        private IEnumerator FillGridCoroutine()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[x, y] == null)
                    {
                        SpawnTsum(x, y, true);
                        yield return new WaitForSeconds(GameConstants.TSUM_SPAWN_DELAY);
                    }
                }
            }
        }
        
        private TsumItem SpawnTsum(int x, int y, bool animate = false)
        {
            if (tsumPrefab == null || playArea == null) return null;
            
            GameObject tsumObj = Instantiate(tsumPrefab, playArea);
            TsumItem tsum = tsumObj.GetComponent<TsumItem>();
            
            if (tsum == null)
            {
                tsum = tsumObj.AddComponent<TsumItem>();
            }
            
            // ランダムなタイプを選択
            int typeIndex = Random.Range(0, Mathf.Min(GameConstants.TSUM_TYPES, tsumSprites.Length));
            TsumType type = (TsumType)typeIndex;
            Sprite sprite = tsumSprites.Length > typeIndex ? tsumSprites[typeIndex] : null;
            
            tsum.Initialize(type, sprite, x, y);
            
            // 位置設定
            RectTransform rectTransform = tsumObj.GetComponent<RectTransform>();
            Vector2 targetPos = GetGridPosition(x, y);
            
            if (animate)
            {
                // 上から落下アニメーション
                Vector2 startPos = new Vector2(targetPos.x, playArea.rect.height + tsumSize);
                rectTransform.anchoredPosition = startPos;
                tsum.DropTo(targetPos, GameConstants.TSUM_DROP_SPEED);
            }
            else
            {
                rectTransform.anchoredPosition = targetPos;
            }
            
            rectTransform.sizeDelta = new Vector2(tsumSize, tsumSize);
            
            grid[x, y] = tsum;
            
            return tsum;
        }
        
        private Vector2 GetGridPosition(int x, int y)
        {
            float totalWidth = gridWidth * (tsumSize + tsumSpacing) - tsumSpacing;
            float totalHeight = gridHeight * (tsumSize + tsumSpacing) - tsumSpacing;
            
            float startX = (playArea.rect.width - totalWidth) / 2f;
            float startY = (playArea.rect.height - totalHeight) / 2f;
            
            float posX = startX + x * (tsumSize + tsumSpacing) + tsumSize / 2f;
            float posY = startY + y * (tsumSize + tsumSpacing) + tsumSize / 2f;
            
            return new Vector2(posX, posY);
        }
        
        private TsumItem GetTsumAtPosition(Vector2 screenPosition)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                playArea, screenPosition, mainCamera, out localPoint);
            
            // ローカル座標からグリッド座標を計算
            float totalWidth = gridWidth * (tsumSize + tsumSpacing) - tsumSpacing;
            float totalHeight = gridHeight * (tsumSize + tsumSpacing) - tsumSpacing;
            
            float startX = (playArea.rect.width - totalWidth) / 2f;
            float startY = (playArea.rect.height - totalHeight) / 2f;
            
            int gridX = Mathf.FloorToInt((localPoint.x - startX) / (tsumSize + tsumSpacing));
            int gridY = Mathf.FloorToInt((localPoint.y - startY) / (tsumSize + tsumSpacing));
            
            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
            {
                return grid[gridX, gridY];
            }
            
            return null;
        }
        
        #region タッチ入力
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
                return;
            
            isDragging = true;
            selectedChain.Clear();
            
            TsumItem tsum = GetTsumAtPosition(eventData.position);
            if (tsum != null)
            {
                AddToChain(tsum);
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
                return;
            
            TsumItem tsum = GetTsumAtPosition(eventData.position);
            if (tsum != null && !selectedChain.Contains(tsum))
            {
                // 最後に選択したツムと隣接していて同じタイプかチェック
                if (selectedChain.Count > 0)
                {
                    TsumItem lastTsum = selectedChain[selectedChain.Count - 1];
                    if (tsum.IsAdjacent(lastTsum) && tsum.IsSameType(lastTsum))
                    {
                        AddToChain(tsum);
                    }
                }
            }
            // 一つ前のツムに戻った場合は選択解除
            else if (tsum != null && selectedChain.Count > 1 && tsum == selectedChain[selectedChain.Count - 2])
            {
                RemoveLastFromChain();
            }
            
            UpdateChainLine();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            isDragging = false;
            
            if (selectedChain.Count >= GameConstants.MIN_CHAIN_COUNT)
            {
                // チェーン成功
                CompleteChain();
            }
            else
            {
                // チェーン失敗 - 選択解除
                ClearChain();
            }
            
            ClearChainLine();
        }
        
        #endregion
        
        #region チェーン処理
        
        private void AddToChain(TsumItem tsum)
        {
            selectedChain.Add(tsum);
            tsum.SetSelected(true);
            UpdateChainLine();
        }
        
        private void RemoveLastFromChain()
        {
            if (selectedChain.Count > 0)
            {
                TsumItem lastTsum = selectedChain[selectedChain.Count - 1];
                lastTsum.SetSelected(false);
                selectedChain.RemoveAt(selectedChain.Count - 1);
            }
        }
        
        private void ClearChain()
        {
            foreach (TsumItem tsum in selectedChain)
            {
                if (tsum != null)
                {
                    tsum.SetSelected(false);
                }
            }
            selectedChain.Clear();
        }
        
        private void CompleteChain()
        {
            int chainCount = selectedChain.Count;
            
            // チェーンの中心位置を計算
            Vector3 centerPos = Vector3.zero;
            foreach (TsumItem tsum in selectedChain)
            {
                centerPos += tsum.transform.position;
            }
            centerPos /= chainCount;
            
            // ツムを消す
            foreach (TsumItem tsum in selectedChain)
            {
                int x = tsum.GridX;
                int y = tsum.GridY;
                grid[x, y] = null;
                tsum.Pop();
            }
            
            selectedChain.Clear();
            
            // スコア加算
            if (ScoreManager.Instance != null && ComboManager.Instance != null)
            {
                bool isFever = FeverManager.Instance != null && FeverManager.Instance.IsFeverActive;
                ScoreManager.Instance.AddScore(chainCount, ComboManager.Instance.CurrentCombo, centerPos, isFever);
            }
            
            // コンボ加算
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.AddCombo();
            }
            
            // フィーバーゲージ加算
            if (FeverManager.Instance != null)
            {
                FeverManager.Instance.AddCount(chainCount);
            }
            
            // イベント発火
            OnChainCompleted?.Invoke(chainCount, centerPos);
            
            // ツムを落下させて補充
            StartCoroutine(DropAndRefill());
        }
        
        private IEnumerator DropAndRefill()
        {
            yield return new WaitForSeconds(GameConstants.TSUM_POP_DURATION);
            
            // 各列のツムを落下
            for (int x = 0; x < gridWidth; x++)
            {
                DropColumn(x);
            }
            
            yield return new WaitForSeconds(0.3f);
            
            // 空いた場所を補充
            RefillGrid();
        }
        
        private void DropColumn(int x)
        {
            int emptyY = -1;
            
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    if (emptyY < 0) emptyY = y;
                }
                else if (emptyY >= 0)
                {
                    // ツムを下に移動
                    TsumItem tsum = grid[x, y];
                    grid[x, emptyY] = tsum;
                    grid[x, y] = null;
                    
                    tsum.SetGridPosition(x, emptyY);
                    Vector2 targetPos = GetGridPosition(x, emptyY);
                    tsum.DropTo(targetPos, GameConstants.TSUM_DROP_SPEED);
                    
                    emptyY++;
                }
            }
        }
        
        private void RefillGrid()
        {
            StartCoroutine(RefillGridCoroutine());
        }
        
        private IEnumerator RefillGridCoroutine()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] == null)
                    {
                        SpawnTsum(x, y, true);
                        yield return new WaitForSeconds(GameConstants.TSUM_SPAWN_DELAY * 0.5f);
                    }
                }
            }
        }
        
        #endregion
        
        #region チェーンライン
        
        private void UpdateChainLine()
        {
            if (chainLine == null) return;
            
            chainLine.positionCount = selectedChain.Count;
            
            for (int i = 0; i < selectedChain.Count; i++)
            {
                if (selectedChain[i] != null)
                {
                    chainLine.SetPosition(i, selectedChain[i].transform.position);
                }
            }
        }
        
        private void ClearChainLine()
        {
            if (chainLine != null)
            {
                chainLine.positionCount = 0;
            }
        }
        
        #endregion
    }
}
