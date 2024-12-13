using System.Collections.Generic;
using UnityEngine;

public class AIPath : MonoBehaviour
{
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();

    public int GetStartIndex(PathFollowMode followMode)
    {
        int startIndex;
        switch (followMode)
        {
            case PathFollowMode.Linear:
                {
                    startIndex = -1;
                    break;
                }
            case PathFollowMode.LinearInverse:
                {
                    startIndex = waypoints.Count;
                    break;
                }
            case PathFollowMode.Random:
                {
                    startIndex = Random.Range(0, waypoints.Count);
                    break;
                }
            default:
                startIndex = 0;
                break;
        }
        return startIndex;
    }
    public Waypoint GetNextWaypoint(int previousIndex, PathFollowMode followMode, out int nextIndex)
    {
        switch(followMode)
        {
            case PathFollowMode.Linear:
                {
                    nextIndex = previousIndex + 1;
                    if (nextIndex >= waypoints.Count) nextIndex = 0;
                    break;
                }
            case PathFollowMode.LinearInverse:
                {
                    nextIndex = previousIndex - 1;
                    if (nextIndex < 0) nextIndex = waypoints.Count - 1;
                    break;
                }
            case PathFollowMode.Random:
                {
                    nextIndex = Random.Range(0, waypoints.Count);
                    break;
                }
            default:
                nextIndex = 0;
                break;
        }
        return waypoints[nextIndex];
    }
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = Color.red;
        foreach(Waypoint waypoint in waypoints)
        {
            if (waypoint.transform) Gizmos.DrawSphere(waypoint.transform.position, 0.2f);
        }
    }
    [System.Serializable]
    public struct Waypoint
    {
        public Transform transform;
        public AnimatorOverrideController overrideController;
        // animation name/id?
        // animation length?
    }
    public enum PathFollowMode
    {
        Linear,
        LinearInverse,
        Random
    }
}