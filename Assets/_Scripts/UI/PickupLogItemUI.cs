using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickupLogItemUI : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private Image itemLogo = null;
    [SerializeField] private Image logoBackground = null;
    [SerializeField] private Image amountBackground = null;
    [SerializeField] private TMP_Text itemName = null;
    [SerializeField] private TMP_Text itemAmount = null;

    private PickupLog pickupLog = null;

    private Vector2 lastSize = Vector2.zero;
    public RectTransform RT => (RectTransform)transform;
    private void Awake()
    {
        lastSize = RT.sizeDelta;
    }
    public void Set(PickupLog pickupLog, EItem item, int amount)
    {
        this.pickupLog = pickupLog;
        itemLogo.sprite = item.IconSprite;
        itemName.text = item.Name;
        itemAmount.text = amount.ToString();
    }
    private void Update()
    {
        if (NovUtil.CountdownCheck(ref lifetime, 1000, Time.deltaTime))
        {
            pickupLog.DestroyPickupLogItem(this);
        }
    }
    private void OnRectTransformDimensionsChange()
    {
        if (lastSize != RT.sizeDelta) Debug.Log("Changed " + itemName.text);
        float changeX = RT.sizeDelta.x / lastSize.x;
        float changeY = RT.sizeDelta.y / lastSize.y;

        itemName.rectTransform.sizeDelta = new Vector2(
            itemName.rectTransform.sizeDelta.x * changeX,
            itemName.rectTransform.sizeDelta.y * changeY);
        
        amountBackground.rectTransform.sizeDelta = new Vector2(
            amountBackground.rectTransform.sizeDelta.x * changeX,
            amountBackground.rectTransform.sizeDelta.y * changeY);

        logoBackground.rectTransform.sizeDelta = new Vector2(
            logoBackground.rectTransform.sizeDelta.x * changeX,
            logoBackground.rectTransform.sizeDelta.y * changeY);

        lastSize = RT.sizeDelta;
    }
}
