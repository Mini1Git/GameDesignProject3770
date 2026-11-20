using UnityEngine;

// Melee is standard enemy role, moderate speed uses navmesh ai pathfinding towards player so it doesn't get stuck

public class enemyMelee : enemyBase
{

    private UnityEngine.AI.NavMeshAgent navAgent;
    [SerializeField] private float facingAngleOffset;

    void Awake()
    {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
    }

    void FixedUpdate()
    {
        if (!aggroed || player == null || !navAgent.isOnNavMesh) return;
        Vector2 toPlayer = player.transform.position - transform.position;
        if (toPlayer.magnitude > aggroRange)
        {
            navAgent.ResetPath();
            aggroed = false;
            return;
        }
        bool inReach = toPlayer.magnitude <= navAgent.stoppingDistance;
        navAgent.isStopped = inReach;
        if (!inReach) navAgent.destination = player.transform.position;
        Vector2 direction = inReach ? toPlayer : (Vector2)navAgent.velocity;
        if (direction.sqrMagnitude < 0.0001f) direction = toPlayer;
        if (direction.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + facingAngleOffset;
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.Euler(0, 0, angle), 180f * Time.fixedDeltaTime * turnSpeed);
    }
}
