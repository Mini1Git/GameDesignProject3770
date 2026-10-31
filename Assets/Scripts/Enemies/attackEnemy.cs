using UnityEngine;

public class attackEnemy : MonoBehaviour
{
    public int damage = 1;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }


    public void doAttack()
    {
        //if not already attacking, attack
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.IsInTransition(0))
        {
            animator.SetTrigger("Swing");
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.GetComponent<player>().takeDmg(damage);
        }
    }
}
