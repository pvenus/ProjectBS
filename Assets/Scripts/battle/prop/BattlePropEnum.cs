

namespace Battle.Prop
{
    public enum BattlePropRole
    {
        None = 0,

        Grave = 100,
        Altar = 200,
        Seal = 300,

        Gate = 400,
        Core = 500,
        Generator = 600,

        EscortTarget = 700,
        DefenseTarget = 800,
        SpawnPoint = 900
    }

    public enum BattlePropState
    {
        None = 0,

        Normal = 100,
        Activated = 200,
        Casting = 300,

        Contested = 400,
        Damaged = 500,
        Corrupted = 600,

        Destroyed = 700,
        Cleared = 800
    }
}