
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
    public static readonly int CrossbowShotHash = Animator.StringToHash("CrossbowShot");
    public static readonly int ThrowHash = Animator.StringToHash("Throw");
    public static readonly int ThrowPrepareHash = Animator.StringToHash("ThrowPrepare");

    public enum AnimEvent
    {
        AttackImpact,
        AttackFinish,
        CombatParry,
        PlayerAnimationFinish,
        OpenAttackWindow,
        TurningRight90Finish,
        TurningRight180Finish,
        TurningLeft90Finish,
        TurningLeft180Finish,
        GetHitFinish,
        GrenadeThrown
    }
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
    public static bool TimeCheck(float? lastTime, float timer)
    {
        return lastTime != null && Time.time - lastTime >= timer;
    }
    public static RaycastHit[] SortHitsByDistance(RaycastHit[] hits)
    {
        List<RaycastHit> hitsList = new List<RaycastHit>(hits);
        List<RaycastHit> sorted = new List<RaycastHit>();
        while (sorted.Count < hits.Length)
        {
            float closest = 10000f;
            int closestIndex = 0;
            for (int i = 0; i < hitsList.Count; i++)
            {
                if (hitsList[i].distance < closest)
                {
                    closest = hits[i].distance;
                    closestIndex = i;
                }
            }
            sorted.Add(hitsList[closestIndex]);
            hitsList.RemoveAt(closestIndex);
        }
        return sorted.ToArray();
    }
    public static float GetRangePercentage(float value, float min, float max)
    {
        return (max - value) / (max - min);
    }
}
