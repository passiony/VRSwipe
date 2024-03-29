using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum SwipeDirection
{
    None = -1,
    Left = 0,
    Right = 1,
    Up = 2,
    Down = 3,
    Circle = 4
}

public class MagicWand : MonoBehaviour
{
    public InputActionProperty triggerAction;
    public Transform point;

    private List<Vector3> PointList = new List<Vector3>();
    private bool isTriggerPressed = false;
    private float angleThreshold = 10f;
    private float startTime;

    public UnityEvent<SwipeDirection> OnSwipe;

    private void OnEnable()
    {
        triggerAction.action.started += OnTriggerStart;
        triggerAction.action.canceled += OnTriggerEnd;
    }

    private void OnDisable()
    {
        triggerAction.action.started -= OnTriggerStart;
        triggerAction.action.canceled -= OnTriggerEnd;
    }

    private void OnTriggerStart(InputAction.CallbackContext obj)
    {
        isTriggerPressed = true;
        PointList.Clear();
        startTime = 0;
    }

    private void Update()
    {
        if (isTriggerPressed)
        {
            //每隔0.1s，加入一次position
            if (Time.time - startTime > 0.1f)
            {
                startTime = Time.time;
                PointList.Add(point.position);
            }
        }
    }

    private void OnTriggerEnd(InputAction.CallbackContext obj)
    {
        isTriggerPressed = false;

        var points = PointList.ToArray();
        bool isCircle = CheckCircularPath(points);
        if (isCircle)
        {
            Debug.LogWarning("Swip Circle");
            OnSwipe?.Invoke(SwipeDirection.Circle);
            return;
        }

        var swipeDirection = DetectSwipe(points);
        OnSwipe?.Invoke(swipeDirection);
        Debug.LogWarning("Swip" + swipeDirection);
    }

    bool CheckCircularPath(Vector3[] points)
    {
        if (points.Length < 3) return false; // 至少需要3个点形成路径

        Vector3 center = GetPathCenter(points);
        float totalAngle = 0f;

        // 计算路径上相邻点之间的角度差
        for (int i = 0; i < points.Length - 2; i++)
        {
            Vector3 p1 = points[i] - center;
            Vector3 p2 = points[i + 1] - center;
            Vector3 p3 = points[i + 2] - center;

            float angle = Vector3.Angle(p1, p2) + Vector3.Angle(p2, p3);
            totalAngle += angle;
        }

        Debug.Log(totalAngle);
        // 判断总角度是否接近360度（圆形的角度总和）
        return Mathf.Abs(totalAngle - 360f) >= 300;
    }

    // 获取路径的中心点
    Vector3 GetPathCenter(Vector3[] points)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in points)
        {
            sum += point;
        }

        return sum / points.Length;
    }

    public SwipeDirection DetectSwipe(Vector3[] points)
    {
        if (points.Length < 2)
            return SwipeDirection.None;

        // 计算第一个向量
        Vector2 firstVector = points[1] - points[0];
        SwipeDirection swipeDirection = GetSwipeDirection(firstVector);

        // 遍历路径中的每个点，比较相邻向量的方向
        for (int i = 1; i < points.Length - 1; i++)
        {
            Vector2 currentVector = points[i + 1] - points[i];
            SwipeDirection currentDirection = GetSwipeDirection(currentVector);

            // 如果当前向量的方向和第一个向量的方向不同，则返回None
            if (currentDirection != swipeDirection)
                return SwipeDirection.None;
        }

        // 返回整体滑动的方向
        return swipeDirection;
    }

    private SwipeDirection GetSwipeDirection(Vector2 direction)
    {
        // 根据向量的x和y分量确定方向
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                return SwipeDirection.Right;
            if (direction.x < 0)
                return SwipeDirection.Left;
        }
        else
        {
            if (direction.y > 0)
                return SwipeDirection.Up;
            if (direction.y < 0)
                return SwipeDirection.Down;
        }

        // 如果向量太小或者无法确定方向，则返回None
        return SwipeDirection.None;
    }
}