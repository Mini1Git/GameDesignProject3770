using UnityEngine;

public class attackEnemy : MonoBehaviour
{
    public bool canAttack = false;

    void FixedUpdate()
    {
        
        if (canAttack)
        {
            GetComponent<Animator>().SetTrigger("Swing");
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Hit player");
        }
    }
}
