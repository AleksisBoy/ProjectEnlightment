using UnityEngine;

public class PickupLog : MonoBehaviour
{
    [SerializeField] private VerticalHighlightGrid grid = null;
    [SerializeField] private PickupLogItemUI logItemPrefab = null;

    public void Init()
    {
        grid.SetDefaultSize(logItemPrefab.RT.sizeDelta);
    }
    public void SpawnPickupLogItem(EItem item, int amount)
    {
        PickupLogItemUI logUI = Instantiate(logItemPrefab, grid.transform);
        logUI.Set(this, item, amount);
        grid.AddChild(logUI.RT);
    }
    public void DestroyPickupLogItem(PickupLogItemUI logUI)
    {
        grid.RemoveChild(logUI.RT);
        Destroy(logUI.gameObject);
    }
}
