
public interface IPickup
{
    public void Pickup(Player player);
    public void HighlightUpdate();
    public enum Type
    {
        Money,
        HealingPotion,
        ManaPotion,
        AbilityPoint
    }
}
