using UnityEngine;

public abstract class EItem : ScriptableObject
{
    [Header("EItem")]
    [SerializeField] protected string itemName = "NAMENULL";
    [TextArea(2, 6)]
    [SerializeField] protected string description = "Description";
    [SerializeField] protected bool unique = false;
    [SerializeField] protected Sprite iconSprite = null;

    public string Name => itemName;
    public string Description => description;
    public bool Unique => unique;
    public Sprite IconSprite => iconSprite;
}
