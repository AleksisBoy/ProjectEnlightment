using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PlayerCombat : PlayerAction
{
    [Header("Combat")]
    [SerializeField] private KeyCode attackActionKey;
    [SerializeField] private KeyCode blockActionKey;
    [SerializeField] private float attackDistance = 0.3f;
    [SerializeField] private int damage = 25;
    [SerializeField] private Vector3 attackOffset = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 halfExtents = new Vector3(0.1f, 0.5f, 0.1f);

    private bool isAttacking = false;
    private bool queueAttack = false;
    private float lastAttackTime = 0;

    private bool isBlocking = false;
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;
        bool attackActionKeyDown = Input.GetKeyDown(attackActionKey);
        bool blockActionKeyHold = Input.GetKey(blockActionKey);
        
        if (attackActionKeyDown && !isBlocking)
        {
            Attack();
            blockOther = true;
        }
        else if (isAttacking)
        {
            blockOther = true;
        }
        else
        {
            if (blockActionKeyHold)
            {
                isBlocking = true;
            }
            else
            {
                isBlocking = false;
            }
            master.Animator.SetBool(NovUtil.IsBlockingHash, isBlocking);
            blockOther = isBlocking;
        }
    }
    private void Attack()
    {
        if (isAttacking)
        {
            queueAttack = true;
        }
        else
        {
            lastAttackTime = Time.time;
            isAttacking = true;
        }
        master.Animator.SetTrigger(NovUtil.AttackHash);
    }
    public override void ActionDisturbed(CharacterAction disturber)
    {
        master.Animator.ResetTrigger(NovUtil.AttackHash);
        isAttacking = false;
        queueAttack = false;
    }
    public override void CallAnimationEvent(string animEvent)
    {
        switch (animEvent)
        {
            case "AttackImpact":
                AnimationEvent_AttackImpact();
                break;
            case "AttackFinish":
                AnimationEvent_AttackFinish();
                break;
        }
    }
    private void AnimationEvent_AttackImpact()
    {
        //Collider[] colls = Physics.OverlapBox(transform.position + transform.forward
        //    * attackDistance + master.RB.rotation * attackOffset, halfExtents, master.RB.rotation);

        if (!master.BoxCastForward(attackDistance, halfExtents,
            InternalSettings.CharacterMask, out Collider[] hits)) return;
        
        foreach(Collider hit in hits)
        {
            IHealth health = hit.transform.GetComponentInParent<IHealth>();
            if (health == null || health.Equals(master)) continue;

            health.GetHit(damage, gameObject);
        }

        /*
        if (colls.Length > 0)
        {
            List<Component> list = NovUtil.CreateListFromArray<Component>(colls);
            Component closest = NovUtil.GetClosestFromList<Component>(transform.position,
                50, list, master.Collider);

            if (closest == null) return;
            IHealth health = closest.GetComponent<IHealth>();
            if (health != null) health.GetHit(damage, gameObject);
        }
        */
    }
    private void AnimationEvent_AttackFinish()
    {
        if (!queueAttack)
        {
            isAttacking = false;
            //master.Mesh.transform.localPosition = Vector3.zero;
        }
        else
        {
            queueAttack = false;
        }
    }
    private void AnimationEvent_CombatParry()
    {
        // parry
    }
    private void Debug_PlaceAttackArea()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.GetComponent<Collider>().enabled = false;
        go.transform.position = transform.position + transform.forward
            * attackDistance + master.RB.rotation * attackOffset;
        go.transform.localScale = halfExtents * 2f;
        go.transform.rotation = master.RB.rotation;
        Destroy(go, 10f);
    }
    private void OnDrawGizmosSelected()
    {
        if (!master || !master.PlayerCamera) return;

        Gizmos.DrawCube(
            (master.PlayerCamera.Position + master.PlayerCamera.Position + (master.PlayerCamera.Forward * attackDistance)) / 2f,
            master.PlayerCamera.Rotation * halfExtents * 2f);
    }
}
