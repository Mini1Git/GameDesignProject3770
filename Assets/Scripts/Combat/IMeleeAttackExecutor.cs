public interface IMeleeAttackExecutor
{
    // return false if execution cannot start; do not spend stamina here
    bool TryStartAttack(MeleeAttackRequest request, PlayerCombat combat);

    // stop hit detection and animation without reporting further hits
    void CancelAttack(int attackId);
}
