using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Extensions;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public Vector2Int gridSize;
    public List<SpriteRenderer> chips;
    public SpriteMask mask;
    private SpriteRenderer[,] grid;
    
    private void Awake()
    {
        grid = new SpriteRenderer[gridSize.x, gridSize.y];
        InitField();
        transform.position = -new Vector3(gridSize.x / 2f - 0.5f, gridSize.y / 2f - 0.5f);
        mask.transform.localScale = new Vector3(gridSize.x, gridSize.y, 1);
    }

    private void InitField()
    {
        for (var x = 0; x < grid.GetLength(0); x++)
        {
            for (var y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = CreateRandomChip();
                grid[x, y].transform.position = new Vector3(x, y);
            }
        }
        
        var draggers = new List<Dragger>();
        
        for (var x = 0; x < grid.GetLength(0); x++)
        {
            var go = new GameObject($"Dragger Vertical {x}");
            var boxCollider = go.AddComponent<BoxCollider2D>();
            var dragger = go.AddComponent<Dragger>();
            draggers.Add(dragger);
            dragger.transform.SetParent(transform);
            dragger.transform.localPosition = Vector3.zero;
            dragger.index = x;
            dragger.isVertical = true;
            
            boxCollider.size = new Vector2(1, gridSize.y);
            boxCollider.offset = new Vector3(x, gridSize.y / 2f - 0.5f);
        }
        
        for (var y = 0; y < grid.GetLength(1); y++)
        {
            var go = new GameObject($"Dragger Horizontal {y}");
            var boxCollider = go.AddComponent<BoxCollider2D>();
            var dragger = go.AddComponent<Dragger>();
            draggers.Add(dragger);
            dragger.transform.SetParent(transform);
            dragger.transform.localPosition = Vector3.zero;
            dragger.index = y;
            dragger.isVertical = false;
            
            boxCollider.size = new Vector2(gridSize.x, 1);
            boxCollider.offset = new Vector3(gridSize.x / 2f - 0.5f, y);
        }
        
        foreach (var dragger in draggers)
        {
            dragger.Ended += OnEnd;
        }
        
        void OnEnd(int index, Dragger.SwipeDirection direction)
        {
            Tween tween = direction switch
            {
                Dragger.SwipeDirection.Up => SwipeVertical(index, true),
                Dragger.SwipeDirection.Down => SwipeVertical(index, false),
                Dragger.SwipeDirection.Right => SwipeHorizontal(index, true),
                Dragger.SwipeDirection.Left => SwipeHorizontal(index, false),
                Dragger.SwipeDirection.None => null,
            };

            if (direction != Dragger.SwipeDirection.None)
            {
                tween.OnComplete(() =>
                {
                    var set = new HashSet<Vector2Int>();
                    CheckAndDestroyChips(set);
                });
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
            var lastIndex = grid.GetLength(1) - 1;
            var toDestroy = grid[index, lastIndex];
            var tween = toDestroy.transform.DOLocalMove(Vector3.up, 0.3f).SetRelative(true);
            sequence.Insert(0, tween);
            
            for (int i = lastIndex; i >= 1; i--)
            {
                grid[index, i] = grid[index, i - 1];
                tween = grid[index, i - 1].transform.DOLocalMove(Vector3.up, 0.3f).SetRelative(true);
                sequence.Insert(0, tween);
            }
            
            grid[index, 0] = CreateRandomChip();
            grid[index, 0].transform.localPosition = new(index, -1);
            var tween2 = grid[index, 0].transform.DOLocalMove(new(index, 0), 0.3f)
                .OnComplete(() => Destroy(toDestroy.gameObject));
            sequence.Insert(0, tween2);
        }
        else
        {
            var lastIndex = grid.GetLength(1) - 1;
            var toDestroy = grid[index, 0];
            var tween = toDestroy.transform.DOLocalMove(Vector3.down, 0.3f).SetRelative(true);
            sequence.Insert(0, tween);
            
            for (int i = 0; i < lastIndex; i++)
            {
                grid[index, i] = grid[index, i + 1];
                tween = grid[index, i + 1].transform.DOLocalMove(Vector3.down, 0.3f).SetRelative(true);
                sequence.Insert(0, tween);
            }

            grid[index, lastIndex] = CreateRandomChip();
            grid[index, lastIndex].transform.localPosition = new(index, lastIndex + 1);
            var tween2 = grid[index, lastIndex].transform.DOLocalMove(new(index, lastIndex), 0.3f)
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
            var lastIndex = grid.GetLength(0) - 1;
            var toDestroy = grid[lastIndex, index];
            var tween = toDestroy.transform.DOLocalMove(Vector3.right, 0.3f).SetRelative(true);
            sequence.Insert(0, tween);
            
            for (int i = lastIndex; i >= 1; i--)
            {
                grid[i, index] = grid[i - 1, index]; 
                tween = grid[i - 1, index].transform.DOLocalMove(Vector3.right, 0.3f).SetRelative(true);
                sequence.Insert(0, tween);
            }
            
            grid[0, index] = CreateRandomChip();
            grid[0, index].transform.localPosition = new(-1, index);
            var tween2 = grid[0, index].transform.DOLocalMove(new(0, index), 0.3f)
                .OnComplete(() => Destroy(toDestroy.gameObject));
            sequence.Insert(0, tween2);
        }
        else
        {
            var lastIndex = grid.GetLength(0) - 1;
            var toDestroy = grid[0, index];
            var tween = toDestroy.transform.DOLocalMove(Vector3.left, 0.3f).SetRelative(true);
            sequence.Insert(0, tween);
            
            for (int i = 0; i < lastIndex; i++)
            {
                grid[i, index] = grid[i + 1, index];
                tween = grid[i + 1, index].transform.DOLocalMove(Vector3.left, 0.3f).SetRelative(true);
                sequence.Insert(0, tween);
            }
            
            grid[lastIndex, index] = CreateRandomChip();
            grid[lastIndex, index].transform.localPosition = new(lastIndex + 1, index);
            var tween2 = grid[lastIndex, index].transform.DOLocalMove(new(lastIndex, index), 0.3f)
                .OnComplete(() => Destroy(toDestroy.gameObject));
            sequence.Insert(0, tween2);
        }
        
        return sequence;
    }

    private void CheckAndDestroyChips(HashSet<Vector2Int> indexes)
    {
        CheckAndDestroyChips(indexes, false);
        CheckAndDestroyChips(indexes, true);
        
        foreach (var index in indexes)
        {
            Destroy(grid[index.x, index.y].gameObject);
            grid[index.x, index.y] = null;
        }

        FillGrid();
    }

    private void FillGrid()
    {
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
                            pos = new Vector3(x, i);
                            needMove = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (needMove)
                    {
                        target.transform.DOLocalMove(pos, 0.3f);
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
                var pos = new Vector3(x, y);
                
                setPos += () =>
                {
                    target.transform.localPosition = new(pos.x, pos.y + startY);
                    target.transform.DOLocalMove(pos, 0.3f);
                };
            }

            setPos?.Invoke();
        }
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