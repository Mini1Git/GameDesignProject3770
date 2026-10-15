using UnityEngine;

// Melee is standard enemy role, moderate speed uses navmesh ai pathfinding towards player so it doesn't get stuck

public class enemyMelee : enemyBase
{

    private UnityEngine.AI.NavMeshAgent navAgent;
    public Collider2D attackArea;

    void Awake()
    {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // makes enemy face where its going
        Vector2 direction = navAgent.velocity;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            180f * Time.deltaTime * turnSpeed
        );

        if (aggroed)
        {
            navAgent.destination = player.transform.position;

            if (Vector2.Distance(player.transform.position, transform.position) > aggroRange)
            {
                aggroed = false;
            }
        }
    }
}
