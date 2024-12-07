using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponWheel : MonoBehaviour
{
    [SerializeField] private ItemIconUI itemIconPrefab = null;
    [SerializeField] private TMP_Text selectedItemInfo = null;
    [SerializeField] private CircularGrid grid = null;

    private List<ItemIconUI> icons = new List<ItemIconUI>();

    private Inventory inventory = null;

    public void Init()
    {
        foreach(Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
        ItemIconUI.Assign_OnSelectedChanged(OnSelectedIconChanged);
        selectedItemInfo.text = string.Empty;
    }
    private void OnSelectedIconChanged()
    {
        selectedItemInfo.text = ItemIconUI.Selected ? ItemIconUI.Selected.Item.get.Description : string.Empty;
    }
    private void OnEnable()
    {
        SetWheel();
    }
    private void OnDisable()
    {
        
    }
    private void SetWheel()
    {
        if (inventory == null) return;

        List<Inventory.Item> items = inventory.Items;
        for (int i = 0; i < inventory.ItemCount; i++)
        {
            ItemIconUI icon;
            if (i >= icons.Count)
            {
                icon = Instantiate(itemIconPrefab, grid.transform);
                icons.Insert(i, icon);
            }
            else
            {
                icon = icons[i];
            }
            icon.SetItem(items[i]);
        }

        grid.SetGrid();
    }
    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
    }
}
