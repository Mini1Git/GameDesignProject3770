using UnityEngine;

public class itemPickup : MonoBehaviour
{
    public itemData item;
    public SpriteRenderer spriteRenderer;
    public float bobHeight = 0.1f;
    public float bobSpeed = 2f;
    public float pickupDelay;

    void Start()
    {
        showItem();
    }

    void Update()
    {
        if (pickupDelay > 0) pickupDelay -= Time.deltaTime;

        float height = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        spriteRenderer.transform.localPosition = new Vector3(0, height, 0);
    }

    private void OnValidate()
    {
        showItem();
    }

    void showItem()
    {
        if (item != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = item.icon;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (pickupDelay > 0) return;

        inventory playerInventory = collision.GetComponent<inventory>();
        if (playerInventory == null) return;

        if (playerInventory.addItem(item))
        {
            Destroy(gameObject);
        }
    }
}
