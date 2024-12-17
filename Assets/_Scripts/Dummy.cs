using UnityEngine;

public class Dummy : MonoBehaviour, IHealth
{
    public void GetHit(int damage, IHealth.DamageType dType, GameObject actor, out bool died)
    {
        died = true;
        Destroy(gameObject);
    }
    public void AssignOnHealthChanged(IHealth.OnHealthChanged action)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveOnHealthChanged(IHealth.OnHealthChanged action)
    {
        throw new System.NotImplementedException();
    }
    public Animator GetAnimator()
    {
        return null;
    }
}
