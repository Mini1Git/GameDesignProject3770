public enum MeleeAttackKind
{
    Light,
    Heavy
}

public readonly struct MeleeAttackRequest
{
    public int AttackId { get; }
    public WeaponDefinition Weapon { get; }
    public MeleeAttackKind Kind { get; }
    public float ChargeRatio { get; }
    public float DamageMultiplier { get; }
    public int Damage { get; }
    public MeleePatternData Pattern { get; }

    // damage is fixed when the attack is released
    public MeleeAttackRequest(int attackId, WeaponDefinition weapon,
        MeleeAttackKind kind, float chargeRatio, float damageMultiplier, int damage,
        MeleePatternData pattern = null)
    {
        AttackId = attackId;
        Weapon = weapon;
        Kind = kind;
        ChargeRatio = chargeRatio;
        DamageMultiplier = damageMultiplier;
        Damage = damage;
        Pattern = pattern;
    }
}
