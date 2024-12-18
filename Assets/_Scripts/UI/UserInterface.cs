using System.Collections.Generic;
using UnityEngine;

public class UserInterface : MonoBehaviour
{
    [SerializeField] private Crosshair crosshair = null;
    [SerializeField] private List<Crosshair.CrossData> crosshairs = null;
    [SerializeField] private StatsBar statsBar = null;
    [SerializeField] private PickupLog pickupLog = null;

    private Player player;
    private void Start()
    {
        AddCrosshair(Crosshair.Type.Default);
        pickupLog.Init();
    }
    public void SetPlayer(Player player)
    {
        if (this.player) return;

        this.player = player;
        player.AssignOnHealthChanged(OnHealthChanged);
        player.AssignOnManaChanged(OnManaChanged);
        player.Equipment.AssignOnEquippedChanged(OnEquippedChanged);
        player.Equipment.Inventory.AssignOnInventoryChanged(OnInventoryChanged);
    }
    // Stats Bar
    private void OnHealthChanged(int hp, int maxHP)
    {
        statsBar.SetHealth(hp, maxHP);
    }
    private void OnManaChanged(float mana, float maxMana)
    {
        statsBar.SetMana(mana, maxMana);
    }
    public void OnEquippedChanged(Inventory.Item item, bool equipped)
    {
        if (equipped && !(item.get as ItemActive).RightHanded)
        {
            statsBar.SetCurrentEquippedIcon(item);
        }
        else
        {
            statsBar.SetCurrentEquippedIcon(null);
        }
    }
    private void OnInventoryChanged()
    {
        statsBar.UpdateCurrentEquippedIcon();
    }
    // Crosshair
    public void AddCrosshair(Crosshair.Type type)
    {
        foreach(Crosshair.CrossData cross in crosshairs)
        {
            if (cross.type == type)
            {
                crosshair.Add(cross);
                return;
            }
        }
        Debug.Log("Did not find crosshair " + type.ToString());
    }
    public void RemoveCrosshair(Crosshair.Type type)
    {
        foreach(Crosshair.CrossData cross in crosshairs)
        {
            if (cross.type == type)
            {
                crosshair.Remove(cross);
                return;
            }
        }
        Debug.Log("Did not find crosshair " + type.ToString());
    }
    public void ShowHitCrosshair()
    {
        crosshair.ShowHitDisplay();
    }
    // Pickup Log
    public void ItemPickedUp(EItem item, int amount)
    {
        pickupLog.SpawnPickupLogItem(item, amount);
    }
    private void OnDestroy()
    {
        if (!player) return;

        player.RemoveOnHealthChanged(OnHealthChanged);
        player.RemoveOnManaChanged(OnManaChanged);
        player.Equipment.RemoveOnEquippedChanged(OnEquippedChanged);
        player.Equipment.Inventory.RemoveOnInventoryChanged(OnInventoryChanged);
    }
}
