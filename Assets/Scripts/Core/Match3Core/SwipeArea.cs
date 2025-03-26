using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using UnityEngine;

public class SwipeArea : MonoBehaviour
{
    public enum SwipeDirection
    {
        None = 0,
        Up,
        Down,
        Right,
        Left,
    }
    
    public float threshold = 10f;
    public bool isVertical;
    public int index;
    
    private bool isDragging = false;
    private Vector3 offset;
    private Vector2 startTouchPos;
    private SwipeDirection currentSwipeDirection = SwipeDirection.None;
    public event Action<int, SwipeDirection, Vector2> Dragging;
    public event Action<int, SwipeDirection, Vector2> Swiped;
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
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        var delta = touch.position - startTouchPos;
                        
                        if (currentSwipeDirection == SwipeDirection.None)
                        {
                            if (delta.magnitude > threshold)
                            {
                                startTouchPos = touch.position;
                                currentSwipeDirection = GetSwipeDirection(delta);
                            }
                        }
                        else
                        {
                            Dragging?.Invoke(index, currentSwipeDirection, ClampDeltaByDirection(delta, currentSwipeDirection));
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    if (isDragging)
                    {
                        var dir = touch.position - startTouchPos;
                        Swiped?.Invoke(index, currentSwipeDirection, ClampDeltaByDirection(dir, currentSwipeDirection));
                        isDragging = false;
                        currentSwipeDirection = SwipeDirection.None;
                    }

                    break;
                case TouchPhase.Canceled:
                    currentSwipeDirection = SwipeDirection.None;
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

    private Vector2 ClampDeltaByDirection(Vector2 delta, SwipeDirection swipeDirection)
    {
        if (swipeDirection == SwipeDirection.None)
        {
            return delta;
        }
        
        if (swipeDirection is SwipeDirection.Up or SwipeDirection.Down)
        {
            delta.x = 0;
        }
        else
        {
            delta.y = 0;
        }
        
        return delta;
    }
}
