using UnityEngine;

public interface IActor
{
    public bool RaycastForward(float distance, LayerMask mask, out RaycastHit hit,
        QueryTriggerInteraction query = QueryTriggerInteraction.Ignore);

    public bool OverlapBoxForward(float distance, Vector3 halfExtents, LayerMask mask,
        out Collider[] colls, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore);

    public bool BoxCastForward(float distance, Vector3 halfExtents, LayerMask mask,
        out RaycastHit[] hits, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore);

    public IHealth GetHealth();
    public Animator GetAnimator();
    public GameObject GetGameObject();
}