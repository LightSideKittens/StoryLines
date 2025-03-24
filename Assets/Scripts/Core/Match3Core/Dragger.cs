using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using UnityEngine;

public class Dragger : MonoBehaviour
{
    public enum SwipeDirection
    {
        None = 0,
        Up,
        Down,
        Right,
        Left,
    }
    
    public float threshold = 100f;
    public bool isVertical;
    public int index;
    
    private bool isDragging = false;
    private Vector3 offset;
    private Vector2 startTouchPos;
    public event Action Started;
    public event Action<int, SwipeDirection> Ended;
    private Collider2D col2D;

    private void Start()
    {
        col2D = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPos = touch.position;
                    CheckTouch(touchPosition);
                    if (isDragging)
                    {
                        Started?.Invoke();
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        var dir = touch.position - startTouchPos;

                        if (dir.magnitude > threshold)
                        {
                            isDragging = false;
                            Ended?.Invoke(index, GetSwipeDirection(dir));
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
    }
    
    
    private void CheckTouch(Vector3 touchPosition)
    {
        var hitCollider = new HashSet<Collider2D>(Physics2DExt.FindAll(touchPosition));
        
        if (hitCollider.Contains(col2D))
        {
            isDragging = true;
            offset = transform.position - touchPosition;
        }
    }

    public SwipeDirection GetSwipeDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (!isVertical)
            {
                return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            
            return SwipeDirection.None;
        }

        if (isVertical)
        {
            return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }

        return SwipeDirection.None;
    }
}
