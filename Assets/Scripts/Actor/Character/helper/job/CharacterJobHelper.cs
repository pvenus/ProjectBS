namespace Character
{
    public static class CharacterJobHelper
    {
        public static CharacterJobFamily GetFamily(CharacterJob job)
        {
            CharacterJob family = job & CharacterJob.FamilyMask;

            return family switch
            {
                CharacterJob.Soldier => CharacterJobFamily.Soldier,
                CharacterJob.Archer => CharacterJobFamily.Archer,
                CharacterJob.Scholar => CharacterJobFamily.Scholar,
                CharacterJob.Physician => CharacterJobFamily.Physician,
                CharacterJob.Monk => CharacterJobFamily.Monk,
                _ => CharacterJobFamily.None
            };
        }

        public static CharacterJobTier GetTier(CharacterJob job)
        {
            CharacterJob tier = job & CharacterJob.TierMask;

            return tier switch
            {
                CharacterJob.Base => CharacterJobTier.Base,
                CharacterJob.First => CharacterJobTier.First,
                CharacterJob.Second => CharacterJobTier.Second,
                _ => CharacterJobTier.None
            };
        }

        public static CharacterJobBranch GetBranch(CharacterJob job)
        {
            CharacterJob branch = job & CharacterJob.BranchMask;

            return branch switch
            {
                CharacterJob.MainBranch => CharacterJobBranch.Main,
                CharacterJob.AltBranch => CharacterJobBranch.Alt,
                _ => CharacterJobBranch.None
            };
        }

        public static bool IsFamily(CharacterJob job, CharacterJobFamily family)
        {
            return GetFamily(job) == family;
        }

        public static bool IsBaseJob(CharacterJob job)
        {
            return GetTier(job) == CharacterJobTier.Base;
        }

        public static bool IsFirstJob(CharacterJob job)
        {
            return GetTier(job) == CharacterJobTier.First;
        }

        public static bool IsSecondJob(CharacterJob job)
        {
            return GetTier(job) == CharacterJobTier.Second;
        }

        public static bool CanFirstJobChange(CharacterJob job)
        {
            return GetFamily(job) != CharacterJobFamily.None
                   && GetTier(job) == CharacterJobTier.Base;
        }

        public static bool CanSecondJobChange(CharacterJob job)
        {
            return GetFamily(job) != CharacterJobFamily.None
                   && GetTier(job) == CharacterJobTier.First;
        }

        public static CharacterJob ToBaseJob(CharacterJob job)
        {
            CharacterJob family = job & CharacterJob.FamilyMask;
            return family == CharacterJob.None
                ? CharacterJob.None
                : family | CharacterJob.Base;
        }

        public static CharacterJob ToFirstJob(
            CharacterJob job,
            CharacterJobBranch branch = CharacterJobBranch.Main)
        {
            CharacterJob family = job & CharacterJob.FamilyMask;
            if (family == CharacterJob.None)
            {
                return CharacterJob.None;
            }

            CharacterJob branchFlag = branch == CharacterJobBranch.Alt
                ? CharacterJob.AltBranch
                : CharacterJob.MainBranch;

            return family | CharacterJob.First | branchFlag;
        }

        public static CharacterJob ToSecondJob(
            CharacterJob job,
            CharacterJobBranch branch = CharacterJobBranch.Main)
        {
            CharacterJob family = job & CharacterJob.FamilyMask;
            if (family == CharacterJob.None)
            {
                return CharacterJob.None;
            }

            CharacterJob branchFlag = branch == CharacterJobBranch.Alt
                ? CharacterJob.AltBranch
                : CharacterJob.MainBranch;

            return family | CharacterJob.Second | branchFlag;
        }
    }
}
