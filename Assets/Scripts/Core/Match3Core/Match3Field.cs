using System;
using System.Collections.Generic;
using Core.Match3Core;
using DG.Tweening;
using LSCore;
using LSCore.DataStructs;
using LSCore.Extensions;
using UnityEngine;

public class Match3Field : MonoBehaviour 
{
    [Serializable]
    public class ExpandMask : LSAction
    {
        public Vector2Int expand;

        public override void Invoke()
        {
            instance.ExpandMaskSize(expand);
        }
    }
    
    public Vector2Int gridSize;
    public List<SpriteRenderer> chips;
    public SpriteRenderer[] masks;
    public Vector2 maskSizeOffset;
    public int gridSizeOffset = 5;
    public int pointsToWin = 100;

    private bool isGridAnimating;
    private GameObject fieldParent;
    private Vector2Int realGridSize;
    private SpriteRenderer[,] fullGrid;
    private ArraySpan<SpriteRenderer> grid;
    private float lastCameraSize;
    private int steps;

    private int Steps
    {
        get => steps;
        set
        {
            steps = value;
            Match3Window.StepsText.text = "Steps: " + steps;
        }
    }
    
    private int points;

    private int Points
    {
        get => points;
        set
        {
            points = value;
            Match3Window.PointsText.text = "Points: " + points;
            if (points >= pointsToWin)
            {
                Match3Window.Win();
            }
        }
    }
    
    private static Match3Field instance;
    
    private void Awake()
    {
        instance = this;
        lastCameraSize = Camera.main.orthographicSize;
        Camera.main.orthographicSize = gridSize.x + 2;
        
        Match3Window.Show();
        Steps = 0;
        Points = 0;
        realGridSize = gridSize;
        gridSize.x += gridSizeOffset * 2;
        gridSize.y += gridSizeOffset * 2;
        
        fullGrid = new SpriteRenderer[gridSize.x, gridSize.y];
        grid = fullGrid.ToSpan(gridSizeOffset..(gridSize.x - gridSizeOffset), gridSizeOffset..(gridSize.y - gridSizeOffset));

        InitField();
        transform.position = -new Vector3(gridSize.x / 2f - 0.5f, gridSize.y / 2f - 0.5f);
        fieldParent = new GameObject("FieldParent");
        transform.SetParent(fieldParent.transform);
        for (var i = 0; i < masks.Length; i++)
        {
            masks[i].transform.SetParent(fieldParent.transform);
            masks[i].size = new Vector2(realGridSize.x / 2f, realGridSize.y) + maskSizeOffset;
        }
    }

    private void OnDestroy()
    {
        new GoBack().Invoke();
        Camera.main.orthographicSize = lastCameraSize;
    }

    public void ExpandMaskSize(Vector2Int expand)
    {
        var factor = (float)realGridSize.x / (realGridSize.x + expand.x);
        fieldParent.transform.DOScale(Vector3.one * factor, .3f);
        
        for (var i = 0; i < masks.Length; i++)
        {
            masks[i].DOSize(new Vector2((realGridSize.x + expand.x) / 2f, realGridSize.y + expand.y) + maskSizeOffset, 0.3f);
        }
    }

    private void InitField()
    {
        for (var x = 0; x < fullGrid.GetLength(0); x++)
        {
            for (var y = 0; y < fullGrid.GetLength(1); y++)
            {
                fullGrid[x, y] = CreateRandomChip();
                fullGrid[x, y].transform.position = new Vector3(x, y);
            }
        }
        
        var draggers = new List<SwipeArea>();
        
        for (var x = gridSizeOffset; x < fullGrid.GetLength(0) - gridSizeOffset; x++)
        {
            var go = new GameObject($"Dragger Vertical {x}");
            var boxCollider = go.AddComponent<BoxCollider2D>();
            var dragger = go.AddComponent<SwipeArea>();
            draggers.Add(dragger);
            dragger.transform.SetParent(transform);
            dragger.transform.localPosition = Vector3.zero;
            dragger.index = x;
            dragger.isVertical = true;
            
            boxCollider.size = new Vector2(1, realGridSize.y);
            boxCollider.offset = new Vector3(x, realGridSize.y / 2f - 0.5f + gridSizeOffset);
        }
        
        for (var y = gridSizeOffset; y < fullGrid.GetLength(1) - gridSizeOffset; y++)
        {
            var go = new GameObject($"Dragger Horizontal {y}");
            var boxCollider = go.AddComponent<BoxCollider2D>();
            var dragger = go.AddComponent<SwipeArea>();
            draggers.Add(dragger);
            dragger.transform.SetParent(transform);
            dragger.transform.localPosition = Vector3.zero;
            dragger.index = y;
            dragger.isVertical = false;
            
            boxCollider.size = new Vector2(realGridSize.x, 1);
            boxCollider.offset = new Vector3(realGridSize.x / 2f - 0.5f + gridSizeOffset, y);
        }
        
        foreach (var dragger in draggers)
        {
            dragger.Swiped += OnEnd;
        }
        
        void OnEnd(int index, SwipeArea.SwipeDirection direction)
        {
            if(isGridAnimating) return;
            
            Tween tween = direction switch
            {
                SwipeArea.SwipeDirection.Up => SwipeVertical(index, true),
                SwipeArea.SwipeDirection.Down => SwipeVertical(index, false),
                SwipeArea.SwipeDirection.Right => SwipeHorizontal(index, true),
                SwipeArea.SwipeDirection.Left => SwipeHorizontal(index, false),
                SwipeArea.SwipeDirection.None => null,
            };

            if (direction != SwipeArea.SwipeDirection.None)
            {
                Steps++;
                isGridAnimating = true;
                tween.OnComplete(() =>
                {
                    var set = new HashSet<Vector2Int>();
                    CheckAndDestroyChips(set);
                });
            }
            else
            {
                isGridAnimating = false;
            }
        }
    }

