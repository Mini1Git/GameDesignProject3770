using UnityEngine;

[CreateAssetMenu(fileName = "MeleePattern", menuName = "Combat/Melee Pattern")]
public class MeleePatternData : ScriptableObject
{
    [Header("Timing")]
    [Min(0f)] public float windup = 0.05f;
    [Min(0.01f)] public float activeDuration = 0.18f;
    [Min(0f)] public float recovery = 0.18f;

    [Header("Slash")]
    public Sprite slashSprite;
    public Color tint = Color.white;
    public Vector2 visualSize = new Vector2(1.2f, 2f);
    public float spriteAngleOffset;
    [Min(0f)] public float startDistance = 0.8f;
    [Min(0f)] public float travelDistance = 0.6f;
    public float startAngle = -40f;
    public float endAngle = 40f;

    [Header("Hit Detection")]
    public Vector2 hitboxSize = new Vector2(0.65f, 1.5f);
}
