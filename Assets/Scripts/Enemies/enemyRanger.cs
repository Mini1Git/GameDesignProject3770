using UnityEngine;

//rangers will not try to get closer to player, but instead move further away, and shoot from range

public class enemyRanger : enemyBase
{

    public GameObject projectilePrefab;
    public float cooldown = 5f;
    float currCooldown;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        currCooldown = cooldown;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (aggroed)
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

            currCooldown -= Time.deltaTime;
            if(currCooldown <= 0)
            {
                Debug.Log("Spawn Rock");
                Instantiate(projectilePrefab, transform.position, transform.rotation);
                currCooldown = cooldown;
            }

            if (Vector2.Distance(player.transform.position, transform.position) > aggroRange)
            {
                aggroed = false;
            }
        }
    }
}
