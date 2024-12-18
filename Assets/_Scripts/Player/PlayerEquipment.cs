using System.Collections.Generic;
using UnityEngine;
using static ItemActive;
using static Inventory;

public class PlayerEquipment : PlayerAction
{
    [Header("Equipment")]
    [SerializeField] private KeyCode healKey = KeyCode.R;
    [SerializeField] private KeyCode manaKey = KeyCode.T;
    [SerializeField] private Transform rightHandTransform = null;
    [SerializeField] private Transform leftHandTransform = null;

    private bool changingEquip = false;

    private Inventory inventory;
    public Inventory Inventory => inventory;

    private ItemActive mainItem = null;
    public ItemActive MainItem => mainItem;

    private ItemActive secondaryItem = null;
    public ItemActive SecondaryItem => secondaryItem;

    public delegate void OnEquippedChanged(Item item, bool equipped);
    private OnEquippedChanged onEquippedChanged;

    private Dictionary<ItemActive, ItemUseData> storedItemData = new Dictionary<ItemActive, ItemUseData>();

    public override void Init(params object[] objects)
    {
        base.Init(objects);

        inventory = InternalSettings.GetDefaultPlayerInventory();
    }
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        bool healKeyDown = Input.GetKeyDown(healKey);
        bool manaKeyDown = Input.GetKeyDown(manaKey);

        if (healKeyDown)
        {
            HealWithPotion();
        }
        if (manaKeyDown)
        {
            RestoreManaWithPotion();
        }
        MainItem?.EquippedUpdate(GetItemUseData(MainItem));
        SecondaryItem?.EquippedUpdate(GetItemUseData(SecondaryItem));
    }
    private void HealWithPotion()
    {
        if (!inventory.HasItem(Inventory.ItemName_HPotion, out Inventory.Item hpotion) || hpotion.amount <= 0) return; // add feedback that not enough potions
        if (master.TryHealFromMax(InternalSettings.HealPotionStrength)) inventory.Decrease(hpotion, 1);
    }
    private void RestoreManaWithPotion()
    {
        if (!inventory.HasItem(Inventory.ItemName_MPotion, out Inventory.Item mpotion) || mpotion.amount <= 0) return; // add feedback that not enough potions
        if (master.TryRestoreMana(InternalSettings.ManaPotionRestore)) inventory.Decrease(mpotion, 1);
    }
    // Equipment
    public void AddItem(EItem item, int amount)
    {
        inventory.Add(item, amount);
        if(item == secondaryItem)
        {
            ItemUseData itemData = GetItemUseData(secondaryItem);
            itemData.amount += amount;
        }
    }
    public void Decrease(EItem item, int amount)
    {
        inventory.Decrease(item, amount);
        if (item == secondaryItem)
        {
            ItemUseData itemData = GetItemUseData(secondaryItem);
            itemData.amount -= amount;
        }
    }
    public void Equip(ItemActive item)
    {
        if (inventory.HasItem(item, out Inventory.Item invItem))
        {
            Equip(invItem);
        }
    }
    public void Equip(Item invItem)
    {
        ItemActive item = invItem.get as ItemActive;
        if (item.RightHanded)
        {
            if (mainItem)
            {
                GetItemUseData(mainItem).instance.SetActive(false);
            }
            PutItemInHand(item, rightHandTransform);
            mainItem = item;
        }
        else
        {
            if (secondaryItem && inventory.HasItem(secondaryItem, out Item secInvItem))
            {
                Dequip(secInvItem);
            }
            PutItemInHand(item, leftHandTransform);
            secondaryItem = item;
        }
        ItemUseData itemData = GetItemUseData(item);
        itemData.amount = invItem.amount;
        item.OnEquip(itemData, master);
        onEquippedChanged?.Invoke(invItem, true);
    }
    private void PutItemInHand(ItemActive item, Transform handTransform)
    {
        ItemUseData itemData = GetItemUseData(item);
        if (itemData == null)
        {
            itemData = InternalSettings.SpawnItemData(item, handTransform);
            storedItemData.Add(item, itemData);
        }
        itemData.instance.SetActive(true);
        handTransform.localPosition = item.HandTransformLocalPosition;
        handTransform.localEulerAngles = item.HandTransformRotation;
    }
    public void Dequip(Item invIitem)
    {
        ItemActive item = invIitem.get as ItemActive;
        if (secondaryItem == item)
        {
            ItemUseData itemData = GetItemUseData(secondaryItem);
            itemData.instance.SetActive(false);
            item.OnDequip(itemData);
            secondaryItem = null;
            onEquippedChanged?.Invoke(invIitem, false);
        }
        else
        {
            Debug.Log("Cannot dequip " + item.Name);
        }
    }
    public void EquipToggle(Item item)
    {
        if (secondaryItem == item.get)
        {
            Dequip(item);
        }
        else if (secondaryItem != item.get)
        {
            Equip(item);
        }
    }
    public void UpdateMainItemInput(bool down, bool hold, bool up)
    {
        if (!MainItem) return;

        UpdateItemInput(MainItem, down, hold, up);
    }
    public void UpdateSecondaryItemInput(bool down, bool hold, bool up)
    {
        if (!SecondaryItem) return;

        UpdateItemInput(SecondaryItem, down, hold, up);
    }
    private void UpdateItemInput(ItemActive item, bool down, bool hold, bool up)
    {
        if (down) item.OnInputDown(GetItemUseData(item));
        if (hold) item.OnInputHold(GetItemUseData(item));
        if (up) item.OnInputUp(GetItemUseData(item));
    }
    public override void ActionDisturbed(CharacterAction disturber)
    {
        MainItem?.Disturb(GetItemUseData(MainItem));
        SecondaryItem?.Disturb(GetItemUseData(SecondaryItem));
    }
    public override void CallAnimationEvent(NovUtil.AnimEvent animEvent)
    {
        // need to know what hand calls the event
        MainItem?.CallEvent(GetItemUseData(MainItem), animEvent);
        SecondaryItem?.CallEvent(GetItemUseData(SecondaryItem),animEvent);
    }
    private ItemUseData GetItemUseData(ItemActive item)
    {
        if (!storedItemData.ContainsKey(item)) return null;
        return storedItemData[item];
    }
    public void AssignOnEquippedChanged(OnEquippedChanged action)
    {
        onEquippedChanged += action;
    }
    public void RemoveOnEquippedChanged(OnEquippedChanged action)
    {
        onEquippedChanged -= action;
    }
    public Transform RightHand => rightHandTransform;
    public Transform LeftHand => leftHandTransform;
}
