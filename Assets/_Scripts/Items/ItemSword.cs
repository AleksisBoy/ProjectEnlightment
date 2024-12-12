using UnityEngine;

[CreateAssetMenu(menuName = "Enlightenment/New Sword")]
public class ItemSword : ItemActive
{
    [Header("Sword")]
    [SerializeField] private float attackDistance = 0.3f;
    [SerializeField] private int damage = 25;
    [SerializeField] private Vector3 halfExtents = new Vector3(0.1f, 0.5f, 0.1f);

    [SerializeField] private float staminaPerAttack = 0.06f;
    [SerializeField] private float staminaPerSecond = 0.16f;
    [SerializeField] private float staminaReloadCooldown = 1.2f;
    [SerializeField] private float staminaMin = 0.66f;
    [SerializeField] private float staminaMax = 1f;

    private float stamina = 1f;

    private bool isAttacking = false;
    private bool queueAttack = false;
    private bool queueWindowOpen = false;

    private float lastAttackTime = 0;

    public override void Init()
    {
        stamina = 1f;
        lastAttackTime = 0f;
        isAttacking = false;
        queueAttack = false;
        queueWindowOpen = false;
    }
    public override void OnEquip(IActor actor)
    {
        this.actor = actor;
    }
    public override void EquippedUpdate()
    {
        StaminaRestoreUpdate();
    }
    private void StaminaRestoreUpdate()
    {
        if (!NovUtil.TimeCheck(lastAttackTime, staminaReloadCooldown)) return;

        stamina = Mathf.Min(staminaMax, stamina + staminaPerSecond * Time.deltaTime);
        actor.GetAnimator().SetFloat(NovUtil.CombatStaminaHash, stamina);
        actor.ProcessActorData(new IActor.Data(IActor.Data.Type.StaminaPerc, GetStaminaPercentage()));
    }
    public override void OnDequip()
    {
        Debug.Log(Name + " dequipped");
    }

    public override void OnInputDown()
    {
        AttackInput();
    }

    public override void OnInputHold()
    { 
    }

    public override void OnInputUp()
    {
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
        actor.GetAnimator().SetTrigger(NovUtil.AttackHash);
    }
    public void AttackImpact()
    {
        stamina = Mathf.Max(staminaMin, stamina - staminaPerAttack);
        actor.GetAnimator().SetFloat(NovUtil.CombatStaminaHash, stamina);
        actor.ProcessActorData(new IActor.Data(IActor.Data.Type.StaminaPerc, GetStaminaPercentage()));

        if (!actor.OverlapBoxForward(attackDistance, halfExtents,
            InternalSettings.CharacterMask, out Collider[] hits)) return;

        foreach (Collider hit in hits)
        {
            IHealth health = hit.transform.GetComponentInParent<IHealth>();
            if (health == null || health.Equals(actor.GetHealth())) continue;

            Vector3 direction = hit.transform.position - actor.GetGameObject().transform.position;
            direction.y = 0f;
            direction.Normalize();
            float dot = -Vector3.Dot(direction, actor.GetGameObject().transform.forward);

            /*
            if (dot < -0.6f) // and its not alert
            {
                health.GetHit(damage * 100, gameObject, out bool diedFromStealth);
                master.PlayerAnimation.LaunchStealthAssassination(health.GetAnimator(),
                    -hit.transform.forward * 0.35f + -hit.transform.right * 0.5f);
                return;
            }
            */

            health.GetHit(damage, actor.GetGameObject(), out bool died);
            if (!died) return;

            /*
            if (dot > frontAssassinationMinDot)
            {
                master.PlayerAnimation.LaunchAssassination(health.GetAnimator(),
                    hit.transform.forward / 2f + hit.transform.right / 2f);
            }
            */
            else
            {
                health.GetAnimator()?.SetTrigger(NovUtil.DiedHash);
            }
        }
    }
    public void AttackFinish()
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
    public void OpenNextAttackWindow()
    {
        queueWindowOpen = true;
    }
    public void CombatParry()
    {
        // parry
    }
    public override void Disturb()
    {
        actor.GetAnimator().ResetTrigger(NovUtil.AttackHash);
        isAttacking = false;
        queueAttack = false;
    }
    public override void CallEvent(NovUtil.AnimEvent animEvent)
    {
        switch (animEvent)
        {
            case NovUtil.AnimEvent.AttackImpact:
                AttackImpact();
                break;
            case NovUtil.AnimEvent.AttackFinish:
                AttackFinish();
                break;
            case NovUtil.AnimEvent.OpenAttackWindow:
                OpenNextAttackWindow();
                break;
        }
    }
    private float GetStaminaPercentage()
    {
        return NovUtil.GetRangePercentage(stamina, staminaMin, staminaMax);
    }
}