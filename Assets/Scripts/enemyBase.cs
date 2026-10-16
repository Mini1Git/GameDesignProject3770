using UnityEngine;

public class enemyBase : MonoBehaviour
{
    protected GameObject player;
    protected bool aggroed = false;

    public Rigidbody2D rb;
    public Collider2D detection; 
    public int health = 10;
    public float aggroRange = 10f;
    public float turnSpeed = 1f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            aggroed = true;

        }
    }

    // return true only when the enemy can be damaged
    public bool TryTakeDamage(int damage)
    {
        if (damage <= 0 || health <= 0 || !isActiveAndEnabled) return false;
        takeDmg(damage);
        return true;
    }

    void takeDmg(int dmg)
    {
        health -= dmg;
        if(health <= 0)
        {
            die();
        }
    }

    void die()
    {
        Destroy(gameObject);
    }
}