    private SpriteRenderer GetRandomChip() => chips.Random();
    private SpriteRenderer CreateRandomChip() => Instantiate(GetRandomChip(), transform);

    private Tween SwipeVertical(int index, bool up)
    {
        var sequence = DOTween.Sequence();
        if (up)
        {
            var lastIndex = fullGrid.GetLength(1) - 1;
            var toDestroy = fullGrid[index, lastIndex];
            var tween = toDestroy.transform.DOLocalMove(Vector3.up, 0.3f).SetRelative(true);
            sequence.Insert(0, tween);
            
            for (int i = lastIndex; i >= 1; i--)
            {
                fullGrid[index, i] = fullGrid[index, i - 1];
                tween = fullGrid[index, i - 1].transform.DOLocalMove(Vector3.up, 0.3f).SetRelative(true);
                sequence.Insert(0, tween);
            }
            
            fullGrid[index, 0] = CreateRandomChip();
            fullGrid[index, 0].transform.localPosition = new(index, -1);
            var tween2 = fullGrid[index, 0].transform.DOLocalMove(new(index, 0), 0.3f)
                .OnComplete(() => Destroy(toDestroy.gameObject));
            sequence.Insert(0, tween2);
        }
        else
        {
            var lastIndex = fullGrid.GetLength(1) - 1;
            var toDestroy = fullGrid[index, 0];
            var tween = toDestroy.transform.DOLocalMove(Vector3.down, 0.3f).SetRelative(true);
            sequence.Insert(0, tween);
            
            for (int i = 0; i < lastIndex; i++)
            {
                fullGrid[index, i] = fullGrid[index, i + 1];
                tween = fullGrid[index, i + 1].transform.DOLocalMove(Vector3.down, 0.3f).SetRelative(true);
                sequence.Insert(0, tween);
            }

            fullGrid[index, lastIndex] = CreateRandomChip();
            fullGrid[index, lastIndex].transform.localPosition = new(index, lastIndex + 1);
            var tween2 = fullGrid[index, lastIndex].transform.DOLocalMove(new(index, lastIndex), 0.3f)
                .OnComplete(() => Destroy(toDestroy.gameObject));
            sequence.Insert(0, tween2);
        }
        
        return sequence;
    }

    private Tween SwipeHorizontal(int index, bool right)
    {
        var sequence = DOTween.Sequence();
        if (right)
        {
            var lastIndex = fullGrid.GetLength(0) - 1;
            var toDestroy = fullGrid[lastIndex, index];
            var tween = toDestroy.transform.DOLocalMove(Vector3.right, 0.3f).SetRelative(true);
            sequence.Insert(0, tween);
            
            for (int i = lastIndex; i >= 1; i--)
            {
                fullGrid[i, index] = fullGrid[i - 1, index]; 
                tween = fullGrid[i - 1, index].transform.DOLocalMove(Vector3.right, 0.3f).SetRelative(true);
                sequence.Insert(0, tween);
            }
            
            fullGrid[0, index] = CreateRandomChip();
            fullGrid[0, index].transform.localPosition = new(-1, index);
            var tween2 = fullGrid[0, index].transform.DOLocalMove(new(0, index), 0.3f)
                .OnComplete(() => Destroy(toDestroy.gameObject));
            sequence.Insert(0, tween2);
        }
        else
        {
            var lastIndex = fullGrid.GetLength(0) - 1;
            var toDestroy = fullGrid[0, index];
            var tween = toDestroy.transform.DOLocalMove(Vector3.left, 0.3f).SetRelative(true);
            sequence.Insert(0, tween);
            
            for (int i = 0; i < lastIndex; i++)
            {
                fullGrid[i, index] = fullGrid[i + 1, index];
                tween = fullGrid[i + 1, index].transform.DOLocalMove(Vector3.left, 0.3f).SetRelative(true);
                sequence.Insert(0, tween);
            }
            
            fullGrid[lastIndex, index] = CreateRandomChip();
            fullGrid[lastIndex, index].transform.localPosition = new(lastIndex + 1, index);
            var tween2 = fullGrid[lastIndex, index].transform.DOLocalMove(new(lastIndex, index), 0.3f)
                .OnComplete(() => Destroy(toDestroy.gameObject));
            sequence.Insert(0, tween2);
        }
        
        return sequence;
    }

