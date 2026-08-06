/// <summary>
/// Summarized combat input for SkillBrainMono.
/// This is the current brain input contract and should contain
/// only perception / context data, not decision or execution data.
/// </summary>
[System.Serializable]
public struct BrainContext
{
    // Self
    public Role role;
    // Tactical
    public StateMono.PartyState partyState;
    // Local combat summary
    public float selfHp01;
    public int nearbyEnemyCount;
    public int nearbyAllyCount;
    public float lowestAllyHp01;
    public bool hasHealTarget;
}