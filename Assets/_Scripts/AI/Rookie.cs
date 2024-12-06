using System.Collections;
using UnityEngine;

public class Rookie : AIAgent, IHealth
{
    [Header("Rookie")]
    [SerializeField] private int maxHP = 70;
    [SerializeField] private Collider debugColl = null;
    [SerializeField] private Collider[] ragdollColls = null;
    [SerializeField] private Rigidbody[] ragdollRbs = null;

    private int hp = 0;
    protected override void Awake()
    {
        base.Awake();
        hp = maxHP;
        EnableRagdoll(false);
    }
    private void EnableRagdoll(bool state)
    {
        foreach (Collider coll in ragdollColls)
        {
            coll.enabled = state;
            coll.gameObject.layer = state ? InternalSettings.RagdollLayer : 0;
        }
        foreach (Rigidbody rb in ragdollRbs)
        {
            rb.isKinematic = !state;
        }
        animator.enabled = !state;
    }
    private void Update()
    {
    }

    public void GetHit(int damage, GameObject actor)
    {
        if (!enabled) return;

        animator.SetTrigger(NovUtil.GetHitHash);
        hp -= damage;
        if(hp <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        animator.ResetTrigger(NovUtil.GetHitHash);
        animator.SetTrigger(NovUtil.DiedHash);
        StartCoroutine(EnableRagdollDelay(3.5f));
        navAgent.enabled = false;
        agentCollider.enabled = false;
    }
    private IEnumerator EnableRagdollDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableRagdoll(true);
        enabled = false;
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
