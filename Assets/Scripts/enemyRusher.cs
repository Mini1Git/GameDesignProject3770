using UnityEngine;

// Rusher is fast and strong but stupid, it rotates and rushes towards player without concern for pathfinding

public class enemyRusher : enemyBase
{

    public float speed = 5f;

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {

        if (aggroed)
        {
            // makes enemy face where its going
            Vector2 direction = player.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                180f * Time.deltaTime * turnSpeed
            );

            rb.linearVelocity = (Vector2)transform.right * speed;

            if (Vector2.Distance(player.transform.position, transform.position) > aggroRange)
            {
                aggroed = false;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
