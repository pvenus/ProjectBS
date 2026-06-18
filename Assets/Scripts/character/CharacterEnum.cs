using System;

namespace Character
{
    [Flags]
    public enum CharacterJob
    {
        None = 0,

        TierMask = 0x000F,
        Base = 1 << 0,
        First = 1 << 1,
        Second = 1 << 2,

        FamilyMask = 0x0FF0,
        Soldier = 1 << 4,
        Archer = 1 << 5,
        Scholar = 1 << 6,
        Physician = 1 << 7,
        Monk = 1 << 8,

        BranchMask = 0xF000,
        MainBranch = 1 << 12,
        AltBranch = 1 << 13,

        SoldierBase = Soldier | Base,
        SoldierFirst = Soldier | First | MainBranch,
        SoldierSecond = Soldier | Second | MainBranch,
        SoldierAltFirst = Soldier | First | AltBranch,
        SoldierAltSecond = Soldier | Second | AltBranch,

        ArcherBase = Archer | Base,
        ArcherFirst = Archer | First | MainBranch,
        ArcherSecond = Archer | Second | MainBranch,
        ArcherAltFirst = Archer | First | AltBranch,
        ArcherAltSecond = Archer | Second | AltBranch,

        ScholarBase = Scholar | Base,
        ScholarFirst = Scholar | First | MainBranch,
        ScholarSecond = Scholar | Second | MainBranch,
        ScholarAltFirst = Scholar | First | AltBranch,
        ScholarAltSecond = Scholar | Second | AltBranch,

        PhysicianBase = Physician | Base,
        PhysicianFirst = Physician | First | MainBranch,
        PhysicianSecond = Physician | Second | MainBranch,
        PhysicianAltFirst = Physician | First | AltBranch,
        PhysicianAltSecond = Physician | Second | AltBranch,

        MonkBase = Monk | Base,
        MonkFirst = Monk | First | MainBranch,
        MonkSecond = Monk | Second | MainBranch,
        MonkAltFirst = Monk | First | AltBranch,
        MonkAltSecond = Monk | Second | AltBranch,
    }

    public enum CharacterJobFamily
    {
        None,
        Soldier,
        Archer,
        Scholar,
        Physician,
        Monk
    }

    public enum CharacterJobTier
    {
        None,
        Base,
        First,
        Second
    }

    public enum CharacterJobBranch
    {
        None,
        Main,
        Alt
    }
}