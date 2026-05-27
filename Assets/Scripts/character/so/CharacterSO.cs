

using System.Collections.Generic;
using Stat;
using UnityEngine;

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
        menuName = "Game/Character/Character")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterId;

        public string displayName;

        public CharacterType characterType = CharacterType.Npc;

        [Header("Prefab")]
        public GameObject prefab;

        [Header("Base Stats")]
        public List<StatEntry> baseStats = new();
    }
}