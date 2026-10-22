using UnityEngine;

public enum itemType
{
    weapon,
    item,
    spell
}

[CreateAssetMenu(fileName = "newItem", menuName = "Inventory/Item")]
public class itemData : ScriptableObject
{
    public string itemName;
    public itemType type;
    public Sprite icon;
}
