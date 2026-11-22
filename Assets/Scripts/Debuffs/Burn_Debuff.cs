using UnityEngine;

public class Burn_Debuff : BaseDebuff
{
    protected override void applyDebuff()
    {
        Debug.Log("Burns!");
    }

    public override void resetTimer()
    {
        Debug.Log("RESET TIMER");
    }
}
