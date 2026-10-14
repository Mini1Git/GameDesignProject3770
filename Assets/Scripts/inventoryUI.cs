using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class inventoryUI : MonoBehaviour
{
    public inventory playerInventory;
    public GameObject panel;
    public Image dragIcon;
    public float weaponIconTilt = -45f;
    public float hoverScale = 1.05f;
    public float hoverAlpha = 0.95f;
    public float hoverDuration = 0.1f;

    public Color slotColor = new Color(0.18f, 0.18f, 0.2f, 0.95f);
    public Color quickSlotColor = new Color(0.32f, 0.25f, 0.16f, 0.95f);
    public Color spellSlotColor = new Color(0.17f, 0.2f, 0.36f, 0.95f);
    public Color activeSlotColor = new Color(0.85f, 0.68f, 0.3f, 1f);
    public Color blockedSlotColor = new Color(0.08f, 0.08f, 0.08f, 0.6f);

    public bool isOpen { get; private set; }

    PlayerInput playerInput;
    InputAction toggleInventory;
    inventorySlot[] slots;
    inventorySlot dragSource;
    player playerController;

    private void Awake()
    {
        playerInput = new PlayerInput();
        slots = GetComponentsInChildren<inventorySlot>(true);

        if (playerInventory == null) playerInventory = FindAnyObjectByType<inventory>();
        playerController = playerInventory.GetComponent<player>();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }

    private void OnEnable()
    {
        playerInput.Enable();
        toggleInventory = playerInput.PlayerGame.toggleInventory;

        toggleInventory.performed += onToggleInventory;
        playerInventory.onChanged += refresh;
    }

    private void OnDisable()
    {
        toggleInventory.performed -= onToggleInventory;
        playerInventory.onChanged -= refresh;
        playerInput.Disable();
    }

    void Start()
    {
        setOpen(false);
    }

    void onToggleInventory(InputAction.CallbackContext ctx)
    {
        setOpen(!isOpen);
    }

    public void setOpen(bool open)
    {
        isOpen = open;
        dragSource = null;
        dragIcon.gameObject.SetActive(false);
        panel.SetActive(open);
        playerController.canAct = !open;
        refresh();
    }

    public void refresh()
    {
        itemData dragged = dragSource == null ? null : playerInventory.getItem(dragSource.group, dragSource.index);

        foreach (inventorySlot slot in slots)
        {
            itemData item = playerInventory.getItem(slot.group, slot.index);
            slot.show(item, getSlotColor(slot, dragged), slot == dragSource, getIconRotation(item));
        }
    }

    Quaternion getIconRotation(itemData item)
    {
        bool isWeapon = item != null && item.type == itemType.weapon;
        return Quaternion.Euler(0, 0, isWeapon ? weaponIconTilt : 0);
    }

    Color getSlotColor(inventorySlot slot, itemData dragged)
    {
        if (dragged != null && !playerInventory.canMove(dragSource.group, dragSource.index, slot.group, slot.index)) return blockedSlotColor;
        if (slot.group == slotGroup.quick) return slot.index == playerInventory.activeQuickSlot ? activeSlotColor : quickSlotColor;
        if (slot.group == slotGroup.spell) return slot.index == playerInventory.activeSpellSlot ? activeSlotColor : spellSlotColor;
        return slotColor;
    }

    public void beginDrag(inventorySlot slot, Vector2 position)
    {
        itemData item = playerInventory.getItem(slot.group, slot.index);
        if (item == null) return;

        dragSource = slot;
        dragIcon.sprite = item.icon;
        dragIcon.rectTransform.localRotation = getIconRotation(item);
        dragIcon.transform.position = position;
        dragIcon.gameObject.SetActive(true);
        refresh();
    }

    public void drag(Vector2 position)
    {
        if (dragSource != null) dragIcon.transform.position = position;
    }

    public void drop(inventorySlot target)
    {
        if (dragSource == null) return;
        playerInventory.moveItem(dragSource.group, dragSource.index, target.group, target.index);
    }

    public void endDrag()
    {
        dragSource = null;
        dragIcon.gameObject.SetActive(false);
        refresh();
    }
}
