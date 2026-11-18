using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(player), typeof(inventory))]
public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private Animator weaponAnimator;
    [Header("Charge Full Cue")]
    [SerializeField, Min(1f)] private float chargeFullPulseScale = 1.2f;
    [SerializeField, Min(0.01f)] private float chargeFullPulseDuration = 0.15f;
    [Header("Hit Detection")]
    [SerializeField] private LayerMask enemyLayers = ~0;

    private const string ChargeFullSoundPath = "Audio/SE/chargeFull";
    private static readonly int SwingState = Animator.StringToHash("Base Layer.Swing");
    private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");
    private static readonly int SwingTrigger = Animator.StringToHash("Swing");
    private readonly HashSet<enemyBase> hitEnemies = new HashSet<enemyBase>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private player owner;
    private inventory items;
    private RuntimeAnimatorController defaultController;
    private AnimationClip defaultSwingClip;
    private AnimationClip attackClip;
    private AnimatorOverrideController swingOverride;
    private GameObject hitPoseSampler;
    private Stamina_System attackStamina;
    private weaponData attackWeapon;
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
    private int lastHitWindow;
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

    private float ChargedCost => Mathf.Max(0f, attackWeapon.staminaCost);

    private void Awake()
    {
        owner = GetComponent<player>();
        items = GetComponent<inventory>();
        if (weaponAnimator == null && owner.mainHand != null)
            weaponAnimator = owner.mainHand.GetComponent<Animator>();
        if (weaponAnimator != null)
        {
            defaultController = weaponAnimator.runtimeAnimatorController;
            // The shared controller has one animated state (Swing); Idle has no clip.
            if (defaultController != null && defaultController.animationClips.Length > 0)
                defaultSwingClip = defaultController.animationClips[0];
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
        if (!CanAct || charging || swinging || defaultController == null || defaultSwingClip == null) return;
        attackWeapon = items.getActiveQuickItem() as weaponData;
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
        if (!CanAct || !HasSameWeapon
            || (!charged && heldTime >= attackWeapon.chargedAttackThreshold))
        {
            CancelAttack();
            return;
        }
        attackStamina = Stamina_System.Instance;
        if (attackStamina == null || !attackStamina.isActiveAndEnabled)
        {
            CancelAttack();
            return;
        }

        normalAttack = !charged;
        // stamina was drained while charging, so the charge progress sets the damage
        float damage = attackWeapon.GetDamage(LevelingSystem.Instance);
        if (!normalAttack)
            damage *= Mathf.Max(1f, attackWeapon.chargedDamageMultiplier)
                * Mathf.Lerp(1f, Mathf.Max(1f, attackWeapon.fullChargeDamageMultiplier), chargeProgress);
        attackDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
        attackOffset = attackWeapon.meleeHitboxOffset;
        attackSize = attackWeapon.meleeHitboxSize;
        attackSize = new Vector2(Mathf.Max(0.01f, attackSize.x), Mathf.Max(0.01f, attackSize.y));

        ClearChargePose();
        weaponAnimator.speed = Mathf.Max(0.1f, attackWeapon.attackSpeed);
        swinging = true;
        staminaRestored = false;
        lastProgress = 0f;
        lastHitWindow = -1;
        hitEnemies.Clear();
        if (normalAttack)
        {
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

    private void UseWeaponController(bool charged = false)
    {
        attackClip = attackWeapon.meleeSwingAnimation != null ? attackWeapon.meleeSwingAnimation : defaultSwingClip;
        if (charged && attackWeapon.chargedSwingAnimation != null)
            attackClip = attackWeapon.chargedSwingAnimation;
        if (attackClip != defaultSwingClip)
        {
            if (swingOverride == null) swingOverride = new AnimatorOverrideController(defaultController);
            swingOverride[defaultSwingClip] = attackClip;
            weaponAnimator.runtimeAnimatorController = swingOverride;
        }
        else weaponAnimator.runtimeAnimatorController = defaultController;
    }

    private void Update()
    {
        if ((charging || swinging) && (!CanAct || !HasSameWeapon)) CancelAttack();
        if (charging) UpdateCharge(Time.deltaTime);
        if (pulsing) UpdatePulse();
    }

    private void UpdateCharge(float deltaTime)
    {
        float previousHeldTime = heldTime;
        heldTime += deltaTime;
        if (heldTime < attackWeapon.chargedAttackThreshold) return;
        Stamina_System stamina = Stamina_System.Instance;
        if (stamina == null || !stamina.isActiveAndEnabled) return;
        if (!chargeStarted)
        {
            if (ChargedCost > 0f && stamina.Stamina < attackWeapon.chargedMinStamina) return;
            UseWeaponController(true);
            chargeStarted = true;
            heldStamina = stamina.Stamina;
            raiseStartedAt = heldTime;
            // freeze the swing clip so the sword stays in its raised pose
            weaponAnimator.speed = 0f;
        }
        float raise = Mathf.Clamp01((heldTime - raiseStartedAt)
            / Mathf.Max(0.01f, attackWeapon.chargeRaiseDuration));
        weaponAnimator.Play(SwingState, 0, raise * attackWeapon.chargePoseTime);

        // stamina stop regenerate while a charge is held, so take back anything gained since last frame
        float available = Mathf.Min(stamina.Stamina, heldStamina);
        float regained = stamina.Stamina - available;
        // Only the part of this frame after the threshold contributes to charge.
        float chargeDelta = heldTime - Mathf.Max(previousHeldTime, attackWeapon.chargedAttackThreshold);
        float gain = Mathf.Min(chargeDelta / Mathf.Max(0.01f,
            attackWeapon.fullChargeDuration - attackWeapon.chargedAttackThreshold), 1f - chargeProgress);
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
        // if out of stamina, automatically swing with the charge built so far
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
        // A very slow frame can advance all the way to Idle. Still resolve the crossed window.
        float progress = state.fullPathHash == SwingState ? state.normalizedTime : 1f;
        ChargedHitWindow[] windows = attackWeapon.chargedHitWindows;
        if (!normalAttack && windows != null && windows.Length > 0)
        {
            for (int i = 0; i < windows.Length; i++)
                ProcessHitWindow(i, progress, windows[i].startPercent / 100f, windows[i].endPercent / 100f);
        }
        else
            ProcessHitWindow(0, progress,
                normalAttack ? attackWeapon.hitStart : attackWeapon.chargedHitStart,
                normalAttack ? attackWeapon.hitEnd : attackWeapon.chargedHitEnd);
        lastProgress = progress;
        if (state.fullPathHash != SwingState)
        {
            weaponAnimator.speed = 1f;
            swinging = false;
            return;
        }
    }

    private void ProcessHitWindow(int index, float progress, float start, float end)
    {
        end = Mathf.Max(start, end);
        if (index < lastHitWindow || progress < start || lastProgress > end) return;
        if (index != lastHitWindow)
        {
            // Multi-strike attacks may hit the same target again, once per distinct strike.
            hitEnemies.Clear();
            lastHitWindow = index;
        }
        SampleHitWindow(progress, start, end);
    }

    private void SampleHitWindow(float progress, float start, float end)
    {
        if (hitPoseSampler == null)
        {
            // Transform-only probe: sample the real animation without moving the visible weapon.
            hitPoseSampler = new GameObject("Weapon Hit Pose") { hideFlags = HideFlags.HideAndDontSave };
            hitPoseSampler.SetActive(false);
        }
        Transform pose = hitPoseSampler.transform;
        pose.SetParent(weaponAnimator.transform.parent, false);
        pose.localScale = weaponScale;
        float from = Mathf.Max(lastProgress, start);
        float to = Mathf.Min(progress, end);
        int steps = Mathf.Max(1, Mathf.CeilToInt((to - from) / 0.02f));
        for (int i = 0; i <= steps; i++)
        {
            float time = Mathf.Lerp(from, to, (float)i / steps) * attackClip.length;
            attackClip.SampleAnimation(hitPoseSampler, time);
            QueryHits(pose.position, pose.eulerAngles.z, pose.lossyScale);
        }
    }

    private void QueryHits(Vector3 position, float angle, Vector3 scale)
    {
        Matrix4x4 pose = Matrix4x4.TRS(position, Quaternion.Euler(0f, 0f, angle), scale);
        Vector2 centre = pose.MultiplyPoint3x4(attackOffset);
        Vector2 size = new Vector2(Mathf.Abs(attackSize.x * scale.x), Mathf.Abs(attackSize.y * scale.y));
        var filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(enemyLayers);
        overlaps.Clear();
        if (attackWeapon.attackShape == AttackShape.Circle)
            Physics2D.OverlapCircle(centre, Mathf.Max(0.01f, attackWeapon.areaRadius)
                * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)), filter, overlaps);
        else
            Physics2D.OverlapBox(centre, size, angle, filter, overlaps);
        foreach (Collider2D collider in overlaps)
        {
            if (collider.transform.IsChildOf(transform)) continue;
            enemyBase enemy = collider.GetComponentInParent<enemyBase>();
            if (enemy == null || hitEnemies.Contains(enemy) || !enemy.takeDmg(attackDamage)) continue;
            hitEnemies.Add(enemy);
            if (normalAttack && !staminaRestored)
            {
                staminaRestored = true;
                attackStamina.Stamina += Mathf.Max(0f, attackWeapon.lightHitStaminaRestore);
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
        if (hitPoseSampler != null) Destroy(hitPoseSampler);
        if (swingOverride != null)
        {
            if (weaponAnimator != null) weaponAnimator.runtimeAnimatorController = defaultController;
            Destroy(swingOverride);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (weaponAnimator == null) return;
        var equipped = GetComponent<inventory>().getActiveQuickItem() as weaponData;
        if (equipped == null) return;
        Gizmos.color = Color.yellow;
        Matrix4x4 previous = Gizmos.matrix;
        if (equipped.attackShape == AttackShape.Circle)
        {
            Vector3 scale = weaponAnimator.transform.lossyScale;
            Gizmos.matrix = Matrix4x4.TRS(weaponAnimator.transform.TransformPoint(equipped.meleeHitboxOffset),
                Quaternion.identity, Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, equipped.areaRadius
                * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
        }
        else
        {
            Gizmos.matrix = weaponAnimator.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(equipped.meleeHitboxOffset, equipped.meleeHitboxSize);
        }
        Gizmos.matrix = previous;
    }
}
