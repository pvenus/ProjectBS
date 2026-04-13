
[System.Serializable]
public class SkillProjectileHitDto
{
    public int maxHitCount = 1;
    public bool ignoreSameRoot = true;
    public bool useRepeatInterval;
    public float repeatInterval = 0.25f;

    public bool useHitWindow;
    public float hitStartTime;
    public float hitDuration = 0.1f;
    public bool deactivateAfterFirstHit;

    public int damage;
    public bool applyDamage = true;

    public bool useSplitMultiHitDamage;
    public int splitHitCount = 1;
    public float splitHitInterval;
}