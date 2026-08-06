using System;
using Character;
using UnityEngine;

namespace Skill
{
    [CreateAssetMenu(
        fileName = "skill.spawn",
        menuName = "Skill/Spawn Skill")]
    public class SpawnSkillSO : ScriptableObject
    {
        [Header("Common")]
        [SerializeField] private int spawnCount = 1;
        [SerializeField] private float spawnInterval;
        [SerializeField] private float spawnLifeTime;

        [Header("Character Spawn")]
        [SerializeField] private SpawnCharacterProfile characterSpawn = new();

        public int SpawnCount => Mathf.Max(1, spawnCount);
        public float SpawnInterval => Mathf.Max(0f, spawnInterval);
        public float SpawnLifeTime => Mathf.Max(0f, spawnLifeTime);

        public CharacterSO CharacterSO => characterSpawn.CharacterSO;

#if UNITY_EDITOR
        public void ApplyEditorData(
            int spawnCount,
            float spawnInterval,
            float spawnLifeTime)
        {
            this.spawnCount = spawnCount;
            this.spawnInterval = spawnInterval;
            this.spawnLifeTime = spawnLifeTime;
        }

        public void ApplyEditorCharacterSpawn(
            CharacterSO characterSO)
        {
            characterSpawn.ApplyEditorData(characterSO);
        }
#endif
    }

    [Serializable]
    public class SpawnCharacterProfile
    {
        [SerializeField] private CharacterSO characterSO;

        public CharacterSO CharacterSO => characterSO;

#if UNITY_EDITOR
        public void ApplyEditorData(
            CharacterSO characterSO)
        {
            this.characterSO = characterSO;
        }
#endif
    }
}