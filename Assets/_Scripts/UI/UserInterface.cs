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
        this.player = player;
        player.AssignOnHealthChanged(OnHealthChanged);
    }
    // Stats Bar
    private void OnHealthChanged(int hp, int maxHP)
    {
        statsBar.SetHealth(hp, maxHP);
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
    // Pickup Log
    public void ItemPickedUp(EItem item, int amount)
    {
        pickupLog.SpawnPickupLogItem(item, amount);
    }
    private void OnDestroy()
    {
        if (player) player.RemoveOnHealthChanged(OnHealthChanged);
    }
}
