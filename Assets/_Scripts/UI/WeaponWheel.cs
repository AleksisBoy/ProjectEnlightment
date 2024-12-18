using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponWheel : MonoBehaviour
{
    [SerializeField] private ItemIconUI itemIconPrefab = null;
    [SerializeField] private ItemQuickSlotUI itemQuickSlotPrefab = null;
    [SerializeField] private TMP_Text selectedItemInfo = null;
    [SerializeField] private CircularGrid grid = null;
    [SerializeField] private HorizontalLayoutGroup quickSlotGrid = null;
    [SerializeField] private ItemIconUI hpotionIcon = null;
    [SerializeField] private ItemIconUI mpotionIcon = null;
    [SerializeField] private List<EItem> excludeList = new List<EItem>();

    private List<ItemIconUI> icons = new List<ItemIconUI>();
    private List<ItemQuickSlotUI> quickSlots = new List<ItemQuickSlotUI>();

    private Inventory inventory = null;

    public void Init(int quickSlotsCount)
    {
        foreach(Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
        ItemIconUI.Assign_OnSelectedChanged(OnSelectedIconChanged);
        selectedItemInfo.text = string.Empty;
        SetQuickSlots(quickSlotsCount);
    }
    private void OnSelectedIconChanged()
    {
        selectedItemInfo.text = ItemIconUI.GetSelected(typeof(ItemIconUI)) ? ItemIconUI.GetSelected(typeof(ItemIconUI)).Item.get.Description : string.Empty;
    }
    private void OnEnable()
    {
        SetWheel();
    }
    private void OnDisable()
    {
        
    }
    private void UpdatePotionIcons()
    {
        if (!enabled) return;

        if (inventory.HasItem(Inventory.ItemName_HPotion, out Inventory.Item hpotion))
        {
            hpotionIcon.SetItem(hpotion);
        }
        if (inventory.HasItem(Inventory.ItemName_MPotion, out Inventory.Item mpotion))
        {
            mpotionIcon.SetItem(mpotion);
        }
    }
    private void SetWheel()
    {
        if (inventory == null) return;

        List<Inventory.Item> items = inventory.Items;

        UpdatePotionIcons();

        int passiveCount = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].get is not ItemActive || excludeList.Contains(items[i].get))
            {
                passiveCount++;
                continue;
            }

            ItemIconUI icon;
            if (i - passiveCount >= icons.Count)
            {
                icon = Instantiate(itemIconPrefab, grid.transform);
                icons.Add(icon);
            }
            else
            {
                icon = icons[i - passiveCount];
            }
            icon.SetItem(items[i]);
        }
        selectedItemInfo.text = string.Empty;
        grid.SetGrid();
    }
    private void SetQuickSlots(int count)
    {
        foreach(Transform child in quickSlotGrid.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 1; i < count + 1; i++)
        {
            ItemQuickSlotUI slot = Instantiate(itemQuickSlotPrefab, quickSlotGrid.transform);
            slot.SetItem(null, i);
            quickSlots.Add(slot);
        }
    }
    public void SetItemInQuickSlot(int slot, Inventory.Item item)
    {
        if (slot < 1 || slot > quickSlots.Count + 1) return;

        foreach(ItemQuickSlotUI quickSlot in quickSlots)
        {
            if (quickSlot.Item == item)
            {
                quickSlot.SetItem(null);
            }
        }

        quickSlots[slot - 1].SetItem(item, slot);
    }
    public Inventory.Item GetItemInQuickSlot(int slot)
    {
        foreach(ItemQuickSlotUI quickSlot in quickSlots)
        {
            if (quickSlot.Numeric == slot)
            {
                return quickSlot.Item;
            }
        }
        return null;
    }
    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
        inventory.AssignOnInventoryChanged(UpdatePotionIcons);
    }
    private void OnDestroy()
    {
        if (inventory != null) inventory.RemoveOnInventoryChanged(UpdatePotionIcons);
    }
}
