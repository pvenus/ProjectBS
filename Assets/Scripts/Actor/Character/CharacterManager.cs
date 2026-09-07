using System;
using Stat;
using UnityEngine;
using Party;
using Character.Service;
using Character.Skill;
using Effect;
using Battle.Presentation;

namespace Character
{
    public class CharacterManager : MonoBehaviour
    {
        public static event Action<CharacterManager> OnAnyCharacterDied = delegate { };
        public static event Action<CharacterDamageRequest, CharacterDamageResult> OnAnyDamageApplied = delegate { };
        public static event Action<CharacterManager, CharacterManager, GoldDropService.Result> OnAnyGoldDropped = delegate { };
        public static event Action<CharacterManager, float> OnAnyHealed = delegate { };
        [Header("Runtime")]
        [SerializeField] private CharacterRuntimeData runtimeData;


        [Header("Damage Popup")]
        [SerializeField] private bool showDamagePopup = true;

        [SerializeField] private Vector3 damagePopupOffset = new(0f, 0.9f, 0f);


        [Header("Death")]
        [SerializeField] private bool playDeathDissolveOnDeath = true;

        [SerializeField] private float deathDestroyDelay = 0.6f;

        private CharacterStatService statService;

        private CharacterDamageService damageService;

        private CharacterStatusTickService statusTickService;

        private CharacterPresentationService presentationService;

        private CharacterDeathService deathService;

        private GoldDropService goldDropService;

        private CharacterExperienceService experienceService;

        private bool characterScaleBaselineCaptured;
        private Vector3 baseCharacterScale = Vector3.one;


        private bool isDying;
        private Coroutine npcDeathPresentationRoutine;
        private Coroutine spawnRevealRoutine;
        private bool spawnRevealPending;

        private CharacterManager lastHitAttacker;

        [Header("Regen")]
        [SerializeField] private float hpRegenTickInterval = 1f;

        [Header("Bleed")]
        [SerializeField] private float bleedTickInterval = 1f;

        public CharacterRuntimeData RuntimeData => runtimeData;

        public bool IsStunned => GetStatValue(StatType.StunDuration) > 0f;

        public bool IsRooted => GetStatValue(StatType.RootDuration) > 0f;

        public bool CanMove => CanMoveNow();

        public void SetShowDamagePopup(bool show)
        {
            showDamagePopup = show;
        }

        public bool CanMoveNow()
        {
            if (isDying)
            {
                return false;
            }

            if (runtimeData != null && runtimeData.isDead)
            {
                return false;
            }

            if (IsStunned)
            {
                return false;
            }

            if (IsRooted)
            {
                return false;
            }

            return true;
        }

        public bool CanUseSkill => !IsStunned && !Battle.Morpg.BattleMorpgLiveRoute.IsTransitionLocked(this);

        public bool IsDying => isDying;

        public bool IsTargetable => isActiveAndEnabled &&
                                    gameObject.activeInHierarchy &&
                                    !isDying &&
                                    runtimeData != null &&
                                    !runtimeData.isDead;

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

        public void ConfigureIndomitablePassiveSnapshot(
            Character.Skill.ResolvedConditionalPassiveSnapshot snapshot)
        {
            if (statusTickService == null)
            {
                statusTickService = new CharacterStatusTickService();
            }

            statusTickService.ConfigureIndomitableSnapshot(this, snapshot);
        }

        private void OnDisable()
        {
            if (spawnRevealRoutine != null)
            {
                StopCoroutine(spawnRevealRoutine);
                spawnRevealRoutine = null;
            }
            statusTickService?.SuspendIndomitableProjection(this);
        }

        private void OnEnable()
        {
            TryStartPendingSpawnReveal();
        }

        private void ResetSpawnPresentationForInitialization()
        {
            spawnRevealPending = false;
            if (spawnRevealRoutine != null) StopCoroutine(spawnRevealRoutine);
            spawnRevealRoutine = null;
            if (npcDeathPresentationRoutine != null) StopCoroutine(npcDeathPresentationRoutine);
            npcDeathPresentationRoutine = null;
            isDying = false;
            lastHitAttacker = null;
        }

        private void RequestSpawnReveal()
        {
            spawnRevealPending = true;
            TryStartPendingSpawnReveal();
        }

