using System.Collections;
using UnityEngine;

public class Rookie : AIAgent, IHealth, IAnimationDispatch
{
    [Header("Rookie")]
    [SerializeField] private int maxHP = 70;
    [SerializeField] private Collider debugColl = null;
    [SerializeField] private Collider[] ragdollColls = null;
    [SerializeField] private Rigidbody[] ragdollRbs = null;
    [SerializeField] private AIPath path = null;
    [SerializeField] private AIPath.PathFollowMode pathFollowMode;
    [SerializeField] private Vector2 idleTimerRange = new Vector2(1f, 10f);
    [SerializeField] private float seeMaxDistance = 10f;
    [SerializeField] private float seeMinDot = 0.1f;
    [SerializeField] private float suspicionMaxIncrease = 1f;
    [SerializeField] private float suspicionTimer = 10f;
    [SerializeField] private float runSpeed = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float hitStunTimer = 0.5f;

    private RuntimeAnimatorController defaultAnimController;

    private BehaviourTree treePatrolling = null;
    private BehaviourTree treeWaypointIdle = null;
    private BehaviourTree treeSuspicious = null;
    private BehaviourTree treeAlert = null;

    private int currentWaypointIndex = 0;
    private AIPath.Waypoint currentWaypoint;
    private float timeArrivedAtWaypoint = 0f;
    private float idleTimer = 0f;

    private bool alert = false;
    private Vector3 lastAlertPos = Vector3.zero;
    private Vector3 lastSuspiciousPos = Vector3.zero;
    private Vector3 lastKnownPlayerPos = Vector3.zero;
    private float lastSuspicionTime = 0f;
    private float lastSeenPlayerTime = 0f;
    private float lastAttackTime = 0f;
    private float lastHitTime = 0f;
    private float suspicion = 0f;

    private int hp = 0;

    private Player player;
    private SeePlayerState seePlayer;
    private bool rotatingAnim = false;

    private float speed_animator = 0f;
    private enum SeePlayerState
    {
        DidNotCheck,
        DoNotSeePlayer,
        SeePlayer
    }
    protected override bool Prebehave()
    {
        seePlayer = SeePlayerState.DidNotCheck;
        return true;
    }
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

        player = FindAnyObjectByType<Player>(); // !!!!!!!!!!!!!!!!!!!!!!!

        if (!path) return;
        currentWaypointIndex = path.GetStartIndex(pathFollowMode);

        // General
        Leaf canSeePlayer = new Leaf("Can See player", CanSeePlayer);
        Inverter cannotSeePlayer = new Inverter("Cannot See player");
        cannotSeePlayer.AddChild(canSeePlayer);
        Leaf becomeSuspicious = new Leaf("Become suspicious", BecomeSuspicious);
        Leaf becomePatrolling = new Leaf("Become Patrolling", BecomePatrolling);
        Leaf becomeAlert = new Leaf("Become Alert", BecomeAlert);

        Sequence checkForPlayer = new Sequence("Check for Player");
        checkForPlayer.AddChild(canSeePlayer);
        checkForPlayer.AddChild(becomeSuspicious);

        // Patrolling
        Leaf hasWaypoint = new Leaf("Has Waypoint?", HasWaypoint);
        Inverter hasNotWaypoint = new Inverter("Has Not Waypoint?");
        hasNotWaypoint.AddChild(hasWaypoint);
        Leaf assignWaypoint = new Leaf("Assign Next Waypoint", AssignNextWaypoint);
        Leaf goToWaypoint = new Leaf("Go to Waypoint", GoToWaypoint);
        Leaf arriveAtWaypoint = new Leaf("Arrive at Waypoint", ArriveAtWaypoint);

        Selector waypointProcessSelector = new Selector("Selector");

        BehaviourTree cannotSeePlayerDependancy = new BehaviourTree();
        cannotSeePlayerDependancy.AddChild(cannotSeePlayer);

        DepSequence followWaypoint = new DepSequence("Follow waypoint", cannotSeePlayerDependancy, navAgent);
        followWaypoint.AddChild(goToWaypoint);
        followWaypoint.AddChild(arriveAtWaypoint);

        Sequence waypointAssignSequence = new Sequence("Waypoint assign");
        waypointAssignSequence.AddChild(hasNotWaypoint);
        waypointAssignSequence.AddChild(assignWaypoint);
        waypointAssignSequence.AddChild(followWaypoint);

