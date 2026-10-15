using UnityEngine;

public class attackAreaEnemy : MonoBehaviour
{
    private bool canAttack = false;
    public GameObject attackObject;
    private attackEnemy attack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attack = attackObject.GetComponent<attackEnemy>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(canAttack);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Something entered");
        if (collision.gameObject.tag == "Player")
        {
            attack.canAttack = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
           attack.canAttack = false;
        }
    }
}
