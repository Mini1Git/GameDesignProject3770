#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

// an editor test double; no attack pattern is implemented here
public class CombatTestExecutor : MonoBehaviour, IMeleeAttackExecutor
{
    public readonly List<MeleeAttackRequest> requests = new List<MeleeAttackRequest>();
    public bool accept = true;
    public bool completeImmediately;
    public int cancelCount;

    public bool TryStartAttack(MeleeAttackRequest request, PlayerCombat combat)
    {
        requests.Add(request);
        if (completeImmediately) combat.NotifyAttackFinished(request.AttackId);
        return accept;
    }

    public void CancelAttack(int attackId)
    {
        cancelCount++;
    }
}
#endif
