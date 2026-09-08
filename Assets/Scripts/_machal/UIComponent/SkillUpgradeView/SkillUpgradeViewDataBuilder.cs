using System.Collections.Generic;
using Action = System.Action;
using Character;
using Presentation;
using Skill;
using UIFramework.Data;
using UnityEngine;
using Progression;

public struct SkillUpgradeOptionContext
{
    public EquipmentSkillInstanceData skillInstance;
    public EquipmentSkillSO skillSo;
    public CharacterSkillManager skillManager;
    public int maxSkillLevel;
}

public struct SkillUpgradeBuildResult
{
    public SkillUpgradeViewData viewData;
    public List<SkillUpgradeOptionContext> contexts;
}

public static class SkillUpgradeViewDataBuilder
{
    private static readonly EquipmentUpgradeComparisonService comparisonService = new();
    private static readonly EquipmentSkillResolver equipmentSkillResolver = new();
    private static readonly SkillPresentationResolver skillPresentationResolver = new();
    private static readonly SkillPresentationGroupResolver skillGroupResolver = new();

    public static SkillUpgradeOptionData BuildFixedOfferOption(
        CharacterRuntimeData owner,
        EquipmentSkillInstanceData instance,
        EquipmentSkillSO skillSo)
    {
        if (owner?.characterSO == null || instance == null || skillSo == null
            || !string.Equals(instance.equipmentId, skillSo.EquipmentId, System.StringComparison.Ordinal))
            return null;
        int current = Mathf.Max(1, instance.currentLevel);
        int next = current + 1;
        if (!HasRuntimeApplicableUpgrade(skillSo, next)) return null;
        return new SkillUpgradeOptionData
        {
            equipmentId = skillSo.EquipmentId,
            characterPortrait = owner.characterSO.Portrait,
            characterName = owner.characterSO.DisplayName,
            currentLevel = current,
            nextLevel = next,
            statComparisonText = comparisonService.BuildComparisonText(skillSo, current, next),
            content = BuildNextLevelContent(skillSo, instance, next)
        };
    }

    public static SkillUpgradeBuildResult Build(
        IReadOnlyList<CharacterManager> characterManagers, 
        int randomOptionCount = 3, 
        int maxSkillLevel = int.MaxValue)
    {
        List<SkillUpgradeOptionData> optionDatas = new List<SkillUpgradeOptionData>();
        List<SkillUpgradeOptionContext> contexts = new List<SkillUpgradeOptionContext>();

        // 1. Collect all candidates
        List<SkillUpgradeOptionContext> allCandidates = CollectCandidates(characterManagers, maxSkillLevel);

        if (allCandidates.Count < randomOptionCount)
        {
            Debug.LogWarning(
                $"[SkillUpgradeViewDataBuilder] Requested {randomOptionCount} unique options, "
                + $"but only {allCandidates.Count} owned skills have a valid next upgrade entry.");
        }

        // 2. Shuffle and select candidates
        List<SkillUpgradeOptionContext> selectedCandidates = SelectRandom(allCandidates, randomOptionCount);

        // 3. Convert selected candidates to UI-ready data
        foreach (var candidate in selectedCandidates)
        {
            EquipmentSkillSO skillSo = candidate.skillSo;
            
            int currentLevel = Mathf.Max(1, candidate.skillInstance.currentLevel);
            int nextLevel = Mathf.Min(candidate.maxSkillLevel, currentLevel + 1);

            string characterName = string.Empty;
            Sprite characterPortrait = null;
            if (candidate.skillManager != null && candidate.skillManager.GetComponent<CharacterManager>() != null)
            {
                var charManager = candidate.skillManager.GetComponent<CharacterManager>();
                if (charManager.RuntimeData != null && charManager.RuntimeData.characterSO != null)
                {
                    characterName = charManager.RuntimeData.characterSO.DisplayName;
                    characterPortrait = charManager.RuntimeData.characterSO.Portrait;
                }
            }

            optionDatas.Add(new SkillUpgradeOptionData
            {
                equipmentId = skillSo?.EquipmentId ?? candidate.skillInstance.equipmentId,
                characterPortrait = characterPortrait,
                characterName = !string.IsNullOrEmpty(characterName) ? characterName : "Character",
                currentLevel = currentLevel,
                nextLevel = nextLevel,
                statComparisonText = skillSo != null
                    ? comparisonService.BuildComparisonText(skillSo, currentLevel, nextLevel)
                    : string.Empty,
                content = BuildNextLevelContent(
                    skillSo,
                    candidate.skillInstance,
                    nextLevel)
            });

            Debug.Log(
                $"[SkillUpgradeViewDataBuilder] card={optionDatas.Count - 1}, "
                + $"equipmentId={candidate.skillInstance.equipmentId}, "
                + $"level={currentLevel}->{nextLevel}, "
                + $"comparison={optionDatas[optionDatas.Count - 1].statComparisonText.Replace('\n', '|')}");

            contexts.Add(candidate);
        }

        return new SkillUpgradeBuildResult
        {
            viewData = new SkillUpgradeViewData
            {
                title = "스킬 업그레이드 선택",
                options = optionDatas
            },
            contexts = contexts
        };
    }

