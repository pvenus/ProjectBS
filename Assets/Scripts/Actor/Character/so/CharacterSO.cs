using System.Collections.Generic;
using Stat;
using UnityEngine;
using String;
using System;
using Skill;

namespace Character
{
    [Serializable]
    public class CharacterAnimationClipEntry
    {
        public CharacterAnimationClipType clipType;
        public AnimationClip clip;
    }

    [Serializable]
    public class CharacterSkillEntry
    {
        public string slotKey;
        public EquipmentSkillSO skillSo;
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

        [Header("Presentation")]
        [SerializeField] private Sprite portrait;
        [SerializeField, Min(0.01f)] private float scale = 0.2f;


        [Header("Animation Clips")]
        [SerializeField] private List<CharacterAnimationClipEntry> animationClips = new();

        [Header("Skills")]
        [SerializeField] private List<CharacterSkillEntry> skills = new();

        [Header("Base Stats")]
        [SerializeField] private List<StatEntry> baseStats = new();
        public string LocalizationMainKey => characterId;
        public string CharacterId => characterId;
        public CharacterType CharacterType => characterType;
        public CharacterJob Job => job;
        public Sprite Portrait => portrait;
        public float Scale => Mathf.Max(0.01f, scale);
        public IReadOnlyList<CharacterAnimationClipEntry> AnimationClips => animationClips;
        public IReadOnlyList<CharacterSkillEntry> Skills => skills;
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

        public bool HasAnimationClips => animationClips != null && animationClips.Count > 0;
        public bool HasSkills => skills != null && skills.Count > 0;

#if UNITY_EDITOR
        public void ApplyEditorData(
            string characterId,
            CharacterType characterType,
            CharacterJob job,
            List<CharacterAnimationClipEntry> animationClips,
            List<CharacterSkillEntry> skills,
            List<StatEntry> baseStats,
            float scale = 0.2f)
        {
            this.characterId = characterId;
            this.characterType = characterType;
            this.job = job;
            this.scale = Mathf.Max(0.01f, scale);
            this.animationClips = animationClips ?? new List<CharacterAnimationClipEntry>();
            this.skills = skills ?? new List<CharacterSkillEntry>();
            this.baseStats = baseStats ?? new List<StatEntry>();
        }
#endif
    }
}
