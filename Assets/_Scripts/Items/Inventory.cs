using System;
using System.Collections.Generic;

public class Inventory
{
    private List<Item> items = new List<Item>();

    public void Add(EItem item, int amount)
    {
        if (HasItem(item, out Item invItem))
        {
            AddAmount(invItem, amount);
        }
        else
        {
            items.Add(new Item(item, amount));
        }
    }
    public void Decrease(EItem item, int amount)
    {
        if (!HasItem(item, out Item invItem)) return;

        invItem.amount -= amount;
        if (invItem.amount <= 0) Remove(invItem);
    }
    private void AddAmount(Item item, int amount)
    {
        item.amount += amount;
    }
    private void Remove(Item invItem)
    {
        items.Remove(invItem);
    }
    public bool HasItem(EItem item, out Item invItem)
    {
        foreach (Item i in items)
        {
            if (i.get == item)
            {
                invItem = i;
                return true;
            }
        }
        invItem = new Item();
        return false;
    }
    public bool HasItem(string itemName, out Item invItem)
    {
        foreach (Item i in items)
        {
            if (i.get.Name == itemName)
            {
                invItem = i;
                return true;
            }
        }
        invItem = new Item();
        return false;
    }
    public int ItemCount => items.Count;
    public List<Item> Items => items;
    [Serializable]
    public struct Item
    {
        public EItem get;
        public int amount;  
        public Item(EItem item, int amount)
        {
            this.get = item;
            this.amount = amount;
        }
    }
}