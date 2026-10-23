using UnityEngine;

public class Projectile_Movement : SpellBehaviour
{
    public float speed = 10;
    public override void UpdateLogic()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }
}
