using UnityEngine;

public abstract class SpellBehaviour : MonoBehaviour
{
    public virtual void StartLogic() { 
        //initialize.
    }
    public virtual void UpdateLogic() {
        //for example, movement of spell.
    }
    public virtual void onImpactLogic(Collision2D collision){
        //when a spell hits smt
    }
}