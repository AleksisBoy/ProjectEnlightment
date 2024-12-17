using UnityEngine;

public class Crossbolt : Missile
{
    protected override void OnCollisionEnter(Collision collision)
    {
        if (CheckCollisionHealth(collision))
        {
            Destroy(gameObject);
        }
        else
        {
            rb.isKinematic = true;
        }
    }
}