    private static List<SkillUpgradeOptionContext> CollectCandidates(
        IReadOnlyList<CharacterManager> characterManagers, 
        int maxSkillLevel)
    {
        List<SkillUpgradeOptionContext> candidates = new List<SkillUpgradeOptionContext>();

        if (characterManagers == null)
            return candidates;

        for (int i = 0; i < characterManagers.Count; i++)
        {
            CharacterManager manager = characterManagers[i];
            CharacterRuntimeData runtimeData = manager?.RuntimeData;

            if (manager == null || runtimeData == null || runtimeData.skillInstances == null)
                continue;

            CharacterSkillManager skillManager =
                manager.GetComponent<CharacterSkillManager>()
                ?? manager.GetComponentInChildren<CharacterSkillManager>();

            if (skillManager == null)
                continue;

            for (int skillIndex = 0; skillIndex < runtimeData.skillInstances.Count; skillIndex++)
            {
                EquipmentSkillInstanceData skillInstance = runtimeData.skillInstances[skillIndex];
                EquipmentSkillSO skillSo = ResolveSkillSo(
                    skillManager,
                    skillInstance.equipmentId);
                if (skillSo == null)
                {
                    Debug.LogWarning(
                        $"[SkillUpgradeViewDataBuilder] SkillPool does not contain skill '{skillInstance.equipmentId}' for '{manager.name}'.");
                    continue;
                }

                int currentLevel = Mathf.Max(1, skillInstance.currentLevel);
                int tableMaxLevel = GetRuntimeUpgradeMaxLevel(skillSo);
                int effectiveMaxLevel = Mathf.Min(maxSkillLevel, tableMaxLevel);
                if (!CanUpgrade(skillInstance, effectiveMaxLevel))
                    continue;

                int nextLevel = currentLevel + 1;
                if (!HasRuntimeApplicableUpgrade(skillSo, nextLevel))
                {
                    Debug.LogWarning(
                        $"[SkillUpgradeViewDataBuilder] Skill '{skillSo.EquipmentId}' has no runtime-applicable stat/effect upgrade for level {nextLevel}.");
                    continue;
                }

                candidates.Add(new SkillUpgradeOptionContext
                {
                    skillInstance = skillInstance,
                    skillSo = skillSo,
                    skillManager = skillManager,
                    maxSkillLevel = effectiveMaxLevel
                });
            }
        }

        return candidates;
    }

    private static List<SkillUpgradeOptionContext> SelectRandom(
        List<SkillUpgradeOptionContext> candidates, 
        int optionCount)
    {
        List<SkillUpgradeOptionContext> selected = new List<SkillUpgradeOptionContext>();
        if (candidates == null || candidates.Count == 0) return selected;

        int count = Mathf.Min(optionCount, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            int selectedIndex = Random.Range(i, candidates.Count);
            (candidates[i], candidates[selectedIndex]) = (candidates[selectedIndex], candidates[i]);
            selected.Add(candidates[i]);
        }

        return selected;
    }

    private static bool CanUpgrade(EquipmentSkillInstanceData skillInstance, int maxSkillLevel)
    {
        if (skillInstance == null || string.IsNullOrWhiteSpace(skillInstance.equipmentId))
            return false;

        return Mathf.Max(1, skillInstance.currentLevel) < maxSkillLevel;
    }

    private static EquipmentSkillSO ResolveSkillSo(
        CharacterSkillManager skillManager,
        string equipmentId)
    {
        if (skillManager == null || string.IsNullOrWhiteSpace(equipmentId))
            return null;

        IReadOnlyList<SkillPoolSlotData> slots = skillManager.SkillPool?.Slots;
        if (slots == null)
            return null;

        for (int i = 0; i < slots.Count; i++)
        {
            EquipmentSkillSO skill = slots[i]?.SkillSo;
            if (skill != null && skill.EquipmentId == equipmentId)
                return skill;
        }

        return null;
    }

    public static int GetRuntimeUpgradeMaxLevel(EquipmentSkillSO skillSo)
    {
        IReadOnlyList<EquipmentUpgradeEntry> entries = skillSo?.UpgradeTableSo?.Entries;
        int maxLevel = 0;
        if (entries == null) return maxLevel;

        for (int i = 0; i < entries.Count; i++)
        {
            EquipmentUpgradeEntry entry = entries[i];
            if (entry != null && entry.HasAnyModifier)
                maxLevel = Mathf.Max(maxLevel, entry.Level);
        }

        return maxLevel;
    }

    public static bool HasRuntimeApplicableUpgrade(
        EquipmentSkillSO skillSo,
        int nextLevel)
    {
        IReadOnlyList<EquipmentUpgradeEntry> entries =
            skillSo?.UpgradeTableSo?.Entries;
        if (entries == null)
            return false;

        for (int i = 0; i < entries.Count; i++)
        {
            EquipmentUpgradeEntry entry = entries[i];
            if (entry == null || entry.Level != nextLevel)
                continue;

            return entry.HasAnyModifier;
        }

        return false;
    }

