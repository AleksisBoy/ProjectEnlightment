
using System.Collections.Generic;
using UnityEngine;

public static class NovUtil
{
    public static readonly int JumpHash = Animator.StringToHash("Jump");
    public static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    public static readonly int IsCrouchedHash = Animator.StringToHash("IsCrouched");
    public static readonly int SpeedHash = Animator.StringToHash("Speed");
    public static readonly int AttackHash = Animator.StringToHash("Attack");
    public static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");
    public static readonly int GetHitHash = Animator.StringToHash("GetHit");
    public static readonly int DiedHash = Animator.StringToHash("Died");
    public static readonly int CombatStaminaHash = Animator.StringToHash("CombatStamina");
    public static readonly int SheathedHash = Animator.StringToHash("Sheathed");
    public static readonly int GunshotHash = Animator.StringToHash("Gunshot");

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
    public static Collider GetClosestFromRaycastHits(Vector3 position, in RaycastHit[] list, params Collider[] exceptions)
    {
        if (list == null || list.Length == 0) return null;

        float closestDistance = 1000f;
        Collider closest = null;
        foreach (RaycastHit entity in list)
        {
            if (IsException(entity.collider, exceptions)) continue;

            float distance = Vector3.Distance(position, entity.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = entity.collider;
            }
        }
        return closest;
    }
    private static bool IsException<T>(T obj, params T[] exceptions) where T : Component
    {
        foreach(T exception in exceptions)
        {
            if (obj == exception) return true;
        }
        return false;
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
    public static bool TimerCheck(ref float time, in float timer, in float incrementer,  bool resetOnTrue = true)
    {
        time += incrementer;
        if (time >= timer)
        {
            if (resetOnTrue) time = 0f;
            return true;
        }
        return false;
    }
    public static bool CountdownCheck(ref float time, in float timer, in float decrementer,  bool resetOnTrue = true)
    {
        time -= decrementer;
        if (time <= 0f)
        {
            if (resetOnTrue) time = timer;
            return true;
        }
        return false;
    }
    public static bool TimeCheck(float lastTime, float timer)
    {
        return Time.time - lastTime >= timer;
    }
}
