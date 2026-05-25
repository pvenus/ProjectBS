using System;
using Stat;
using UnityEngine;
using Party.UI;
using Party;

namespace Character
{
    public class CharacterManager : MonoBehaviour
    {
        public static event Action<CharacterManager> OnAnyCharacterDied = delegate { };
        [Header("Runtime")]
        [SerializeField] private CharacterRuntimeData runtimeData;

        [Header("Hud")]
        [SerializeField] private CharacterBattleHudUI battleHudPrefab;

        [SerializeField] private Transform hudRoot;

        [Header("Damage Popup")]
        [SerializeField] private bool showDamagePopup = true;

        [SerializeField] private Vector3 damagePopupOffset = new(0f, 0.9f, 0f);

        [Header("Effect")]
        [SerializeField] private ShaderControllerMono shaderController;

        [Header("Death")]
        [SerializeField] private bool playDeathDissolveOnDeath = true;

        [SerializeField] private float deathDestroyDelay = 0.6f;

        private CharacterStatService statService;

        private CharacterDamageService damageService;

        private CharacterBattleHudUI spawnedBattleHud;

        private bool isDying;

        public CharacterRuntimeData RuntimeData => runtimeData;

        public void InitializeFromSO(CharacterSO characterSO)
        {
            ResolveComponents();
            runtimeData = new CharacterRuntimeData
            {
                characterSO = characterSO
            };

            if (characterSO == null)
            {
                Debug.LogError(
                    "[CharacterManager] CharacterSO is null.");

                return;
            }

            runtimeData.stats.Clear();
            runtimeData.finalStats.Clear();

            if (characterSO.baseStats != null)
            {
                for (int i = 0;
                     i < characterSO.baseStats.Count;
                     i++)
                {
                    StatEntry entry =
                        characterSO.baseStats[i];

                    if (entry == null)
                    {
                        continue;
                    }

                    runtimeData.stats.Add(
                        new StatEntry
                        {
                            statType = entry.statType,
                            value = entry.value
                        });
                }
            }

            statService =
                new CharacterStatService(runtimeData);

            damageService =
                new CharacterDamageService();

            statService.RefreshFinalStats();

            SetStat(
                StatType.Hp,
                GetStatValue(StatType.MaxHp));

            CreateBattleHud();
        }

        public void Initialize(CharacterRuntimeData data)
        {
            ResolveComponents();

            runtimeData = data;

            statService =
                new CharacterStatService(runtimeData);

            damageService =
                new CharacterDamageService();

            statService.RefreshFinalStats();

            if (GetStatValue(StatType.Hp) <= 0f
                && !runtimeData.isDead)
            {
                SetStat(
                    StatType.Hp,
                    GetStatValue(StatType.MaxHp));
            }

            CreateBattleHud();
        }

        private void ResolveComponents()
        {
            if (shaderController == null)
            {
                shaderController =
                    GetComponent<ShaderControllerMono>();
            }

            if (shaderController == null)
            {
                shaderController =
                    GetComponentInChildren<ShaderControllerMono>();
            }
        }

        private void CreateBattleHud()
        {
            if (battleHudPrefab == null)
            {
                return;
            }

            if (spawnedBattleHud != null)
            {
                return;
            }

            Transform parent =
                hudRoot != null
                    ? hudRoot
                    : null;

            spawnedBattleHud =
                Instantiate(
                    battleHudPrefab,
                    transform.position,
                    Quaternion.identity,
                    parent);

            spawnedBattleHud.Initialize(this);
        }

        public float GetStatValue(StatType statType)
        {
            if (statService == null)
            {
                return 0f;
            }

            return statService.GetStat(statType);
        }

        public void AddStat(
            StatType statType,
            float value)
        {
            if (statService == null)
            {
                return;
            }

            statService.AddStat(
                statType,
                value);
        }

        public void SetStat(
            StatType statType,
            float value)
        {
            if (statService == null)
            {
                return;
            }

            statService.SetStat(
                statType,
                value);
        }

        public void RemoveStat(StatType statType)
        {
            if (statService == null)
            {
                return;
            }

            statService.RemoveStat(statType);
        }

        public CharacterDamageResult ApplyDamage(CharacterDamageRequest request)
        {
            if (damageService == null)
            {
                damageService =
                    new CharacterDamageService();
            }

            return damageService.Apply(request);
        }

        public void TakeDamage(
            float damage,
            bool isCritical = false)
        {
            if (damageService == null)
            {
                damageService =
                    new CharacterDamageService();
            }

            damageService.TakeDamage(
                this,
                damage,
                isCritical);
        }

        public void PlayDamagePresentation(
            float damage,
            bool isCritical)
        {
            shaderController?.PlayHitFlash();

            if (!showDamagePopup)
            {
                return;
            }

            if (DamagePupupManager.Instance == null)
            {
                return;
            }

            Vector3 popupPosition =
                transform.position
                + damagePopupOffset
                + new Vector3(
                    UnityEngine.Random.Range(-0.15f, 0.15f),
                    0f,
                    0f);

            DamagePupupManager.Instance.ShowDamage(
                damage,
                popupPosition,
                isCritical);
        }

        public void Heal(float value)
        {
            if (runtimeData == null)
            {
                return;
            }

            float currentHp =
                GetStatValue(StatType.Hp);

            currentHp += value;

            float maxHp =
                GetStatValue(StatType.MaxHp);

            currentHp =
                Mathf.Min(currentHp, maxHp);

            SetStat(
                StatType.Hp,
                currentHp);
        }

        public void HandleDeath()
        {
            if (isDying)
            {
                return;
            }

            isDying = true;

            NotifyCharacterDied();

            venus.eldawn.party.AnimationMono animationMono =
                GetComponentInChildren<venus.eldawn.party.AnimationMono>();

            if (animationMono != null)
            {
                animationMono.PlayDeath();
            }

            Collider2D[] colliders =
                GetComponentsInChildren<Collider2D>(true);

            for (int i = 0;
                 i < colliders.Length;
                 i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }

            Rigidbody2D rigidbody2D =
                GetComponent<Rigidbody2D>();

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
                rigidbody2D.angularVelocity = 0f;
                rigidbody2D.simulated = false;
            }

            if (playDeathDissolveOnDeath
                && shaderController != null)
            {
                shaderController.PlayDeathDissolve();
            }

            StartCoroutine(DestroyAfterDeathRoutine());
        }

        private void NotifyCharacterDied()
        {
            OnAnyCharacterDied?.Invoke(this);
        }

        private System.Collections.IEnumerator DestroyAfterDeathRoutine()
        {
            if (deathDestroyDelay > 0f)
            {
                yield return new WaitForSeconds(deathDestroyDelay);
            }

            Destroy(gameObject);
        }
    }
}