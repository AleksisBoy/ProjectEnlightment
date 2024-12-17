using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private GameObject explosionVFX = null;
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private Collider coll = null;

    private ItemGrenade grenadeInfo = null;
    private float? triggerTime = null;

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
            IHealth health = coll.transform.root.GetComponent<IHealth>();
            if (health == null) continue;

            // add collision check

            float distance = Vector3.Distance(transform.position, coll.transform.position);
            float perc = Mathf.Abs(1f - (distance / grenadeInfo.BlowRadius));
            health.GetHit((int)(grenadeInfo.MaxDamage * perc), IHealth.DamageType.Explosion, gameObject, out bool died);
            if (died) health.GetAnimator().SetTrigger(NovUtil.DiedHash);
        }
        GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        Destroy(vfx, 5f);
        // sound
        Destroy(gameObject);
    }
    public void Set(ItemGrenade grenadeInfo, ItemGrenade.GrenadeUseData grenData, float force)
    {
        this.grenadeInfo = grenadeInfo;
        Vector3 pos = grenData.actor.GetGameObject().transform.position + Vector3.up * grenData.actor.GetHeight();
        if (grenData.actor.RaycastForward(10f, InternalSettings.EnvironmentLayer, out RaycastHit hit, out Vector3 dir))
        {
            dir = (hit.point - grenData.instance.transform.position).normalized;
        }
        else
        {
            dir = ((pos + dir * 10f) - grenData.instance.transform.position).normalized;
        }
        dir.y += 0.1f;
        rb.AddForce(dir.normalized * force, ForceMode.Impulse);
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
