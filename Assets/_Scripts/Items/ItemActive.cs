
using UnityEngine;

public abstract class ItemActive : EItem
{
    [Header("Active")]
    [SerializeField] private bool rightHanded = true;
    [SerializeField] private GameObject meshPrefab = null;
    [SerializeField] private Vector3 handTransformLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 handTransformRotation = Vector3.zero;
    [SerializeField] private string animationLayer = "LAYER";

    public abstract ItemUseData Init();
    public virtual void OnEquip(ItemUseData data, IActor actor)
    {
        Debug.Log(Name + " Equipped by " + actor.GetGameObject().name);
        data.actor = actor;
        data.actor.GetAnimator().SetLayerWeight(data.actor.GetAnimator().GetLayerIndex(AnimationLayer), 1f);
    }
    public virtual void OnDequip(ItemUseData data)
    {
        Debug.Log(Name + " Dequipped by " + data.actor.GetGameObject().name);
        data.actor.GetAnimator().SetLayerWeight(data.actor.GetAnimator().GetLayerIndex(AnimationLayer), 0f);
        data.actor = null; 
    }
    public abstract void EquippedUpdate(ItemUseData data);
    public abstract void OnInputDown(ItemUseData data);
    public abstract void OnInputHold(ItemUseData data);
    public abstract void OnInputUp(ItemUseData data);
    public abstract void CallEvent(ItemUseData data, NovUtil.AnimEvent combatEvent);
    public abstract void Disturb(ItemUseData data);

    public bool RightHanded => rightHanded;
    public GameObject MeshPrefab => meshPrefab;
    public Vector3 HandTransformLocalPosition => handTransformLocalPosition;
    public Vector3 HandTransformRotation => handTransformRotation;
    public string AnimationLayer => animationLayer;

    public abstract class ItemUseData
    {
        public IActor actor;
        public GameObject instance;
        public int amount = 0;
    }
}