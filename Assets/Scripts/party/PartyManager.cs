using System.Collections.Generic;
using Session;
using Character;
using Effect;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Party
{
    public class PartyManager : MonoBehaviour
    {
        [Header("Spawn")]
        [SerializeField] private Transform spawnRoot;

        [SerializeField] private float spacing = 1.5f;

        private readonly List<GameObject> spawnedMembers = new();

        private const string BattleSceneName = "BattleScene";

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
                    || characterRuntime.characterSO == null
                    || characterRuntime.characterSO.prefab == null)
                {
                    continue;
                }

                Vector3 spawnPosition =
                    transform.position
                    + Vector3.right * (i * spacing);

                GameObject spawnedObject =
                    Instantiate(
                        characterRuntime.characterSO.prefab,
                        spawnPosition,
                        Quaternion.identity,
                        spawnRoot);

                spawnedMembers.Add(spawnedObject);

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
        }
    }
}
