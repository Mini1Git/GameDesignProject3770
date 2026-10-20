using UnityEngine;


public enum elementType
{
    Frost,Fire, Wind,Dark,Poison,Water,Nature, Summon
}


[CreateAssetMenu(fileName = "spell_SO", menuName = "Scriptable Objects/spell_SO")]
public class spell_SO : ScriptableObject
{
    public string spellName;
    public elementType spellElement;
    public float manaCost;
    

    
}
