using UnityEngine;

public class projectileSpell : MonoBehaviour
{
    public float speed;
    
    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }
}
