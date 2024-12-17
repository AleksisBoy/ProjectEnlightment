using UnityEngine;

public class PlayerWeaponWheel : PlayerAction
{
    [Header("Weapon Wheel")]
    [SerializeField] private KeyCode actionKey = KeyCode.Mouse2;
    [SerializeField] private KeyCode item1Key = KeyCode.Alpha1;
    [SerializeField] private int numericKeysCount = 9;
    [SerializeField] private WeaponWheel weaponWheel = null;
    [SerializeField] private float timeToOpen = 0.15f;
    [SerializeField] private float timeScaleWhileOpen = 0.05f;

    private bool wheelActive = false;
    private float inputHoldTime = 0f;
    public override void Init(params object[] other)
    {
        base.Init(other);
        weaponWheel.Init(numericKeysCount);
        CloseWeaponWheel(); // to remove
    }
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        bool actionKeyHold = Input.GetKey(actionKey);
        bool actionKeyUp = Input.GetKeyUp(actionKey);
        for (int i = 0; i < numericKeysCount; i++)
        {
            // check each numeric key till 9
            if (!Input.GetKeyDown(item1Key + i)) continue;

            EquipItemInQuickSlot(i + 1);
            break;
        }

        if (!actionKeyHold) 
        {
            if (actionKeyUp)
            {
                CloseWeaponWheel();
            }
            return; 
        }

        if (!wheelActive && NovUtil.TimerCheck(ref inputHoldTime, timeToOpen, Time.deltaTime))
        {
            OpenWeaponWheel();
        }
        blockOther = wheelActive;
    }
    private void OpenWeaponWheel()
    {
        wheelActive = true;
        weaponWheel.SetInventory(master.Equipment.Inventory);
        weaponWheel.gameObject.SetActive(true);
        Time.timeScale = timeScaleWhileOpen;
        InternalSettings.EnableCursor(true);
        master.UI.AddCrosshair(Crosshair.Type.None);
    }
    private void CloseWeaponWheel()
    {
        Inventory.Item item = ItemIconUI.GetSelected<ItemIconUI>()?.Item;
        if (item != null && item.get as ItemActive)
        {
            master.Equipment.EquipToggle(item);
        }
        wheelActive = false;
        weaponWheel.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        InternalSettings.EnableCursor(false);
        master.UI.RemoveCrosshair(Crosshair.Type.None);
    }
    private void EquipItemInQuickSlot(int slot)
    {
        ItemIconUI selected = ItemIconUI.GetSelected<ItemIconUI>();
        // if icon is selected and numeric key pressed, set it to quick slot
        if (selected)
        {
            weaponWheel.SetItemInQuickSlot(slot, selected.Item);
            return;
        }

        // if wheel is not active equip item thats in quick slot
        if (!wheelActive)
        {
            Inventory.Item item = weaponWheel.GetItemInQuickSlot(slot);
            if (item) master.Equipment.EquipToggle(item);
        }
    }
}
