using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(player), typeof(inventory))]
public class PlayerCombat : MonoBehaviour
{
    private enum CombatState { Idle, PendingPress, Charging, Executing }

    [SerializeField] private player playerController;
    [SerializeField] private inventory playerInventory;
    [SerializeField] private MonoBehaviour attackExecutor;
    [SerializeField, Min(0.01f)] private float heavyThreshold = 0.2f;

    private CombatState state;
    private bool pressHeld;
    private bool lightRestoreGranted;
    private bool warnedMissingExecutor;
    private double chargeStartsAt;
    private double lastChargeUpdate;
    private float chargeElapsed;
    private int nextAttackId;
    private int activeAttackId;
    private MeleeAttackKind activeAttackKind;
    private Stamina_System staminaSystem;
    private WeaponDefinition weapon;

    //copy values at press time to prevent switching weapons change the attack
    private int lightDamage;
    private int heavyDamage;
    private float lightRestore;
    private float chargeDrain;
    private float chargeDuration;
    private float maxMultiplier;
    private MeleePatternData lightPattern;
    private MeleePatternData heavyPattern;

    public bool IsBusy => state != CombatState.Idle;
    public bool IsCharging => state == CombatState.Charging;
    public float ChargeRatio => chargeDuration > 0f
        ? Mathf.Clamp01(chargeElapsed / chargeDuration) : 0f;

    private bool CanAct => isActiveAndEnabled && playerController != null
        && playerController.isActiveAndEnabled && playerController.canAct;

    private bool HasExecutor => attackExecutor != null
        && attackExecutor.isActiveAndEnabled && attackExecutor is IMeleeAttackExecutor;

    private void Awake()
    {
        if (playerController == null) playerController = GetComponent<player>();
        if (playerInventory == null) playerInventory = GetComponent<inventory>();
    }

    public void OnAttackPressed()
    {
        if (pressHeld) return;
        pressHeld = true;

        if (!CanAct || IsBusy || playerInventory == null) return;
        if (!HasExecutor)
        {
            if (!warnedMissingExecutor)
            {
                Debug.LogWarning("Assign an IMeleeAttackExecutor to PlayerCombat before attacking.", this);
                warnedMissingExecutor = true;
            }
            return;
        }

        itemData item = playerInventory.getActiveQuickItem();
        if (item == null || item.type != itemType.weapon)
            return;

        weapon = item.weaponDefinition;
        WeaponCombatData data = weapon != null ? weapon.combatDefaults : item.weaponDefaults;
        staminaSystem = playerController.stamina_System;
        if (data == null || staminaSystem == null || !staminaSystem.isActiveAndEnabled)
            return;

        lightDamage = Mathf.Max(0, data.lightDamage);
        heavyDamage = Mathf.Max(0, data.heavyBaseDamage);
        lightRestore = Mathf.Max(0f, data.lightHitStaminaRestore);
        chargeDrain = Mathf.Max(0f, data.chargeStaminaPerSecond);
        chargeDuration = Mathf.Max(0.01f, data.fullChargeDuration);
        maxMultiplier = Mathf.Max(1f, data.maxChargeDamageMultiplier)
            * (1f + (weapon != null ? Mathf.Max(0f, weapon.extraFullChargeBonus) : 0f));
        lightPattern = data.lightPattern;
        heavyPattern = data.heavyPattern;
        chargeElapsed = 0f;
        chargeStartsAt = Time.timeAsDouble + Mathf.Max(0.01f, heavyThreshold);
        lastChargeUpdate = chargeStartsAt;
        state = CombatState.PendingPress;
    }

    public void OnAttackReleased()
    {
        if (!pressHeld) return;
        pressHeld = false;
        if (!CanContinue()) return;

        //include the final interval even if release happens before update
        AdvanceCharge(Time.timeAsDouble);
        if (state == CombatState.PendingPress)
            StartAttack(MeleeAttackKind.Light);
        else if (state == CombatState.Charging)
            StartAttack(MeleeAttackKind.Heavy);
    }

