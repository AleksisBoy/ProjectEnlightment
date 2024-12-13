
public interface IMana
{
    public bool UseMana(float usedMana);

    public delegate void OnManaChanged(float mana, float maxMana);
    public void AssignOnManaChanged(OnManaChanged action);
    public void RemoveOnManaChanged(OnManaChanged action);
}