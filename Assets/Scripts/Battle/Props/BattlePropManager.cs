using System;
using System.Collections.Generic;
using Battle.Prop.SO;
using Session;
using UnityEngine;

namespace Battle.Prop
{
    /// <summary>
    /// Battle prop runtime owner.
    /// A prop is a battlefield object such as a grave, altar, gate, core, or generator.
    /// It is not a character, but can be spawned, tracked, and later allowed to use skills/effects.
    /// </summary>
    public class BattlePropManager : MonoBehaviour
    {
        public static BattlePropManager Instance { get; private set; }

        private readonly List<BattlePropController> props = new();
        private readonly HashSet<int> spawnedTimedPropIndexes = new();

        private BattleSession battleSession;
        private BattleSO battleSO;
        private float elapsedTime;
        private bool initialized;

        public IReadOnlyList<BattlePropController> Props => props;

        public event Action<BattlePropController> OnPropRegistered;
        public event Action<BattlePropController> OnPropUnregistered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            InitializeFromSession();
        }

        private void Update()
        {
            if (!initialized || battleSO == null)
            {
                return;
            }

            elapsedTime += Time.deltaTime;
            TrySpawnTimedProps();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeFromSession()
        {
            GameSession gameSession = GameSession.Instance;

            if (gameSession == null)
            {
                Debug.LogError("[BattlePropManager] GameSession not found.");
                return;
            }

            battleSession = gameSession.BattleSession;

            if (battleSession == null)
            {
                Debug.LogError("[BattlePropManager] BattleSession not found.");
                return;
            }

            battleSO = battleSession.BattleSO;

            if (battleSO == null)
            {
                Debug.LogError("[BattlePropManager] BattleSO not found.");
                return;
            }

            elapsedTime = 0f;
            spawnedTimedPropIndexes.Clear();
            initialized = true;

            TrySpawnTimedProps();
        }

        private void TrySpawnTimedProps()
        {
            IReadOnlyList<BattleSO.TimedPropPlacement> placements =
                battleSO.TimedPropPlacements;

            if (placements == null || placements.Count == 0)
            {
                return;
            }

            for (int i = 0; i < placements.Count; i++)
            {
                if (spawnedTimedPropIndexes.Contains(i))
                {
                    continue;
                }

                BattleSO.TimedPropPlacement placement = placements[i];

                if (placement == null || placement.prop == null)
                {
                    spawnedTimedPropIndexes.Add(i);
                    continue;
                }

                if (elapsedTime < placement.spawnTimeSeconds)
                {
                    continue;
                }

                SpawnProp(placement);
                spawnedTimedPropIndexes.Add(i);
            }
        }

        public BattlePropController SpawnProp(
            BattlePropSO propSO,
            Vector3 position,
            Quaternion rotation,
            string runtimeId = null)
        {
            if (propSO == null)
            {
                return null;
            }

            if (propSO.prefab == null)
            {
                Debug.LogWarning(
                    $"[BattlePropManager] Prop prefab is null. propId={propSO.propId}");
                return null;
            }

            GameObject instance = Instantiate(
                propSO.prefab,
                position,
                rotation,
                transform);

            BattlePropController controller =
                instance.GetComponent<BattlePropController>();

            if (controller == null)
            {
                controller = instance.AddComponent<BattlePropController>();
            }

            string resolvedRuntimeId = string.IsNullOrEmpty(runtimeId)
                ? propSO.propId
                : runtimeId;

            controller.Initialize(
                resolvedRuntimeId,
                propSO);

            return controller;
        }

        private BattlePropController SpawnProp(BattleSO.TimedPropPlacement placement)
        {
            if (placement == null || placement.prop == null)
            {
                return null;
            }

            string runtimeId = string.IsNullOrEmpty(placement.runtimeId)
                ? placement.prop.propId
                : placement.runtimeId;

            return SpawnProp(
                placement.prop,
                placement.position,
                placement.rotation,
                runtimeId);
        }

        public void Register(BattlePropController prop)
        {
            if (prop == null || props.Contains(prop))
            {
                return;
            }

            props.Add(prop);
            OnPropRegistered?.Invoke(prop);
        }

        public void Unregister(BattlePropController prop)
        {
            if (prop == null)
            {
                return;
            }

            if (!props.Remove(prop))
            {
                return;
            }

            OnPropUnregistered?.Invoke(prop);
        }

        public void Clear()
        {
            for (int i = props.Count - 1; i >= 0; i--)
            {
                BattlePropController prop = props[i];

                if (prop != null)
                {
                    Destroy(prop.gameObject);
                }
            }

            props.Clear();
            spawnedTimedPropIndexes.Clear();
        }

        public BattlePropController FindByRuntimeId(string runtimeId)
        {
            if (string.IsNullOrEmpty(runtimeId))
            {
                return null;
            }

            for (int i = 0; i < props.Count; i++)
            {
                BattlePropController prop = props[i];

                if (prop != null && prop.RuntimeId == runtimeId)
                {
                    return prop;
                }
            }

            return null;
        }

        public List<BattlePropController> FindByRole(BattlePropRole role)
        {
            List<BattlePropController> result = new();

            for (int i = 0; i < props.Count; i++)
            {
                BattlePropController prop = props[i];

                if (prop != null && prop.Role == role)
                {
                    result.Add(prop);
                }
            }

            return result;
        }
    }
}