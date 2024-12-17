using UnityEngine;

[CreateAssetMenu(menuName = "Enlightenment/New Crossbow")]
public class ItemCrossbow : ItemActive
{
    [Header("Crossbow")]
    [SerializeField] private int damage = 40;
    [SerializeField] private float shootCooldown = 0.7f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private Missile missilePrefab = null;
    [SerializeField] private float missileForce = 20f;
    [SerializeField] private float raycastDistance = 10f;
    public override ItemUseData Init()
    {
        return new CrossbowUseData();
    }
    public override void EquippedUpdate(ItemUseData data)
    {
    }
    public override void OnInputDown(ItemUseData data)
    {
        CrossbowUseData crossData = data as CrossbowUseData;
        if (!NovUtil.TimeCheck(crossData.lastTimeShot, shootCooldown)) return;

        Shoot(crossData);
    }

    private void Shoot(CrossbowUseData data)
    {
        Vector3 pos = data.actor.GetGameObject().transform.position + Vector3.up * data.actor.GetHeight();
        if (data.actor.RaycastForward(raycastDistance, InternalSettings.EnvironmentLayer, out RaycastHit hit, out Vector3 dir))
        {
            dir = (hit.point - data.instance.transform.position).normalized;
        }
        else
        {
            dir = ((pos + dir * raycastDistance) - data.instance.transform.position).normalized;
        }

        Missile missile = Instantiate(missilePrefab, data.instance.transform.position, data.instance.transform.rotation);
        missile.Set(dir.normalized, missileForce, damage, data.actor.GetCollider());

        data.actor.GetAnimator().SetTrigger(NovUtil.CrossbowShotHash);
        data.lastTimeShot = Time.time;
    }
    public override void OnInputHold(ItemUseData data)
    {
    }

    public override void OnInputUp(ItemUseData data)
    {
    }

    public override void CallEvent(ItemUseData data, NovUtil.AnimEvent combatEvent)
    {
    }

    public override void Disturb(ItemUseData data)
    {
    }
    public class CrossbowUseData : ItemUseData
    {
        public float lastTimeShot = 0f;
    }
}