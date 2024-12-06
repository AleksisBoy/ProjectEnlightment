using UnityEngine;

public class InternalSettings : MonoBehaviour
{
    [SerializeField] private GUIStyle debugStyle = GUIStyle.none;
    [SerializeField] private float healPotionStrength = 0.2f;
    [SerializeField] private LayerMask characterMask;
    [SerializeField] private int outlineLayer = -1;
    [SerializeField] private int ragdollLayer = 11;
    public static InternalSettings Get { get; private set; }

    public static GUIStyle DebugStyle => Get.debugStyle;
    public static float HealPotionStrength => Get.healPotionStrength;
    public static LayerMask CharacterMask => Get.characterMask;
    public static int OutlineLayer => Get.outlineLayer;
    public static int RagdollLayer => Get.ragdollLayer;
    private void Awake()
    {
        if (Get == null) Get = this;
        else { Destroy(gameObject); return; }
    }
}
