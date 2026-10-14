using UnityEngine;

public class enemyMelee : enemyBase
{

    private bool aggroed = true;

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(aggroed)
        {
            // makes the player face the mouse
            Vector2 direction = player.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            rb.linearVelocity = new Vector2(direction.x * speed, direction.y * speed);
        }
    }
}
