using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(player), typeof(inventory))]
public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private Animator weaponAnimator;
    [SerializeField, Min(0.1f)] private float lightSwingSpeed = 1f;
    [Header("Charge")]
    [SerializeField, Min(0.01f)] private float chargedAttackThreshold = 0.2f;
    [SerializeField, Min(0.01f)] private float fullChargeDuration = 1.2f;
    [SerializeField, Min(0.01f)] private float chargeRaiseDuration = 0.1f;
    [SerializeField, Min(0f)] private float chargedMinStamina = 2f;
    [Header("Charge Full Cue")]
    [SerializeField, Min(1f)] private float chargeFullPulseScale = 1.2f;
    [SerializeField, Min(0.01f)] private float chargeFullPulseDuration = 0.15f;
    [Header("Damage and Stamina")]
    [SerializeField, Min(1)] private int normalDamage = 2;
    [SerializeField, Min(1)] private int chargedBaseDamage = 4;
    [SerializeField, Min(1f)] private float fullChargeDamageMultiplier = 2f;
    [SerializeField, Min(0f)] private float lightHitStaminaRestore = 2f;
    // TEMP value， feel free to adjust 
    [SerializeField, Min(0f)] private float chargedStaminaCost = 2f;
    [Header("Default Hitbox")]
    [SerializeField] private Vector2 hitboxOffset = new Vector2(0f, 0.25f);
    [SerializeField] private Vector2 hitboxSize = new Vector2(0.25f, 0.75f);
    [SerializeField] private LayerMask enemyLayers = ~0;
    [SerializeField, Range(0f, 1f)] private float hitStart = 0.1f;
    [SerializeField, Range(0f, 1f)] private float hitEnd = 0.55f;
    [SerializeField, Range(0f, 1f)] private float chargedHitStart = 0.4f;
    [SerializeField, Range(0f, 1f)] private float chargedHitEnd = 0.62f;

    private const string ChargeFullSoundPath = "Audio/SE/chargeFull";
    private const float ChargePoseTime = 0.4f;
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
    private AudioClip chargeFullSound;
    private AudioSource sfx;
    private bool charging;
    private bool chargeStarted;
    private bool chargeFull;
    private bool pulsing;
    private bool swinging;
    private bool normalAttack;
    private bool staminaRestored;
    private int attackDamage;
    private float heldTime;
    private float raiseStartedAt;
    private float chargeProgress;
    private float heldStamina;
    private float pulseElapsed;
    private float lastProgress;
    private Vector3 weaponScale;
    private Vector2 attackOffset;
    private Vector2 attackSize;

    private bool CanAct => isActiveAndEnabled && owner != null
        && owner.isActiveAndEnabled && owner.canAct && weaponAnimator != null
        && weaponAnimator.isActiveAndEnabled;

    private bool HasSameWeapon => attackWeapon != null && items != null
        && items.getActiveQuickItem() == attackWeapon;

    private float ChargedCost => Mathf.Max(0f, chargedStaminaCost);

    private void Awake()
    {
        owner = GetComponent<player>();
        items = GetComponent<inventory>();
        if (weaponAnimator == null && owner.mainHand != null)
            weaponAnimator = owner.mainHand.GetComponent<Animator>();
        if (weaponAnimator != null)
        {
            defaultController = weaponAnimator.runtimeAnimatorController;
            weaponScale = weaponAnimator.transform.localScale;
        }

        //the audio is loaded by path temporarily
        chargeFullSound = Resources.Load<AudioClip>(ChargeFullSoundPath);
        if (chargeFullSound == null)
        {
            Debug.LogWarning("Missing charge full sound at Resources/" + ChargeFullSoundPath);
            return;
        }
        sfx = GetComponent<AudioSource>();
        if (sfx == null)
        {
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        items.onChanged += OnInventoryChanged;
    }

    public void BeginCharge()
    {
        if (!CanAct || charging || swinging || defaultController == null) return;
        attackWeapon = items.getActiveQuickItem();
        if (attackWeapon == null || attackWeapon.type != itemType.weapon) return;
        UseWeaponController();
        if (!weaponAnimator.HasState(0, SwingState)) return;
        heldTime = 0f;
        chargeProgress = 0f;
        charging = true;
    }

    public void ReleaseCharge()
    {
        if (!charging) return;
        charging = false;
        bool charged = chargeStarted;
        // held past the threshold without enough stamina to charge, the release does nothing
        if ((!charged && heldTime >= chargedAttackThreshold) || !CanAct || !HasSameWeapon)
        {
            CancelAttack();
            return;
        }
        attackStamina = owner.stamina_System;
        if (attackStamina == null || !attackStamina.isActiveAndEnabled)
        {
            CancelAttack();
            return;
        }

        normalAttack = !charged;
        // stamina was drained while charging, so the charge progress sets the damage
        attackDamage = normalAttack ? normalDamage : Mathf.RoundToInt(chargedBaseDamage
            * Mathf.Lerp(1f, Mathf.Max(1f, fullChargeDamageMultiplier), chargeProgress));
        attackOffset = attackWeapon.overrideMeleeHitbox ? attackWeapon.meleeHitboxOffset : hitboxOffset;
        attackSize = attackWeapon.overrideMeleeHitbox ? attackWeapon.meleeHitboxSize : hitboxSize;
        attackSize = new Vector2(Mathf.Max(0.01f, attackSize.x), Mathf.Max(0.01f, attackSize.y));

        ClearChargePose();
        swinging = true;
        staminaRestored = false;
        lastProgress = 0f;
        hitEnemies.Clear();
        if (normalAttack)
        {
            weaponAnimator.speed = lightSwingSpeed;
            weaponAnimator.SetTrigger(SwingTrigger);
        }
        weaponAnimator.Update(0f);
        AnimatorStateInfo state = weaponAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.fullPathHash != SwingState)
        {
            CancelAttack();
            return;
        }
        // a charged swing carries on from the raised pose 
        if (!normalAttack) lastProgress = state.normalizedTime;
    }

    private void UseWeaponController()
    {
        if (attackWeapon.meleeSwingAnimation != null)
        {
            if (swingOverride == null) swingOverride = new AnimatorOverrideController(defaultController);
            swingOverride["Swing"] = attackWeapon.meleeSwingAnimation;
            weaponAnimator.runtimeAnimatorController = swingOverride;
        }
        else weaponAnimator.runtimeAnimatorController = defaultController;
    }

    private void Update()
    {
        if ((charging || swinging) && (!CanAct || !HasSameWeapon)) CancelAttack();
        if (charging) UpdateCharge();
        if (pulsing) UpdatePulse();
    }

    private void UpdateCharge()
    {
        heldTime += Time.deltaTime;
        if (heldTime < chargedAttackThreshold) return;
        Stamina_System stamina = owner.stamina_System;
        if (stamina == null || !stamina.isActiveAndEnabled) return;
        if (!chargeStarted)
        {
            if (stamina.Stamina < chargedMinStamina) return;
            chargeStarted = true;
            heldStamina = stamina.Stamina;
            raiseStartedAt = heldTime;
            // freeze the swing clip so the sword stays in its raised pose
            weaponAnimator.speed = 0f;
        }
        float raise = Mathf.Clamp01((heldTime - raiseStartedAt) / chargeRaiseDuration);
        weaponAnimator.Play(SwingState, 0, raise * ChargePoseTime);

        // stamina stop regenerate while a charge is held, so take back anything gained since last frame
        float available = Mathf.Min(stamina.Stamina, heldStamina);
        float regained = stamina.Stamina - available;
        float gain = Mathf.Min(Time.deltaTime
            / Mathf.Max(0.01f, fullChargeDuration - chargedAttackThreshold), 1f - chargeProgress);
        float want = gain * ChargedCost;
        float take = Mathf.Min(want, available);
        stamina.ConsumeStamina(regained + take);
        heldStamina = available - take;
        chargeProgress += ChargedCost > 0f ? take / ChargedCost : gain;

        if (chargeFull) return;
        if (chargeProgress >= 0.999f)
        {
            chargeProgress = 1f;
            chargeFull = true;
            pulsing = true;
            pulseElapsed = 0f;
            if (sfx != null) sfx.PlayOneShot(chargeFullSound);
        }
        // if out of stamina, wautomatically swing with the charge built so far
        else if (take < want) ReleaseCharge();
    }

    private void UpdatePulse()
    {
        if (weaponAnimator == null)
        {
            pulsing = false;
            return;
        }
        pulseElapsed += Time.deltaTime;
        float t = pulseElapsed / chargeFullPulseDuration;
        if (t >= 1f)
        {
            pulsing = false;
            weaponAnimator.transform.localScale = weaponScale;
            return;
        }
        // scale the sword up and down to cue the player that the charge is full
        weaponAnimator.transform.localScale = weaponScale
            * Mathf.Lerp(1f, chargeFullPulseScale, Mathf.Sin(t * Mathf.PI));
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
            weaponAnimator.speed = 1f;
            swinging = false;
            return;
        }
        float progress = state.normalizedTime;
        float start = normalAttack ? hitStart : chargedHitStart;
        float end = normalAttack ? hitEnd : chargedHitEnd;
        // include a window crossed between frames without rebuilding the swing trajectory
        bool inWindow = progress >= start && lastProgress <= Mathf.Max(start, end);
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
        if ((swinging || chargeStarted) && weaponAnimator != null && weaponAnimator.isActiveAndEnabled)
        {
            weaponAnimator.ResetTrigger(SwingTrigger);
            weaponAnimator.Play(IdleState, 0, 0f);
            weaponAnimator.Update(0f);
        }
        ClearChargePose();
        swinging = false;
        hitEnemies.Clear();
    }

    private void ClearChargePose()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.speed = 1f;
            if (pulsing) weaponAnimator.transform.localScale = weaponScale;
        }
        chargeStarted = false;
        chargeFull = false;
        pulsing = false;
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
