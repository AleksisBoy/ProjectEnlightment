using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : PlayerAction
{
    [Header("Combat")]
    [SerializeField] private KeyCode rightHandKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode leftHandKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode blockActionKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode sheatheActionKey = KeyCode.F;
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
    public override void Init(params object[] objects)
    {
        base.Init(objects);

        combatStamina = 1f;
        master.Animator.SetFloat(NovUtil.CombatStaminaHash, combatStamina);
        master.Equipment.AssignOnEquippedChanged(OnEquippedChanged);
    }
    private void OnEquippedChanged(ItemActive item, bool equipped)
    {
        // swap items animation, other setup?
        
    }
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        bool rightHandKeyDown = Input.GetKeyDown(rightHandKey);
        bool rightHandKeyHold = Input.GetKey(rightHandKey);
        bool rightHandKeyUp = Input.GetKeyUp(rightHandKey);
        
        bool leftHandKeyDown = Input.GetKeyDown(leftHandKey);
        bool leftHandKeyHold = Input.GetKey(leftHandKey);
        bool leftHandKeyUp = Input.GetKeyUp(leftHandKey);

        bool blockActionKeyHold = Input.GetKey(blockActionKey);

        bool sheatheActionKeyHold = Input.GetKey(sheatheActionKey);
        bool sheatheActionKeyUp = Input.GetKeyUp(sheatheActionKey);

        if (rightHandKeyDown) master.Equipment.MainItem?.OnInputDown();
        if (rightHandKeyHold) master.Equipment.MainItem?.OnInputHold();
        if (rightHandKeyUp) master.Equipment.MainItem?.OnInputUp();

        if (leftHandKeyDown) master.Equipment.SecondaryItem?.OnInputDown();
        if (leftHandKeyHold) master.Equipment.SecondaryItem?.OnInputHold();
        if (leftHandKeyUp) master.Equipment.SecondaryItem?.OnInputUp();

        if (rightHandKeyDown && !isBlocking && sheathed)
        {
            AttackInput();
            //blockOther = true;
        }
        else if (isAttacking)
        {
            //blockOther = true;
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
            //blockOther = isBlocking;
        }

        StaminaRestoreUpdate();
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

        sheathInputReset = false;
        Sheathe(!sheathed);
    }
    private void Sheathe(bool state)
    {
        sheathed = state;
        master.Animator.SetBool(NovUtil.SheathedHash, sheathed);
    }
    private void StaminaRestoreUpdate()
    {
        if (!NovUtil.TimeCheck(lastAttackTime, staminaReloadCooldown)) return;

        combatStamina = Mathf.Min(staminaMax, combatStamina + staminaPerSecond * Time.deltaTime);
        master.Animator.SetFloat(NovUtil.CombatStaminaHash, combatStamina);
        master.Camera.SetNoise((staminaMax - combatStamina) / (staminaMax - staminaMin));
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
        master.Camera.SetNoise((staminaMax - combatStamina) / (staminaMax - staminaMin));

        if (!master.OverlapBoxForward(attackDistance, halfExtents,
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
        if (!master || !master.Camera) return;

        Gizmos.DrawCube(
            (master.Camera.Position + master.Camera.Position + (master.Camera.Forward * attackDistance)) / 2f,
            master.Camera.Rotation * halfExtents * 2f);
    }
    private void OnGUI()
    {
        if (Time.timeScale < 1f) return;
        //GUI.Label(new Rect(10, 70, 500, 80), string.Format("Combat Stamina: {0:F}", combatStamina), InternalSettings.DebugStyle);

    }
}
