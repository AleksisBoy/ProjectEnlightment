using UnityEngine;

public class PlayerPickupInteraction : PlayerAction
{
    [Header("Pickup Interaction")]
    [SerializeField] private float range = 1f;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private KeyCode actionKey = KeyCode.None;

    private PickupObject currentHighlighted = null;
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;
        if (!master.RaycastForward(range, pickupMask, out RaycastHit hit, QueryTriggerInteraction.Collide))
        {
            ResetCurrentHighlighted();
            return;
        }

        PickupObject pickup = hit.transform.GetComponent<PickupObject>();
        if (pickup == null || !pickup.enabled)
        {
            ResetCurrentHighlighted();
            return;
        }

        AssignCurrentHighlighted(pickup);

        bool actionKeyDown = Input.GetKeyDown(actionKey);
        if (actionKeyDown)
        {
            PickupCurrentHighlighted();
        }
        else
        {
            pickup.HighlightUpdate();
        }
    }
    public override bool ActionBlocked(CharacterAction blocker)
    {
        ResetCurrentHighlighted();
        return true;
    }
    private void ResetCurrentHighlighted()
    {
        if (!currentHighlighted) return;
        
        currentHighlighted.HighlightEnd();
        currentHighlighted = null;
        master.UI.RemoveCrosshair(Crosshair.Type.Grab);
    }
    private void AssignCurrentHighlighted(PickupObject pickup)
    {
        if (currentHighlighted) return;

        currentHighlighted = pickup;
        currentHighlighted.HighlightBegin();
        master.UI.AddCrosshair(Crosshair.Type.Grab);
    }
    private void PickupCurrentHighlighted()
    {
        currentHighlighted.HighlightEnd();
        currentHighlighted.Pickup(master);
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
