using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private Image image = null;

    private List<CrossData> current = new List<CrossData>();

    public void Add(CrossData unit)
    {
        if (!current.Contains(unit)) current.Add(unit);
        UpdateCrosshair();
    }
    public void Remove(CrossData unit)
    {
        if (current.Contains(unit)) current.Remove(unit);
        UpdateCrosshair();
    }
    private void UpdateCrosshair()
    {
        int mostPriority = -1000;
        CrossData cross = new CrossData();
        for(int i = 0; i < current.Count; i++)
        {
            if (current[i].priority > mostPriority)
            {
                mostPriority = current[i].priority;
                cross = current[i];
            }
        }
        Set(cross);
    }
    private void Set(CrossData unit)
    {
        image.sprite = unit.sprite;
        image.color = unit.color;
        image.rectTransform.sizeDelta = unit.size;
        image.rectTransform.anchoredPosition = unit.offset;
    }

    // Data
    public enum Type
    {
        None,
        Default,
        Grab
    }
    [Serializable]
    public struct CrossData
    {
        public Type type;
        public Sprite sprite;
        public Color color;
        public Vector2 size;
        public Vector2 offset;
        public int priority;
    }
}