using UnityEngine;

public interface IHealth
{
    public void GetHit(int damage, GameObject actor);

    public delegate void OnHealthChanged(int health, int maxHealth);
    public void AssignOnHealthChanged(OnHealthChanged action);
    public void RemoveOnHealthChanged(OnHealthChanged action);
}
