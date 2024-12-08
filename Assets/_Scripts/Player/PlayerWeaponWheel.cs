using UnityEngine;

public class PlayerWeaponWheel : PlayerAction
{
    [Header("Weapon Wheel")]
    [SerializeField] private KeyCode actionKey = KeyCode.Mouse2;
    [SerializeField] private WeaponWheel weaponWheel = null;
    [SerializeField] private float timeToOpen = 0.15f;
    [SerializeField] private float timeScaleWhileOpen = 0.05f;

    private bool wheelActive = false;
    private float inputHoldTime = 0f;
    private void Start()
    {
        weaponWheel.Init();
        CloseWeaponWheel();
    }
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        bool actionKeyHold = Input.GetKey(actionKey);
        bool actionKeyUp = Input.GetKeyUp(actionKey);

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
        weaponWheel.SetInventory(master.Inventory);
        weaponWheel.gameObject.SetActive(true);
        Time.timeScale = timeScaleWhileOpen;
        InternalSettings.EnableCursor(true);
        master.UI.AddCrosshair(Crosshair.Type.None);
    }
    private void CloseWeaponWheel()
    {
        wheelActive = false;
        weaponWheel.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        InternalSettings.EnableCursor(false);
        master.UI.RemoveCrosshair(Crosshair.Type.None);
    }
}
