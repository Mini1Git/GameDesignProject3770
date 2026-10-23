using UnityEngine;

public class Explode_on_Hit_Behaviour : SpellBehaviour
{
    public override void onImpactLogic(Collision2D collision)
    {
        if (collision.collider.GetComponent<enemyBase>())
        {
            Debug.Log("HIT " + collision.collider.gameObject.name);
            Destroy(gameObject);
        }
        
    }
}
