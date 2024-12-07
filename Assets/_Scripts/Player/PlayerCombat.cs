using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : PlayerAction
{
    [Header("Combat")]
    [SerializeField] private KeyCode attackActionKey;
    [SerializeField] private KeyCode blockActionKey;
    [SerializeField] private KeyCode sheatheActionKey;
    [SerializeField] private float attackDistance = 0.3f;
    [SerializeField] private int damage = 25;
    [SerializeField] private Vector3 attackOffset = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 halfExtents = new Vector3(0.1f, 0.5f, 0.1f);
    [SerializeField] private float frontAssassinationMinDot = 0.5f;
    [SerializeField] private float sheathTimer = 0.5f;

    [SerializeField] private float staminaPerSecond = 0.1f;
    [SerializeField] private float staminaReloadCooldown = 2f;
    [SerializeField] private float staminaPerAttack = 0.1f;
    [SerializeField] private float staminaMin = 0.5f;
    [SerializeField] private float staminaMax = 1f;

    private bool isAttacking = false;
    private bool queueAttack = false;
    private bool queueWindowOpen = false;
    private float lastAttackTime = 0;

    private float combatStamina = 0f;

    private bool isBlocking = false;

    private bool sheathed = false;
    private float sheathInputTime = 0f;
    private bool sheathInputReset = true;
    private void Start()
    {
        combatStamina = 1f;
        master.Animator.SetFloat(NovUtil.CombatStaminaHash, combatStamina);
    }
    public override void ActionUpdate(out bool blockOther)
    {
        bool attackActionKeyDown = Input.GetKeyDown(attackActionKey);
        bool blockActionKeyHold = Input.GetKey(blockActionKey);
        bool sheatheActionKeyHold = Input.GetKey(sheatheActionKey);
        bool sheatheActionKeyUp = Input.GetKeyUp(sheatheActionKey);
        
        if (attackActionKeyDown && !isBlocking && sheathed)
        {
            AttackInput();
            blockOther = true;
        }
        else if (isAttacking)
        {
            blockOther = true;
        }
        else
        {
            if (blockActionKeyHold && sheathed)
            {
                isBlocking = true;
            }
            else
            {
                isBlocking = false;
                SheatheInput(sheatheActionKeyHold, sheatheActionKeyUp);
            }
            master.Animator.SetBool(NovUtil.IsBlockingHash, isBlocking);
            blockOther = isBlocking;
        }

        if (NovUtil.TimeCheck(lastAttackTime, staminaReloadCooldown))
        {
            combatStamina = Mathf.Min(staminaMax, combatStamina + staminaPerSecond * Time.deltaTime);
            master.Animator.SetFloat(NovUtil.CombatStaminaHash, combatStamina);
        }
    }
    private void AttackInput()
    {
        if (isAttacking)
        {
            queueAttack = queueWindowOpen;
        }
        else
        {
            Attack();
        }
    }
    private void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        queueWindowOpen = false;
        queueAttack = false;
        master.Animator.SetTrigger(NovUtil.AttackHash);
    }
    private void SheatheInput(bool sheatheActionKeyHold, bool sheatheActionKeyUp)
    {
        if (sheatheActionKeyUp) { sheathInputReset = true; sheathInputTime = 0f; return; }
        if (!sheathInputReset) return;
        if (!sheatheActionKeyHold) return;
        if (!NovUtil.TimerCheck(ref sheathInputTime, sheathTimer, Time.deltaTime)) return;

        Sheathe();
    }
    private void Sheathe()
    {
        sheathed = !sheathed;
        sheathInputReset = false;
        master.Animator.SetBool(NovUtil.SheathedHash, sheathed);
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
            case "OpenAttackWindow":
                AnimationEvent_OpenNextAttackWindow();
                break;
        }
    }
    private void AnimationEvent_AttackImpact()
    {
        combatStamina = Mathf.Max(staminaMin, combatStamina - staminaPerAttack);
        master.Animator.SetFloat(NovUtil.CombatStaminaHash, combatStamina);

        if (!master.BoxCastForward(attackDistance, halfExtents,
            InternalSettings.CharacterMask, out Collider[] hits)) return;
        
        foreach (Collider hit in hits)
        {
            IHealth health = hit.transform.GetComponentInParent<IHealth>();
            if (health == null || health.Equals(master)) continue;

            Vector3 direction = hit.transform.position - transform.position;
            direction.y = 0f;
            direction.Normalize();
            float dot = -Vector3.Dot(direction, transform.forward);
            if (dot < -0.6f) // and its not alert
            {
                health.GetHit(damage * 100, gameObject, out bool diedFromStealth);
                master.PlayerAnimation.LaunchStealthAssassination(health.GetAnimator(),
                    -hit.transform.forward * 0.35f + -hit.transform.right * 0.5f);
                return;
            }
            
            health.GetHit(damage, gameObject, out bool died);
            if (!died) return;
            
            if (dot > frontAssassinationMinDot)
            {
                master.PlayerAnimation.LaunchAssassination(health.GetAnimator(),
                    hit.transform.forward / 2f + hit.transform.right / 2f);
            }
            else
            {
                health.GetAnimator()?.SetTrigger(NovUtil.DiedHash);
            }
        }
    }
    private void AnimationEvent_AttackFinish()
    {
        if (!queueAttack)
        {
            isAttacking = false;
            queueWindowOpen = false;
        }
        else
        {
            Attack();
        }
    }
    private void AnimationEvent_OpenNextAttackWindow()
    {
        queueWindowOpen = true;
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
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 310, 500, 80), string.Format("Combat Stamina: {0:F}", combatStamina), InternalSettings.DebugStyle);

    }
}
