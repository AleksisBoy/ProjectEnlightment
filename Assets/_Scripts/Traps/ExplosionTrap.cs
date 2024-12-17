using UnityEngine;

public class ExplosionTrap : PickupObject
{
    [Header("Trap")]
    [SerializeField] private ItemGrenade explosiveInfo = null;
    [SerializeField] private Grenade explosive = null;
    [SerializeField] private EItem pickupItem = null;

    private bool working = true;
    private bool loaded = true;
    private void Start()
    {
        explosive?.SetStatic(explosiveInfo);
    }
    public void ActivateTrap()
    {
        if (!loaded || !working) return;

        explosive?.Explode();

        loaded = false;
        working = false;
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
        player.AddItem(pickupItem, 1);
        Destroy(explosive.gameObject);
    }
}
