using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image background = null;
    [SerializeField] private Image icon = null;
    [SerializeField] private TMP_Text amountText = null;

    private Inventory.Item item;
    public Inventory.Item Item => item;

    public delegate void OnSelectedChanged();
    private static OnSelectedChanged onSelectedChanged;

    private static ItemIconUI selected = null;
    public static ItemIconUI Selected
    {
        get => selected;
        set
        {
            if (value != selected)
            {
                if (selected) selected.Deselect();
                selected = value;
                if (value) selected.Select();
                if (onSelectedChanged != null) onSelectedChanged();
            }
        }
    }

    public void Select()
    {
        background.color = InternalSettings.SelectedIconColor;
    }
    public void Deselect()
    {
        background.color = InternalSettings.DefaultIconColor;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Selected = this;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Selected = null;
    }
    public void SetItem(Inventory.Item item)
    {
        this.item = item;
        icon.sprite = item.get.IconSprite;
        SetAmount();
    }
    private void SetAmount()
    {
        if (item.amount > 0 && !item.get.Unique)
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
        if (Selected == this) Deselect();
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