        private void TryStartPendingSpawnReveal()
        {
            if (!spawnRevealPending || spawnRevealRoutine != null || !isActiveAndEnabled)
                return;
            spawnRevealRoutine = StartCoroutine(PlaySpawnRevealNextFrame());
        }

        public void InitializeFromSO(CharacterSO characterSO)
        {
            ResetSpawnPresentationForInitialization();
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

            ApplyCharacterScale(characterSO);
            ApplyNpcVisualPresentationScale(characterSO);

            runtimeData.stats.Clear();
            runtimeData.finalStats.Clear();

            if (characterSO.BaseStats != null)
            {
                for (int i = 0;
                     i < characterSO.BaseStats.Count;
                     i++)
                {
                    StatEntry entry =
                        characterSO.BaseStats[i];

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

            experienceService =
                new CharacterExperienceService();

            statService.RefreshFinalStats();

            SetStat(
                StatType.Hp,
                GetStatValue(StatType.MaxHp));

            // Removed old animation/skill override flow
            InitializeSkillManager(characterSO);
            EnsureBattleCharacterAura();
            ConfigureNpcCastDefaults(runtimeData?.characterSO);
            Control.SeojinManualControl.Bind(this);

            RequestSpawnReveal();
        }


        private void InitializeSkillManager(CharacterSO characterSO)
        {
            CharacterSkillManager skillManager = GetComponent<CharacterSkillManager>()
                ?? GetComponentInChildren<CharacterSkillManager>();

            if (skillManager == null)
            {
                skillManager = gameObject.AddComponent<CharacterSkillManager>();
            }

            skillManager.InitializeSkills(characterSO);

            Character.UI.CharacterSkillCooldownUI recentSkillUI =
                GetComponentInChildren<Character.UI.CharacterSkillCooldownUI>(true);
            if (recentSkillUI != null)
            {
                recentSkillUI.SkillManager = skillManager;
            }
        }

        public void Initialize(CharacterRuntimeData data)
        {
            ResetSpawnPresentationForInitialization();
            ResolveComponents();

            runtimeData = data;

            ApplyCharacterScale(runtimeData?.characterSO);
            ApplyNpcVisualPresentationScale(runtimeData?.characterSO);

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

            experienceService =
                new CharacterExperienceService();

            statService.RefreshFinalStats();

            if (GetStatValue(StatType.Hp) <= 0f
                && !runtimeData.isDead)
            {
                SetStat(
                    StatType.Hp,
                    GetStatValue(StatType.MaxHp));
            }

            // Removed old animation/skill override flow
            InitializeSkillManager(runtimeData?.characterSO);
            EnsureBattleCharacterAura();
            ConfigureNpcCastDefaults(runtimeData?.characterSO);
            Control.SeojinManualControl.Bind(this);

            RequestSpawnReveal();

        }

        private void ConfigureNpcCastDefaults(CharacterSO character)
        {
            var presentation = GetComponent<CharacterSkillCastPresentationMono>();
            if (presentation == null && CharacterSkillCastPresentationMono.SupportsNpcPalette(character))
                presentation = gameObject.AddComponent<CharacterSkillCastPresentationMono>();
            presentation?.ConfigureNpcDefaults(character);
        }

        private void EnsureBattleCharacterAura()
        {
            if (runtimeData?.characterSO == null)
            {
                return;
            }

            BattleCharacterAuraInstaller.EnsureFor(this);
        }

        private System.Collections.IEnumerator PlaySpawnRevealNextFrame()
        {
            yield return null;

            spawnRevealRoutine = null;
            spawnRevealPending = false;
            GetComponent<ShaderControllerMono>()?.PlaySpawnReveal();
        }

        private void ResolveComponents()
        {
            EnsureComponent<CharacterStateManager>();
            EnsureComponent<AnimationMono>();
            EnsureComponent<ShaderControllerMono>();
            EnsureComponent<EffectManager>();
            EnsureComponent<MovementMono>();
        }

        private void ApplyCharacterScale(CharacterSO characterSO)
        {
            if (characterSO == null)
            {
                return;
            }

            if (!characterScaleBaselineCaptured)
            {
                baseCharacterScale = transform.localScale;
                characterScaleBaselineCaptured = true;
            }

            float resolvedScale = characterSO.Scale;
            transform.localScale = baseCharacterScale * resolvedScale;
        }

        private void ApplyNpcVisualPresentationScale(CharacterSO characterSO)
        {
            string characterId = characterSO != null
                ? characterSO.CharacterId
                : null;
            float multiplier =
                NpcVisualScalePresentationMono.ResolveMultiplier(characterId);
            NpcVisualScalePresentationMono presentation =
                GetComponent<NpcVisualScalePresentationMono>();

            if (!Mathf.Approximately(multiplier, 1f) || presentation != null)
            {
                presentation ??= gameObject.AddComponent<NpcVisualScalePresentationMono>();
                presentation.Configure(characterId);
            }
        }

        private T EnsureComponent<T>()
            where T : Component
        {
            T component = GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            component = GetComponentInChildren<T>();
            if (component != null)
            {
                return component;
            }

            return gameObject.AddComponent<T>();
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
            GetComponent<ShaderControllerMono>()?.PlayHitFlash();

            if (presentationService == null)
            {
                presentationService =
                    new CharacterPresentationService();
            }

            presentationService.PlayDamagePresentation(
                GetComponentInChildren<Party.UI.CharacterBattleHudUI>(),
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

            OnAnyHealed?.Invoke(
                this,
                value);
        }

        public void GainExperience(float baseExperience)
        {
            if (runtimeData == null || baseExperience <= 0f)
            {
                return;
            }

            if (experienceService == null)
            {
                experienceService =
                    new CharacterExperienceService();
            }

            experienceService.GainExperience(
                this,
                baseExperience);
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
                    destroyDelay = ResolveDeathDestroyDelay(),
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
            ProcessKillStack(attacker);
            if (GetComponent<Battle.Morpg.MorpgOwnedObject>() == null)
                ProcessGoldDrop(attacker);

            AnimationMono animationMono =
                GetComponentInChildren<AnimationMono>();

            if (UsesEpisode1NpcDeathSequence())
            {
                (GetComponent<CharacterSkillManager>()
                    ?? GetComponentInChildren<CharacterSkillManager>())?.CancelCasting();
                if (animationMono != null)
                    animationMono.PlayDeath(1f);
                else
                    Debug.LogWarning($"[CharacterManager] Required NPC death clip controller missing: {runtimeData?.characterSO?.CharacterId}", this);

                if (npcDeathPresentationRoutine == null && gameObject.activeInHierarchy)
                    npcDeathPresentationRoutine = StartCoroutine(PlayNpcDeathDissolveAfterAnimation());
                return;
            }

            if (animationMono != null)
                animationMono.PlayDeath();

            ShaderControllerMono shaderController =
                GetComponent<ShaderControllerMono>();

            if (playDeathDissolveOnDeath
                && shaderController != null)
            {
                shaderController.PlayDeathDissolve();
            }
        }

        private bool UsesEpisode1NpcDeathSequence()
        {
            string id = runtimeData?.characterSO?.CharacterId;
            return string.Equals(id, "character.black_cloth_raider.1", StringComparison.Ordinal) ||
                string.Equals(id, "character.chain_dragger_raider.1", StringComparison.Ordinal);
        }

        private float ResolveDeathDestroyDelay()
        {
            if (!UsesEpisode1NpcDeathSequence())
                return deathDestroyDelay;

            ShaderControllerMono shader = GetComponent<ShaderControllerMono>();
            float dissolveDuration = playDeathDissolveOnDeath && shader != null
                ? shader.DeathDissolveDuration
                : 0f;
            return 1f + dissolveDuration;
        }

        private System.Collections.IEnumerator PlayNpcDeathDissolveAfterAnimation()
        {
            yield return new WaitForSeconds(1f);
            npcDeathPresentationRoutine = null;
            ShaderControllerMono shader = GetComponent<ShaderControllerMono>();
            if (playDeathDissolveOnDeath && shader != null && shader.DeathDissolveDuration > 0f)
            {
                shader.PlayDeathDissolve();
            }
            else
            {
                Debug.LogWarning($"[CharacterManager] NPC death dissolve missing; using timed despawn fallback: {runtimeData?.characterSO?.CharacterId}", this);
            }
        }

        private void ProcessKillStack(CharacterManager attacker)
        {
            if (attacker == null || attacker == this)
            {
                return;
            }

            attacker.AddStat(
                StatType.KillStack,
                1f);
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
        }
    }
}
