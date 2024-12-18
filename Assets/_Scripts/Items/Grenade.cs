using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private GameObject explosionVFX = null;
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private Collider coll = null;

    private ItemGrenade grenadeInfo = null;
    private float? triggerTime = null;
    private IActor actor = null;

    private void Update()
    {
        if (NovUtil.TimeCheck(triggerTime, grenadeInfo.TriggerDuration))
        {
            Explode();
        }
    }
    public void Explode()
    {
        foreach (Collider coll in Physics.OverlapSphere(transform.position, grenadeInfo.BlowRadius, InternalSettings.CharacterMask))
        {
            IHealth health = coll.transform.GetComponentInParent<IHealth>();
            if (health == null) continue;

            // add collision check

            float distance = Vector3.Distance(transform.position, coll.transform.position);
            float perc = Mathf.Abs(1f - (distance / grenadeInfo.BlowRadius));
            health.GetHit((int)(grenadeInfo.MaxDamage * perc), IHealth.DamageType.Explosion, gameObject, out bool died);
            if (died) health.GetAnimator()?.SetTrigger(NovUtil.DiedHash);
            actor?.ProcessActorData(new IActor.Data(IActor.Data.Type.HitReport, 0f));
        }
        GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        Destroy(vfx, 5f);
        // sound
        Destroy(gameObject);
    }
    public void Set(ItemGrenade grenadeInfo, IActor actor, Vector3 direction, float force)
    {
        this.grenadeInfo = grenadeInfo;
        this.actor = actor;
        direction.y += 0.1f;
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        triggerTime = Time.time;
    }
    public void SetStatic(ItemGrenade grenadeInfo)
    {
        this.grenadeInfo = grenadeInfo;
        if (rb) rb.isKinematic = true;
        if (coll) coll.enabled = false;
        enabled = false;
    }
}
