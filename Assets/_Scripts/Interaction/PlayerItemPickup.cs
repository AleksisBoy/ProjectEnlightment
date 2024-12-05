using UnityEngine;

public class PlayerItemPickup : MonoBehaviour, IPickup
{
    [SerializeField] private IPickup.Type type;
    [SerializeField] private int amount = 10;
    public void Pickup(Player player)
    {
        player.AddPickup(type, amount);
        Destroy(gameObject);
    }
    public void HighlightUpdate()
    {
        Debug.Log(name + " highlighed");
    }
}
