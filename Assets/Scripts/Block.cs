using JetBrains.Annotations;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int rowNumber;
    public int columnNumber;
    public Sprite blockType;
    public bool isSelected;
    public bool isRemoved;

    private SpriteRenderer sr;
    private Color originalColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void Select()
    {
        isSelected = true;
        sr.color = Color.green;
    }

    public void Deselect()
    {
        isSelected = false;
        sr.color = originalColor;
    }
    public void Initialize(int row, int col, Sprite type, bool removed)
    {
        rowNumber = row;
        columnNumber = col;
        blockType = type;
        sr.sprite = type;
        isRemoved = removed;
    }
}