    private Tween CheckAndDestroyChips(HashSet<Vector2Int> indexes)
    {
        isGridAnimating = true;
        CheckAndDestroyChips(indexes, false);
        CheckAndDestroyChips(indexes, true);
        
        foreach (var index in indexes)
        {
            Destroy(grid[index.x, index.y].gameObject);
            grid[index.x, index.y] = null;
        }

        if (indexes.Count > 0)
        {
            Points += indexes.Count * 10;
            var tween = FillGrid();

            tween?.OnComplete(() =>
            {
                isGridAnimating = false;
                indexes.Clear();
                CheckAndDestroyChips(indexes);
            });
            
            return tween;
        }

        isGridAnimating = false;
        return null;
    }

    private Tween FillGrid()
    {
        var tween = DOTween.Sequence();
        
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                var target = grid[x, y];
                if (target != null)
                {
                    int yy = y;
                    bool needMove = false;
                    Vector3 pos = Vector3.negativeInfinity;
                    
                    for (int i = y - 1; i >= 0; i--)
                    {
                        if (grid[x, i] == null)
                        {
                            (grid[x, yy], grid[x, i]) = (grid[x, i], grid[x, yy]);
                            yy--;
                            pos = new Vector3(x + gridSizeOffset, i + gridSizeOffset);
                            needMove = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (needMove)
                    {
                        tween.Insert(0, target.transform.DOLocalMove(pos, 0.3f));
                    }
                }
            }

            int startY = 0;
            Action setPos = null;
            
            for (int y = grid.GetLength(1) - 1; y >= 0; y--)
            {
                var target = grid[x, y];
                if (target != null) break;

                startY = grid.GetLength(1) - y;
                grid[x, y] = CreateRandomChip();
                target = grid[x, y];
                var pos = new Vector3(x + gridSizeOffset, y + gridSizeOffset);
                
                setPos += () => 
                {
                    target.transform.localPosition = new(pos.x, pos.y + startY);
                    tween.Insert(0, target.transform.DOLocalMove(pos, 0.3f));
                };
            }

            setPos?.Invoke();
        }

        if (tween.Duration() > 0)
        {
            return tween;
        }
        
        return null;
    }
    
    private void CheckAndDestroyChips(HashSet<Vector2Int> indexes, bool checkRows)
    {
        int x;
        int y = 0;
        int xCount = checkRows ? grid.GetLength(1) : grid.GetLength(0);
        int yCount = checkRows ? grid.GetLength(0) : grid.GetLength(1);
        
        for (int i = 0; i < xCount; i++)
        {
            x = i;
            SpriteRenderer prev = null;
            Vector2Int range = new Vector2Int(int.MinValue, int.MinValue);
            
            for (int j = 0; j < yCount; j++)
            {
                x = i;
                y = j;
                if (checkRows) (x, y) = (y, x);
                
                if (grid[x, y].sprite == prev?.sprite)
                {
                    if (range.x == int.MinValue)
                    {
                        range.x = j - 1;
                    }

                    range.y = j;
                }
                else
                {
                    AddIndexes();
                    range = new Vector2Int(int.MinValue, int.MinValue);
                }

                prev = grid[x, y];
            }

            AddIndexes();

            void AddIndexes()
            {
                if (range.y - range.x > 1)
                {
                    range.y += 1;
                        
                    for (int k = range.x; k < range.y; k++)
                    {
                        var yc = k;
                        int xc = checkRows ? y : x;
                        if (checkRows) (xc, yc) = (yc, xc);
                        indexes.Add(new Vector2Int(xc, yc));
                    }
                }
            }
        }
    }
}