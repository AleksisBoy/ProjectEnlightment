using UnityEngine;
using UnityEngine.UI;

public class StatsBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider = null;
    [SerializeField] private Slider manaSlider = null;

    public void SetupBar(int healthMax, int manaMax)
    {
        healthSlider.maxValue = healthMax;
        manaSlider.maxValue = manaMax;
    }
    public void SetHealth(float value, float maxValue)
    {
        healthSlider.maxValue = maxValue;
        healthSlider.value = value;
    }
    public void SetMana(float value)
    {
        manaSlider.value = value;
    }
}
