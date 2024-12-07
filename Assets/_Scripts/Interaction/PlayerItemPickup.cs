using UnityEngine;

public class PlayerItemPickup : PickupObject
{
    [Header("Player Item")]
    [SerializeField] private EItem item;
    [SerializeField] private int amount = 1;
    private void OnValidate()
    {
        if (!item) return;

        if (item.Unique) amount = 1;
        if (amount <= 0) amount = 1;
    }
    public override void Pickup(Player player)
    {
        player.AddItem(item, amount);
        Destroy(gameObject);
    }
}
