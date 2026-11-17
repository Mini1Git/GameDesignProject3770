using UnityEngine;

public class projectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;
    private Collider2D myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Something entered: " + collision);
        if (collision.gameObject.tag == "Player")
        {
            collision.GetComponent<player>().takeDmg(damage);
        }
        if (collision.gameObject.tag != "Enemy")
        {
            Destroy(gameObject);
        }

    }
}
