using System;
using UnityEngine;

public enum AttackShape
{
    Arc,
    Circle,
    Thrust
}

[Serializable]
public struct WeaponStatScaling
{
    public Stats stat;
    [Min(0)] public int damagePerEightStatPoints;
}

[Serializable]
public struct ChargedHitWindow
{
    [Range(0, 100)] public int startPercent;
    [Range(0, 100)] public int endPercent;
}

[CreateAssetMenu(fileName = "newWeapon", menuName = "Inventory/Weapon")]
public class weaponData : itemData
{
    [Header("Damage and Speed")]
    [Min(1)] public int baseDamage = 2;
    [Tooltip("Animation playback multiplier for both light and charged attacks.")]
    [SerializeField, Min(10)] private int attackSpeedPercent = 200;
    public float attackSpeed => attackSpeedPercent / 100f;
    public WeaponStatScaling[] statScaling = Array.Empty<WeaponStatScaling>();

    [Header("Charge and Stamina")]
    [Tooltip("Total stamina spent reaching full charge. Light attacks remain free.")]
    [Min(0)] public int staminaCost = 4;
    [Min(0)] public int lightHitStaminaRestore = 2;
    [Min(0)] public int chargedMinStamina = 2;
    [SerializeField, Min(100)] private int chargedDamagePercent = 200;
    public float chargedDamageMultiplier => chargedDamagePercent / 100f;
    [Min(1)] public int fullChargeDamageMultiplier = 2;
    [SerializeField, Min(1)] private int chargeThresholdCentiseconds = 20;
    public float chargedAttackThreshold => chargeThresholdCentiseconds / 100f;
    [Tooltip("Time from button press to full charge.")]
    [SerializeField, Min(1)] private int fullChargeCentiseconds = 50;
    public float fullChargeDuration => fullChargeCentiseconds / 100f;
    [SerializeField, Min(1)] private int chargeRaiseCentiseconds = 5;
    public float chargeRaiseDuration => chargeRaiseCentiseconds / 100f;
    [SerializeField, Range(0, 100)] private int chargePosePercent = 40;
    public float chargePoseTime => chargePosePercent / 100f;

    [Header("Animation and Hit Shape")]
    [Tooltip("Arc and thrust follow the animated weapon's box. Circle hits around its offset.")]
    public AttackShape attackShape;
    [Tooltip("Leave empty to use the default sword swing.")]
    public AnimationClip meleeSwingAnimation;
    [Tooltip("Optional heavy-attack animation. Leave empty to reuse the light attack.")]
    public AnimationClip chargedSwingAnimation;
    [SerializeField] private int hitboxOffsetXCentimeters;
    [SerializeField] private int hitboxOffsetYCentimeters = 25;
    [SerializeField, Min(1)] private int hitboxWidthCentimeters = 25;
    [SerializeField, Min(1)] private int hitboxHeightCentimeters = 75;
    public Vector2 meleeHitboxOffset => new Vector2(hitboxOffsetXCentimeters, hitboxOffsetYCentimeters) / 100f;
    public Vector2 meleeHitboxSize => new Vector2(hitboxWidthCentimeters, hitboxHeightCentimeters) / 100f;
    [SerializeField, Min(1)] private int areaRadiusCentimeters = 50;
    public float areaRadius => areaRadiusCentimeters / 100f;
    [SerializeField, Range(0, 100)] private int hitStartPercent = 10;
    public float hitStart => hitStartPercent / 100f;
    [SerializeField, Range(0, 100)] private int hitEndPercent = 55;
    public float hitEnd => hitEndPercent / 100f;
    [SerializeField, Range(0, 100)] private int chargedHitStartPercent = 40;
    public float chargedHitStart => chargedHitStartPercent / 100f;
    [SerializeField, Range(0, 100)] private int chargedHitEndPercent = 62;
    public float chargedHitEnd => chargedHitEndPercent / 100f;
    [Tooltip("Optional ordered heavy-attack hit windows (start, end). Each window can hit a target once. Empty uses the single charged window above.")]
    public ChargedHitWindow[] chargedHitWindows = Array.Empty<ChargedHitWindow>();

    public float GetDamage(LevelingSystem stats)
    {
        float damage = Mathf.Max(1, baseDamage);
        if (stats != null && statScaling != null)
        {
            foreach (WeaponStatScaling scaling in statScaling)
                damage += Mathf.Max(0, stats.getPlayerStat(scaling.stat))
                    * (Mathf.Max(0, scaling.damagePerEightStatPoints) / 8f);
        }
        return damage;
    }

    private void OnEnable()
    {
        type = itemType.weapon;
    }

    private void OnValidate()
    {
        type = itemType.weapon;
        fullChargeCentiseconds = Mathf.Max(chargeThresholdCentiseconds + 1, fullChargeCentiseconds);
        hitEndPercent = Mathf.Max(hitStartPercent, hitEndPercent);
        chargedHitEndPercent = Mathf.Max(chargedHitStartPercent, chargedHitEndPercent);
        chargePosePercent = Mathf.Min(chargePosePercent, chargedHitStartPercent);
        if (chargedHitWindows != null && chargedHitWindows.Length > 0)
        {
            int previousEnd = 0;
            for (int i = 0; i < chargedHitWindows.Length; i++)
            {
                ref ChargedHitWindow window = ref chargedHitWindows[i];
                window.startPercent = Mathf.Clamp(window.startPercent, previousEnd, 100);
                window.endPercent = Mathf.Clamp(window.endPercent, window.startPercent, 100);
                previousEnd = window.endPercent;
            }
            chargePosePercent = Mathf.Min(chargePosePercent, chargedHitWindows[0].startPercent);
        }
    }
}