        waypointProcessSelector.AddChild(checkForPlayer);
        waypointProcessSelector.AddChild(waypointAssignSequence);
        waypointProcessSelector.AddChild(followWaypoint);

        treePatrolling = new BehaviourTree("Patrolling");
        treePatrolling.AddChild(waypointProcessSelector);

        treePatrolling.PrintTree();

        // Waypoint Idle
        Leaf waypointIdle = new Leaf("Waypoint Idle", IdleAtWaypoint);

        Selector waypointIdleProcess = new Selector("Waypoint Idle Process");
        waypointIdleProcess.AddChild(checkForPlayer);
        waypointIdleProcess.AddChild(waypointIdle);

        treeWaypointIdle = new BehaviourTree("Waypoint Idle");
        treeWaypointIdle.AddChild(waypointIdleProcess);

        // Suspicious Tree

        Leaf lookAtPlayerKnownPosition = new Leaf("Look At Player Known Position", LookAtPlayerKnownPosition);
        Leaf wasSuspiciousRecently = new Leaf("Was suspicious recently", WasSuspiciousRecently);
        Leaf goToLastSuspiciousPos = new Leaf("Go to last suspicious pos", GoToLastSuspiciousPosition);
        Leaf checkAround = new Leaf("Check Around", CheckAround);
        Leaf lowerAwareness = new Leaf("lower awareness", LowerAwareness);

        BehaviourTree deppp = new BehaviourTree();
        Sequence seq = new Sequence("");
        seq.AddChild(canSeePlayer);
        seq.AddChild(wasSuspiciousRecently);
        deppp.AddChild(seq);

        DepSequence seePlayerSuspicious = new DepSequence("see player suspicious", deppp, navAgent);
        seePlayerSuspicious.AddChild(lookAtPlayerKnownPosition);

        BehaviourTree wasSuspiciousRecentlyDependancy = new BehaviourTree();
        Sequence wsp = new Sequence("sd");
        wsp.AddChild(wasSuspiciousRecently);
        wsp.AddChild(cannotSeePlayer);
        wasSuspiciousRecentlyDependancy.AddChild(wsp);

        DepSequence checkRecentSuspicion = new DepSequence("Check recent suspicion", wasSuspiciousRecentlyDependancy, navAgent);
        checkRecentSuspicion.AddChild(goToLastSuspiciousPos);
        checkRecentSuspicion.AddChild(checkAround);

        Sequence resetSuspicion = new Sequence("Reset suspicion");
        resetSuspicion.AddChild(lowerAwareness);

        Selector suspiciousProcess = new Selector("Suspicious Process");
        suspiciousProcess.AddChild(seePlayerSuspicious);
        suspiciousProcess.AddChild(checkRecentSuspicion);
        suspiciousProcess.AddChild(resetSuspicion);

        treeSuspicious = new BehaviourTree("Suspicious Tree");
        treeSuspicious.AddChild(suspiciousProcess);

        // Alert tree

        Leaf isMyTurnToAttack = new Leaf("Is my turn to attack?", IsMyTurnToAttack);
        Leaf goToPlayer = new Leaf("Go To player", GoToPlayer);
        Leaf attack = new Leaf("Attack", Attack);
        Leaf surroundPlayer = new Leaf("Surround Player", SurroundPlayer);

        Sequence attackSequence = new Sequence("attack sequence");
        attackSequence.AddChild(isMyTurnToAttack);
        attackSequence.AddChild(goToPlayer);
        attackSequence.AddChild(attack);

        Selector alertProcess = new Selector("Alert Process");
        alertProcess.AddChild(attackSequence);
        alertProcess.AddChild(surroundPlayer);

        treeAlert = new BehaviourTree("Alert Tree");
        treeAlert.AddChild(alertProcess);

        // Launch
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
        Node.Status s = GoTo(currentWaypoint.transform.position);
        if(s == Node.Status.RUNNING)
        {
            navAgent.speed = NovUtil.MoveTowards(navAgent.speed, walkSpeed, updateTime * 2f);
            //animator.SetFloat(NovUtil.SpeedHash, NovUtil.MoveTowards(animator.GetFloat(NovUtil.SpeedHash), 1f, updateTime * 2f));
            speed_animator = 1f;
        }

