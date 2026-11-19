using UnityEngine;

public class attackEnemy : MonoBehaviour
{
    public int damage = 1;
    public float cooldown = 0f;
    private float currCooldown = 0f;
    private Animator animator;
    private bool playerInside = false;
    private player playerScript;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (currCooldown > 0f)
        {
            currCooldown -= Time.deltaTime;
        }
    }

    public void doAttack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.IsInTransition(0) && currCooldown <= 0)
        {
            animator.SetTrigger("Swing");
            currCooldown = cooldown;

            //if player is already inside attack range, we deal damage right away
            if (playerInside)
            {
                playerScript.takeDmg(damage);
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //keep track if player is inside the attack range
            playerInside = true;
            playerScript = collision.GetComponent<player>();

            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                playerScript.takeDmg(damage);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            playerScript = null;
        }
    }
}