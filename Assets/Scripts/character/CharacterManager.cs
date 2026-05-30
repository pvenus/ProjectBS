using System;
using Stat;
using UnityEngine;
using Party.UI;
using Party;
using Character.Service;

namespace Character
{
    public class CharacterManager : MonoBehaviour
    {
        public static event Action<CharacterManager> OnAnyCharacterDied = delegate { };
        public static event Action<CharacterDamageRequest, CharacterDamageResult> OnAnyDamageApplied = delegate { };
        public static event Action<CharacterManager, CharacterManager, GoldDropService.Result> OnAnyGoldDropped = delegate { };
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

        private CharacterStatusTickService statusTickService;

        private CharacterPresentationService presentationService;

        private CharacterDeathService deathService;

        private GoldDropService goldDropService;

        private CharacterBattleHudUI spawnedBattleHud;

        private bool isDying;

        private CharacterManager lastHitAttacker;

        [Header("Regen")]
        [SerializeField] private float hpRegenTickInterval = 0.25f;

        [Header("Bleed")]
        [SerializeField] private float bleedTickInterval = 1f;

        public CharacterRuntimeData RuntimeData => runtimeData;

        public bool IsStunned => GetStatValue(StatType.StunDuration) > 0f;

        public bool IsRooted => GetStatValue(StatType.RootDuration) > 0f;

        public bool CanMove => !IsStunned && !IsRooted;

        public bool CanUseSkill => !IsStunned;

        private void Update()
        {
            if (statusTickService == null)
            {
                statusTickService =
                    new CharacterStatusTickService();
            }

            if (damageService == null)
            {
                damageService =
                    new CharacterDamageService();
            }

            statusTickService.Tick(
                this,
                runtimeData,
                statService,
                damageService,
                Time.deltaTime,
                hpRegenTickInterval,
                bleedTickInterval);
        }

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

            statusTickService =
                new CharacterStatusTickService();
            statusTickService.Reset();

            presentationService =
                new CharacterPresentationService();

            deathService =
                new CharacterDeathService();
            deathService.Reset();

            goldDropService =
                new GoldDropService();

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

            statusTickService =
                new CharacterStatusTickService();
            statusTickService.Reset();

            presentationService =
                new CharacterPresentationService();

            deathService =
                new CharacterDeathService();
            deathService.Reset();

            goldDropService =
                new GoldDropService();

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
            if (battleHudPrefab == null || spawnedBattleHud != null)
            {
                return;
            }

            if (presentationService == null)
            {
                presentationService =
                    new CharacterPresentationService();
            }

            spawnedBattleHud =
                presentationService.CreateBattleHud(
                    battleHudPrefab,
                    transform,
                    hudRoot,
                    hud => hud.Initialize(this));
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

            CharacterDamageResult result =
                damageService.Apply(request);

            NotifyDamageApplied(
                request,
                result);

            return result;
        }

        private void NotifyDamageApplied(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            if (request == null || result == null)
            {
                return;
            }

            OnAnyDamageApplied?.Invoke(
                request,
                result);
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

            if (presentationService == null)
            {
                presentationService =
                    new CharacterPresentationService();
            }

            presentationService.PlayDamagePresentation(
                spawnedBattleHud,
                damage,
                isCritical,
                (hud, damageValue, critical) =>
                {
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
                        damageValue,
                        popupPosition,
                        critical);
                });
        }

        public void Heal(float value)
        {
            if (runtimeData == null || value <= 0f)
            {
                return;
            }

            if (damageService == null)
            {
                damageService =
                    new CharacterDamageService();
            }

            damageService.Heal(
                this,
                value);
        }

        public void RegisterLastHitAttacker(CharacterManager attacker)
        {
            if (attacker == null || attacker == this)
            {
                return;
            }

            lastHitAttacker = attacker;
        }

        public void HandleDeath()
        {
            if (isDying)
            {
                return;
            }

            isDying = true;

            if (deathService == null)
            {
                deathService =
                    new CharacterDeathService();
            }

            deathService.HandleDeath(
                new CharacterDeathService.Context
                {
                    characterManager = this,
                    runtimeData = runtimeData,
                    lastHitAttacker = lastHitAttacker,
                    coroutineOwner = this,
                    gameObject = gameObject,
                    rigidbody2D = GetComponent<Rigidbody2D>(),
                    colliders = GetComponentsInChildren<Collider2D>(true),
                    destroyDelay = deathDestroyDelay,
                    onDeath = OnDeathStarted,
                    onBeforeDestroy = OnBeforeDeathDestroy
                });
        }

        private void NotifyCharacterDied()
        {
            OnAnyCharacterDied?.Invoke(this);
        }

        private void OnDeathStarted(CharacterManager attacker)
        {
            NotifyCharacterDied();
            ProcessGoldDrop(attacker);

            venus.eldawn.party.AnimationMono animationMono =
                GetComponentInChildren<venus.eldawn.party.AnimationMono>();

            if (animationMono != null)
            {
                animationMono.PlayDeath();
            }

            if (playDeathDissolveOnDeath
                && shaderController != null)
            {
                shaderController.PlayDeathDissolve();
            }
        }

        private void ProcessGoldDrop(CharacterManager attacker)
        {
            if (goldDropService == null)
            {
                goldDropService =
                    new GoldDropService();
            }
            GoldDropService.Result result =
                goldDropService.DropGold(
                    this,
                    attacker);

            if (result.totalGold <= 0)
            {
                return;
            }

            OnAnyGoldDropped?.Invoke(
                this,
                attacker,
                result);
        }

        private void OnBeforeDeathDestroy()
        {
            if (presentationService != null)
            {
                presentationService.DestroyBattleHud(spawnedBattleHud);
                spawnedBattleHud = null;
            }
        }
    }
}