using UnityEngine;


public enum ElementType
{
    Frost,Fire, Wind,Dark,Poison,Water,Nature, Summon
}

public enum PositionType
{
    Caster, Mouse
}

[CreateAssetMenu(fileName = "spell_SO", menuName = "Scriptable Objects/spell_SO")]
public class spell_SO : ScriptableObject
{
    public string spellName;
    public GameObject spellPrefab;
    public ElementType spellElement;
    public PositionType positionType;
    public float manaCost;
    

    
}