        return s;
    }
    private Node.Status GoTo(Vector3 position)
    {
        if(!NovUtil.TimeCheck(lastHitTime, hitStunTimer))
        {
            return Node.Status.FAILURE;
        }
        if (rotatingAnim)
        {
            //
            navAgent.speed = 0f;
            return Node.Status.RUNNING;
        }
        if (IsCloseTo(position) == Node.Status.SUCCESS)
        {
            return Node.Status.SUCCESS;
        }
        navAgent.destination = position;
        Vector3 dirToWaypoint = position - transform.position;
        float waypointAngle = Vector3.SignedAngle(transform.forward, dirToWaypoint.normalized, Vector3.up);
        //Debug.Log(waypointAngle);
        //if (waypointAngle >= 170f || waypointAngle <= -170f)
        //{
        //    rotatingAnim = true;
        //    navAgent.speed = 0f;
        //    navAgent.ResetPath();
        //    animator.SetTrigger(waypointAngle >= 170f ? "TurnRight180" : "TurnLeft180");
        //    return Node.Status.RUNNING;
        //}
        //else if (waypointAngle >= 90f || waypointAngle <= -90f)
        //{
        //    rotatingAnim = true;
        //    navAgent.speed = 0f;
        //    navAgent.ResetPath();
        //    animator.SetTrigger(waypointAngle >= 90f ? "TurnRight90" : "TurnLeft90");
        //    return Node.Status.RUNNING;
        //}
        if (navAgent.path.corners.Length > 1)
        {
            Vector3 corner = navAgent.path.corners[1];
            corner.y = transform.position.y;
            Vector3 dir = corner - transform.position;
            float turnAngle = Vector3.SignedAngle(transform.forward, dir.normalized, Vector3.up);
            Debug.Log(turnAngle);
            if (turnAngle >= 90f)
            {
                rotatingAnim = true;
                navAgent.speed = 0f;
                navAgent.ResetPath();
                animator.SetTrigger("TurnRight90");
                return Node.Status.RUNNING;
            }
            else if (turnAngle <= -90f)
            {
                rotatingAnim = true;
                navAgent.speed = 0f;
                navAgent.ResetPath();
                animator.SetTrigger("TurnLeft90");
                return Node.Status.RUNNING;
            }
        }
        
        navAgent.destination = position;
        return Node.Status.RUNNING;
    }
    private Node.Status GoToPlayer()
    {
        Node.Status s = GoTo(player.transform.position);
        if(s == Node.Status.RUNNING)
        {
            navAgent.speed = NovUtil.MoveTowards(navAgent.speed, runSpeed, updateTime * 3f);
            //animator.SetFloat(NovUtil.SpeedHash, NovUtil.MoveTowards(animator.GetFloat(NovUtil.SpeedHash), 2f, updateTime * 3f));
            speed_animator = 2f;
        }
        return s;
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
            return BecomePatrolling();
        }
        //animator.SetFloat(NovUtil.SpeedHash, NovUtil.MoveTowards(animator.GetFloat(NovUtil.SpeedHash), 0f, updateTime * 2f));
        speed_animator = 0f;
        // play idle anim
        return Node.Status.SUCCESS;
    }
    private Node.Status CanSeePlayer()
    {
        if (seePlayer == SeePlayerState.DidNotCheck)
        {
            Node.Status s = CanSee(player.Collider, seeMaxDistance, seeMinDot);
            if (s == Node.Status.SUCCESS)
            {
                seePlayer = SeePlayerState.SeePlayer;
                lastKnownPlayerPos = player.transform.position;
                lastSuspicionTime = Time.time;
                lastSeenPlayerTime = Time.time;
            }
            else
            {
                seePlayer = SeePlayerState.DoNotSeePlayer;
            }
            return s;
        }
        else
        {
            return seePlayer == SeePlayerState.SeePlayer ? Node.Status.SUCCESS : Node.Status.FAILURE;
        }

    }
    private Node.Status BecomeSuspicious()
    {
        GoTo(transform.position);
        // play anim?
        tree = treeSuspicious;
        animator.runtimeAnimatorController = defaultAnimController;
        lastSuspicionTime = Time.time;
        //animator.SetFloat(NovUtil.SpeedHash, 0f);
        speed_animator = 0f;
        return Node.Status.SUCCESS;
    }
    private Node.Status BecomeAlert()
    {
        tree = treeAlert;
        animator.SetBool("LookingAround", false);
        return Node.Status.SUCCESS;
    }
    private Node.Status BecomePatrolling()
    {
        tree = treePatrolling;
        currentWaypoint.transform = null;
        suspicion = 0f;
        animator.SetBool("LookingAround", false);
        return Node.Status.SUCCESS;
    }
    private Node.Status LookAtPlayerKnownPosition()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float perc = NovUtil.GetRangePercentage(distanceToPlayer, 0f, seeMaxDistance);
        float suspicionIncrease = perc * suspicionMaxIncrease * updateTime;
        suspicion += suspicionIncrease;
        if (suspicion > 1f)
        {
            return BecomeAlert();
        }
        //

        return Node.Status.RUNNING;
    }
    private Node.Status LowerAwareness()
    {
        suspicion -= updateTime * 0.2f;
        if (suspicion < 0f) return BecomePatrolling();
        return Node.Status.SUCCESS;
    }
    private Node.Status WasSuspiciousRecently()
    {
        return NovUtil.TimeCheck(lastSuspicionTime, suspicionTimer) ? Node.Status.FAILURE : Node.Status.SUCCESS;
    }
    private Node.Status GoToLastSuspiciousPosition()
    {
        Node.Status s = GoTo(lastKnownPlayerPos);
        if(s == Node.Status.RUNNING)
        {
            navAgent.speed = NovUtil.MoveTowards(navAgent.speed, walkSpeed, updateTime * 2f);
            //animator.SetFloat(NovUtil.SpeedHash, NovUtil.MoveTowards(animator.GetFloat(NovUtil.SpeedHash), 1f, updateTime * 2f));
            speed_animator = 1f;
        }
        return s;
    }
    private Node.Status CheckAround()
    {
        // implement
        animator.SetBool("LookingAround", true);
        return Node.Status.SUCCESS;
    }
    private Node.Status IsMyTurnToAttack()
    {
        // implement
        return Node.Status.SUCCESS;
    }
    private Node.Status Attack()
    {
        if(!NovUtil.TimeCheck(lastHitTime, hitStunTimer))
        {
            return Node.Status.FAILURE;
        }
        LookAtPlayer();
        if (NovUtil.TimeCheck(lastAttackTime, attackCooldown))
        {
            Debug.Log("attack");
            animator.SetFloat(NovUtil.SpeedHash, 0f);
            animator.SetTrigger(NovUtil.AttackHash);
            lastAttackTime = Time.time;
            float dot = Vector3.Dot(transform.forward, (player.transform.position - transform.position).normalized);
            if (dot > 0.7f && Vector3.Distance(transform.position, player.transform.position) < closeDistance)
            {
                player.GetHit(18, IHealth.DamageType.Stab, gameObject, out bool died);
                if (died) enabled = false;
            }
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILURE;
    } 
    private Node.Status LookAtPlayer()
    {
        return LookAt(player.transform.position);
    }
    private Node.Status LookAt(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 3f * updateTime);
        return Node.Status.SUCCESS;
    }
    private Node.Status SurroundPlayer()
    {
        return Node.Status.SUCCESS;
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
        animator.SetFloat(NovUtil.SpeedHash, NovUtil.MoveTowards(animator.GetFloat(NovUtil.SpeedHash), speed_animator, Time.deltaTime * 5f));
    }

    public void GetHit(int damage, IHealth.DamageType dType, GameObject actor, out bool died)
    {
        died = false;
        if (!enabled) return;

        lastHitTime = Time.time;
        navAgent.speed = 0f;
        speed_animator = 0f;
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
        StopAllCoroutines();
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
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(lastKnownPlayerPos, 0.3f);
    }

    public void CallAnimationEvent(NovUtil.AnimEvent animEvent)
    {
        //Debug.Break();
        switch (animEvent)
        {
            case NovUtil.AnimEvent.TurningRight90Finish:
                rotatingAnim = false;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 90f, transform.eulerAngles.z);
                //StartCoroutine(RotateYNextFrame(90f));
                break;
            case NovUtil.AnimEvent.TurningLeft90Finish:
                rotatingAnim = false;
                //StartCoroutine(RotateYNextFrame(-90f));
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - 90f, transform.eulerAngles.z);
                break;
            case NovUtil.AnimEvent.TurningRight180Finish:
                rotatingAnim = false;
                //StartCoroutine(RotateYNextFrame(180f));
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 180f, transform.eulerAngles.z);
                break;
            case NovUtil.AnimEvent.TurningLeft180Finish:
                rotatingAnim = false;
                //StartCoroutine(RotateYNextFrame(-180f));
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - 180f, transform.eulerAngles.z);
                break;
            case NovUtil.AnimEvent.GetHitFinish:
                break;
        }
    }
    private IEnumerator RotateYNextFrame(float y)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + y, transform.eulerAngles.z);
        yield return null;
    }
}
