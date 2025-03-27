using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Match3Core;
using DG.Tweening;
using LSCore;
using LSCore.Async;
using LSCore.DataStructs;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.Utilities;
using UnityEngine;

public partial class Match3Field : MonoBehaviour 
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
    public int maxStepsCount = 20;
    public int gridSizeOffset = 3;
    public ParticleSystem fx;

    private HashSet<Vector2Int> expandedMaskIndexes = new();
    private HashSet<Vector2Int> outerMaskIndexes = new();
    private HashSet<Vector2Int> playGridIndexes = new();
    private bool isGridAnimating;
    private GameObject fieldParent;
    private Vector2Int playGridSize;
    private SpriteRenderer[,] fullGrid;
    private ArraySpan<SpriteRenderer> grid;
    private int steps;

    private int Steps
    {
        get => steps;
        set
        {
            steps = value;
            var left = maxStepsCount - steps;
            Match3Window.MovesText.text = "Moves left: " + left;

            if (left <= 0)
            {
                Match3Window.Lose();
            }
        }
    }
    
    private static Match3Field instance;
    private Vector3 fieldParentScale;
    
    private void Awake()
    {
        fieldParent = new GameObject("FieldParent");
        instance = this;
        
        Match3Window.Show();
        Steps = 0;
        playGridSize = gridSize;
        gridSize.x += gridSizeOffset * 2;
        gridSize.y += gridSizeOffset * 2;
        
        fullGrid = new SpriteRenderer[gridSize.x, gridSize.y];
        grid = fullGrid.ToSpan(gridSizeOffset..(gridSize.x - gridSizeOffset), gridSizeOffset..(gridSize.y - gridSizeOffset));
        
        InitExpandedIndexes();
        InitOuterMaskIndexes();
        InitField();
        
        SetFade(expandedMaskIndexes, 0);
        SetFade(outerMaskIndexes, 0);
        SetFade(playGridIndexes, 1);
        
        transform.position = -new Vector3(gridSize.x / 2f - 0.5f, gridSize.y / 2f - 0.5f);
        transform.SetParent(fieldParent.transform);
        for (var i = 0; i < masks.Length; i++)
        {
            masks[i].transform.SetParent(fieldParent.transform);
            masks[i].size = new Vector2(playGridSize.x / 2f, playGridSize.y) + maskSizeOffset;
        }
        
        var cam = Camera.main;
        fieldParent.transform.localScale *= (cam.aspect * 2 * (cam.orthographicSize * 0.9f)) / (playGridSize.x);
        fieldParentScale = fieldParent.transform.localScale;
    }

    private void OnDestroy()
    {
       new GoBack().Invoke();
    }

    private void InitExpandedIndexes()
    {
        HashSet<Vector2Int> outline = expandedMaskIndexes;

        int startRow = gridSizeOffset - 1;
        int startCol = gridSizeOffset - 1;
        int endRow = startRow + playGridSize.x + 1;
        int endCol = startCol + playGridSize.y + 1;
        
        for (int c = startCol; c <= endCol; c++)
            outline.Add(new Vector2Int(startRow, c));

        for (int r = startRow + 1; r < endRow; r++)
            outline.Add(new Vector2Int(r, endCol));
        
        for (int c = endCol; c >= startCol; c--)
            outline.Add(new Vector2Int(endRow, c));

        for (int r = endRow - 1; r > startRow; r--)
            outline.Add(new Vector2Int(r, startCol));
    }
    
    private void InitOuterMaskIndexes()
    {
        HashSet<Vector2Int> outline = outerMaskIndexes;

        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                outline.Add(new Vector2Int(i, j));
            }
        }
        
        for (int i = gridSizeOffset; i < gridSizeOffset + playGridSize.x; i++)
        {
            for (int j = gridSizeOffset; j < gridSizeOffset + playGridSize.y; j++)
            {
                var index = new Vector2Int(i, j);
                playGridIndexes.Add(index);
                outline.Remove(index);
            }
        }
        
        playGridIndexes.ExceptWith(expandedMaskIndexes);
        outline.ExceptWith(expandedMaskIndexes);
    }

    private void SetFade(HashSet<Vector2Int> indexes, float fade)
    {
        foreach (var index in indexes)
        {
            fullGrid[index.x, index.y].Alpha(fade);
        }
    }

    private Sequence expandSequence;
    private float lastFactor;
    
    public void ExpandMaskSize(Vector2Int expand)
    {
        var factor = (float)playGridSize.x / (playGridSize.x + expand.x);
        
        if (expand.x == 0)
        {
            expandSequence.Goto(expandSequence.Duration());
            expandSequence.PlayBackwards();
        }
        else
        {
            expandSequence = GetAnim();
        }

        Sequence GetAnim()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(SetHintChipsAlpha(expandedMaskIndexes, 0, 0, 0));
            sequence.Append(fieldParent.transform.DOScale(fieldParentScale * factor, .3f));
            
            for (var i = 0; i < masks.Length; i++)
            {
                sequence.Insert(0.3f, masks[i].DOSize(new Vector2((playGridSize.x + expand.x) / 2f, playGridSize.y + expand.y) + maskSizeOffset, 0.3f));
            }

            sequence.Append(SetHintChipsAlpha(expandedMaskIndexes, 0, 0.15f, 0.5f));
            
            return sequence.SetAutoKill(false);
        }
    }

    private Tween SetHintChipsAlpha(HashSet<Vector2Int> indexes, float from, float to, float duration)
    {
        return Wait.FromTo(from, to, duration, x =>
        {
            SetFade(indexes, x);
        });
    }

    private Tween endTween;
    private (int index, SwipeArea.SwipeDirection direction) currentDragger;

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
        
        var swipeAreas = new List<SwipeArea>();
        
        for (var x = gridSizeOffset; x < fullGrid.GetLength(0) - gridSizeOffset; x++)
        {
            var go = new GameObject($"Dragger Vertical {x}");
            var boxCollider = go.AddComponent<BoxCollider2D>();
            var swipeArea = go.AddComponent<SwipeArea>();
            swipeAreas.Add(swipeArea);
            swipeArea.transform.SetParent(transform);
            swipeArea.transform.localPosition = Vector3.zero;
            swipeArea.index = x;
            swipeArea.isVertical = true;
            
            boxCollider.size = new Vector2(1, playGridSize.y);
            boxCollider.offset = new Vector3(x, playGridSize.y / 2f - 0.5f + gridSizeOffset);
        }
        
        for (var y = gridSizeOffset; y < fullGrid.GetLength(1) - gridSizeOffset; y++)
        {
            var go = new GameObject($"Dragger Horizontal {y}");
            var boxCollider = go.AddComponent<BoxCollider2D>();
            var swipeArea = go.AddComponent<SwipeArea>();
            swipeAreas.Add(swipeArea);
            swipeArea.transform.SetParent(transform);
            swipeArea.transform.localPosition = Vector3.zero;
            swipeArea.index = y;
            swipeArea.isVertical = false;
            
            boxCollider.size = new Vector2(playGridSize.x, 1);
            boxCollider.offset = new Vector3(playGridSize.x / 2f - 0.5f + gridSizeOffset, y);
        }
        
        foreach (var dragger in swipeAreas)
        {
            dragger.Dragging += OnDragging;
            dragger.Swiped += OnEnd;
        }

        void OnDragging(int index, SwipeArea.SwipeDirection direction, Vector2 delta)
        {
            if(isGridAnimating) return;
            var data = (index, direction);
            if(currentDragger != default && currentDragger != data) return;

            endTween?.Complete();
            currentDragger = data;
            delta = GetWorldDelta(delta);
            
            if (direction is SwipeArea.SwipeDirection.Up or SwipeArea.SwipeDirection.Down)
            {
                DragVertical(index, delta);
            }
            else
            {
                DragHorizontal(index, delta);
            }
            Vector2 ElasticClamp(Vector2 value)
            {
                value.x = SpringFunction(value.x);
                value.y = SpringFunction(value.y);

                return value;
                
                float SpringFunction(float x)
                {
                    float maxExtension = 2.0f;
                    float k = 0.2f; 

                    if (Math.Abs(x) <= 1.0f)
                    {
                        return x;
                    }
                    if (x > 1)
                    {
                        return 1 + (maxExtension - 1) * (1 - Mathf.Exp(-k * (x - 1)));
                    }
                    
                    return -1 - (maxExtension - 1) * (1 - Mathf.Exp(-k * (-x - 1)));
                }
            }
        }

        void OnEnd(int index, SwipeArea.SwipeDirection direction, Vector2 delta)
        {
            if(isGridAnimating) return;
            var data = (index, direction);
            
            if(currentDragger != default && currentDragger != data) return;
            
            isGridAnimating = true;
            Swipe(index, ref delta);
            
            if (direction is SwipeArea.SwipeDirection.Up or SwipeArea.SwipeDirection.Down)
            {
                endTween = Wait.InverseRun(0.3f, x =>
                {
                    DragVertical(index, delta * x);
                }).SetEase(Ease.InOutSine);
            }
            else
            {
                endTween = Wait.InverseRun(0.3f, x =>
                {
                    DragHorizontal(index, delta * x);
                }).SetEase(Ease.InOutSine);
            }
                
            endTween.onComplete += () =>
            {
                var sets = new List<HashSet<Vector2Int>>();
                CheckAndDestroyChips(sets);
                var set = new HashSet<Vector2Int>();
                
                for (int i = 0; i < sets.Count; i++)
                {
                    set.AddRange(sets[i]);
                }
                
                if (direction is SwipeArea.SwipeDirection.Up or SwipeArea.SwipeDirection.Down)
                {
                    for (int i = 0; i < gridSize.y; i++)
                    {
                        FadeChip(new Vector2Int(index, i), set);
                    }
                }
                else
                {
                    for (int i = 0; i < gridSize.x; i++)
                    {
                        FadeChip(new Vector2Int(i, index), set);
                    }
                }
            };
        }

        void Swipe(int index, ref Vector2 delta)
        {
            delta = GetWorldDelta(delta);
            if (Mathf.Abs(delta.x) < 0.5f && Mathf.Abs(delta.y) < 0.5f) return;

            if (delta.x != 0) SwipeHorizontal(index, ref delta);
            if (delta.y != 0) SwipeVertical(index, ref delta);

            Steps++;
        }
    }

    private void FadeChip(Vector2Int pos, HashSet<Vector2Int> ignored)
    {
        if(ignored != null && ignored.Contains(pos)) return;
                    
        if (expandedMaskIndexes.Contains(pos))
        {
            float fade = 0;
                        
            if (expandSequence != null)
            {
                fade = expandSequence.isBackwards ? 0 : 0.15f;
            }
                        
            fullGrid[pos.x, pos.y].DOFade(fade, 0.3f);
        }
        else if(playGridIndexes.Contains(pos))
        {
            fullGrid[pos.x, pos.y].DOFade(1f, 0.3f);
        }
        else if(outerMaskIndexes.Contains(pos))
        {
            fullGrid[pos.x, pos.y].Alpha(0);
        }
    }

    private Vector2 GetWorldDelta(Vector2 delta)
    {
        delta = Camera.main.ScreenToWorldDelta(delta);
        delta = transform.localScale * delta;
        return delta;
    }

    private SpriteRenderer GetRandomChip() => chips.Random();
    private SpriteRenderer CreateRandomChip()
    {
        var chip = Instantiate(GetRandomChip(), transform);
        chip.Alpha(0);
        chip.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        return chip;
    }

    private void CheckAndDestroyChips()
    {
        CheckAndDestroyChips(new List<HashSet<Vector2Int>>());
    }

    private async void CheckAndDestroyChips(List<HashSet<Vector2Int>> sets)
    {
        isGridAnimating = true;
        sets.AddRange(CheckAndDestroyChips(false));
        sets.AddRange(CheckAndDestroyChips(true));
        
        await Dest();

        var isWin = GoalView.Goals.All(x => x.Value.IsReached);

        if (isWin)
        {
            Match3Window.Win();
        }
        
        if (sets.Count > 0)
        {
            FillOnDestroyEnded();
        }
        else
        {
            currentDragger = default;
            isGridAnimating = false;
        }

        async Task Dest()
        {
            if(sets.Count == 0) return;
            
            for (var i = 0; i < sets.Count - 1; i++)
            {
                Des(sets[i]);   
                await Task.Delay(500);
            }
            
            Des(sets[^1]);
            
            void Des(HashSet<Vector2Int> set)
            {
                if (set.Count > 0)
                {
                    foreach (var index in set)
                    {
                        var target = grid[index.x, index.y];
            
                        if (GoalView.Goals.TryGetValue(target.sprite, out var goal))
                        {
                            goal.Count++;
                        }

                        Instantiate(fx, target.transform.position, Quaternion.identity);
                        Destroy(target.gameObject);
                        grid[index.x, index.y] = null;
                    }
                }
            }
        }
    }

    private Tween FillOnDestroyEnded()
    {
        var tween = FillGrid();
        tween?.OnComplete(CheckAndDestroyChips);
        return tween;
    }
    
    private Tween FillGrid()
    {
        var tween = DOTween.Sequence();
        
        for (int x = 0; x < fullGrid.GetLength(0); x++)
        {
            for (int y = 0; y < fullGrid.GetLength(1); y++)
            {
                var target = fullGrid[x, y];
                if (target != null)
                {
                    int yy = y;
                    bool needMove = false;
                    Vector3 pos = Vector3.negativeInfinity;
                    
                    for (int i = y - 1; i >= 0; i--)
                    {
                        if (fullGrid[x, i] == null)
                        {
                            (fullGrid[x, yy], fullGrid[x, i]) = (fullGrid[x, i], fullGrid[x, yy]);
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
                        FadeChip(new Vector2Int((int)pos.x, (int)pos.y), null);
                        tween.Insert(0, target.transform.DOLocalMove(pos, 0.3f));
                    }
                }
            }

            int startY = 0;
            Action setPos = null;
            
            for (int y = fullGrid.GetLength(1) - 1; y >= 0; y--)
            {
                var target = fullGrid[x, y];
                if (target != null) break;

                startY = fullGrid.GetLength(1) - y;
                fullGrid[x, y] = CreateRandomChip();
                FadeChip(new Vector2Int(x, y), null);
                target = fullGrid[x, y];
                var pos = new Vector3(x, y);
                
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
    
    private List<HashSet<Vector2Int>> CheckAndDestroyChips(bool checkRows)
    {
        var list = new List<HashSet<Vector2Int>>();
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
                    var indexes = new HashSet<Vector2Int>();
                    for (int k = range.x; k < range.y; k++)
                    {
                        var yc = k;
                        int xc = checkRows ? y : x;
                        if (checkRows) (xc, yc) = (yc, xc);
                        indexes.Add(new Vector2Int(xc, yc));
                    }
                    list.Add(indexes);
                }
            }
        }
        
        for (var i = 0; i < list.Count - 1; i++)
        {
            var set1 = list[i];
            var set2 = list[i + 1];

            if (set1.Overlaps(set2))
            {
                set1.UnionWith(set2);
                list.RemoveAt(i + 1);
                i--;
            }
        }
        
        return list;
    }
}