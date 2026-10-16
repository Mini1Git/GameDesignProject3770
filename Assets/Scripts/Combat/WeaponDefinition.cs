using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Combat/Weapon")]
public class WeaponDefinition : ScriptableObject
{
    public WeaponCombatData combatDefaults;

    // a value of 0.25 increases the full charge multiplier by 25 percent
    [Min(0f)] public float extraFullChargeBonus;
}
