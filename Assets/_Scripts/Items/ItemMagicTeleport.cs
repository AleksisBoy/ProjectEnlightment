using UnityEngine;

[CreateAssetMenu(menuName = "Enlightenment/New Magic Teleport")]
public class ItemMagicTeleport : ItemActive
{
    [Header("Teleport")]
    [SerializeField] private int manaUse = 20;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float minHoldTime = 0.05f;
    [SerializeField] private float hitNormalDistance = 0.4f;
    [SerializeField] private float teleportSpeed = 8f;
    [SerializeField] private GameObject showTeleportMeshPrefab = null;
    [SerializeField] private Vector3 boxcastHalfExtents = new Vector3(0.2f, 0.2f, 0.2f);

    public override ItemUseData Init()
    {
        TeleportUseData data = new TeleportUseData();
        data.showTeleportMesh = Instantiate(showTeleportMeshPrefab);
        data.showTeleportMesh.gameObject.SetActive(false);
        return data;
    }
    public override void EquippedUpdate(ItemUseData data)
    {
    }

    public override void OnInputDown(ItemUseData data)
    {
        TeleportUseData tpData = data as TeleportUseData;
        tpData.holdTime = 0f;
        tpData.showTeleportMesh.gameObject.SetActive(true);
        tpData.showTeleportMesh.transform.position = new Vector3(0f, -100f, 0f);
    }
    public override void OnInputHold(ItemUseData data)
    {
        TeleportUseData tpData = data as TeleportUseData;
        tpData.holdTime += Time.deltaTime;
        if(tpData.holdTime > minHoldTime)
        {
            // freeze time
            if (tpData.actor.BoxCastForward(maxDistance, boxcastHalfExtents, InternalSettings.EnvironmentLayer, out RaycastHit hit, out Vector3 direction))
            {
                tpData.showTeleportMesh.transform.position = hit.point + hit.normal * hitNormalDistance;//- new Vector3(0f, actor.GetHeight() / 2f, 0f);
            }
            else
            {
                tpData.showTeleportMesh.transform.position = tpData.actor.GetGameObject().transform.position + direction * maxDistance + new Vector3(0f, data.actor.GetHeight(), 0f);
            }
        }
    }
    public override void OnInputUp(ItemUseData data)
    {
        TeleportUseData tpData = data as TeleportUseData;
        if (tpData.holdTime > minHoldTime)
        {
            // launch animation that will then teleport in animation event
            Teleport(data.actor);
        }
        tpData.showTeleportMesh.gameObject.SetActive(false);
    }
    private void Teleport(IActor actor)
    {
        if (!actor.GetMana().UseMana(manaUse)
            || !actor.CanTeleport()) return;

        if (actor.BoxCastForward(maxDistance, boxcastHalfExtents, InternalSettings.EnvironmentLayer, out RaycastHit hit, out Vector3 direction))
        {
            actor.Teleport(hit.point + hit.normal * hitNormalDistance,
                teleportSpeed);
        }
        else
        {
            actor.Teleport(actor.GetGameObject().transform.position + direction * maxDistance, 
                teleportSpeed);
        }
    }
    public override void Disturb(ItemUseData data)
    {

    }

    public override void CallEvent(ItemUseData data, NovUtil.AnimEvent combatEvent)
    {

    }

    public class TeleportUseData : ItemUseData
    {
        public GameObject showTeleportMesh = null;
        public float holdTime = 0f;
    }
}