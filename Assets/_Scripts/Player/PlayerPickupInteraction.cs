using UnityEngine;

public class PlayerPickupInteraction : PlayerAction
{
    [Header("Pickup Interaction")]
    [SerializeField] private bool debugOn = false;
    [SerializeField] private float range = 1f;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private KeyCode actionKey = KeyCode.None;
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;
        if (!master.RaycastForward(range, pickupMask, out RaycastHit hit, QueryTriggerInteraction.Collide)) return;

        IPickup pickup = hit.transform.GetComponent<IPickup>();
        if (pickup == null) return;

        pickup.HighlightUpdate();
        if (!Input.GetKeyDown(actionKey)) return;

        pickup.Pickup(master);
    }
    private void OnDrawGizmosSelected()
    {
        if (master && master.PlayerCamera)
        {
            Gizmos.DrawWireSphere(master.PlayerCamera.Position, range);
            Gizmos.DrawLine(master.PlayerCamera.Position, master.PlayerCamera.Position + master.PlayerCamera.Forward * 4f);
        }
    }
}
