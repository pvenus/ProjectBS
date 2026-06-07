using System;
using System.Collections.Generic;
using UnityEngine;
using Character;

namespace Wave.SO
{
    [CreateAssetMenu(
        fileName = "StageWaveSO",
        menuName = "BS/Wave/StageWaveSO")]
    public class StageWaveSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string waveId;

        [Header("Stage Time")]
        [SerializeField] private float duration = 60f;

        [Header("Spawn Area")]
        [SerializeField] private float spawnStartX = 0f;
        [SerializeField] private float spawnEndX = 9f;
        [SerializeField] private float yMin = -3f;
        [SerializeField] private float yMax = 3f;
        [SerializeField] private float groupSpreadRadius = 0.75f;


        [Header("Phases")]
        [SerializeField] private List<SpawnPhase> phases = new();

        public string WaveId => waveId;
        public float Duration => Mathf.Max(0f, duration);
        public float SpawnStartX => spawnStartX;
        public float SpawnEndX => spawnEndX;
        public float YMin => yMin;
        public float YMax => yMax;
        public float GroupSpreadRadius => Mathf.Max(0f, groupSpreadRadius);
        public IReadOnlyList<SpawnPhase> Phases => phases;

        public Vector3 GetRandomSpawnPosition(float elapsedTime)
        {
            float progress = Duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsedTime / Duration);

            float currentSpawnX = Mathf.Lerp(
                spawnStartX,
                spawnEndX,
                progress);

            float y = UnityEngine.Random.Range(yMin, yMax);

            return new Vector3(
                currentSpawnX,
                y,
                0f);
        }

        public Vector3 GetRandomGroupOffset()
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * GroupSpreadRadius;
            return new Vector3(offset.x, offset.y, 0f);
        }
    }


    [Serializable]
    public class SpawnPhase
    {
        [Header("Time")]
        public string phaseId;
        public float startTime;
        public float endTime = 10f;

        [Header("Spawn Rule")]
        public float spawnInterval = 3f;
        public int spawnCountPerTick = 1;
        public int maxAliveCount = 6;

        [Header("Monster Pool")]
        public List<SpawnMonsterEntry> monsters = new();

        public bool IsActive(float elapsedTime)
        {
            return elapsedTime >= startTime
                && elapsedTime < endTime;
        }

        public float SpawnInterval => Mathf.Max(0.01f, spawnInterval);
        public int SpawnCountPerTick => Mathf.Max(1, spawnCountPerTick);
        public int MaxAliveCount => Mathf.Max(0, maxAliveCount);
    }

    [Serializable]
    public class SpawnMonsterEntry
    {
        public CharacterSO characterSo;
        public int weight = 1;

        public int Weight => Mathf.Max(0, weight);
    }
}