using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class inventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public slotGroup group;
    public int index;
    public Image background;
    public Image icon;

    inventoryUI ui;
    Color baseColor;
    bool hovered;
    float hoverAmount;

    private void Awake()
    {
        ui = GetComponentInParent<inventoryUI>(true);
        baseColor = background.color;
    }

    private void OnDisable()
    {
        hovered = false;
        hoverAmount = 0;
        applyHover();
    }

    void Update()
    {
        float target = hovered ? 1 : 0;
        if (hoverAmount == target) return;
        hoverAmount = Mathf.MoveTowards(hoverAmount, target, Time.unscaledDeltaTime / ui.hoverDuration);
        applyHover();
    }

    public void show(itemData item, Color color, bool beingDragged, Quaternion iconRotation)
    {
        baseColor = color;
        applyHover();
        icon.sprite = item == null ? null : item.icon;
        icon.enabled = item != null && !beingDragged;
        icon.rectTransform.localRotation = iconRotation;
    }

    void applyHover()
    {
        transform.localScale = Vector3.one * Mathf.Lerp(1, ui.hoverScale, hoverAmount);
        Color color = baseColor;
        color.a = Mathf.Lerp(baseColor.a, ui.hoverAlpha, hoverAmount);
        background.color = color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ui.beginDrag(this, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ui.drag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ui.endDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        ui.drop(this);
    }
}
