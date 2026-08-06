using System.Collections.Generic;
using Session;
using Character;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Character.Helper;

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

        public IReadOnlyList<CharacterManager> Members =>
            spawnedMembers
                .Select(x => x != null
                    ? x.GetComponent<CharacterManager>()
                    : null)
                .Where(x => x != null)
                .ToList();
        private readonly List<GameObject> runtimeMemberObjects = new();

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

                // Removed prefab support; pass null as prefab argument.

                Vector3 spawnPosition =
                    new Vector3(
                        battleSpawnX + Random.Range(-0.5f, 0.5f),
                        Random.Range(-battleSpawnYRange, battleSpawnYRange),
                        0f);

                GameObject spawnedObject =
                    CharacterBuilder.CreateOrBuildPlayerObject(
                        null,
                        characterRuntime.characterSO.name,
                        spawnRoot,
                        spawnPosition,
                        Quaternion.identity,
                        null,
                        true);

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
            ClearRuntimeMemberObjects();
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
                    CharacterBuilder.CreateOrBuildPlayerObject(
                        null,
                        $"CharacterRuntime_{characterRuntime.characterSO.name}",
                        transform,
                        Vector3.zero,
                        Quaternion.identity,
                        null,
                        true);

                runtimeMemberObjects.Add(runtimeObject);

                CharacterManager characterManager =
                    runtimeObject.GetComponent<CharacterManager>();

                if (characterManager == null)
                {
                    Debug.LogError(
                        "[PartyManager] CharacterManager not found on runtime object.");

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

        public bool TryChangePartyMemberJob(
            CharacterJob fromJob,
            CharacterJob toJob)
        {
            PartyRuntimeData runtimeData = GetPartyRuntimeData();
            if (runtimeData == null)
            {
                return false;
            }

            CharacterSO targetCharacterSO = FindPlayerCharacterSOByJob(toJob);
            if (targetCharacterSO == null)
            {
                Debug.LogWarning(
                    $"[PartyManager] Target job CharacterSO not found. toJob={toJob}");

                return false;
            }

            for (int i = 0; i < runtimeData.Members.Count; i++)
            {
                CharacterRuntimeData currentRuntime = runtimeData.Members[i];
                if (currentRuntime?.characterSO == null)
                {
                    continue;
                }

                if (currentRuntime.characterSO.CharacterType != CharacterType.Player)
                {
                    continue;
                }

                if (currentRuntime.characterSO.Job != fromJob)
                {
                    continue;
                }

                DestroyRuntimeObject(currentRuntime);

                CharacterManager newCharacterManager = CreateRuntimeCharacterManager(targetCharacterSO);
                if (newCharacterManager == null)
                {
                    return false;
                }

                runtimeData.Members[i] = newCharacterManager.RuntimeData;

                Debug.Log(
                    $"[PartyManager] Party member job changed. from={fromJob}, to={toJob}");

                return true;
            }

            Debug.LogWarning(
                $"[PartyManager] Source job party member not found. fromJob={fromJob}");

            return false;
        }

        public bool TryAddPartyMember(CharacterSO characterSO)
        {
            if (characterSO == null)
            {
                return false;
            }

            PartyRuntimeData runtimeData = GetPartyRuntimeData();
            if (runtimeData == null)
            {
                return false;
            }

            CharacterManager characterManager = CreateRuntimeCharacterManager(characterSO);
            if (characterManager == null)
            {
                return false;
            }

            runtimeData.Members.Add(characterManager.RuntimeData);
            return true;
        }

        public bool TryRemovePartyMember(CharacterJob job)
        {
            PartyRuntimeData runtimeData = GetPartyRuntimeData();
            if (runtimeData == null)
            {
                return false;
            }

            for (int i = runtimeData.Members.Count - 1; i >= 0; i--)
            {
                CharacterRuntimeData currentRuntime = runtimeData.Members[i];
                if (currentRuntime?.characterSO == null)
                {
                    continue;
                }

                if (currentRuntime.characterSO.Job != job)
                {
                    continue;
                }

                DestroyRuntimeObject(currentRuntime);
                runtimeData.Members.RemoveAt(i);
                return true;
            }

            return false;
        }

        private PartyRuntimeData GetPartyRuntimeData()
        {
            GameSession gameSession = GameSession.Instance;
            if (gameSession == null || gameSession.BattleSession == null)
            {
                Debug.LogWarning("[PartyManager] GameSession or BattleSession is null.");
                return null;
            }

            PartyRuntimeData runtimeData = gameSession.BattleSession.PartyRuntimeData;
            if (runtimeData == null)
            {
                Debug.LogWarning("[PartyManager] PartyRuntimeData is null.");
            }

            return runtimeData;
        }

        private CharacterManager CreateRuntimeCharacterManager(CharacterSO characterSO)
        {
            if (characterSO == null)
            {
                return null;
            }

            GameObject runtimeObject =
                CharacterBuilder.CreateOrBuildPlayerObject(
                    null,
                    $"CharacterRuntime_{characterSO.name}",
                    transform,
                    Vector3.zero,
                    Quaternion.identity,
                    null,
                    true);

            runtimeMemberObjects.Add(runtimeObject);

            CharacterManager characterManager =
                runtimeObject.GetComponent<CharacterManager>();

            characterManager.InitializeFromSO(characterSO);

            return characterManager;
        }

        private void DestroyRuntimeObject(CharacterRuntimeData runtimeData)
        {
            if (runtimeData == null)
            {
                return;
            }

            CharacterManager[] managers = FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);
            foreach (CharacterManager manager in managers)
            {
                if (manager == null || manager.RuntimeData != runtimeData)
                {
                    continue;
                }

                runtimeMemberObjects.Remove(manager.gameObject);
                spawnedMembers.Remove(manager.gameObject);
                Destroy(manager.gameObject);
                return;
            }
        }

        private CharacterSO FindPlayerCharacterSOByJob(CharacterJob job)
        {
            CharacterSO[] characterSOs = Resources.LoadAll<CharacterSO>("character");
            foreach (CharacterSO characterSO in characterSOs)
            {
                if (characterSO == null)
                {
                    continue;
                }

                if (characterSO.CharacterType != CharacterType.Player)
                {
                    continue;
                }

                if (characterSO.Job == job)
                {
                    return characterSO;
                }
            }

            return null;
        }

        private void ClearRuntimeMemberObjects()
        {
            for (int i = 0; i < runtimeMemberObjects.Count; i++)
            {
                if (runtimeMemberObjects[i] != null)
                {
                    Destroy(runtimeMemberObjects[i]);
                }
            }

            runtimeMemberObjects.Clear();
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
            ClearRuntimeMemberObjects();
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
