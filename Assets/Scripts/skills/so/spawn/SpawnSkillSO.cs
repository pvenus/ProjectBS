using Character;
using UnityEngine;

namespace Skill
{
    [CreateAssetMenu(
        fileName = "skill.spawn",
        menuName = "Skill/Spawn Skill")]
    public class SpawnSkillSO : ScriptableObject
    {
        [SerializeField] private SpawnSkillTiming timing;
        [SerializeField] private SpawnSkillPosition position;
        [SerializeField] private int spawnCount = 1;
        [SerializeField] private float spawnInterval;
        [SerializeField] private float duration;
        [SerializeField] private CharacterSO characterSO;
        [SerializeField] private EquipmentSkillSO skill;

        public SpawnSkillTiming Timing => timing;
        public SpawnSkillPosition Position => position;
        public int SpawnCount => spawnCount;
        public float SpawnInterval => spawnInterval;
        public float Duration => duration;
        public CharacterSO CharacterSO => characterSO;
        public EquipmentSkillSO Skill => skill;
    }
}