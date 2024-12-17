using UnityEngine;

public abstract class EItem : ScriptableObject
{
    [Header("EItem")]
    [SerializeField] protected string itemName = "NAMENULL";
    [TextArea(2, 6)]
    [SerializeField] protected string description = "Description";
    [SerializeField] protected bool unique = false;
    [SerializeField] protected int maxStack = 10;
    [SerializeField] protected Sprite iconSprite = null;

    private void OnValidate()
    {
        if (unique) maxStack = 1;
        //else maxStack = Mathf.Min(maxStack, 1);
    }
    public string Name => itemName;
    public string Description => description;
    public bool Unique => unique;
    public int MaxStack => maxStack;
    public Sprite IconSprite => iconSprite;
}
