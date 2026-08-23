using System;
using System.Collections.Generic;
using Character;
using Character.Runtime.Skill;
using Party;
using Presentation;
using Session;
using Skill;
using Stat;
using UnityEngine;

namespace Battle.UI.PartyHud
{
    /// <summary>
    /// PartyManager 및 CharacterManager의 런타임 상태(HP, 스킬 쿨타임, 상태이상 등)를
    /// 지속적으로 관찰하여 PartyHudView에 실시간으로 반영하는 Presenter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PartyHudPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private PartyHudView hudView;

        [Header("Update Interval")]
        [Tooltip("0이면 매 프레임(LateUpdate), 0보다 크면 주기적 갱신")]
        [SerializeField] private float updateInterval = 0.05f;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        private float elapsedSinceLastUpdate;
        private readonly List<PartyHudMemberData> cachedMemberDataList = new();
        private readonly CharacterPresentationResolver characterPresentationResolver = new();

        public PartyHudView HudView => hudView;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Refresh();
        }

        private void LateUpdate()
        {
            if (updateInterval <= 0f)
            {
                Refresh();
                return;
            }

            elapsedSinceLastUpdate += Time.deltaTime;
            if (elapsedSinceLastUpdate >= updateInterval)
            {
                elapsedSinceLastUpdate = 0f;
                Refresh();
            }
        }

        private void ResolveReferences()
        {
            if (hudView == null)
            {
                hudView = GetComponent<PartyHudView>();
            }
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (hudView == null)
            {
                return;
            }

            cachedMemberDataList.Clear();

            // 1. Try get active characters from PartyManager
            PartyManager partyManager = PartyManager.Instance;
            IReadOnlyList<CharacterManager> activeMembers = partyManager != null ? partyManager.Members : null;

            if (activeMembers != null && activeMembers.Count > 0)
            {
                for (int i = 0; i < activeMembers.Count; i++)
                {
                    CharacterManager character = activeMembers[i];
                    if (character == null)
                    {
                        continue;
                    }

                    PartyHudMemberData memberData = CreateMemberDataFromCharacter(character);
                    cachedMemberDataList.Add(memberData);
                }
            }
            else
            {
                // Fallback to Session PartyRuntimeData if characters haven't spawned yet
                PartyRuntimeData sessionPartyData = GameSession.Instance?.BattleSession?.PartyRuntimeData;
                if (sessionPartyData != null && sessionPartyData.Members != null)
                {
                    for (int i = 0; i < sessionPartyData.Members.Count; i++)
                    {
                        CharacterRuntimeData runtimeData = sessionPartyData.Members[i];
                        if (runtimeData?.characterSO != null)
                        {
                            cachedMemberDataList.Add(CreateFallbackMemberData(runtimeData));
                        }
                    }
                }
            }

            PartyHudViewData viewData = new PartyHudViewData(cachedMemberDataList);
            hudView.Render(viewData);
        }

        private PartyHudMemberData CreateMemberDataFromCharacter(CharacterManager character)
        {
            CharacterRuntimeData runtimeData = character.RuntimeData;
            CharacterSO characterSO = runtimeData?.characterSO;

            string displayName = character.name;
            Sprite portrait = null;

            if (characterSO != null)
            {
                CharacterPresentationData presentation = characterPresentationResolver.ResolveData(
                    characterSO,
                    PresentationContext.Runtime);
                displayName = presentation?.Identity?.DisplayName;
                portrait = presentation?.Identity?.Icon;

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = !string.IsNullOrWhiteSpace(characterSO.CharacterId)
                        ? characterSO.CharacterId
                        : characterSO.name;
                }
            }

            float currentHp = character.GetStatValue(StatType.Hp);
            float maxHp = character.GetStatValue(StatType.MaxHp);

            bool isDead = runtimeData != null && runtimeData.isDead;
            bool isStunned = character.IsStunned;

            string statusText = isDead ? "DEAD" : (isStunned ? "STUN" : string.Empty);
            Color statusColor = isDead ? Color.red : (isStunned ? Color.yellow : Color.white);

            // Skills from CharacterSkillManager
            CharacterSkillManager skillManager = character.GetComponent<CharacterSkillManager>();
            CharacterSkillRuntimeData skillRuntime = skillManager?.SkillRuntimeData;
            SkillPoolRuntimeData skillPool = skillRuntime?.skillPool;

            List<PartyHudSkillSlotData> activeSkills = new();
            PartyHudSkillSlotData passiveSkill = null;

            if (skillPool != null && skillPool.Slots != null)
            {
                for (int i = 0; i < skillPool.Slots.Count; i++)
                {
                    SkillPoolSlotData slot = skillPool.Slots[i];
                    if (slot == null || slot.SkillSo == null)
                    {
                        continue;
                    }

                    string slotKey = slot.SlotKey ?? string.Empty;
                    bool isBasicAttack = slotKey.Equals("basic_attack", StringComparison.OrdinalIgnoreCase);
                    bool isPassive = slotKey.StartsWith("passive", StringComparison.OrdinalIgnoreCase);

                    Sprite icon = slot.SkillSo.Icon;
                    float durationCooldown = slot.SkillSo.CastSo != null ? slot.SkillSo.CastSo.Cooldown : 0f;
                    float remainingCooldown = 0f;

                    if (skillRuntime != null && !string.IsNullOrEmpty(slot.SkillSo.EquipmentId))
                    {
                        if (skillRuntime.cooldownEndTimes.TryGetValue(slot.SkillSo.EquipmentId, out float endTime))
                        {
                            remainingCooldown = Mathf.Max(0f, endTime - Time.time);
                        }
                    }

                    PartyHudSkillState state = isPassive
                        ? PartyHudSkillState.Passive
                        : (remainingCooldown > 0f ? PartyHudSkillState.Unavailable : PartyHudSkillState.Available);

                    PartyHudSkillSlotData slotData = new PartyHudSkillSlotData(
                        icon,
                        remainingCooldown,
                        durationCooldown,
                        state,
                        isBasicAttack);

                    if (isPassive)
                    {
                        if (passiveSkill == null)
                        {
                            passiveSkill = slotData;
                        }
                    }
                    else
                    {
                        activeSkills.Add(slotData);
                    }
                }
            }

            return new PartyHudMemberData(
                displayName,
                portrait,
                currentHp,
                maxHp,
                statusText,
                statusColor,
                activeSkills,
                passiveSkill);
        }

        private PartyHudMemberData CreateFallbackMemberData(CharacterRuntimeData runtimeData)
        {
            CharacterSO characterSO = runtimeData.characterSO;
            string displayName = "Character";
            Sprite portrait = null;

            if (characterSO != null)
            {
                CharacterPresentationData presentation = characterPresentationResolver.ResolveData(
                    characterSO,
                    PresentationContext.Runtime);
                displayName = presentation?.Identity?.DisplayName ?? characterSO.name;
                portrait = presentation?.Identity?.Icon;
            }

            float maxHp = 100f;
            if (characterSO != null && characterSO.BaseStats != null)
            {
                for (int i = 0; i < characterSO.BaseStats.Count; i++)
                {
                    if (characterSO.BaseStats[i].statType == StatType.MaxHp)
                    {
                        maxHp = Mathf.Max(0f, characterSO.BaseStats[i].value);
                        break;
                    }
                }
            }
            float currentHp = maxHp;

            return new PartyHudMemberData(
                displayName,
                portrait,
                currentHp,
                maxHp,
                string.Empty,
                Color.white,
                null,
                null);
        }
    }
}
