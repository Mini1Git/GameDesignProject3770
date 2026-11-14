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
    [Min(0f)] public float coefficient;
}

[CreateAssetMenu(fileName = "newWeapon", menuName = "Inventory/Weapon")]
public class weaponData : itemData
{
    [Header("Damage and Speed")]
    [Min(1)] public int baseDamage = 2;
    [Tooltip("Animation playback multiplier for both light and charged attacks.")]
    [Min(0.1f)] public float attackSpeed = 2f;
    public WeaponStatScaling[] statScaling = Array.Empty<WeaponStatScaling>();

    [Header("Charge and Stamina")]
    [Tooltip("Total stamina spent reaching full charge. Light attacks remain free.")]
    [Min(0f)] public float staminaCost = 4f;
    [Min(0f)] public float lightHitStaminaRestore = 2f;
    [Min(0f)] public float chargedMinStamina = 2f;
    [Min(1f)] public float chargedDamageMultiplier = 2f;
    [Min(1f)] public float fullChargeDamageMultiplier = 2f;
    [Min(0.01f)] public float chargedAttackThreshold = 0.2f;
    [Tooltip("Time from button press to full charge.")]
    [Min(0.01f)] public float fullChargeDuration = 0.5f;
    [Min(0.01f)] public float chargeRaiseDuration = 0.05f;
    [Range(0f, 1f)] public float chargePoseTime = 0.4f;

    [Header("Animation and Hit Shape")]
    [Tooltip("Arc and thrust follow the animated weapon's box. Circle hits around its offset.")]
    public AttackShape attackShape;
    [Tooltip("Leave empty to use the default sword swing.")]
    public AnimationClip meleeSwingAnimation;
    [Tooltip("Optional heavy-attack animation. Leave empty to reuse the light attack.")]
    public AnimationClip chargedSwingAnimation;
    public Vector2 meleeHitboxOffset = new Vector2(0f, 0.25f);
    public Vector2 meleeHitboxSize = new Vector2(0.25f, 0.75f);
    [Min(0.01f)] public float areaRadius = 0.5f;
    [Range(0f, 1f)] public float hitStart = 0.1f;
    [Range(0f, 1f)] public float hitEnd = 0.55f;
    [Range(0f, 1f)] public float chargedHitStart = 0.4f;
    [Range(0f, 1f)] public float chargedHitEnd = 0.62f;
    [Tooltip("Optional ordered heavy-attack hit windows (start, end). Each window can hit a target once. Empty uses the single charged window above.")]
    public Vector2[] chargedHitWindows = Array.Empty<Vector2>();

    public float GetDamage(LevelingSystem stats)
    {
        float damage = Mathf.Max(1, baseDamage);
        if (stats != null && statScaling != null)
        {
            foreach (WeaponStatScaling scaling in statScaling)
                damage += Mathf.Max(0, stats.getPlayerStat(scaling.stat))
                    * Mathf.Max(0f, scaling.coefficient);
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
        fullChargeDuration = Mathf.Max(chargedAttackThreshold + 0.01f, fullChargeDuration);
        hitEnd = Mathf.Max(hitStart, hitEnd);
        chargedHitEnd = Mathf.Max(chargedHitStart, chargedHitEnd);
        chargePoseTime = Mathf.Min(chargePoseTime, chargedHitStart);
        if (chargedHitWindows != null && chargedHitWindows.Length > 0)
        {
            float previousEnd = 0f;
            for (int i = 0; i < chargedHitWindows.Length; i++)
            {
                float start = Mathf.Clamp(chargedHitWindows[i].x, previousEnd, 1f);
                float end = Mathf.Clamp(chargedHitWindows[i].y, start, 1f);
                chargedHitWindows[i] = new Vector2(start, end);
                previousEnd = end;
            }
            chargePoseTime = Mathf.Min(chargePoseTime, chargedHitWindows[0].x);
        }
        meleeHitboxSize = new Vector2(Mathf.Max(0.01f, meleeHitboxSize.x),
            Mathf.Max(0.01f, meleeHitboxSize.y));
    }
}
