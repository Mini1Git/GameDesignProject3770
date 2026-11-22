using UnityEngine;

public abstract class BaseDebuff : MonoBehaviour
{

    protected abstract void applyDebuff();

    public abstract void resetTimer(); // for if it gets hit again.
}
