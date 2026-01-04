using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using PONPONLemon.Core;

namespace PONPONLemon.Tsum
{
    public class TsumManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform playArea;
        [SerializeField] private GameObject tsumPrefab;
        [SerializeField] private Sprite[] tsumSprites;
        [SerializeField] private LineRenderer chainLine;
        
        [Header("Settings")]
        [SerializeField] private int gridWidth = 7;
        [SerializeField] private int gridHeight = 9;
        [SerializeField] private float tsumSize = 120f;
        [SerializeField] private float tsumSpacing = 10f;
        
        private Tsum[,] grid;
        private List<Tsum> selectedChain = new List<Tsum>();
        private bool isDragging;
        private bool canInput = true;
        
        public event Action<int, int> OnChainCompleted;
        public event Action OnChainCancelled;
        
        private Vector2 gridOffset;
        private float cellSize;
        
        private void Start()
        {
            CalculateGridLayout();
            InitializeGrid();
        }
        
        private void CalculateGridLayout()
        {
            cellSize = tsumSize + tsumSpacing;
            float totalWidth = gridWidth * cellSize;
            float totalHeight = gridHeight * cellSize;
            gridOffset = new Vector2(
                -totalWidth / 2f + cellSize / 2f,
                -totalHeight / 2f + cellSize / 2f
            );
        }
        
