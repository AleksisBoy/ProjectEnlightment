using UnityEngine;

public class PlayerPickupInteraction : PlayerAction
{
    [Header("Pickup Interaction")]
    [SerializeField] private float range = 1f;
    [SerializeField] private LayerMask pickupMask; // env, character, pickup, ground
    [SerializeField] private KeyCode actionKey = KeyCode.None;

    private PickupObject currentHighlighted = null;
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;
        if (!master.RaycastForwardAll(range, pickupMask, out RaycastHit[] hits, QueryTriggerInteraction.Collide))
        {
            ResetCurrentHighlighted();
            return;
        }

        hits = NovUtil.SortHitsByDistance(hits);
        PickupObject pickup = hits[0].transform.GetComponent<PickupObject>();
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
        if (!pickup || currentHighlighted == pickup) return;
        else currentHighlighted?.HighlightEnd();

        currentHighlighted = pickup;
        currentHighlighted.HighlightBegin();
        master.UI.AddCrosshair(Crosshair.Type.Grab);
    }
    private void PickupCurrentHighlighted()
    {
        currentHighlighted.HighlightEnd();
        currentHighlighted.Pickup(master);
        ResetCurrentHighlighted();
    }
    private void OnDrawGizmosSelected()
    {
        if (master && master.Camera)
        {
            Gizmos.DrawWireSphere(master.Camera.Position, range);
            Gizmos.DrawLine(master.Camera.Position, master.Camera.Position + master.Camera.Forward * 4f);
        }
    }
}
