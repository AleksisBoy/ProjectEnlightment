using UnityEngine;

public class PlayerItemPickup : PickupObject
{
    [Header("Player Item")]
    [SerializeField] private Type type;
    [SerializeField] private int amount = 10;
    public enum Type
    {
        None,
        Money,
        HealingPotion,
        ManaPotion,
        AbilityPoint
    }
    public override void Pickup(Player player)
    {
        player.AddPickup(type, amount);
        Destroy(gameObject);
    }
}
