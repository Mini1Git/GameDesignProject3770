using UnityEngine;

public class enemyBase : MonoBehaviour
{
    protected GameObject player;
    public Rigidbody2D rb;
    public int health = 10;
    public float speed = 10;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void takeDmg(int dmg)
    {
        health -= dmg;
        if(health < 0)
        {
            die();
        }
    }

    void die()
    {
        Destroy(gameObject);
    }
}
