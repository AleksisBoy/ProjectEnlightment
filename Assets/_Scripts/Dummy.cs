using UnityEngine;

public class Dummy : MonoBehaviour, IHealth
{
    public void GetHit(int damage, GameObject actor)
    {
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
}
