using System;
using System.Collections.Generic;
using System.Linq;
using Core.Match3Core;
using DG.Tweening;
using LSCore;
using LSCore.Async;
using LSCore.DataStructs;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
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
    public SpriteRenderer fadePrefab;
    public Vector2 maskSizeOffset;
    public int gridSizeOffset = 5;
    public int pointsToWin = 100;

    private SpriteRenderer[] fades = new SpriteRenderer[4];
    private bool isGridAnimating;
    private GameObject fieldParent;
    private Vector2Int realGridSize;
    private SpriteRenderer[,] fullGrid;
    private ArraySpan<SpriteRenderer> grid;
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
    
    private static Match3Field instance;
    private Vector3 fieldParentScale;
    
    private void Awake()
    {
        fieldParent = new GameObject("FieldParent");
        instance = this;
        
        Match3Window.Show();
        Steps = 0;
        realGridSize = gridSize;
        gridSize.x += gridSizeOffset * 2;
        gridSize.y += gridSizeOffset * 2;
        
        fullGrid = new SpriteRenderer[gridSize.x, gridSize.y];
        grid = fullGrid.ToSpan(gridSizeOffset..(gridSize.x - gridSizeOffset), gridSizeOffset..(gridSize.y - gridSizeOffset));

        
        InitField();
        transform.position = -new Vector3(gridSize.x / 2f - 0.5f, gridSize.y / 2f - 0.5f);
        transform.SetParent(fieldParent.transform);
        for (var i = 0; i < masks.Length; i++)
        {
            masks[i].transform.SetParent(fieldParent.transform);
            masks[i].size = new Vector2(realGridSize.x / 2f, realGridSize.y) + maskSizeOffset;
        }
        

        var cam = Camera.main;
        fieldParent.transform.localScale *= (cam.aspect * 2 * (cam.orthographicSize * 0.9f)) / (realGridSize.x);
        fieldParentScale = fieldParent.transform.localScale;
    }

    private void OnDestroy()
    {
        Match3Window.Destroy();
        new GoBack().Invoke();
    }

    private Sequence expandSequence;
    private float lastFactor;
    
    public void ExpandMaskSize(Vector2Int expand)
    {
        var factor = (float)realGridSize.x / (realGridSize.x + expand.x);
        
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
            sequence.Append(SetHintChipsAlpha(1, 0));
            sequence.Append(fieldParent.transform.DOScale(fieldParentScale * factor, .3f));
            for (var i = 0; i < masks.Length; i++)
            {
                sequence.Insert(0.3f, masks[i].DOSize(new Vector2((realGridSize.x + expand.x) / 2f, realGridSize.y + expand.y) + maskSizeOffset, 0.3f));
            }

            sequence.Append(SetHintChipsAlpha(0.85f, 0.5f));
            
            return sequence.SetAutoKill(false);
        }
    }

    private Tween SetHintChipsAlpha(float alpha, float duration)
    {
        var sequence = DOTween.Sequence();
        
        for (var i = 0; i < fades.Length; i++)
        {
            sequence.Insert(0, fades[i].DOFade(alpha, duration));
        }
        
        return sequence;
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
        
        float ex = (realGridSize.x + 1) / 2f;
        float ey = (realGridSize.y + 1) / 2f;
        
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-ex, 0),
            new Vector2(ex, 0),
            new Vector2(0, -ey),
            new Vector2(0, ey),
        };
        
        for (int i = 0; i < 2; i++)
        {
            var fade = Instantiate(fadePrefab, fieldParent.transform);
            fade.transform.localPosition = positions[i];
            fades[i] = fade;
        }
        
        for (int i = 2; i < 4; i++)
        {
            var fade = Instantiate(fadePrefab, Vector3.zero, Quaternion.Euler(0, 0, 90), fieldParent.transform);
            fade.transform.localPosition = positions[i];
            fades[i] = fade;
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
            dragger.Dragging += OnDragging;
            dragger.Swiped += OnEnd;
        }

        void OnDragging(int index, SwipeArea.SwipeDirection direction, Vector2 delta)
        {
            var data = (index, direction);
            if(currentDragger != default && currentDragger != data) return;

            endTween?.Complete();
            currentDragger = data;
            delta = GetWorldDelta(delta);
            
            if (direction is SwipeArea.SwipeDirection.Up or SwipeArea.SwipeDirection.Down)
            {
                DragVertical(index, ElasticClamp(delta));
            }
            else
            {
                DragHorizontal(index, ElasticClamp(delta));
            }
            Vector2 ElasticClamp(Vector2 value)
            {
                value.x = SpringFunction(value.x);
                value.y = SpringFunction(value.y);

                return value;
                
                float SpringFunction(float x)
                {
                    float maxExtension = 2.0f;
                    float k = 0.8f; 

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
            var data = (index, direction);
            
            if(currentDragger != default && currentDragger != data) return;
            
            endTween = OnEndAnim(index, delta);
            
            if (endTween == null)
            {
                isGridAnimating = true;
                delta = GetWorldDelta(delta);
            
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
            }

            endTween.onComplete += () =>
            {
                currentDragger = default;
                isGridAnimating = false;
            };
        }

        Tween OnEndAnim(int index, Vector2 delta)
        {
            delta = GetWorldDelta(delta);
            if (Mathf.Abs(delta.x) < 0.5f && Mathf.Abs(delta.y) < 0.5f) return null;
            if(isGridAnimating) return null;
            
            Tween tween = null;

            if (delta.x > 0) tween = SwipeHorizontal(index, true);
            if (delta.x < 0) tween = SwipeHorizontal(index, false);
            if (delta.y > 0) tween = SwipeVertical(index, true);
            if (delta.y < 0) tween = SwipeVertical(index, false);

            Steps++;
            isGridAnimating = true;
            tween.OnComplete(() =>
            {
                var set = new HashSet<Vector2Int>();
                CheckAndDestroyChips(set);
            });
            
            return tween;
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
        chip.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        return chip;
    }
    

    private Tween CheckAndDestroyChips(HashSet<Vector2Int> indexes)
    {
        isGridAnimating = true;
        CheckAndDestroyChips(indexes, false);
        CheckAndDestroyChips(indexes, true);
        
        foreach (var index in indexes)
        {
            var target = grid[index.x, index.y];
            
            if (GoalView.Goals.TryGetValue(target.sprite, out var goal))
            {
                goal.Count++;
            }
            
            Destroy(target.gameObject);
            grid[index.x, index.y] = null;
        }

        var isWin = GoalView.Goals.All(x => x.Value.IsReached);

        if (isWin)
        {
            Match3Window.Win();
        }

        if (indexes.Count > 0)
        {
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