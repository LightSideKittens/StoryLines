using System.Collections.Generic;
using UnityEngine;

public partial class Match3Field
{
    private void SwipeVertical(int index, ref Vector2 delta)
    {
        var list = new List<SpriteRenderer>();
        for (int i = 0; i < fullGrid.GetLength(1); i++)
        {
            list.Add(fullGrid[index, i]);
            fullGrid[index, i] = null;
        }
        
        for (var i = 0; i < list.Count; i++)
        {
            var t = list[i];
            var pos = t.transform.localPosition;
            var y = Mathf.RoundToInt(pos.y);
            if (y >= 0 && y < fullGrid.GetLength(1))
            {
                delta.y = pos.y - y;
                fullGrid[index, y] = t;
            }
            else
            {
                Destroy(t.gameObject);
            }
        }
        
        for (int i = 0; i < fullGrid.GetLength(1); i++)
        {
            if (fullGrid[index, i] == null)
            {
                fullGrid[index, i] = CreateRandomChip();
            }
        }
    }

    private void SwipeHorizontal(int index, ref Vector2 delta)
    {
        var list = new List<SpriteRenderer>();
        for (int i = 0; i < fullGrid.GetLength(0); i++)
        {
            list.Add(fullGrid[i, index]);
            fullGrid[i, index] = null;
        }
        
        for (var i = 0; i < list.Count; i++)
        {
            var t = list[i];
            var pos = t.transform.localPosition;
            var x = Mathf.RoundToInt(pos.x);
            if (x >= 0 && x < fullGrid.GetLength(0))
            {
                delta.x = pos.x - x;
                fullGrid[x, index] = t;
            }
            else
            {
                Destroy(t.gameObject);
            }
        }
        
        for (int i = 0; i < fullGrid.GetLength(0); i++)
        {
            if (fullGrid[i, index] == null)
            {
                fullGrid[i, index] = CreateRandomChip();
            }
        }
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