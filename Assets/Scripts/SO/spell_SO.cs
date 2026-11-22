using UnityEngine;
using UnityEngine.UI;



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
    public int damage;
    public float manaCost;

    // spells always count as spell items so they only go in spell slots
    private void OnValidate()
    {
        type = itemType.spell;
    }
}
