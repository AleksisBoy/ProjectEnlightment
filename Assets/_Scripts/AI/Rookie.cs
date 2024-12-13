using System.Collections;
using UnityEngine;

public class Rookie : AIAgent, IHealth
{
    [Header("Rookie")]
    [SerializeField] private int maxHP = 70;
    [SerializeField] private Collider debugColl = null;
    [SerializeField] private Collider[] ragdollColls = null;
    [SerializeField] private Rigidbody[] ragdollRbs = null;
    [SerializeField] private AIPath path = null;
    [SerializeField] private AIPath.PathFollowMode pathFollowMode;
    [SerializeField] private Vector2 idleTimerRange = new Vector2(1f, 10f);

    private RuntimeAnimatorController defaultAnimController;

    private BehaviourTree treePatrolling = null;
    private BehaviourTree treeWaypointIdle = null;

    private int currentWaypointIndex = 0;
    private AIPath.Waypoint currentWaypoint;
    private float timeArrivedAtWaypoint = 0f;
    private float idleTimer = 0f;

    private bool alert = false;
    private Vector3 lastAlertPos = Vector3.zero;

    private int hp = 0;
    protected override void Awake()
    {
        base.Awake();
        defaultAnimController = animator.runtimeAnimatorController;
        hp = maxHP;
        foreach(Rigidbody rb in ragdollRbs)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        EnableRagdoll(false);

        if (!path) return;
        currentWaypointIndex = path.GetStartIndex(pathFollowMode);

        // Patrolling
        Leaf hasWaypoint = new Leaf("Has Waypoint?", HasWaypoint);
        Inverter hasNotWaypoint = new Inverter("Has Not Waypoint?");
        hasNotWaypoint.AddChild(hasWaypoint);
        Leaf assignWaypoint = new Leaf("Assign Next Waypoint", AssignNextWaypoint);
        Leaf goToWaypoint = new Leaf("Go to Waypoint", GoToWaypoint);

        Selector waypointProcessSelector = new Selector("Selector");

        Sequence waypointAssignSequence = new Sequence("Waypoint assign");
        waypointAssignSequence.AddChild(hasNotWaypoint);
        waypointAssignSequence.AddChild(assignWaypoint);
        waypointAssignSequence.AddChild(goToWaypoint);

        waypointProcessSelector.AddChild(waypointAssignSequence);
        waypointProcessSelector.AddChild(goToWaypoint);

        treePatrolling = new BehaviourTree("Patrolling");
        treePatrolling.AddChild(waypointProcessSelector);

        treePatrolling.PrintTree();

        // Waypoint Idle
        Leaf waypointIdle = new Leaf("Waypoint Idle", IdleAtWaypoint);

        treeWaypointIdle = new BehaviourTree("Waypoint Idle");
        treeWaypointIdle.AddChild(waypointIdle);


        tree = treePatrolling;
    }
    private Node.Status HasWaypoint()
    {
        return currentWaypoint.transform ? Node.Status.SUCCESS : Node.Status.FAILURE;
    }
    private Node.Status AssignNextWaypoint()
    {
        currentWaypoint = path.GetNextWaypoint(currentWaypointIndex, pathFollowMode, out currentWaypointIndex);
        return Node.Status.SUCCESS;
    }
    private Node.Status GoToWaypoint()
    {
        navAgent.speed = walkSpeed;
        animator.SetFloat(NovUtil.SpeedHash, NovUtil.MoveTowards(animator.GetFloat(NovUtil.SpeedHash), 1f, updateTime * 2f));
        if (IsCloseTo(currentWaypoint.transform.position) == Node.Status.SUCCESS)
        {
            return ArriveAtWaypoint();
        }

        return GoTo(currentWaypoint.transform.position);
    }
    private Node.Status GoTo(Vector3 position)
    {
        navAgent.destination = position;
        return Node.Status.RUNNING;
    }
    private Node.Status IsCloseTo(Vector3 position)
    {
        float distance = Vector3.Distance(transform.position, position);
        return distance < closeDistance ? Node.Status.SUCCESS : Node.Status.FAILURE;
    }
    private Node.Status ArriveAtWaypoint()
    {
        tree = treeWaypointIdle;
        timeArrivedAtWaypoint = Time.time;
        idleTimer = Random.Range(idleTimerRange.x, idleTimerRange.y);

        if (currentWaypoint.overrideController) animator.runtimeAnimatorController = currentWaypoint.overrideController;
        else animator.runtimeAnimatorController = defaultAnimController;

        return Node.Status.SUCCESS;
    }
    private Node.Status IdleAtWaypoint()
    {
        if (NovUtil.TimeCheck(timeArrivedAtWaypoint, idleTimer))
        {
            tree = treePatrolling;
            currentWaypoint.transform = null;
            return Node.Status.SUCCESS;
        }
        animator.SetFloat(NovUtil.SpeedHash, NovUtil.MoveTowards(animator.GetFloat(NovUtil.SpeedHash), 0f, updateTime * 2f));

        // play idle anim
        return Node.Status.RUNNING;
    }
    private void EnableRagdoll(bool state)
    {
        foreach (Collider coll in ragdollColls)
        {
            coll.enabled = state;
            coll.gameObject.layer = state ? InternalSettings.RagdollLayer : 0;
        }
        foreach (Rigidbody rb in ragdollRbs)
        {
            rb.isKinematic = !state;
        }
        animator.enabled = !state;
    }
    private void Update()
    {
    }

    public void GetHit(int damage, GameObject actor, out bool died)
    {
        died = false;
        if (!enabled) return;

        animator.SetTrigger(NovUtil.GetHitHash);
        hp -= damage;
        if(hp <= 0)
        {
            died = true;
            Die();
        }
    }
    private void Die()
    {
        animator.ResetTrigger(NovUtil.GetHitHash);
        //animator.SetTrigger(NovUtil.DiedHash);
        StartCoroutine(EnableRagdollDelay(3f));
        navAgent.enabled = false;
        agentCollider.enabled = false;
    }
    private IEnumerator EnableRagdollDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableRagdoll(true);
        enabled = false;
    }
    public void AssignOnHealthChanged(IHealth.OnHealthChanged action)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveOnHealthChanged(IHealth.OnHealthChanged action)
    {
        throw new System.NotImplementedException();
    }
    public Animator GetAnimator()
    {
        return animator;
    }
}
