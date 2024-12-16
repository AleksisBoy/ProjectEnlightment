using UnityEngine;

[CreateAssetMenu(menuName = "Enlightenment/New Pistol")]
public class ItemPistol : ItemActive
{
    [Header("Pistol")]
    [SerializeField] private int maxDamage = 80;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float shootCooldown = 0.7f;
    [SerializeField] private Vector3 halfExtents = Vector3.zero;

    public override ItemUseData Init()
    {
        return new PistolUseData();
    }
    public override void EquippedUpdate(ItemUseData data)
    {

    }

    public override void OnInputDown(ItemUseData data)
    {
        PistolUseData pistolData = data as PistolUseData;
        if (!NovUtil.TimeCheck(pistolData.lastTimeShot, shootCooldown)) return;

        Shoot(pistolData);
    }

    public override void OnInputHold(ItemUseData data)
    {

    }

    public override void OnInputUp(ItemUseData data)
    {

    }
    public override void CallEvent(ItemUseData data, NovUtil.AnimEvent animEvent)
    {

    }

    public override void Disturb(ItemUseData data)
    {

    }
    private void Shoot(PistolUseData data)
    {
        if (!data.actor.BoxCastForwardAll(maxDistance, halfExtents, InternalSettings.CharacterMask,
            out RaycastHit[] hits)) return;

        foreach (RaycastHit hit in hits)
        {
            IHealth health = hit.transform.GetComponentInParent<IHealth>();
            if (health == null || health.Equals(data.actor.GetHealth())) continue;

            int damage = (int)(maxDamage * (Mathf.Abs(1f - hit.distance / maxDistance)));
            health.GetHit(damage, data.actor.GetGameObject(), out bool died);

            if (died) health.GetAnimator()?.SetTrigger(NovUtil.DiedHash);
        }
        data.actor.GetAnimator().SetTrigger(NovUtil.GunshotHash);
        data.lastTimeShot = Time.time;
    }

    public class PistolUseData : ItemUseData
    {
        public float lastTimeShot = 0f;
    }
}