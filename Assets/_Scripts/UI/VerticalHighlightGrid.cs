using System.Collections.Generic;
using UnityEngine;

public class VerticalHighlightGrid : MonoBehaviour
{
    [SerializeField] private float spacing = 10;
    [SerializeField] private float highlightSpacing = 30;
    [SerializeField] private float paddingBottom = 0f;
    [SerializeField] private float highlightScaleModifier = 1.3f;
    [SerializeField] private Vector2 defaultSize = Vector2.zero;

    private List<RectTransform> children = new List<RectTransform>();

    public RectTransform RT => (RectTransform)transform;

    private void OnValidate()
    {
        if (highlightScaleModifier <= 0f) highlightScaleModifier = 0.01f;
    }
    private void UpdateGrid()
    {
        Debug.Log( children.Count + " upd " + Time.time);
        for (int i = 0; i < children.Count; i++)
        {
            float offsetX = 0;
            float offsetY;
            if (i == 0) // highlighted object
            {
                offsetY = 0f + paddingBottom;
                children[i].sizeDelta = defaultSize * highlightScaleModifier;
                Vector2 rightBottom1 = RT.position + new Vector3(RT.sizeDelta.x / 2f,0f);
                Vector2 rightBottom2 = children[i].position + new Vector3(children[i].sizeDelta.x / 2f, -children[i].sizeDelta.y / 2f);
                Vector2 diff = rightBottom1 - rightBottom2;
                offsetX += diff.x;
                offsetY += diff.y;
            }
            else if (i == 1) // after highlighted
            {
                offsetY = highlightSpacing + paddingBottom;
                children[i].sizeDelta = defaultSize;
            }
            else // other previous
            {
                offsetY = children[i - 1].anchoredPosition.y + spacing;
            }
            children[i].position = RT.position + new Vector3(offsetX, offsetY, 0f);
        }
    }
    public void AddChild(RectTransform child)
    {
        if(!children.Contains(child)) children.Insert(0, child);
        UpdateGrid();
    }
    public void RemoveChild(RectTransform child)
    {
        if(children.Contains(child)) children.Remove(child);
        //UpdateGrid();
    }
    public void SetDefaultSize(Vector2 size)
    {
        defaultSize = size;
    }
}
