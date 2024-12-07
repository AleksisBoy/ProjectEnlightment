using UnityEngine;

public class CircularGrid : MonoBehaviour
{
    [SerializeField] private float gridRadius = 400f;

    private RectTransform RT => (RectTransform)transform;
    private void OnValidate()
    {
        SetGrid();
    }
    public void SetGrid()
    {
        int childCount = transform.childCount;
        float diff = (Mathf.PI * 2f) / childCount;
        for(int i = 0; i < childCount; i++)
        {
            RectTransform childRT = (RectTransform)transform.GetChild(i);
            float t = diff * i;
            float y = Mathf.Sin(t) * gridRadius;
            float x = Mathf.Cos(t) * gridRadius;
            childRT.position = RT.position + new Vector3(x, y, 0f);
        }
    }
}
