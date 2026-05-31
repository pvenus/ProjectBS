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
        public string characterId;


        public string DisplayName =>
            StringManager.Instance.Get(
                characterId,
                "name");

        public CharacterType characterType = CharacterType.Npc;

        [Header("Prefab")]
        public GameObject prefab;

        [Header("Animation Override")]
        public AnimationClipSetSO animationOverrideSet;

        [Header("Skill Override")]
        public SkillPoolOverrideSO skillOverrideSet;

        [Header("Base Stats")]
        public List<StatEntry> baseStats = new();
    }
}