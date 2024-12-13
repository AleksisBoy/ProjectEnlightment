using UnityEngine;

[CreateAssetMenu(menuName = "Enlightenment/New Magic Teleport")]
public class ItemMagicTeleport : ItemActive
{
    [Header("Teleport")]
    [SerializeField] private int manaUse = 20;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float minHoldTime = 0.05f;
    [SerializeField] private float hitNormalDistance = 0.4f;
    [SerializeField] private GameObject showTeleportMeshPrefab = null;

    private GameObject showTeleportMesh = null;

    private float holdTime = 0f;
    public override void Init()
    {
        holdTime = 0f;
        showTeleportMesh = Instantiate(showTeleportMeshPrefab);
        showTeleportMesh.gameObject.SetActive(false);
    }
    public override void OnEquip(IActor actor)
    {
        this.actor = actor;
        actor.GetAnimator().SetLayerWeight(actor.GetAnimator().GetLayerIndex(AnimationLayer), 1f);
    }
    public override void EquippedUpdate()
    {

    }
    public override void OnDequip()
    {
        actor.GetAnimator().SetLayerWeight(actor.GetAnimator().GetLayerIndex(AnimationLayer), 0f);
        actor = null;
    }

    public override void OnInputDown()
    {
        holdTime = 0f;
        showTeleportMesh.gameObject.SetActive(true);
        showTeleportMesh.transform.position = new Vector3(0f, -100f, 0f);
    }
    public override void OnInputHold()
    {
        holdTime += Time.deltaTime;
        if(holdTime > minHoldTime)
        {
            // freeze time
            if (actor.BoxCastForward(maxDistance, new Vector3(0.1f, 0.1f, 0.1f), InternalSettings.EnvironmentLayer, out RaycastHit hit, out Vector3 direction))
            {
                showTeleportMesh.transform.position = hit.point + hit.normal * hitNormalDistance;//- new Vector3(0f, actor.GetHeight() / 2f, 0f);
            }
            else
            {
                showTeleportMesh.transform.position = actor.GetGameObject().transform.position + direction * maxDistance + new Vector3(0f, actor.GetHeight(), 0f);
            }
        }
    }
    public override void OnInputUp()
    {
        if (holdTime > minHoldTime)
        {
            // launch animation that will then teleport in animation event
            Teleport();
        }
        showTeleportMesh.gameObject.SetActive(false);
    }
    private void Teleport()
    {
        if (!actor.GetMana().UseMana(manaUse)
            || !actor.CanTeleport()) return;

        if (actor.BoxCastForward(maxDistance, new Vector3(0.1f, 0.1f, 0.1f), InternalSettings.EnvironmentLayer, out RaycastHit hit, out Vector3 direction))
        {
            actor.Teleport(hit.point + hit.normal * hitNormalDistance);
            //actor.GetGameObject().transform.position = hit.point - direction * hitOffset;
        }
        else
        {
            actor.Teleport(actor.GetGameObject().transform.position + direction * maxDistance);
            //actor.GetGameObject().transform.position += direction * maxDistance;
        }
    }
    public override void Disturb()
    {

    }

    public override void CallEvent(NovUtil.AnimEvent combatEvent)
    {

    }

}