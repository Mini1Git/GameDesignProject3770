using UnityEngine;

public class SpellCollision : MonoBehaviour
{ // this script is NEEDED on every spell provided that there's some sort of impact behaviour.
    private SpellBehaviourManager manager;

    void Awake()
    {
        manager = GetComponent<SpellBehaviourManager>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        manager.onImpact(collision);
    }
}