using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private Inventory.Item healingPotions = null;
    private Inventory.Item manaPotions = null;
    private Inventory.Item abilityPoints = null;
    private Inventory.Item money = null;

    private List<Item> items = new List<Item>();

    public const string ItemName_HPotion = "Healing Potion";
    public const string ItemName_MPotion = "Mana Potion";
    public const string ItemName_Money = "Money";
    public const string ItemName_AP = "AbilityPoint";

    public delegate void InventoryChange();
    private InventoryChange onInventoryChanged;

    public void Add(in EItem item, in int amount)
    {
        if (HasItem(item, out Item invItem))
        {
            AddAmount(ref invItem, amount);
        }
        else
        {
            items.Add(new Item(item, amount));
        }
        onInventoryChanged?.Invoke();
    }
    public void Decrease(in EItem item, in int amount)
    {
        if (!HasItem(item, out Item invItem)) return;

        Decrease(invItem, amount);
    }
    public void Decrease(Item item, in int amount)
    {
        item.amount -= amount;
        //if (item.amount <= 0) Remove(ref item);
        onInventoryChanged?.Invoke();
    }
    private void AddAmount(ref Item item, in int amount)
    {
        item.amount += amount;
        onInventoryChanged?.Invoke();
    }
    private void Remove(ref Item invItem)
    {
        items.Remove(invItem);
        onInventoryChanged?.Invoke();
    }
    public bool HasItem(in Item item)
    {
        foreach (Item i in items)
        {
            if (i == item)
            {
                return true;
            }
        }
        return false;
    }
    public bool HasItem(in EItem item, out Item invItem)
    {
        foreach (Item i in items)
        {
            if (i.get == item)
            {
                invItem = i;
                return true;
            }
        }
        invItem = null;
        return false;
    }
    public bool HasItem(in string itemName, out Item invItem)
    {
        foreach (Item i in items)
        {
            if (i.get.Name == itemName)
            {
                invItem = i;
                return true;
            }
        }
        invItem = null;
        return false;
    }
    public void AddOnInventoryChanged(InventoryChange action)
    {
        onInventoryChanged += action;
    }
    public void RemoveOnInventoryChanged(InventoryChange action)
    {
        onInventoryChanged -= action;
    }
    public int ItemCount => items.Count;
    public List<Item> Items => items;
    [Serializable]
    public class Item
    {
        public EItem get;
        public int amount;  
        public Item(EItem item, int amount)
        {
            this.get = item;
            this.amount = amount;
        }
        public static bool operator true(Item item)
        {
            return item != null;
        }
        public static bool operator false(Item item)
        {
            return item == null;
        }
    }
}