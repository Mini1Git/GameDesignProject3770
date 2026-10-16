using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SwordAttackExecutor : MonoBehaviour, IMeleeAttackExecutor
{
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Material slashMaterial;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private int sortingOrder = 20;

    private readonly HashSet<enemyBase> hitTargets = new HashSet<enemyBase>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private GameObject visual;
    private SpriteRenderer slash;
    private LineRenderer placeholder;
    private PlayerCombat combat;
    private MeleeAttackRequest attack;
    private bool running;
    private double startedAt;
    private float lastActiveTime;
    private bool sampledStart;
    private Vector3 origin;
    private float facing;
    private bool usePlaceholder;

    // snapshot pattern values so edits or equipment changes cannot alter an active slash
    private float windup;
    private float duration;
    private float recovery;
    private float startDistance;
    private float travelDistance;
    private float startAngle;
    private float endAngle;
    private float spriteAngle;
    private Vector2 visualSize;
    private Vector2 hitboxSize;
    private Color tint;

    public bool TryStartAttack(MeleeAttackRequest request, PlayerCombat owner)
    {
        if (!isActiveAndEnabled || running || owner == null || request.Pattern == null
            || slashMaterial == null)
            return false;

        MeleePatternData pattern = request.Pattern;
        windup = Mathf.Max(0f, pattern.windup);
        duration = Mathf.Max(0.01f, pattern.activeDuration);
        recovery = Mathf.Max(0f, pattern.recovery);
        startDistance = Mathf.Max(0f, pattern.startDistance);
        travelDistance = Mathf.Max(0f, pattern.travelDistance);
        startAngle = pattern.startAngle;
        endAngle = pattern.endAngle;
        spriteAngle = pattern.spriteAngleOffset;
        visualSize = new Vector2(Mathf.Max(0.01f, pattern.visualSize.x), Mathf.Max(0.01f, pattern.visualSize.y));
        hitboxSize = new Vector2(Mathf.Max(0.05f, pattern.hitboxSize.x), Mathf.Max(0.05f, pattern.hitboxSize.y));
        tint = pattern.tint;

        EnsureVisual();
        slash.sprite = pattern.slashSprite;
        usePlaceholder = slash.sprite == null
            || (slash.sprite.texture.width == 1 && slash.sprite.texture.height == 1);

        Transform source = attackOrigin != null ? attackOrigin : transform;
        origin = source.position;
        facing = source.eulerAngles.z;
        combat = owner;
        attack = request;
        hitTargets.Clear();
        startedAt = Time.timeAsDouble;
        lastActiveTime = 0f;
        sampledStart = false;
        running = true;
        HideVisual();
        return true;
    }

    private void Update()
    {
        if (running) AdvancePattern((float)(Time.timeAsDouble - startedAt));
    }

    private void AdvancePattern(float elapsed)
    {
        if (!running || elapsed < windup) return;
        if (combat == null || !combat.isActiveAndEnabled)
        {
            CancelAttack(attack.AttackId);
            return;
        }

        float activeTime = Mathf.Clamp(elapsed - windup, 0f, duration);
        if (!sampledStart || activeTime > lastActiveTime)
        {
            // sample the swept path when a frame skips part of the active window
            float pathLength = travelDistance
                + Mathf.Abs(endAngle - startAngle) * Mathf.Deg2Rad * (startDistance + travelDistance);
            float stepSize = Mathf.Max(0.025f, Mathf.Min(hitboxSize.x, hitboxSize.y) * 0.5f);
            int steps = Mathf.Max(1, Mathf.CeilToInt(pathLength * (activeTime - lastActiveTime) / duration / stepSize));
            if (!sampledStart) DetectHits(0f);
            for (int i = 1; i <= steps && running; i++)
                DetectHits(Mathf.Lerp(lastActiveTime, activeTime, (float)i / steps) / duration);
            sampledStart = true;
            lastActiveTime = activeTime;
        }

        if (!running) return;
        if (elapsed < windup + duration)
            ShowVisual(activeTime / duration);
        else
            HideVisual();

        if (elapsed >= windup + duration + recovery)
            FinishAttack();
    }

    private void DetectHits(float progress)
    {
        GetPose(progress, out Vector3 position, out float angle);
        var filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(targetLayers);
        overlaps.Clear();
        Physics2D.OverlapBox(position, hitboxSize, angle, filter, overlaps);

        foreach (Collider2D collider in overlaps)
        {
            if (!running) break;
            if (collider == null || collider.isTrigger || collider.transform.IsChildOf(transform)) continue;
            enemyBase target = collider.GetComponentInParent<enemyBase>();
            if (target == null || hitTargets.Contains(target)) continue;
            if (!target.TryTakeDamage(attack.Damage)) continue;
            hitTargets.Add(target);
            combat.NotifyValidHit(attack.AttackId);
        }
    }

    private void GetPose(float progress, out Vector3 position, out float angle)
    {
        angle = facing + Mathf.Lerp(startAngle, endAngle, progress);
        float radians = angle * Mathf.Deg2Rad;
        float distance = startDistance + travelDistance * progress;
        position = origin + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * distance;
    }

    private void ShowVisual(float progress)
    {
        GetPose(progress, out Vector3 position, out float angle);
        visual.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle + spriteAngle));
        Color color = tint;
        color.a *= Mathf.Min(1f, (1f - progress) / 0.35f);
        slash.enabled = !usePlaceholder;
        placeholder.enabled = usePlaceholder;

        if (usePlaceholder)
        {
            visual.transform.localScale = new Vector3(visualSize.x, visualSize.y, 1f);
            placeholder.startColor = color;
            placeholder.endColor = color;
        }
        else
        {
            Vector3 size = slash.sprite.bounds.size;
            visual.transform.localScale = new Vector3(visualSize.x / Mathf.Max(0.001f, size.x),
                visualSize.y / Mathf.Max(0.001f, size.y), 1f);
            slash.color = color;
        }
    }

    private void EnsureVisual()
    {
        if (visual != null) return;
        visual = new GameObject("sword slash visual");
        visual.hideFlags = HideFlags.DontSave;
        SceneManager.MoveGameObjectToScene(visual, gameObject.scene);
        slash = visual.AddComponent<SpriteRenderer>();
        slash.sharedMaterial = slashMaterial;
        slash.sortingOrder = sortingOrder;
        placeholder = visual.AddComponent<LineRenderer>();
        placeholder.sharedMaterial = slashMaterial;
        placeholder.sortingOrder = sortingOrder;
        placeholder.useWorldSpace = false;
        placeholder.positionCount = 25;
        placeholder.widthMultiplier = 0.14f;
        placeholder.widthCurve = new AnimationCurve(new Keyframe(0f, 0f),
            new Keyframe(0.45f, 1f), new Keyframe(1f, 0f));
        placeholder.numCapVertices = 3;
        for (int i = 0; i < placeholder.positionCount; i++)
        {
            float angle = Mathf.Lerp(-80f, 80f, (float)i / (placeholder.positionCount - 1)) * Mathf.Deg2Rad;
            placeholder.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f - 0.25f, Mathf.Sin(angle) * 0.5f, 0f));
        }
    }

    public void CancelAttack(int attackId)
    {
        if (!running || attackId != attack.AttackId) return;
        FinishAttack();
    }

    private void FinishAttack()
    {
        running = false;
        HideVisual();
        PlayerCombat owner = combat;
        combat = null;
        if (owner != null) owner.NotifyAttackFinished(attack.AttackId);
    }

    private void HideVisual()
    {
        if (slash != null) slash.enabled = false;
        if (placeholder != null) placeholder.enabled = false;
    }

    private void OnDisable()
    {
        if (running) FinishAttack();
    }

    private void OnDestroy()
    {
        if (visual == null) return;
        if (Application.isPlaying) Destroy(visual);
        else DestroyImmediate(visual);
    }
}
