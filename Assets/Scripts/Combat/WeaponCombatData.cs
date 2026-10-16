using UnityEngine;

[CreateAssetMenu(fileName = "WeaponCombatData", menuName = "Combat/Weapon Class")]
public class WeaponCombatData : ScriptableObject
{
    [Header("Light")]
    [Min(0)] public int lightDamage = 2;
    [Min(0f)] public float lightHitStaminaRestore = 2f;
    public MeleePatternData lightPattern;

    [Header("Heavy")]
    [Min(0)] public int heavyBaseDamage = 4;
    [Min(0f)] public float chargeStaminaPerSecond = 4f;
    [Min(0.01f)] public float fullChargeDuration = 1.2f;
    [Min(1f)] public float maxChargeDamageMultiplier = 2f;
    public MeleePatternData heavyPattern;
}
