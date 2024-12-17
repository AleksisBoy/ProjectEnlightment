using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] protected Rigidbody rb;
    [SerializeField] private Collider coll;
    [SerializeField] private float lifetime = 2f;

    private float spawnTime = 0f;
    private int damage = 0;
    private void Update()
    {
        if (NovUtil.TimeCheck(spawnTime, lifetime))
        {
            Destroy(gameObject);
        }
    }
    public void Set(Vector3 direction, float force, int damage, params Collider[] exceptions)
    {
        foreach (Collider exception in exceptions)
        {
            Physics.IgnoreCollision(coll, exception);
        }
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        spawnTime = Time.time;
        this.damage = damage;
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        CheckCollisionHealth(collision);
        Destroy(gameObject);
    }
    protected bool CheckCollisionHealth(Collision collision)
    {
        IHealth health = collision.transform.GetComponentInParent<IHealth>();
        if (health == null) return false;

        health.GetHit(damage, IHealth.DamageType.Arrow, gameObject, out bool died);
        if (died) health.GetAnimator()?.SetTrigger(NovUtil.DiedHash);
        return true;
    }
}
