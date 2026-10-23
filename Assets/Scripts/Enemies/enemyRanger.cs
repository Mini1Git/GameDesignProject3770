using UnityEngine;

//rangers will not try to get closer to player, but instead move further away, and shoot from range

public class enemyRanger : enemyBase
{

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
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

        if (aggroed)
        {
            if (Vector2.Distance(player.transform.position, transform.position) > aggroRange)
            {
                aggroed = false;
            }
        }
    }
}
