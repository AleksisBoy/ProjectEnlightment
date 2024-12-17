using UnityEngine;

[CreateAssetMenu(menuName = "Enlightenment/New Grenade")]
public class ItemGrenade : ItemActive
{
    [Header("Grenade")]
    [SerializeField] private Grenade grenadePrefab = null;
    [SerializeField] private float triggerDuration = 5f;
    [SerializeField] private float maxDamage = 200;
    [SerializeField] private float blowRadius = 10f;
    [SerializeField] private float inputDelay = 0.05f;
    [SerializeField] private float throwForce = 10f;

    public override ItemUseData Init()
    {
        return new GrenadeUseData();
    }
    public override void OnEquip(ItemUseData data, IActor actor)
    {
        base.OnEquip(data, actor);
        
    }
    public override void EquippedUpdate(ItemUseData data)
    {

    }
    public override void OnInputDown(ItemUseData data)
    {
        if (data.amount <= 0) return;

        GrenadeUseData grenData = data as GrenadeUseData;
        grenData.holdTime = 0f;
        grenData.prepare = false;
    }

    public override void OnInputHold(ItemUseData data)
    {
        GrenadeUseData grenData = data as GrenadeUseData;
        grenData.holdTime += Time.deltaTime;
        if (!grenData.prepare && grenData.holdTime > inputDelay)
        {
            grenData.prepare = true;
            grenData.actor.GetAnimator().SetTrigger("ThrowPrepare");
        }
    }

    public override void OnInputUp(ItemUseData data)
    {
        GrenadeUseData grenData = data as GrenadeUseData; 
        if (grenData.prepare)
        {
            ThrowGrenadeStart(grenData);
        }
    }
    private void ThrowGrenadeStart(GrenadeUseData grenData)
    {
        grenData.actor.GetAnimator().SetTrigger("Throw");
        grenData.amount--;
        grenData.actor.ProcessActorData(new IActor.Data(!RightHanded ? IActor.Data.Type.DecrementLeftHand : IActor.Data.Type.DecrementRightHand, 0f));
    }
    private void ThrowGrenade(GrenadeUseData grenData)
    {
        Grenade grenade = Instantiate(grenadePrefab, grenData.instance.transform.position, grenData.instance.transform.rotation);
        grenade.Set(this, grenData, throwForce);
    }
    public override void CallEvent(ItemUseData data, NovUtil.AnimEvent animEvent)
    {
        switch (animEvent)
        {
            case NovUtil.AnimEvent.GrenadeThrown:
                ThrowGrenade(data as GrenadeUseData);
                break;
        }
    }

    public override void Disturb(ItemUseData data)
    {

    }
    public float TriggerDuration => triggerDuration;
    public float MaxDamage => maxDamage;
    public float BlowRadius => blowRadius;
    public class GrenadeUseData : ItemUseData
    {
        public float holdTime = 0f;
        public bool prepare = false;
    }
}
