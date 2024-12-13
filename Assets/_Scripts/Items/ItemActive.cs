
using UnityEngine;

public abstract class ItemActive : EItem
{
    [Header("General")]
    [SerializeField] private bool rightHanded = true;
    [SerializeField] private GameObject meshPrefab = null;
    [SerializeField] private Vector3 handTransformRotation = Vector3.zero;
    [SerializeField] private string animationLayer = "LAYER";

    protected IActor actor;
    public abstract void Init();
    public abstract void OnEquip(IActor actor);
    public abstract void EquippedUpdate();
    public abstract void OnDequip();
    public abstract void OnInputDown();
    public abstract void OnInputHold();
    public abstract void OnInputUp();
    public abstract void CallEvent(NovUtil.AnimEvent combatEvent);
    public abstract void Disturb();

    public bool RightHanded => rightHanded;
    public GameObject MeshPrefab => meshPrefab;
    public Vector3 HandTransformRotation => handTransformRotation;
    public string AnimationLayer => animationLayer;
}