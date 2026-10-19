using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum slotGroup
{
    inventory,
    quick,
    spell
}

public class inventory : MonoBehaviour
{
    public itemData[] inventorySlots = new itemData[12];
    public itemData[] quickSlots = new itemData[4];
    public itemData[] spellSlots = new itemData[3];
    public int activeQuickSlot;
    public int activeSpellSlot;
    public SpriteRenderer mainHandRenderer;
    public itemPickup pickupPrefab;
    public float dropDistance = 0.6f;

    public event Action onChanged;

    PlayerInput playerInput;
    InputAction cycleQuickSlot;
    InputAction cycleSpell;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        playerInput.Enable();
        cycleQuickSlot = playerInput.PlayerGame.cycleQuickSlot;
        cycleSpell = playerInput.PlayerGame.cycleSpell;

        cycleQuickSlot.performed += onCycleQuickSlot;
        cycleSpell.performed += onCycleSpell;
    }

    private void OnDisable()
    {
        cycleQuickSlot.performed -= onCycleQuickSlot;
        cycleSpell.performed -= onCycleSpell;
        playerInput.Disable();
    }

    void Start()
    {
        updateMainHand();
    }

    public itemData[] getSlots(slotGroup group)
    {
        if (group == slotGroup.quick) return quickSlots;
        if (group == slotGroup.spell) return spellSlots;
        return inventorySlots;
    }

    public itemData getItem(slotGroup group, int index)
    {
        return getSlots(group)[index];
    }

    public itemData getActiveQuickItem()
    {
        return quickSlots[activeQuickSlot];
    }

    public itemData getActiveSpell()
    {
        return spellSlots[activeSpellSlot];
    }

    public bool canPlace(slotGroup group, itemData item)
    {
        if (item == null || group == slotGroup.inventory) return true;
        return (group == slotGroup.spell) == (item.type == itemType.spell);
    }

    public bool canMove(slotGroup fromGroup, int fromIndex, slotGroup toGroup, int toIndex)
    {
        return canPlace(toGroup, getItem(fromGroup, fromIndex)) && canPlace(fromGroup, getItem(toGroup, toIndex));
    }

    public bool addItem(itemData item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                inventorySlots[i] = item;
                onChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public void moveItem(slotGroup fromGroup, int fromIndex, slotGroup toGroup, int toIndex)
    {
        if (fromGroup == toGroup && fromIndex == toIndex) return;
        if (!canMove(fromGroup, fromIndex, toGroup, toIndex)) return;

        itemData[] from = getSlots(fromGroup);
        itemData[] to = getSlots(toGroup);
        itemData moving = from[fromIndex];
        from[fromIndex] = to[toIndex];
        to[toIndex] = moving;

        updateMainHand();
        onChanged?.Invoke();
    }

    public void dropItem(slotGroup group, int index)
    {
        itemData[] slots = getSlots(group);
        itemData item = slots[index];
        if (item == null) return;

        slots[index] = null;

        Vector3 dropPosition = transform.position + transform.right * dropDistance;
        itemPickup pickup = Instantiate(pickupPrefab, dropPosition, Quaternion.identity);
        pickup.item = item;
        pickup.pickupDelay = 1;

        updateMainHand();
        onChanged?.Invoke();
    }

    public void cycleQuickSlots(int direction)
    {
        activeQuickSlot = nextFilledSlot(quickSlots, activeQuickSlot, direction);
        updateMainHand();
        onChanged?.Invoke();
    }

    public void cycleSpellSlots(int direction)
    {
        activeSpellSlot = nextFilledSlot(spellSlots, activeSpellSlot, direction);
        onChanged?.Invoke();
    }

    void onCycleQuickSlot(InputAction.CallbackContext ctx)
    {
        float scroll = ctx.ReadValue<float>();
        if (scroll == 0) return;
        cycleQuickSlots(scroll < 0 ? 1 : -1);
    }

    void onCycleSpell(InputAction.CallbackContext ctx)
    {
        cycleSpellSlots(1);
    }

    int nextFilledSlot(itemData[] slots, int current, int direction)
    {
        for (int i = 1; i <= slots.Length; i++)
        {
            int index = ((current + direction * i) % slots.Length + slots.Length) % slots.Length;
            if (slots[index] != null) return index;
        }
        return current;
    }

    void updateMainHand()
    {
        itemData item = getActiveQuickItem();
        mainHandRenderer.enabled = item != null;
        if (item != null) mainHandRenderer.sprite = item.icon;
    }
}
