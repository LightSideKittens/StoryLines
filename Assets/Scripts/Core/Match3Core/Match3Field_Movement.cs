using DG.Tweening;
using UnityEngine;

public partial class Match3Field
{
    private Tween SwipeVertical(int index, bool up)
    {
        var sequence = DOTween.Sequence();
        if (up)
        {
            var lastIndex = fullGrid.GetLength(1) - 1;
            var toDestroy = fullGrid[index, lastIndex];
            var tween = toDestroy.transform.DOLocalMove(new (index, lastIndex+1), 0.3f);
            sequence.Insert(0, tween);
            
            for (int i = lastIndex; i >= 1; i--)
            {
                fullGrid[index, i] = fullGrid[index, i - 1];
                tween = fullGrid[index, i].transform.DOLocalMove(new (index, i), 0.3f);
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
            var tween = toDestroy.transform.DOLocalMove(new (index, -1), 0.3f);
            sequence.Insert(0, tween);
            
            for (int i = 0; i < lastIndex; i++)
            {
                fullGrid[index, i] = fullGrid[index, i + 1];
                tween = fullGrid[index, i].transform.DOLocalMove(new (index, i), 0.3f);
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
            var tween = toDestroy.transform.DOLocalMove(new (lastIndex+1, index), 0.3f);
            sequence.Insert(0, tween);
            
            for (int i = lastIndex; i >= 1; i--)
            {
                fullGrid[i, index] = fullGrid[i - 1, index]; 
                tween = fullGrid[i, index].transform.DOLocalMove(new(i, index), 0.3f);
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
            var tween = toDestroy.transform.DOLocalMove(new(-1, index), 0.3f);
            sequence.Insert(0, tween);
            
            for (int i = 0; i < lastIndex; i++)
            {
                fullGrid[i, index] = fullGrid[i + 1, index];
                tween = fullGrid[i, index].transform.DOLocalMove(new(i, index), 0.3f);
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
    
    private void DragVertical(int index, Vector2 delta)
    {
        var lastIndex = fullGrid.GetLength(1) - 1;
            
        for (int i = lastIndex; i >= 1; i--)
        {
            var pos = new Vector2(index, i);
            fullGrid[index, i].transform.localPosition = pos + delta;
        }
    }

    private void DragHorizontal(int index, Vector2 delta)
    {
        var lastIndex = fullGrid.GetLength(0) - 1;
            
        for (int i = lastIndex; i >= 1; i--)
        {
            var pos = new Vector2(i, index);
            fullGrid[i, index].transform.localPosition = pos + delta;
        }
    }
}