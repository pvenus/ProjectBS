using System.ComponentModel;

public enum Role
{
    DPS,
    Tank,
    Support
}
public enum SkillOutputTargetMode
{
    None,
    Self,
    Target,
    Point
}


public enum BrainPhase
{
    Idle,
    Observe,
    Decide,
    Act
}

public enum TacticalNeed
{
    None,
    SelfDefense,
    AreaControl,
    AllySupport,
    OffensivePressure
}

public enum SurvivalState
{
    Safe,
    Pressured,
    Critical
}

public enum AllySupportState
{
    Stable,
    Wounded,
    Emergency
}

public enum FieldControlState
{
    Sparse,
    Contested,
    Overrun
}