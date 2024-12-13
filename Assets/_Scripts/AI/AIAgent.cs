using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIAgent : MonoBehaviour
{
    [Header("AI Agent")]
    [SerializeField] protected Animator animator = null;
    [SerializeField] protected NavMeshAgent navAgent = null;
    [SerializeField] protected Collider agentCollider = null;
    [SerializeField] protected float walkSpeed = 4f;
    [SerializeField] protected float closeDistance = 0.5f;
    [SerializeField] protected Vector3 eyeOffset = new Vector3(0, 1f, 0);
    [SerializeField] protected Vector3 seeHalfExtents = new Vector3(0.05f, 0.05f, 0.25f);
    [SerializeField] private Vector2 timerRange = new Vector2(0.1f, 1f);

    protected BehaviourTree tree;
    protected Node.Status treeStatus = Node.Status.RUNNING;

    protected float updateTime = 0f;
    private WaitForSeconds timer;

    protected virtual void Awake()
    {
        if (!navAgent) navAgent = GetComponent<NavMeshAgent>();
        if (!agentCollider) agentCollider = GetComponent<Collider>();
        tree = new BehaviourTree();
        SetTimerInRange();
    }
    protected virtual void Start()
    {
        StartCoroutine(Behave());
    }
    private IEnumerator Behave()
    {
        while (true)
        {
            treeStatus = tree.Process();
            yield return timer;
        }
    }
    private void SetTimerInRange()
    {
        updateTime = Random.Range(timerRange.x, timerRange.y);
        timer = new WaitForSeconds(updateTime);
    }
    // Nodes
    protected Node.Status CanSee(Collider coll, float maxDistance, float minDot)
    {
        float distance = Vector3.Distance(transform.position, coll.transform.position);
        if (distance > maxDistance) return Node.Status.FAILURE;

        Vector3 direction = (coll.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, direction);

        if (dot < minDot) return Node.Status.FAILURE;

        RaycastHit[] hits = Physics.BoxCastAll(
            transform.position + eyeOffset,
            seeHalfExtents,
            direction,
            transform.rotation,
            maxDistance);

        if (hits.Length == 0) return Node.Status.FAILURE;

        return coll == NovUtil.GetClosestFromRaycastHits(transform.position + eyeOffset,
            hits, agentCollider) ? Node.Status.SUCCESS : Node.Status.FAILURE;
    }
}
