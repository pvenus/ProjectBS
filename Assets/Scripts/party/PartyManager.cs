using System.Collections.Generic;
using Session;
using Character;
using Effect;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Party
{
    public class PartyManager : MonoBehaviour
    {
        public static PartyManager Instance { get; private set; }
        [Header("Spawn")]
        [SerializeField] private Transform spawnRoot;

        [SerializeField] private float spacing = 1.5f;
        [SerializeField] private float battleSpawnX = -8f;
        [SerializeField] private float battleSpawnYRange = 2f;

        [Header("Battle Zone Anchor")]
        [SerializeField] private LayerMask battleZoneObstacleMask;
        [SerializeField] private float battleZoneDistanceFromZone = 2.0f;
        [SerializeField] private float battleZoneAnchorRadius = 1.25f;
        [SerializeField] private float battleZoneAnchorSearchStep = 0.75f;
        [SerializeField] private int battleZoneAnchorSearchRingCount = 3;
        private const float BattleZoneRefreshInterval = 2.0f;

        private readonly List<GameObject> spawnedMembers = new();

        private readonly List<PartyMovementMono> movementMembers = new();

        private PartyAnchorService partyAnchorService;

        private BattleZoneService battleZoneService;
        private BattleZoneService.BattleZoneData cachedBattleZone;
        private float lastBattleZoneRefreshTime = -999f;

        private const string BattleSceneName = "BattleScene";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            partyAnchorService = new PartyAnchorService();
            battleZoneService = new BattleZoneService();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            SpawnParty();
        }

        public void SpawnParty()
        {
            bool isBattleScene =
                SceneManager.GetActiveScene().name == BattleSceneName;

            if (isBattleScene)
            {
                ClearSpawnedMembers();
            }

            GameSession gameSession =
                GameSession.Instance;

            if (gameSession == null)
            {
                Debug.LogError(
                    "[PartyManager] GameSession not found.");

                return;
            }

            BattleSession battleSession =
                gameSession.BattleSession;

            if (battleSession == null)
            {
                Debug.LogError(
                    "[PartyManager] BattleSession not found.");

                return;
            }

            PartyRuntimeData runtimeData =
                battleSession.PartyRuntimeData;

            if (runtimeData == null)
            {
                Debug.LogError(
                    "[PartyManager] PartyRuntimeData is null.");

                return;
            }

            if (!isBattleScene)
            {
                InitializeExistingCharacters(runtimeData);
                return;
            }

            for (int i = 0;
                 i < runtimeData.Members.Count;
                 i++)
            {
                CharacterRuntimeData characterRuntime =
                    runtimeData.Members[i];

                if (characterRuntime == null
                    || characterRuntime.characterSO == null)
                {
                    continue;
                }

                GameObject characterPrefab =
                    characterRuntime.characterSO.ResolvePrefab();

                if (characterPrefab == null)
                {
                    continue;
                }

                Vector3 spawnPosition =
                    new Vector3(
                        battleSpawnX + Random.Range(-0.5f, 0.5f),
                        Random.Range(-battleSpawnYRange, battleSpawnYRange),
                        0f);

                GameObject spawnedObject =
                    Instantiate(
                        characterPrefab,
                        spawnPosition,
                        Quaternion.identity,
                        spawnRoot);

                spawnedMembers.Add(spawnedObject);

                RegisterMovementMember(spawnedObject);

                CharacterManager characterManager =
                    spawnedObject.GetComponent<CharacterManager>();

                if (characterManager == null)
                {
                    Debug.LogError(
                        "[PartyManager] CharacterManager not found.");

                    continue;
                }

                bool hasRuntimeStats =
                    characterRuntime.stats != null
                    && characterRuntime.stats.Count > 0;

                if (hasRuntimeStats)
                {
                    characterManager.Initialize(characterRuntime);
                }
                else
                {
                    characterManager.InitializeFromSO(
                        characterRuntime.characterSO);

                    runtimeData.Members[i] =
                        characterManager.RuntimeData;
                }
            }
        }

        private void InitializeExistingCharacters(
            PartyRuntimeData runtimeData)
        {
            for (int i = 0;
                 i < runtimeData.Members.Count;
                 i++)
            {
                CharacterRuntimeData characterRuntime =
                    runtimeData.Members[i];

                if (characterRuntime == null
                    || characterRuntime.characterSO == null)
                {
                    continue;
                }

                GameObject runtimeObject =
                    new($"CharacterRuntime_{characterRuntime.characterSO.name}");

                runtimeObject.transform.SetParent(
                    transform,
                    false);

                CharacterManager characterManager =
                    runtimeObject.AddComponent<CharacterManager>();

                runtimeObject.AddComponent<EffectManager>();

                bool hasRuntimeStats =
                    characterRuntime.stats != null
                    && characterRuntime.stats.Count > 0;

                if (hasRuntimeStats)
                {
                    characterManager.Initialize(characterRuntime);
                }
                else
                {
                    characterManager.InitializeFromSO(
                        characterRuntime.characterSO);

                    runtimeData.Members[i] =
                        characterManager.RuntimeData;
                }
            }
        }

        public BattleZoneService.BattleZoneData GetCurrentBattleZone(
            IReadOnlyList<Transform> enemies)
        {
            if (battleZoneService == null)
            {
                battleZoneService = new BattleZoneService();
            }

            bool shouldRefresh = cachedBattleZone == null
                || Time.time - lastBattleZoneRefreshTime >= BattleZoneRefreshInterval;

            if (!shouldRefresh)
            {
                return cachedBattleZone;
            }

            CleanupMovementMembers();

            List<Transform> partyTransforms = new();

            for (int i = 0; i < movementMembers.Count; i++)
            {
                if (movementMembers[i] != null)
                {
                    partyTransforms.Add(movementMembers[i].transform);
                }
            }

            List<BattleZoneService.BattleZoneData> zones =
                battleZoneService.BuildZones(enemies);

            cachedBattleZone = battleZoneService.SelectBestZone(
                zones,
                partyTransforms);

            lastBattleZoneRefreshTime = Time.time;

            return cachedBattleZone;
        }

        public Vector2 ResolveSafeBattleAnchorPosition(
            BattleZoneService.BattleZoneData zone,
            Vector2 partyCenter)
        {
            if (battleZoneService == null)
            {
                battleZoneService = new BattleZoneService();
            }

            return battleZoneService.ResolveSafeAnchorPosition(
                zone,
                partyCenter,
                battleZoneObstacleMask,
                battleZoneDistanceFromZone,
                battleZoneAnchorRadius,
                battleZoneAnchorSearchStep,
                battleZoneAnchorSearchRingCount);
        }

        public PartyAnchorService.PartyAnchorData GetPartyAnchorData()
        {
            CleanupMovementMembers();

            if (partyAnchorService == null)
            {
                partyAnchorService = new PartyAnchorService();
            }

            return partyAnchorService.BuildAnchor(movementMembers);
        }

        public Vector2 GetPartyAnchorPosition()
        {
            return GetPartyAnchorData().AnchorPosition;
        }

        public IReadOnlyList<PartyMovementMono> GetMovementMembers()
        {
            CleanupMovementMembers();
            return movementMembers;
        }

        private void RegisterMovementMember(GameObject spawnedObject)
        {
            if (spawnedObject == null)
            {
                return;
            }

            PartyMovementMono movementMono =
                spawnedObject.GetComponent<PartyMovementMono>();

            if (movementMono == null)
            {
                movementMono =
                    spawnedObject.GetComponentInChildren<PartyMovementMono>();
            }

            if (movementMono == null)
            {
                return;
            }

            if (!movementMembers.Contains(movementMono))
            {
                movementMembers.Add(movementMono);
            }
        }

        private void CleanupMovementMembers()
        {
            for (int i = movementMembers.Count - 1;
                 i >= 0;
                 i--)
            {
                if (movementMembers[i] == null)
                {
                    movementMembers.RemoveAt(i);
                }
            }
        }

        private void ClearSpawnedMembers()
        {
            for (int i = 0;
                 i < spawnedMembers.Count;
                 i++)
            {
                if (spawnedMembers[i] != null)
                {
                    Destroy(spawnedMembers[i]);
                }
            }

            spawnedMembers.Clear();
            movementMembers.Clear();
            cachedBattleZone = null;
            lastBattleZoneRefreshTime = -999f;
        }
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            CleanupMovementMembers();

            List<Transform> enemies = new();

            for (int i = 0; i < movementMembers.Count; i++)
            {
                PartyMovementMono member = movementMembers[i];

                if (member == null)
                {
                    continue;
                }

                PerceptionMono perception =
                    member.GetComponent<PerceptionMono>();

                if (perception == null)
                {
                    continue;
                }

                Transform enemy = perception.ClosestEnemy;

                if (enemy != null && !enemies.Contains(enemy))
                {
                    enemies.Add(enemy);
                }
            }

            BattleZoneService.BattleZoneData zone =
                GetCurrentBattleZone(enemies);

            if (zone == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(zone.Center, 0.25f);
            Gizmos.DrawWireSphere(zone.Center, 2f);

            Gizmos.color = Color.yellow;

            for (int i = 0; i < zone.Enemies.Count; i++)
            {
                Transform enemy = zone.Enemies[i];

                if (enemy == null)
                {
                    continue;
                }

                Gizmos.DrawLine(zone.Center, enemy.position);
                Gizmos.DrawWireSphere(enemy.position, 0.15f);
            }

            PartyAnchorService.PartyAnchorData anchorData =
                GetPartyAnchorData();

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(anchorData.AnchorPosition, 0.2f);
            Gizmos.DrawLine(
                anchorData.AnchorPosition,
                zone.Center);
            Gizmos.DrawWireSphere(
                anchorData.AnchorPosition,
                battleZoneAnchorRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(
                anchorData.PartyCenterPosition,
                0.2f);
        }
    }
}
