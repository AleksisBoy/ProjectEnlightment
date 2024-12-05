using UnityEngine;

public class InternalSettings : MonoBehaviour
{
    [SerializeField] private GUIStyle debugStyle = GUIStyle.none;
    [SerializeField] private float healPotionStrength = 0.2f;
    public static InternalSettings Get { get; private set; }

    public static GUIStyle DebugStyle => Get.debugStyle;
    public static float HealPotionStrength => Get.healPotionStrength;
    private void Awake()
    {
        if (Get == null) Get = this;
        else { Destroy(gameObject); return; }
    }
}
