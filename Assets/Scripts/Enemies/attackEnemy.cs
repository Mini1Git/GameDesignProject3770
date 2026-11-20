using UnityEngine;

public class attackEnemy : MonoBehaviour
{
    public int damage = 1;
    public float cooldown = 0f;
    [SerializeField] private Animator armMotion;
    [SerializeField, Min(0.1f)] private float swingSpeed = 1f;
    private float currCooldown = 0f;
    private Animator animator;
    private Quaternion inverseRestRotation;
    private Collider2D strikeCollider;
    private bool hitThisSwing;
    private bool playerInside = false;
    private player playerScript;

    void Awake()
    {
        animator = armMotion != null ? armMotion : GetComponent<Animator>();
        if (armMotion == null) return;
        armMotion.speed = swingSpeed;
        inverseRestRotation = Quaternion.Inverse(armMotion.transform.localRotation);
        strikeCollider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (armMotion == null) return;
        transform.localRotation = armMotion.transform.localRotation * inverseRestRotation;
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        strikeCollider.enabled = !hitThisSwing && state.IsName("Swing") &&
            state.normalizedTime >= 0.4f && state.normalizedTime <= 0.7f;
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
            hitThisSwing = false;
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
        if (armMotion != null)
        {
            if (!strikeCollider.enabled || hitThisSwing) return;
            player target = collision.GetComponentInParent<player>();
            if (target == null) return;
            hitThisSwing = true;
            target.takeDmg(damage);
            return;
        }
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
