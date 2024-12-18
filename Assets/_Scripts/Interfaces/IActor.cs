using UnityEngine;

public interface IActor
{
    public bool RaycastForward(float distance, LayerMask mask, out RaycastHit hit,
        out Vector3 direction, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore);

    public bool OverlapBoxForward(float distance, Vector3 halfExtents, LayerMask mask,
        out Collider[] colls, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore);

    public bool BoxCastForwardAll(float distance, Vector3 halfExtents, LayerMask mask,
        out RaycastHit[] hits, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore);

    public bool BoxCastForward(float distance, Vector3 halfExtents, LayerMask mask, out RaycastHit hit, out Vector3 direction, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore);

    public void ProcessActorData(Data data);
    public void Teleport(Vector3 position, float teleportSpeed);
    public bool CanTeleport();
    public IHealth GetHealth();
    public IMana GetMana();
    public float GetHeight();
    public Animator GetAnimator();
    public GameObject GetGameObject();
    public Collider GetCollider();

    public struct Data
    {
        public enum Type
        {
            StaminaPerc,
            DecrementLeftHand,
            DecrementRightHand,
            HitReport
        }
        public Type type;
        public float value;

        public Data(Type type)
        {
            this.type = type;
            this.value = 0;
        }
        public Data(Type type, float value)
        {
            this.type = type;
            this.value = value;
        }
    }
}