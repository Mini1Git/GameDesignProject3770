using UnityEngine;

public class enemyRusher : enemyBase
{

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(aggroed)
        {
            // makes enemy face the player
            Vector2 direction = player.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                180f * Time.deltaTime * turnSpeed
            );

            // Move forward in the direction the enemy is facing
            rb.linearVelocity = transform.right * speed;

            if (Vector2.Distance(player.transform.position, transform.position) > aggroRange)
            {
                aggroed = false;
            }
        }
    }
}
