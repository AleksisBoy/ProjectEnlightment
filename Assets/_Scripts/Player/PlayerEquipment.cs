using UnityEngine;

public class PlayerEquipment : PlayerAction
{
    [Header("Equipment")]
    [SerializeField] private KeyCode healKey = KeyCode.R;
    [SerializeField] private Transform rightHandTransform = null;
    [SerializeField] private Transform leftHandTransform = null;

    private bool changingEquip = false;

    private Inventory inventory;
    public Inventory Inventory => inventory;

    private ItemActive mainItem = null;
    public ItemActive MainItem => mainItem;

    private ItemActive secondaryItem = null;
    public ItemActive SecondaryItem => secondaryItem;

    public delegate void OnEquippedChanged(ItemActive item, bool equipped);
    private OnEquippedChanged onEquippedChanged;

    public override void Init(params object[] objects)
    {
        base.Init(objects);

        inventory = InternalSettings.GetDefaultPlayerInventory();
    }
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        bool healKeyDown = Input.GetKeyDown(healKey);

        if (healKeyDown)
        {
            HealWithPotion();
        }
    }
    private void HealWithPotion()
    {
        if (!inventory.HasItem(Inventory.ItemName_HPotion, out Inventory.Item hpotion) || hpotion.amount <= 0) return; // add feedback that not enough potions
        if (master.TryHealFromMax(InternalSettings.HealPotionStrength)) hpotion.amount--;
    }
    // Equipment
    public void AddItem(EItem item, int amount)
    {
        inventory.Add(item, amount);
    }
    public void Equip(ItemActive item)
    {
        if (item.RightHanded)
        {
            PutItemInHand(item, rightHandTransform);
            mainItem = item;
        }
        else
        {
            if (secondaryItem)
            {
                InternalSettings.GetStoredPrefab(secondaryItem).SetActive(false);
            }
            secondaryItem = item;
            PutItemInHand(item, leftHandTransform);
        }
        item.OnEquip(master);
        onEquippedChanged?.Invoke(item, true);
    }
    private void PutItemInHand(ItemActive item, Transform handTransform)
    {
        GameObject itemPrefab = InternalSettings.GetStoredPrefab(item);
        if (!itemPrefab)
        {
            itemPrefab = InternalSettings.SpawnStorePrefab(item, handTransform);
        }
        itemPrefab.SetActive(true);
        handTransform.localEulerAngles = item.HandTransformRotation;
    }
    public void Dequip(ItemActive item)
    {
        if (secondaryItem == item)
        {
            if (secondaryItem)
            {
                InternalSettings.GetStoredPrefab(secondaryItem).SetActive(false);
            }
            item.OnDequip();
            secondaryItem = null;
            onEquippedChanged?.Invoke(item, false);
        }
        else
        {
            Debug.Log("Cannot dequip " + item.Name);
        }
    }
    public void EquipToggle(ItemActive item)
    {
        if (secondaryItem == item)
        {
            Dequip(item);
        }
        else if (secondaryItem != item)
        {
            Equip(item);
        }
    }

    public void AssignOnEquippedChanged(OnEquippedChanged action)
    {
        onEquippedChanged += action;
    }
    public void RemoveOnEquippedChanged(OnEquippedChanged action)
    {
        onEquippedChanged -= action;
    }
}
