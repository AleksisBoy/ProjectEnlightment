using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InternalSettings : MonoBehaviour
{
    [SerializeField] private GUIStyle debugStyle = GUIStyle.none;
    [SerializeField] private float healPotionStrength = 0.2f;
    [Header("Masks")]
    [SerializeField] private LayerMask characterMask;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask environmentLayer;
    [Header("Layers")]
    [SerializeField] private int outlineLayer = -1;
    [SerializeField] private int ragdollLayer = 11;
    [Header("Items")]
    [SerializeField] private Inventory.Item[] playerDefaultItems = null;
    [SerializeField] private ItemActive swordItem = null;
    [Header("UI")]
    [SerializeField] private UserInterface userInterfacePrefab = null;
    [SerializeField] private Color defaultIconColor = Color.white;
    [SerializeField] private Color selectedIconColor = Color.white;
    public static InternalSettings Get { get; private set; }

    public const string ASCII_TABLE = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}";
    public static GUIStyle DebugStyle => Get.debugStyle;
    public static float HealPotionStrength => Get.healPotionStrength;
    public static LayerMask CharacterMask => Get.characterMask;
    public static LayerMask ObstacleLayer => Get.obstacleLayer;
    public static LayerMask EnvironmentLayer => Get.environmentLayer;
    public static int OutlineLayer => Get.outlineLayer;
    public static int RagdollLayer => Get.ragdollLayer;
    public static Color SelectedIconColor => Get.selectedIconColor;
    public static Color DefaultIconColor => Get.defaultIconColor;
    public static ItemActive SwordItem => Get.swordItem;

    private Dictionary<ItemActive, GameObject> storedPrefabs = new Dictionary<ItemActive, GameObject>();
    private void Awake()
    {
        if (Get == null) Get = this;
        else if (Get != this) { Destroy(gameObject); return; }

        InitTMP();
    }
    public static Inventory GetDefaultPlayerInventory()
    {
        Inventory inventory = new Inventory();
        if (Get == null) Get = FindAnyObjectByType<InternalSettings>();
        if (!Get) return inventory;
        foreach(Inventory.Item item in Get.playerDefaultItems)
        {
            inventory.Add(item.get, item.amount);
        }
        return inventory;
    }
    public static void EnableCursor(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }
    public static UserInterface SpawnUserInterface()
    {
        return Instantiate(Get.userInterfacePrefab);
    }
    public static GameObject GetStoredPrefab(ItemActive item)
    {
        if (!Get.storedPrefabs.ContainsKey(item)) return null;
        return Get.storedPrefabs[item];
    }
    public static GameObject SpawnStorePrefab(ItemActive item, Transform parent)
    {
        if (Get.storedPrefabs.ContainsKey(item))
        {
            Debug.LogError("Already spawned " + item.Name);
            return null;
        }
        GameObject prefab = Instantiate(item.MeshPrefab, parent);
        Get.storedPrefabs.Add(item, prefab);
        item.Init();
        return prefab;
    }
    private void InitTMP()
    {
        TextMeshProUGUI initText = gameObject.AddComponent<TextMeshProUGUI>();
        initText.text = InternalSettings.ASCII_TABLE;
        initText.color = new Color(0, 0, 0, 0);
        Destroy(initText, 0.1f);
    }
}