    private static ContentPresentationData BuildNextLevelContent(
        EquipmentSkillSO skillSo,
        EquipmentSkillInstanceData sourceInstance,
        int nextLevel)
    {
        if (skillSo == null || sourceInstance == null)
            return null;

        EquipmentSkillInstanceData previewInstance =
            new EquipmentSkillInstanceData
            {
                equipmentId = sourceInstance.equipmentId,
                currentLevel = Mathf.Max(1, nextLevel),
                upgradeLevel = Mathf.Max(0, sourceInstance.upgradeLevel)
            };

        EquipmentSkillRuntimeData runtime =
            equipmentSkillResolver.Resolve(
                skillSo,
                previewInstance);
        SkillPresentationData presentation =
            skillPresentationResolver.Resolve(
                runtime,
                PresentationContext.Runtime);

        return skillGroupResolver.ResolveForPlayerDisplay(presentation);
    }
}

/// <summary>
/// Adapts the battle-end skill upgrade domain flow to the passive SkillUpgradeView.
/// The View receives display-only data and reports only the selected option index.
/// </summary>
public sealed class BattleEndSkillUpgradePresenter
{
    private const int DefaultOptionCount = 3;

    private SkillUpgradeView activeView;
    private IReadOnlyList<SkillUpgradeOptionContext> activeContexts;
    private IReadOnlyList<string> activeOptionIds;
    private Action completionCallback;
    private bool isOpen;
    private float previousTimeScale = 1f;

    public bool Open(
        IReadOnlyList<CharacterManager> characterManagers,
        Action onCompleted)
    {
        if (isOpen)
        {
            return false;
        }

        SkillUpgradeBuildResult buildResult =
            SkillUpgradeViewDataBuilder.Build(
                characterManagers,
                DefaultOptionCount);

        if (buildResult.viewData?.options == null
            || buildResult.viewData.options.Count == 0
            || buildResult.contexts == null
            || buildResult.contexts.Count == 0)
        {
            return false;
        }

        UIPopupViewController popupController =
            UIPopupViewController.Instance;

        if (popupController == null)
        {
            Debug.LogWarning(
                "[BattleEndSkillUpgradePresenter] UIPopupViewController not found.");
            return false;
        }

        SkillUpgradeView view =
            popupController.Open<SkillUpgradeView>(
                PopupType.SkillUpgrade);

        if (view == null)
        {
            return false;
        }

        activeView = view;
        activeContexts = buildResult.contexts;
        List<string> optionIds = new();
        for (int i = 0; i < buildResult.viewData.options.Count; i++)
            optionIds.Add(buildResult.viewData.options[i]?.equipmentId ?? string.Empty);
        activeOptionIds = optionIds;
        completionCallback = onCompleted;

        activeView.SetData(buildResult.viewData);
        activeView.OnOptionClicked += HandleOptionClicked;
        activeView.SetCloseButtonVisible(false);

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isOpen = true;
        return true;
    }

    public void Dispose()
    {
        CloseActiveView();
    }

    private void HandleOptionClicked(int optionIndex)
    {
        if (!isOpen
            || activeContexts == null
            || optionIndex < 0
            || optionIndex >= activeContexts.Count)
        {
            Debug.LogWarning(
                $"[BattleEndSkillUpgradePresenter] Invalid option index: {optionIndex}.");
            return;
        }

        SkillUpgradeOptionContext context =
            activeContexts[optionIndex];

        string displayedEquipmentId = activeOptionIds != null
            && optionIndex < activeOptionIds.Count
                ? activeOptionIds[optionIndex]
                : string.Empty;
        if (!string.Equals(
                displayedEquipmentId,
                context.skillInstance?.equipmentId,
                System.StringComparison.Ordinal))
        {
            Debug.LogError(
                $"[BattleEndSkillUpgradePresenter] Card/apply identity mismatch at {optionIndex}: "
                + $"displayed='{displayedEquipmentId}', apply='{context.skillInstance?.equipmentId}'.");
            return;
        }

        if (context.skillManager == null
            || context.skillInstance == null)
        {
            Debug.LogWarning(
                "[BattleEndSkillUpgradePresenter] Upgrade context is incomplete.");
            return;
        }

        bool upgraded =
            context.skillManager.TryUpgradeSkill(
                context.skillInstance,
                context.maxSkillLevel);

        if (!upgraded)
        {
            Debug.LogWarning(
                "[BattleEndSkillUpgradePresenter] Skill upgrade failed.");
            return;
        }

        Action completed = completionCallback;
        CloseActiveView();
        completed?.Invoke();
    }

    private void CloseActiveView()
    {
        if (activeView != null)
        {
            activeView.OnOptionClicked -= HandleOptionClicked;
            activeView.SetCloseButtonVisible(true);

            UIPopupViewController popupController =
                UIPopupViewController.Instance;

            if (popupController != null)
            {
                popupController.Close(activeView);
            }
            else
            {
                activeView.Hide();
            }
        }

        if (isOpen)
        {
            Time.timeScale = previousTimeScale;
        }

        activeView = null;
        activeContexts = null;
        activeOptionIds = null;
        completionCallback = null;
        isOpen = false;
    }
}
