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
    public WeaponDefinition weaponDefinition;
    // use class defaults directly when this item has no individual weapon definition
    public WeaponCombatData weaponDefaults;
}
