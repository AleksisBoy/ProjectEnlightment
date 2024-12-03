
using System.Collections.Generic;
using UnityEngine;

public static class NovUtil
{
    public static readonly int JumpHash = Animator.StringToHash("Jump");
    public static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    public static readonly int SpeedHash = Animator.StringToHash("Speed");

    public static T GetClosestFromArray<T>(Vector3 position, in T[] list) where T : Component
    {
        float closestDistance = 1000f;
        T closest = null;
        foreach (T entity in list)
        {
            float distance = Vector3.Distance(position, entity.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = entity;
            }
        }
        return closest;
    }
    public static T GetClosestFromList<T>(Vector3 position, float range, List<T> list, params T[] exceptions) where T : Component
    {
        float closestDistance = 1000f;
        T closest = null;
        foreach (T exception in exceptions)
        {
            if (list.Contains(exception))
            {
                list.Remove(exception);
            }
        }
        foreach (T entity in list)
        {
            float distance = Vector3.Distance(position, entity.transform.position);
            if (distance > range) continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = entity;
            }
        }
        return closest;
    }
    public static List<T> CreateListFromArray<T>(T[] array)
    {
        List<T> list = new List<T>();
        foreach (T entity in array)
        {
            list.Add(entity);
        }
        return list;
    }
    public static float MoveTowards(float start, float end, float delta)
    {
        if (start < end)
            return Mathf.Min(start + delta, end);
        else if (start > end)
            return Mathf.Max(start - delta, end);
        else
            return end;
    }
    public static Vector3 VectorAbs(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }
}
