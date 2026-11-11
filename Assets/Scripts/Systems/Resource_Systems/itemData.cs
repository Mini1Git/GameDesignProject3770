using UnityEngine;

public enum itemType
{
    weapon,
    item,
    spell
}

[CreateAssetMenu(fileName = "newItem", menuName = "Inventory/Item")]
public class itemData : ScriptableObject
{
    public string itemName;
    public itemType type;
    public Sprite icon;

    [Header("Melee")]
    // leave the animation empty and override off to use the default swing and hitbox
    public AnimationClip meleeSwingAnimation;
    public bool overrideMeleeHitbox;
    public Vector2 meleeHitboxOffset = new Vector2(0f, 0.25f);
    public Vector2 meleeHitboxSize = new Vector2(0.25f, 0.75f);
}
