using UnityEngine;

[System.Serializable]
public class itemDrop
{
    public itemData item;
    [Range(0, 100)] public float chance = 50;
}

public class dropTable : MonoBehaviour
{
    public itemPickup pickupPrefab;
    public itemDrop[] drops;
    public float spread = 0.5f;

    [ContextMenu("Drop Loot")]
    public void dropLoot()
    {
        foreach (itemDrop drop in drops)
        {
            if (Random.Range(0f, 100f) < drop.chance)
            {
                Vector3 offset = new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0);
                itemPickup pickup = Instantiate(pickupPrefab, transform.position + offset, Quaternion.identity);
                pickup.item = drop.item;
            }
        }
    }
}