        public void InitializeGrid()
        {
            ClearAllTsums();
            grid = new Tsum[gridWidth, gridHeight];
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    SpawnTsum(x, y, true);
                }
            }
        }
        
        private void ClearAllTsums()
        {
            if (grid != null)
            {
                foreach (var tsum in grid)
                {
                    if (tsum != null)
                        tsum.DestroyImmediate();
                }
            }
            selectedChain.Clear();
        }
        
        private Tsum SpawnTsum(int gridX, int gridY, bool instant = false)
        {
            if (tsumPrefab == null || tsumSprites == null || tsumSprites.Length == 0)
            {
                Debug.LogError("Tsum prefab or sprites not set!");
                return null;
            }
            
            GameObject obj = Instantiate(tsumPrefab, playArea);
            Tsum tsum = obj.GetComponent<Tsum>();
            
            int typeIndex = UnityEngine.Random.Range(0, Mathf.Min(GameConstants.TSUM_TYPES, tsumSprites.Length));
            TsumType type = (TsumType)typeIndex;
            
            tsum.Initialize(type, tsumSprites[typeIndex], gridX, gridY);
            
            Vector2 targetPos = GridToPosition(gridX, gridY);
            
            if (instant)
            {
                tsum.MoveTo(targetPos, true);
            }
            else
            {
                Vector2 startPos = GridToPosition(gridX, gridHeight + 1);
                tsum.MoveTo(startPos, true);
                tsum.MoveTo(targetPos, false);
            }
            
            grid[gridX, gridY] = tsum;
            return tsum;
        }
        
        private Vector2 GridToPosition(int gridX, int gridY)
        {
            return new Vector2(
                gridOffset.x + gridX * cellSize,
                gridOffset.y + gridY * cellSize
            );
        }
        
        private Vector2Int PositionToGrid(Vector2 position)
        {
            int x = Mathf.FloorToInt((position.x - gridOffset.x + cellSize / 2f) / cellSize);
            int y = Mathf.FloorToInt((position.y - gridOffset.y + cellSize / 2f) / cellSize);
            return new Vector2Int(x, y);
        }
        
        private bool IsValidGridPosition(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }
        
        private void Update()
        {
            if (!canInput) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                OnPointerMove(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                OnPointerUp();
            }
        }
        
        private void OnPointerDown(Vector2 screenPosition)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                playArea, screenPosition, null, out localPoint);
            
            Vector2Int gridPos = PositionToGrid(localPoint);
            
            if (IsValidGridPosition(gridPos.x, gridPos.y))
            {
                Tsum tsum = grid[gridPos.x, gridPos.y];
                if (tsum != null && tsum.State == TsumState.Idle)
                {
                    isDragging = true;
                    selectedChain.Clear();
                    SelectTsum(tsum);
                }
            }
        }
        
        private void OnPointerMove(Vector2 screenPosition)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                playArea, screenPosition, null, out localPoint);
            
            Vector2Int gridPos = PositionToGrid(localPoint);
            
            if (IsValidGridPosition(gridPos.x, gridPos.y))
            {
                Tsum tsum = grid[gridPos.x, gridPos.y];
                if (tsum != null && tsum.State == TsumState.Idle && !tsum.IsSelected)
                {
                    TryAddToChain(tsum);
                }
                else if (tsum != null && tsum.IsSelected && selectedChain.Count > 1)
                {
                    TryRemoveFromChain(tsum);
                }
            }
            
            UpdateChainLine();
        }
        
        private void OnPointerUp()
        {
            isDragging = false;
            
            if (selectedChain.Count >= GameConstants.MIN_CHAIN_COUNT)
            {
                StartCoroutine(ProcessChain());
            }
            else
            {
                CancelChain();
            }
            
            ClearChainLine();
        }
        
        private void SelectTsum(Tsum tsum)
        {
            tsum.Select();
            selectedChain.Add(tsum);
        }
        
        private void TryAddToChain(Tsum tsum)
        {
            if (selectedChain.Count == 0) return;
            
            Tsum lastTsum = selectedChain[selectedChain.Count - 1];
            
            if (tsum.IsSameType(lastTsum) && tsum.IsAdjacent(lastTsum))
            {
                SelectTsum(tsum);
            }
        }
        
        private void TryRemoveFromChain(Tsum tsum)
        {
            int index = selectedChain.IndexOf(tsum);
            if (index >= 0 && index < selectedChain.Count - 1)
            {
                for (int i = selectedChain.Count - 1; i > index; i--)
                {
                    selectedChain[i].Deselect();
                    selectedChain.RemoveAt(i);
                }
            }
        }
        
        private void CancelChain()
        {
            foreach (var tsum in selectedChain)
            {
                tsum.Deselect();
            }
            selectedChain.Clear();
            OnChainCancelled?.Invoke();
        }
        
        private IEnumerator ProcessChain()
        {
            canInput = false;
            
            int chainCount = selectedChain.Count;
            int tsumCount = chainCount;
            
            foreach (var tsum in selectedChain)
            {
                grid[tsum.GridX, tsum.GridY] = null;
                tsum.Pop();
            }
            
            OnChainCompleted?.Invoke(tsumCount, chainCount);
            
            yield return new WaitForSeconds(GameConstants.TSUM_POP_DURATION);
            
            selectedChain.Clear();
            
            yield return StartCoroutine(DropTsums());
            yield return StartCoroutine(FillGrid());
            
            canInput = true;
        }
        
        private IEnumerator DropTsums()
        {
            bool hasMoved = true;
            
            while (hasMoved)
            {
                hasMoved = false;
                
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight - 1; y++)
                    {
                        if (grid[x, y] == null)
                        {
                            for (int searchY = y + 1; searchY < gridHeight; searchY++)
                            {
                                if (grid[x, searchY] != null)
                                {
                                    Tsum tsum = grid[x, searchY];
                                    grid[x, searchY] = null;
                                    grid[x, y] = tsum;
                                    tsum.GridY = y;
                                    tsum.MoveTo(GridToPosition(x, y));
                                    hasMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                
                if (hasMoved)
                    yield return new WaitForSeconds(0.05f);
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        
        private IEnumerator FillGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] == null)
                    {
                        SpawnTsum(x, y, false);
                        yield return new WaitForSeconds(GameConstants.TSUM_SPAWN_DELAY);
                    }
                }
            }
            
            yield return new WaitForSeconds(0.3f);
        }
        
        private void UpdateChainLine()
        {
            if (chainLine == null || selectedChain.Count == 0) return;
            
            chainLine.positionCount = selectedChain.Count;
            
            for (int i = 0; i < selectedChain.Count; i++)
            {
                Vector3 worldPos = selectedChain[i].transform.position;
                chainLine.SetPosition(i, worldPos);
            }
        }
        
        private void ClearChainLine()
        {
            if (chainLine != null)
                chainLine.positionCount = 0;
        }
        
        public void SetInputEnabled(bool enabled)
        {
            canInput = enabled;
            
            if (!enabled)
            {
                CancelChain();
                ClearChainLine();
                isDragging = false;
            }
        }
    }
}