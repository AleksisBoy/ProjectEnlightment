using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Base")]
    [SerializeField] private Image background = null;
    [SerializeField] private Image icon = null;
    [SerializeField] protected TMP_Text amountText = null;
    [SerializeField] private bool selectWithPointer = true;
    [SerializeField] private bool showAmountZero = false;

    protected Inventory.Item item;
    public Inventory.Item Item => item;

    public delegate void OnSelectedChanged();
    private static OnSelectedChanged onSelectedChanged;

    private static Dictionary<Type, ItemIconUI> selected = new Dictionary<Type, ItemIconUI>();
    protected static void SetSelected(Type t, ItemIconUI item)
    {
        if (!selected.ContainsKey(t)) selected.Add(t, item);
        else
        {
            DeselectSelected(t);
            selected[t] = item;
            if (selected[t]) selected[t].Select();
            if (onSelectedChanged != null) onSelectedChanged();
        }
    }
    public static T GetSelected<T>() where T : ItemIconUI
    {
        if (!selected.ContainsKey(typeof(T))) return null;
        return (T)selected[typeof(T)];
    }
    public static ItemIconUI GetSelected(Type t)
    {
        if (!selected.ContainsKey(t)) return null;
        return selected[t];
    }
    protected static void DeselectSelected(Type t)
    {
        if (selected.ContainsKey(t) && selected[t]) selected[t].Deselect();
    }

    public virtual void Select()
    {
        background.color = InternalSettings.SelectedIconColor;
    }
    public virtual void Deselect()
    {
        background.color = InternalSettings.DefaultIconColor;
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!selectWithPointer) return;

        SetSelected(GetType(), this);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!selectWithPointer) return;

        SetSelected(GetType(), null);
    }
    public virtual void SetItem(Inventory.Item item)
    {
        this.item = item;
        if(item)
        {
            icon.gameObject.SetActive(true);
            icon.sprite = item.get.IconSprite;
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
        UpdateAmount();
    }
    public virtual void UpdateAmount()
    {
        if (item != null && !item.get.Unique && ((showAmountZero && item.amount >= 0) || (!showAmountZero && item.amount > 0)))
        {
            amountText.text = item.amount.ToString();
        }
        else
        {
            amountText.text = string.Empty;
            // set icon/color to match that theres nothing left
        }
    }
    private void OnDisable()
    {
        if (GetSelected(GetType()) == this) SetSelected(GetType(), null);
    }
    // Action
    public static void Assign_OnSelectedChanged(OnSelectedChanged action)
    {
        onSelectedChanged += action;
    }
    public static void Remove_OnSelectedChanged(OnSelectedChanged action)
    {
        onSelectedChanged -= action;
    }
}
