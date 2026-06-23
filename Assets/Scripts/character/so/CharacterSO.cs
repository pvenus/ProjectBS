using System.Collections.Generic;
using Stat;
using UnityEngine;
using String;

namespace Character
{
    public enum CharacterType
    {
        Player,
        Npc,
        Boss
    }

    [CreateAssetMenu(
        fileName = "Character",
        menuName = "Character/Character")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string characterId;
        [SerializeField] private CharacterType characterType = CharacterType.Npc;
        [SerializeField] private CharacterJob job;

        [Header("Prefab")]
        [SerializeField] private GameObject prefab;

        [Header("Animation Override")]
        [SerializeField] private AnimationClipSetSO animationOverrideSet;

        [Header("Skill Override")]
        [SerializeField] private SkillPoolOverrideSO skillOverrideSet;

        [Header("Base Stats")]
        [SerializeField] private List<StatEntry> baseStats = new();
        public string LocalizationMainKey => characterId;
        public string CharacterId => characterId;
        public CharacterType CharacterType => characterType;
        public CharacterJob Job => job;
        public GameObject Prefab => prefab;
        public AnimationClipSetSO AnimationOverrideSet => animationOverrideSet;
        public SkillPoolOverrideSO SkillOverrideSet => skillOverrideSet;
        public IReadOnlyList<StatEntry> BaseStats => baseStats;

        public string DisplayName =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "name");

        public CharacterJobFamily JobFamily =>
            CharacterJobHelper.GetFamily(job);

        public CharacterJobTier JobTier =>
            CharacterJobHelper.GetTier(job);

        public CharacterJobBranch JobBranch =>
            CharacterJobHelper.GetBranch(job);

        public bool HasPrefab => prefab != null;
        public bool HasAnimationOverrideSet => animationOverrideSet != null;
        public bool HasSkillOverrideSet => skillOverrideSet != null;

        public GameObject ResolvePrefab()
        {
            return prefab;
        }

        public AnimationClipSetSO ResolveAnimationOverrideSet()
        {
            return animationOverrideSet;
        }

        public SkillPoolOverrideSO ResolveSkillOverrideSet()
        {
            return skillOverrideSet;
        }

        public IReadOnlyList<StatEntry> ResolveBaseStats()
        {
            return baseStats;
        }

        public bool IsPlayer()
        {
            return characterType == CharacterType.Player;
        }

        public bool IsNpc()
        {
            return characterType == CharacterType.Npc;
        }

        public bool IsBoss()
        {
            return characterType == CharacterType.Boss;
        }
    }
}