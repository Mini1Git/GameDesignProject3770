using UnityEngine;


public class SpellBehaviourManager : MonoBehaviour
{
    private SpellBehaviour[] behaviours; //making this private since only the manager needs to know about the behaviours.


    void Awake()
    {
        behaviours = GetComponents<SpellBehaviour>();
    }
    public void Start()
    {
        foreach (SpellBehaviour behaviour in behaviours)
        {
            behaviour.StartLogic();
        }
    }

    public void Update()
    {
        foreach (SpellBehaviour behaviour in behaviours)
        {
            behaviour.UpdateLogic(); 
        }
    }

    public void onImpact(Collision2D collision) { // this function handles what happens if a spell collides with smt
        foreach (SpellBehaviour behaviour in behaviours)
        {
            behaviour.onImpactLogic(collision);
        }
    }
}
