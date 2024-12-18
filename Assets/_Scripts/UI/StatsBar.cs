using UnityEngine;
using UnityEngine.UI;

public class StatsBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider = null;
    [SerializeField] private Slider manaSlider = null;
    [SerializeField] private ItemIconUI currentEquippedIcon = null;

    public void SetupBar(int healthMax, int manaMax)
    {
        healthSlider.maxValue = healthMax;
        manaSlider.maxValue = manaMax;
        currentEquippedIcon.SetItem(null);
    }
    public void SetHealth(float value, float maxValue)
    {
        healthSlider.maxValue = maxValue;
        healthSlider.value = value;
    }
    public void SetMana(float value, float maxValue)
    {
        manaSlider.maxValue = maxValue;
        manaSlider.value = value;
    }
    public void SetCurrentEquippedIcon(Inventory.Item item)
    {
        currentEquippedIcon.SetItem(item);
    }
    public void UpdateCurrentEquippedIcon()
    {
        currentEquippedIcon.UpdateAmount();
    }
}
