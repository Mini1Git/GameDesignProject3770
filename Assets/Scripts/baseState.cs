using UnityEngine;

public abstract class baseState
{
    public baseState nextState;
    public bool isInState;
    public string stateName;

    public abstract void enterState();
    public abstract void exitState();
    public abstract void updateState();

}
