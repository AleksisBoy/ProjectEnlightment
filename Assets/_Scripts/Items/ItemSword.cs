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

    public override ItemUseData Init()
    {
        return new SwordUseData();
    }
    public override void EquippedUpdate(ItemUseData data)
    {
        StaminaRestoreUpdate(data);
    }
    private void StaminaRestoreUpdate(ItemUseData data)
    {
        SwordUseData swordData = data as SwordUseData;
        if (!NovUtil.TimeCheck(swordData.lastAttackTime, staminaReloadCooldown)) return;

        swordData.stamina = Mathf.Min(staminaMax, swordData.stamina + staminaPerSecond * Time.deltaTime);
        swordData.actor.GetAnimator().SetFloat(NovUtil.CombatStaminaHash, swordData.stamina);
        swordData.actor.ProcessActorData(new IActor.Data(IActor.Data.Type.StaminaPerc, GetStaminaPercentage(swordData.stamina)));
    }

    public override void OnInputDown(ItemUseData data)
    {
        SwordUseData swordData = data as SwordUseData;
        AttackInput(swordData);
    }

    public override void OnInputHold(ItemUseData data)
    { 
    }

    public override void OnInputUp(ItemUseData data)
    {
    }

    private void AttackInput(SwordUseData data)
    {
        if (data.isAttacking)
        {
            data.queueAttack = data.queueWindowOpen;
        }
        else
        {
            Attack(data);
        }
    }
    private void Attack(SwordUseData data)
    {
        data.lastAttackTime = Time.time;
        data.isAttacking = true;
        data.queueWindowOpen = false;
        data.queueAttack = false;
        data.actor.GetAnimator().SetTrigger(NovUtil.AttackHash);
    }
    public void AttackImpact(SwordUseData data)
    {
        data.stamina = Mathf.Max(staminaMin, data.stamina - staminaPerAttack);
        data.actor.GetAnimator().SetFloat(NovUtil.CombatStaminaHash, data.stamina);
        data.actor.ProcessActorData(new IActor.Data(IActor.Data.Type.StaminaPerc, GetStaminaPercentage(data.stamina)));

        if (!data.actor.OverlapBoxForward(attackDistance, halfExtents,
            InternalSettings.CharacterMask, out Collider[] hits)) return;

        foreach (Collider hit in hits)
        {
            IHealth health = hit.transform.GetComponentInParent<IHealth>();
            if (health == null || health.Equals(data.actor.GetHealth())) continue;

            Vector3 direction = hit.transform.position - data.actor.GetGameObject().transform.position;
            direction.y = 0f;
            direction.Normalize();
            float dot = -Vector3.Dot(direction, data.actor.GetGameObject().transform.forward);

            /*
            if (dot < -0.6f) // and its not alert
            {
                health.GetHit(damage * 100, gameObject, out bool diedFromStealth);
                master.PlayerAnimation.LaunchStealthAssassination(health.GetAnimator(),
                    -hit.transform.forward * 0.35f + -hit.transform.right * 0.5f);
                return;
            }
            */

            health.GetHit(damage, data.actor.GetGameObject(), out bool died);
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
    public void AttackFinish(SwordUseData data)
    {
        if (!data.queueAttack)
        {
            data.isAttacking = false;
            data.queueWindowOpen = false;
        }
        else
        {
            Attack(data);
        }
    }
    public void OpenNextAttackWindow(SwordUseData data)
    {
        data.queueWindowOpen = true;
    }
    public void CombatParry()
    {
        // parry
    }
    public override void Disturb(ItemUseData data)
    {
        SwordUseData swordData = data as SwordUseData;
        swordData.actor.GetAnimator().ResetTrigger(NovUtil.AttackHash);
        swordData.isAttacking = false;
        swordData.queueAttack = false;
    }
    public override void CallEvent(ItemUseData data,NovUtil.AnimEvent animEvent)
    {
        SwordUseData swordData = data as SwordUseData;
        switch (animEvent)
        {
            case NovUtil.AnimEvent.AttackImpact:
                AttackImpact(swordData);
                break;
            case NovUtil.AnimEvent.AttackFinish:
                AttackFinish(swordData);
                break;
            case NovUtil.AnimEvent.OpenAttackWindow:
                OpenNextAttackWindow(swordData);
                break;
        }
    }
    private float GetStaminaPercentage(float stamina)
    {
        return NovUtil.GetRangePercentage(stamina, staminaMin, staminaMax);
    }
    public class SwordUseData : ItemUseData
    {
        public float stamina = 1f;

        public bool isAttacking = false;
        public bool queueAttack = false;
        public bool queueWindowOpen = false;

        public float lastAttackTime = 0;

    }
}