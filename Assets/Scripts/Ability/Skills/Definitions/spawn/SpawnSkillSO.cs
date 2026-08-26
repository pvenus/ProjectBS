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
        [SerializeField] private SpawnSkillTiming timing;
        [SerializeField] private SpawnSkillPosition position = SpawnSkillPosition.ProjectilePosition;
        [SerializeField] private int spawnCount = 1;
        [SerializeField] private float spawnInterval;
        [SerializeField] private float spawnLifeTime;
        [SerializeField, Min(0f)] private float spawnRadius = 0.75f;

        [Header("Spawn Config")]
        [SerializeField] private SpawnConfig config = new();

        public SpawnSkillTiming Timing => timing;
        public SpawnSkillPosition Position => position;
        public int SpawnCount => Mathf.Max(1, spawnCount);
        public float SpawnInterval => Mathf.Max(0f, spawnInterval);
        public float SpawnLifeTime => Mathf.Max(0f, spawnLifeTime);
        public float SpawnRadius => spawnRadius > 0f ? spawnRadius : 0.75f;

        public SpawnConfig Config => config;
        public CharacterSO CharacterSO => config != null ? config.CharacterSO : null;
        public EquipmentSkillSO Skill => config != null ? config.Skill : null;

#if UNITY_EDITOR
        public void ApplyEditorData(
            SpawnSkillTiming timing,
            SpawnSkillPosition position,
            int spawnCount,
            float spawnInterval,
            float spawnLifeTime,
            float spawnRadius)
        {
            this.timing = timing;
            this.position = position;
            this.spawnCount = spawnCount;
            this.spawnInterval = spawnInterval;
            this.spawnLifeTime = spawnLifeTime;
            this.spawnRadius = spawnRadius;
        }

        public void ApplyEditorConfig(UnityEngine.Object spawnObject)
        {
            config ??= new SpawnConfig();
            config.ApplyEditorData(spawnObject);
        }
#endif
    }

    [Serializable]
    public class SpawnConfig
    {
        [SerializeField] private UnityEngine.Object spawnObject;

        public UnityEngine.Object SpawnObject => spawnObject;
        public CharacterSO CharacterSO => spawnObject as CharacterSO;
        public EquipmentSkillSO Skill => spawnObject as EquipmentSkillSO;

#if UNITY_EDITOR
        public void ApplyEditorData(UnityEngine.Object spawnObject)
        {
            this.spawnObject = spawnObject;
        }
#endif
    }
}
