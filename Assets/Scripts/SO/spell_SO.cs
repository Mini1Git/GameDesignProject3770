using UnityEngine;
using UnityEngine.UI;

public enum ElementType
{
    Frost,Fire, Wind,Dark,Poison,Water,Nature, Summon
}


public enum PositionType
{
    Caster, Mouse
}

[CreateAssetMenu(fileName = "spell_SO", menuName = "Scriptable Objects/spell_SO")]
public class spell_SO : itemData
{
    public string spellName;
    public GameObject spellPrefab;
    public Image spellImage;
    [TextArea(2,10)]
    public string spellDescription;
    
    public ElementType spellElement;
    public PositionType positionType;
    public float manaCost;

    // spells always count as spell items so they only go in spell slots
    private void OnValidate()
    {
        type = itemType.spell;
    }
}
