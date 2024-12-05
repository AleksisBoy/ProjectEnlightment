using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private GameObject[] meshes = null;

    protected int defaultLayer = 0;
    public virtual void Pickup(Player player)
    {

    }
    public virtual void HighlightBegin()
    {
        foreach(GameObject mesh in meshes)
        {
            mesh.layer = InternalSettings.OutlineLayer;
        }
    }
    public virtual void HighlightUpdate()
    {

    }
    public virtual void HighlightEnd()
    {
        foreach (GameObject mesh in meshes)
        {
            mesh.layer = defaultLayer;
        }
    }
}
