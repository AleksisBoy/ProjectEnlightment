using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : PlayerAction
{
    [Header("Combat")]
    [SerializeField] private KeyCode rightHandKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode leftHandKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode blockActionKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode sheatheActionKey = KeyCode.F;
    [SerializeField] private float frontAssassinationMinDot = 0.5f;
    [SerializeField] private float sheathTimer = 0.5f;

    private bool isBlocking = false;

    private bool sheathed = false;
    private float sheathInputTime = 0f;
    private bool sheathInputReset = true;
    public override void Init(params object[] objects)
    {
        base.Init(objects);

        master.Animator.SetFloat(NovUtil.CombatStaminaHash, 1f);
    }
    private void OnEquippedChanged(ItemActive item, bool equipped)
    {
        // swap items animation, other setup?
    }
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        bool rightHandKeyDown = Input.GetKeyDown(rightHandKey);
        bool rightHandKeyHold = Input.GetKey(rightHandKey);
        bool rightHandKeyUp = Input.GetKeyUp(rightHandKey);
        
        bool leftHandKeyDown = Input.GetKeyDown(leftHandKey);
        bool leftHandKeyHold = Input.GetKey(leftHandKey);
        bool leftHandKeyUp = Input.GetKeyUp(leftHandKey);

        bool blockActionKeyHold = Input.GetKey(blockActionKey);

        bool sheatheActionKeyHold = Input.GetKey(sheatheActionKey);
        bool sheatheActionKeyUp = Input.GetKeyUp(sheatheActionKey);

        if (blockActionKeyHold && sheathed)
        {
            isBlocking = true;
            master.Animator.SetBool(NovUtil.IsBlockingHash, isBlocking);
            return; // return if is blocking
        }
        else
        {
            isBlocking = false;
            SheatheInput(sheatheActionKeyHold, sheatheActionKeyUp);
            master.Animator.SetBool(NovUtil.IsBlockingHash, isBlocking);
        }

        if (sheathed)
        {
            master.Equipment.UpdateMainItemInput(rightHandKeyDown, rightHandKeyHold, rightHandKeyUp);
            master.Equipment.UpdateSecondaryItemInput(leftHandKeyDown, leftHandKeyHold, leftHandKeyUp);
        }
    }
    private void SheatheInput(bool sheatheActionKeyHold, bool sheatheActionKeyUp)
    {
        if (sheatheActionKeyUp) { sheathInputReset = true; sheathInputTime = 0f; return; }
        if (!sheathInputReset) return;
        if (!sheatheActionKeyHold) return;
        if (!NovUtil.TimerCheck(ref sheathInputTime, sheathTimer, Time.deltaTime)) return;

        sheathInputReset = false;
        Sheathe(!sheathed);
    }
    private void Sheathe(bool state)
    {
        sheathed = state;
        master.Animator.SetBool(NovUtil.SheathedHash, sheathed);
    }
}
