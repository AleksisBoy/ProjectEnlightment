using UnityEngine;

public class Crossbolt : Missile
{
    [Header("Crossbolt")]
    [SerializeField] private float hitAttackDistance = 0.05f;
    protected override void OnCollisionEnter(Collision collision)
    {
        if (CheckCollisionHealth(collision))
        {
            Destroy(gameObject);
        }
        else
        {
            rb.isKinematic = true;
            transform.position += transform.forward * hitAttackDistance;
        }
    }
}