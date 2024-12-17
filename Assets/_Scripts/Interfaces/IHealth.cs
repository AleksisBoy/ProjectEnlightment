using UnityEngine;

public interface IHealth
{
    public enum DamageType
    {
        World,
        Stab,
        Fall,
        Explosion,
        GunShot
    }
    public void GetHit(int damage, DamageType damageType, GameObject actor, out bool died);

    public delegate void OnHealthChanged(int health, int maxHealth);
    public void AssignOnHealthChanged(OnHealthChanged action);
    public void RemoveOnHealthChanged(OnHealthChanged action);
    public Animator GetAnimator();
}