    private void Update()
    {
        if (!IsBusy || !CanContinue()) return;
        AdvanceCharge(Time.timeAsDouble);
    }

    private bool CanContinue()
    {
        if (!CanAct || !HasExecutor || staminaSystem == null || !staminaSystem.isActiveAndEnabled)
        {
            CancelCombat();
            return false;
        }
        return true;
    }

    private void AdvanceCharge(double now)
    {
        if (state == CombatState.PendingPress && now >= chargeStartsAt)
        {
            if (staminaSystem.Stamina <= 0f)
            {
                FinishAction();
                return;
            }
            state = CombatState.Charging;
            staminaSystem.BeginCombatAction();
        }

        if (state != CombatState.Charging) return;

        float remaining = Mathf.Max(0f, chargeDuration - chargeElapsed);
        float requested = Mathf.Min(Mathf.Max(0f, (float)(now - lastChargeUpdate)), remaining);
        lastChargeUpdate = now;
        float available = staminaSystem.Stamina;
        float affordable = chargeDrain > 0f ? available / chargeDrain : requested;
        float actual = Mathf.Min(requested, affordable);

        // an exhausted charge only gains the time its remaining stamina paid for
        bool exhausted = chargeDrain > 0f && affordable <= requested;
        staminaSystem.ConsumeStamina(exhausted ? available : actual * chargeDrain);
        chargeElapsed = actual >= remaining ? chargeDuration : chargeElapsed + actual;

        if (chargeElapsed >= chargeDuration || exhausted)
            StartAttack(MeleeAttackKind.Heavy);
    }

    private void StartAttack(MeleeAttackKind kind)
    {
        state = CombatState.Executing;
        activeAttackId = ++nextAttackId;
        activeAttackKind = kind;
        lightRestoreGranted = false;
        staminaSystem.BeginCombatAction();

        float ratio = kind == MeleeAttackKind.Heavy ? ChargeRatio : 0f;
        float multiplier = kind == MeleeAttackKind.Heavy
            ? Mathf.Lerp(1f, maxMultiplier, ratio) : 1f;
        int damage = Mathf.RoundToInt((kind == MeleeAttackKind.Heavy ? heavyDamage : lightDamage)
            * multiplier);
        var request = new MeleeAttackRequest(activeAttackId, weapon, kind, ratio, multiplier, damage,
            kind == MeleeAttackKind.Heavy ? heavyPattern : lightPattern);

        // enter execution before calling out so immediate feedback is safe
        try
        {
            if (!((IMeleeAttackExecutor)attackExecutor).TryStartAttack(request, this))
                CancelCombat();
        }
        catch (System.Exception exception)
        {
            CancelCombat();
            Debug.LogException(exception, this);
        }
    }

    public void NotifyValidHit(int attackId)
    {
        if (!CanContinue() || state != CombatState.Executing || attackId != activeAttackId
            || activeAttackKind != MeleeAttackKind.Light || lightRestoreGranted)
            return;

        // the executor reports this only after the target accepts damage
        lightRestoreGranted = true;
        staminaSystem.Stamina += lightRestore;
    }

    // completion includes recovery  ; interruption uses the same completion path
    public void NotifyAttackFinished(int attackId)
    {
        if (state != CombatState.Executing || attackId != activeAttackId) return;
        FinishAction();
    }

    public void CancelCombat()
    {
        int canceledId = activeAttackId;
        bool wasExecuting = state == CombatState.Executing;
        FinishAction();
        pressHeld = false;

        // invalidate feedback before canceling an external executor
        if (wasExecuting && attackExecutor != null && attackExecutor is IMeleeAttackExecutor executor)
        {
            try { executor.CancelAttack(canceledId); }
            catch (System.Exception exception) { Debug.LogException(exception, this); }
        }
    }

    private void FinishAction()
    {
        state = CombatState.Idle;
        activeAttackId = 0;
        chargeElapsed = 0f;
        if (staminaSystem != null) staminaSystem.EndCombatAction();
    }

    private void OnDisable() => CancelCombat();

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) CancelCombat();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) CancelCombat();
    }
}
