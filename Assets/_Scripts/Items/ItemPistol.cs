using UnityEngine;

[CreateAssetMenu(menuName = "Enlightenment/New Pistol")]
public class ItemPistol : ItemActive
{
    [Header("Pistol")]
    [SerializeField] private int maxDamage = 80;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float shootCooldown = 0.7f;
    [SerializeField] private Vector3 halfExtents = Vector3.zero;

    private float lastTimeShot = 0f;
    public override void Init()
    {
        lastTimeShot = 0f;
    }
    public override void OnEquip(IActor actor)
    {
        lastTimeShot = Time.time - (shootCooldown / 2f);
        this.actor = actor;
    }

    public override void EquippedUpdate()
    {

    }
    public override void OnDequip()
    {
        Debug.Log(Name + " dequipped");
    }

    public override void OnInputDown()
    {
        if (!NovUtil.TimeCheck(lastTimeShot, shootCooldown)) return;

        Shoot();
    }

    public override void OnInputHold()
    {

    }

    public override void OnInputUp()
    {

    }
    public override void CallEvent(NovUtil.AnimEvent animEvent)
    {

    }

    public override void Disturb()
    {

    }
    private void Shoot()
    {
        if (!actor.BoxCastForward(maxDistance, halfExtents, InternalSettings.CharacterMask,
            out RaycastHit[] hits)) return;

        foreach (RaycastHit hit in hits)
        {
            IHealth health = hit.transform.GetComponentInParent<IHealth>();
            if (health == null || health.Equals(actor.GetHealth())) continue;

            int damage = (int)(maxDamage * (Mathf.Abs(1f - hit.distance / maxDistance)));
            health.GetHit(damage, actor.GetGameObject(), out bool died);

            if (died) health.GetAnimator()?.SetTrigger(NovUtil.DiedHash);
        }
        actor.GetAnimator().SetTrigger(NovUtil.GunshotHash);
        lastTimeShot = Time.time;
    }

}