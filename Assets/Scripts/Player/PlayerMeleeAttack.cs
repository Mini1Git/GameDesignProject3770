using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(player), typeof(inventory))]
public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private Animator weaponAnimator;
    [Header("Charge")]
    [SerializeField, Min(0.01f)] private float chargedAttackThreshold = 0.2f;
    [SerializeField, Min(0.01f)] private float fullChargeDuration = 1.2f;
    [Header("Damage and Stamina")]
    [SerializeField, Min(1)] private int normalDamage = 2;
    [SerializeField, Min(1)] private int chargedBaseDamage = 4;
    [SerializeField, Min(1f)] private float fullChargeDamageMultiplier = 2f;
    [SerializeField, Min(0f)] private float lightHitStaminaRestore = 2f;
    [SerializeField, Min(0f)] private float chargedStaminaCost = 10f;
    [Header("Default Hitbox")]
    [SerializeField] private Vector2 hitboxOffset = new Vector2(0f, 0.25f);
    [SerializeField] private Vector2 hitboxSize = new Vector2(0.25f, 0.75f);
    [SerializeField] private LayerMask enemyLayers = ~0;
    // normalized animation progress, shared by default and custom swing clips
    [SerializeField, Range(0f, 1f)] private float hitStart = 0.1f;
    [SerializeField, Range(0f, 1f)] private float hitEnd = 0.55f;

    private static readonly int SwingState = Animator.StringToHash("Base Layer.Swing");
    private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");
    private static readonly int SwingTrigger = Animator.StringToHash("Swing");
    private readonly HashSet<enemyBase> hitEnemies = new HashSet<enemyBase>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private player owner;
    private inventory items;
    private RuntimeAnimatorController defaultController;
    private AnimatorOverrideController swingOverride;
    private Stamina_System attackStamina;
    private itemData attackWeapon;
    private bool charging;
    private bool swinging;
    private bool normalAttack;
    private bool staminaRestored;
    private double pressedAt;
    private int attackDamage;
    private float lastProgress;
    private Vector2 attackOffset;
    private Vector2 attackSize;

    private bool CanAct => isActiveAndEnabled && owner != null
        && owner.isActiveAndEnabled && owner.canAct && weaponAnimator != null
        && weaponAnimator.isActiveAndEnabled;

    private bool HasSameWeapon => attackWeapon != null && items != null
        && items.getActiveQuickItem() == attackWeapon;

    private void Awake()
    {
        owner = GetComponent<player>();
        items = GetComponent<inventory>();
        if (weaponAnimator == null && owner.mainHand != null)
            weaponAnimator = owner.mainHand.GetComponent<Animator>();
        if (weaponAnimator != null) defaultController = weaponAnimator.runtimeAnimatorController;
    }

    private void OnEnable()
    {
        items.onChanged += OnInventoryChanged;
    }

    public void BeginCharge(double inputTime)
    {
        if (!CanAct || charging || swinging || defaultController == null) return;
        attackWeapon = items.getActiveQuickItem();
        if (attackWeapon == null || attackWeapon.type != itemType.weapon) return;
        pressedAt = inputTime;
        charging = true;
    }

    public void ReleaseCharge(double inputTime)
    {
        if (!charging) return;
        // clear first so repeated release callbacks cannot spend stamina twice
        charging = false;
        if (!CanAct || !HasSameWeapon) return;

        float heldTime = Mathf.Max(0f, (float)(inputTime - pressedAt));
        normalAttack = heldTime < chargedAttackThreshold;
        float cost = normalAttack ? 0f : Mathf.Max(0f, chargedStaminaCost);
        attackStamina = owner.stamina_System;
        if (attackStamina == null || !attackStamina.isActiveAndEnabled || attackStamina.Stamina < cost) return;

        // only release computes charge strength; holding full charge never executes an attack
        float ratio = Mathf.InverseLerp(chargedAttackThreshold,
            Mathf.Max(chargedAttackThreshold + 0.01f, fullChargeDuration), heldTime);
        attackDamage = normalAttack ? normalDamage : Mathf.RoundToInt(chargedBaseDamage
            * Mathf.Lerp(1f, Mathf.Max(1f, fullChargeDamageMultiplier), ratio));
        attackOffset = attackWeapon.overrideMeleeHitbox ? attackWeapon.meleeHitboxOffset : hitboxOffset;
        attackSize = attackWeapon.overrideMeleeHitbox ? attackWeapon.meleeHitboxSize : hitboxSize;
        attackSize = new Vector2(Mathf.Max(0.01f, attackSize.x), Mathf.Max(0.01f, attackSize.y));

        if (attackWeapon.meleeSwingAnimation != null)
        {
            if (swingOverride == null) swingOverride = new AnimatorOverrideController(defaultController);
            swingOverride["Swing"] = attackWeapon.meleeSwingAnimation;
            weaponAnimator.runtimeAnimatorController = swingOverride;
        }
        else weaponAnimator.runtimeAnimatorController = defaultController;
        if (!weaponAnimator.HasState(0, SwingState)) return;

        swinging = true;
        staminaRestored = false;
        lastProgress = 0f;
        hitEnemies.Clear();
        weaponAnimator.SetTrigger(SwingTrigger);
        // evaluate the existing trigger before paying for the accepted animation
        weaponAnimator.Update(0f);
        if (weaponAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash != SwingState)
        {
            CancelAttack();
            return;
        }
        if (!normalAttack) attackStamina.ConsumeStamina(cost);
    }

    private void Update()
    {
        if ((charging || swinging) && (!CanAct || !HasSameWeapon)) CancelAttack();
    }

    private void LateUpdate()
    {
        if (!swinging) return;
        if (!CanAct || !HasSameWeapon || attackStamina == null || !attackStamina.isActiveAndEnabled)
        {
            CancelAttack();
            return;
        }

        AnimatorStateInfo state = weaponAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.fullPathHash != SwingState)
        {
            swinging = false;
            return;
        }
        float progress = state.normalizedTime;
        // include a window crossed between frames without rebuilding the swing trajectory
        bool inWindow = progress >= hitStart && lastProgress <= Mathf.Max(hitStart, hitEnd);
        lastProgress = progress;
        if (!inWindow) return;

        Transform hand = weaponAnimator.transform;
        Vector3 scale = hand.lossyScale;
        Vector2 size = new Vector2(Mathf.Abs(attackSize.x * scale.x), Mathf.Abs(attackSize.y * scale.y));
        var filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(enemyLayers);
        overlaps.Clear();
        Physics2D.OverlapBox(hand.TransformPoint(attackOffset), size, hand.eulerAngles.z, filter, overlaps);
        foreach (Collider2D collider in overlaps)
        {
            if (collider.transform.IsChildOf(transform)) continue;
            enemyBase enemy = collider.GetComponentInParent<enemyBase>();
            if (enemy == null || hitEnemies.Contains(enemy) || !enemy.takeDmg(attackDamage)) continue;
            hitEnemies.Add(enemy);
            if (normalAttack && !staminaRestored)
            {
                staminaRestored = true;
                attackStamina.Stamina += Mathf.Max(0f, lightHitStaminaRestore);
            }
        }
    }

    private void OnInventoryChanged()
    {
        if ((charging || swinging) && !HasSameWeapon) CancelAttack();
    }

    public void CancelAttack()
    {
        charging = false;
        if (swinging && weaponAnimator != null && weaponAnimator.isActiveAndEnabled)
        {
            weaponAnimator.ResetTrigger(SwingTrigger);
            weaponAnimator.Play(IdleState, 0, 0f);
            weaponAnimator.Update(0f);
        }
        swinging = false;
        hitEnemies.Clear();
    }

    private void OnDisable()
    {
        items.onChanged -= OnInventoryChanged;
        CancelAttack();
    }

    private void OnDestroy()
    {
        if (swingOverride != null)
        {
            if (weaponAnimator != null) weaponAnimator.runtimeAnimatorController = defaultController;
            Destroy(swingOverride);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (weaponAnimator == null) return;
        var equipped = GetComponent<inventory>().getActiveQuickItem();
        bool useOverride = equipped != null && equipped.overrideMeleeHitbox;
        Gizmos.color = Color.yellow;
        Matrix4x4 previous = Gizmos.matrix;
        Gizmos.matrix = weaponAnimator.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(useOverride ? equipped.meleeHitboxOffset : hitboxOffset,
            useOverride ? equipped.meleeHitboxSize : hitboxSize);
        Gizmos.matrix = previous;
    }
}
