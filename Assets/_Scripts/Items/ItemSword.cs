
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(menuName = "Enlightenment/New Sword")]
public class ItemSword : ItemActive
{
    [Header("Pistol")]
    [SerializeField] private int maxDamage = 80;
    [SerializeField] private float maxDistance = 10f;

    private Animator animator = null;
    public override void OnEquip(IActor actor)
    {
        this.animator = animator;
        Debug.Log(Name + " equipped");
    }

    public override void OnDequip()
    {
        Debug.Log(Name + " dequipped");
    }

    public override void OnInputDown()
    {
    }

    public override void OnInputHold()
    {
    }

    public override void OnInputUp()
    {
    }
}