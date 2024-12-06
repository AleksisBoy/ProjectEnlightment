using UnityEngine;

public class ExplosionTrap : PickupObject
{
    [Header("Trap")]
    [SerializeField] private int maxDamage = 80;
    [SerializeField] private float damageScatterDistance = 5f;

    private bool working = true;
    private bool loaded = true;
    public void ActivateTrap()
    {
        if (!loaded || !working) return;
        loaded = false;
        working = false;

        foreach(Collider coll in Physics.OverlapSphere(transform.position, damageScatterDistance, InternalSettings.CharacterMask))
        {
            IHealth health = coll.transform.root.GetComponent<IHealth>();
            if (health == null) continue;

            float distance = Vector3.Distance(transform.position, coll.transform.position);
            float perc = Mathf.Abs(1f - (distance / damageScatterDistance));
            health.GetHit((int)(maxDamage * perc), gameObject);
        }
        enabled = false;
    }
    public void DeactivateTrap()
    {
        working = false;
    }
    public override void Pickup(Player player)
    {
        if (!loaded) return;

        DeactivateTrap();
        loaded = false;
        enabled = false;
        // Give player that item (grenade or other)
    }
}
