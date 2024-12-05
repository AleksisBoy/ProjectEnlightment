using UnityEngine;

public class ExplosionTrap : PickupObject
{
    [Header("Trap")]
    [SerializeField] private int maxDamage = 80;
    [SerializeField] private float damageScatterDistance = 5f;

    private bool loaded = true;
    public void ActivateTrap()
    {
        if (!loaded) return;
        loaded = false;

        foreach(Collider coll in Physics.OverlapSphere(transform.position, damageScatterDistance, InternalSettings.CharacterMask))
        {
            IHealth health = coll.transform.root.GetComponent<IHealth>();
            if (health == null) continue;

            float distance = Vector3.Distance(transform.position, coll.transform.position);
            float perc = Mathf.Abs(1f - (distance / damageScatterDistance));
            health.GetHit((int)(maxDamage * perc), gameObject);
        }
    }
    public void DeactivateTrap()
    {
        loaded = false;
    }
    public override void Pickup(Player player)
    {
        if (!loaded) return;

        DeactivateTrap();
        // Give player that item (grenade or other)
    }
}
