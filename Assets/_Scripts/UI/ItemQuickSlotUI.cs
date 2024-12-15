using TMPro;
using UnityEngine;
using static Inventory;

public class ItemQuickSlotUI : ItemIconUI
{
    [Header("Quick Slot")]
    [SerializeField] private TMP_Text numericKeyTMP = null;

    private int numeric;
    public int Numeric => numeric;

    public void SetItem(Item item, int numeric)
    {
        base.SetItem(item);
        this.numeric = numeric;
        numericKeyTMP.text = numeric.ToString();
    }
}