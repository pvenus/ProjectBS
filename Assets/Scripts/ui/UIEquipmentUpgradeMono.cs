using System.Collections.Generic;
using System.Text;
using Character;
using Skill;
using String;
using TMPro;
using Effect;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 중 스킬 레벨업 선택 UI.
/// - 보유 캐릭터의 스킬 인스턴스 중 업그레이드 가능한 후보를 랜덤으로 보여준다.
/// - 선택 시 해당 스킬 레벨을 1 증가시킨다.
/// - 창이 열려 있는 동안 전투 타이머를 정지한다.
/// </summary>
public class UIEquipmentUpgradeMono : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform listRoot;
    [SerializeField] private Button optionButtonPrefab;

    [Header("Text UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;

    [Header("Skill Upgrade")]
    [SerializeField] private int randomOptionCount = 3;
    [SerializeField] private int maxSkillLevel = 10;
    [SerializeField] private bool pauseTimeOnOpen = true;

    private readonly List<Button> spawnedButtons = new();
    private readonly List<SkillUpgradeOption> options = new();

    private bool isOpen;
    private float previousTimeScale = 1f;
    private readonly EquipmentStatResolver statResolver = new();

    public bool IsOpen => isOpen;

    private void Awake()
    {
        ResolveReferences();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        RestoreTimeScaleIfNeeded();
        ClearOptions();
        isOpen = false;
    }

    public void Open()
    {
        Open(CollectPlayerCharacterManagers());
    }

    public void Open(
        IReadOnlyList<CharacterManager> characterManagers)
    {
        ResolveReferences();
        ClearOptions();
        BuildRandomOptions(characterManagers);

        isOpen = true;
        gameObject.SetActive(true);

        PauseTimeIfNeeded();
        Refresh();
    }

    private IReadOnlyList<CharacterManager> CollectPlayerCharacterManagers()
    {
        List<CharacterManager> result = new();

        CharacterManager[] characterManagers =
            FindObjectsOfType<CharacterManager>(true);

        for (int i = 0; i < characterManagers.Length; i++)
        {
            CharacterManager characterManager = characterManagers[i];
            CharacterRuntimeData runtimeData = characterManager?.RuntimeData;

            if (runtimeData == null || runtimeData.characterSO == null)
            {
                continue;
            }

            if (runtimeData.characterSO.CharacterType != CharacterType.Player)
            {
                continue;
            }

            result.Add(characterManager);
        }

        return result;
    }

    public void Close()
    {
        if (!isOpen)
        {
            gameObject.SetActive(false);
            return;
        }

        isOpen = false;
        RestoreTimeScaleIfNeeded();
        ClearOptions();
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        ClearSpawnedButtons();

        SetTitle("스킬 업그레이드 선택");

        if (options.Count == 0)
        {
            SetStatus("업그레이드 가능한 스킬이 없습니다.");
            return;
        }

        SetStatus("업그레이드할 스킬을 선택하세요.");

        for (int i = 0; i < options.Count; i++)
        {
            CreateOptionButton(options[i], i);
        }
    }

    private void BuildRandomOptions(
        IReadOnlyList<CharacterManager> characterManagers)
    {
        List<SkillUpgradeOption> candidates = new();

        if (characterManagers != null)
        {
            for (int characterIndex = 0; characterIndex < characterManagers.Count; characterIndex++)
            {
                CharacterManager characterManager = characterManagers[characterIndex];
                CharacterRuntimeData characterRuntimeData = characterManager?.RuntimeData;

                if (characterManager == null || characterRuntimeData == null || characterRuntimeData.skillInstances == null)
                {
                    continue;
                }

                CharacterSkillManager skillManager =
                    characterManager.GetComponent<CharacterSkillManager>()
                    ?? characterManager.GetComponentInChildren<CharacterSkillManager>();

                if (skillManager == null)
                {
                    continue;
                }

                for (int skillIndex = 0; skillIndex < characterRuntimeData.skillInstances.Count; skillIndex++)
                {
                    EquipmentSkillInstanceData skillInstance = characterRuntimeData.skillInstances[skillIndex];
                    if (!CanUpgrade(skillInstance))
                    {
                        continue;
                    }

                    EquipmentSkillSO skillSo = ResolveSkillSo(skillInstance.equipmentId);

                    candidates.Add(
                        new SkillUpgradeOption(
                            characterRuntimeData,
                            skillInstance,
                            skillSo,
                            skillManager));
                }
            }
        }

        int optionCount = Mathf.Min(
            Mathf.Max(1, randomOptionCount),
            candidates.Count);

        for (int i = 0; i < optionCount; i++)
        {
            int selectedIndex = Random.Range(i, candidates.Count);
            (candidates[i], candidates[selectedIndex]) =
                (candidates[selectedIndex], candidates[i]);

            options.Add(candidates[i]);
        }
    }

    private bool CanUpgrade(EquipmentSkillInstanceData skillInstance)
    {
        if (skillInstance == null || string.IsNullOrWhiteSpace(skillInstance.equipmentId))
        {
            return false;
        }

        return Mathf.Max(1, skillInstance.currentLevel) < maxSkillLevel;
    }

    private void CreateOptionButton(
        SkillUpgradeOption option,
        int index)
    {
        if (option == null || listRoot == null || optionButtonPrefab == null)
        {
            return;
        }

        Button button = Instantiate(optionButtonPrefab, listRoot);

        LayoutElement layoutElement =
            button.GetComponent<LayoutElement>();

        if (layoutElement == null)
        {
            layoutElement = button.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.preferredWidth = 300f;
        layoutElement.minWidth = 300f;
        layoutElement.preferredHeight = 180f;
        layoutElement.minHeight = 180f;

        button.gameObject.SetActive(true);
        spawnedButtons.Add(button);

        // Set the skill icon if possible.
        Image iconImage =
            button.transform.Find("Icon")?.GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.sprite = option.SkillSo?.Icon;
            iconImage.enabled = iconImage.sprite != null;
        }

        if (iconImage == null)
        {
            Image[] images = button.GetComponentsInChildren<Image>(true);

            for (int imageIndex = 0; imageIndex < images.Length; imageIndex++)
            {
                if (images[imageIndex].gameObject == button.gameObject)
                {
                    continue;
                }

                images[imageIndex].sprite = option.SkillSo?.Icon;
                images[imageIndex].enabled = images[imageIndex].sprite != null;
                break;
            }
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = BuildOptionLabel(option, index);
        }

        button.onClick.AddListener(() => ApplyUpgrade(option));
    }

    private string BuildOptionLabel(
        SkillUpgradeOption option,
        int index)
    {
        string characterName = option.CharacterRuntimeData?.characterSO != null
            ? option.CharacterRuntimeData.characterSO.DisplayName
            : "Character";

        string skillName = option.SkillSo != null
            ? option.SkillSo.DisplayName
            : option.SkillInstance.equipmentId;

        int currentLevel = Mathf.Max(1, option.SkillInstance.currentLevel);
        int nextLevel = Mathf.Min(maxSkillLevel, currentLevel + 1);

        StringBuilder builder = new();
        builder.AppendLine($"{index + 1}. {characterName}");
        builder.AppendLine(skillName);
        builder.AppendLine($"Lv.{currentLevel} → Lv.{nextLevel}");

        string comparisonText = BuildUpgradeComparisonText(
            option,
            currentLevel,
            nextLevel);

        if (!string.IsNullOrWhiteSpace(comparisonText))
        {
            builder.Append(comparisonText);
        }

        return builder.ToString();
    }

    private string BuildUpgradeComparisonText(
        SkillUpgradeOption option,
        int currentLevel,
        int nextLevel)
    {
        if (option == null || option.SkillSo == null || option.SkillSo.UpgradeTableSo == null)
        {
            return string.Empty;
        }

        List<SkillStatModifierData> currentModifiers =
            EquipmentUpgradeRuntimeData.FromEntries(
                currentLevel,
                option.SkillSo.UpgradeTableSo.Entries,
                option.SkillSo.EquipmentId).statModifiers;

        List<SkillStatModifierData> nextModifiers =
            EquipmentUpgradeRuntimeData.FromEntries(
                nextLevel,
                option.SkillSo.UpgradeTableSo.Entries,
                option.SkillSo.EquipmentId).statModifiers;

        List<EffectUpgradeModifierData> currentEffectModifiers =
            EquipmentUpgradeRuntimeData.FromEntries(
                currentLevel,
                option.SkillSo.UpgradeTableSo.Entries,
                option.SkillSo.EquipmentId).effectModifiers;

        List<EffectUpgradeModifierData> nextEffectModifiers =
            EquipmentUpgradeRuntimeData.FromEntries(
                nextLevel,
                option.SkillSo.UpgradeTableSo.Entries,
                option.SkillSo.EquipmentId).effectModifiers;

        List<SkillStatModifierType> changedTypes = CollectChangedModifierTypes(
            option.SkillSo,
            currentModifiers,
            nextModifiers);

        StringBuilder builder = new();

        for (int i = 0; i < changedTypes.Count; i++)
        {
            SkillStatModifierType modifierType = changedTypes[i];
            float currentValue = statResolver.ResolveStat(
                option.SkillSo,
                modifierType,
                currentModifiers);
            float nextValue = statResolver.ResolveStat(
                option.SkillSo,
                modifierType,
                nextModifiers);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                continue;
            }

            builder.AppendLine(
                $"{GetModifierDisplayName(modifierType)} {FormatModifierValue(modifierType, currentValue)} → {FormatModifierValue(modifierType, nextValue)}");
        }

        string effectComparisonText = BuildEffectUpgradeComparisonText(
            option.SkillSo,
            currentEffectModifiers,
            nextEffectModifiers);

        if (!string.IsNullOrWhiteSpace(effectComparisonText))
        {
            builder.Append(effectComparisonText);
        }

        return builder.ToString();
    }

    private string BuildEffectUpgradeComparisonText(
        EquipmentSkillSO skillSo,
        IReadOnlyList<EffectUpgradeModifierData> currentModifiers,
        IReadOnlyList<EffectUpgradeModifierData> nextModifiers)
    {
        List<EffectUpgradeModifierKey> changedKeys = CollectChangedEffectModifierKeys(
            skillSo,
            currentModifiers,
            nextModifiers);

        if (changedKeys.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();

        for (int i = 0; i < changedKeys.Count; i++)
        {
            EffectUpgradeModifierKey key = changedKeys[i];
            float currentValue = ResolveEffectModifierValue(
                skillSo,
                key,
                currentModifiers);
            float nextValue = ResolveEffectModifierValue(
                skillSo,
                key,
                nextModifiers);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                continue;
            }

            builder.AppendLine(
                $"{GetEffectModifierDisplayName(key)} {FormatEffectModifierValue(key.fieldType, currentValue)} → {FormatEffectModifierValue(key.fieldType, nextValue)}");
        }

        return builder.ToString();
    }

    private List<EffectUpgradeModifierKey> CollectChangedEffectModifierKeys(
        EquipmentSkillSO skillSo,
        IReadOnlyList<EffectUpgradeModifierData> currentModifiers,
        IReadOnlyList<EffectUpgradeModifierData> nextModifiers)
    {
        List<EffectUpgradeModifierKey> result = new();
        AddEffectModifierKeys(currentModifiers, result);
        AddEffectModifierKeys(nextModifiers, result);

        for (int i = result.Count - 1; i >= 0; i--)
        {
            EffectUpgradeModifierKey key = result[i];
            float currentValue = ResolveEffectModifierValue(
                skillSo,
                key,
                currentModifiers);
            float nextValue = ResolveEffectModifierValue(
                skillSo,
                key,
                nextModifiers);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                result.RemoveAt(i);
            }
        }

        return result;
    }

    private void AddEffectModifierKeys(
        IReadOnlyList<EffectUpgradeModifierData> modifiers,
        List<EffectUpgradeModifierKey> result)
    {
        if (modifiers == null || result == null)
        {
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            EffectUpgradeModifierData modifier = modifiers[i];
            if (modifier == null || string.IsNullOrWhiteSpace(modifier.effectId))
            {
                continue;
            }

            EffectUpgradeModifierKey key = new EffectUpgradeModifierKey(
                modifier.effectId,
                modifier.fieldType);

            if (!ContainsEffectModifierKey(result, key))
            {
                result.Add(key);
            }
        }
    }

    private bool ContainsEffectModifierKey(
        List<EffectUpgradeModifierKey> keys,
        EffectUpgradeModifierKey target)
    {
        if (keys == null)
        {
            return false;
        }

        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].Equals(target))
            {
                return true;
            }
        }

        return false;
    }

    private float ResolveEffectModifierValue(
        EquipmentSkillSO skillSo,
        EffectUpgradeModifierKey key,
        IReadOnlyList<EffectUpgradeModifierData> modifiers)
    {
        float value = ResolveEffectBaseValue(
            key.effectId,
            key.fieldType);

        if (modifiers == null)
        {
            return value;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            EffectUpgradeModifierData modifier = modifiers[i];
            if (modifier == null ||
                modifier.effectId != key.effectId ||
                modifier.fieldType != key.fieldType)
            {
                continue;
            }

            value = ApplyEffectModifierValue(
                value,
                modifier);
        }

        return value;
    }

    private float ResolveEffectBaseValue(
        string effectId,
        EffectModifierFieldType fieldType)
    {
        if (string.IsNullOrWhiteSpace(effectId))
        {
            return 0f;
        }

        EffectSO effectSo = ResolveEffectSo(effectId);
        if (effectSo == null)
        {
            return 0f;
        }

        switch (fieldType)
        {
            case EffectModifierFieldType.Value:
                return ResolveEffectValue(effectSo);
            default:
                return 0f;
        }
    }

    private float ResolveEffectValue(EffectSO effectSo)
    {
        switch (effectSo)
        {
            case StatModifierEffectSO statModifierEffectSo:
                return statModifierEffectSo.value;
            default:
                return 0f;
        }
    }

    private EffectSO ResolveEffectSo(string effectId)
    {
        if (string.IsNullOrWhiteSpace(effectId))
        {
            return null;
        }

        EffectSO[] effects = Resources.LoadAll<EffectSO>(string.Empty);
        for (int i = 0; i < effects.Length; i++)
        {
            EffectSO effectSo = effects[i];
            if (effectSo != null && effectSo.effectId == effectId)
            {
                return effectSo;
            }
        }

        return null;
    }

    private float ApplyEffectModifierValue(
        float currentValue,
        EffectUpgradeModifierData modifier)
    {
        switch (modifier.operationType)
        {
            case SkillStatModifierOperationType.Flat:
                return currentValue + modifier.value;
            case SkillStatModifierOperationType.Percent:
                return currentValue * (1f + modifier.value);
            case SkillStatModifierOperationType.Override:
                return modifier.value;
            default:
                return currentValue;
        }
    }

    private string GetEffectModifierDisplayName(
        EffectUpgradeModifierKey key)
    {
        string effectName = StringManager.Instance.Get(
            key.effectId,
            "name");

        if (string.IsNullOrWhiteSpace(effectName))
        {
            effectName = key.effectId;
        }

        switch (key.fieldType)
        {
            case EffectModifierFieldType.Duration:
                return $"{effectName} 지속시간";
            case EffectModifierFieldType.MaxApplyCount:
                return $"{effectName} 적용 횟수";
            default:
                return effectName;
        }
    }

    private string FormatEffectModifierValue(
        EffectModifierFieldType fieldType,
        float value)
    {
        switch (fieldType)
        {
            case EffectModifierFieldType.Duration:
            case EffectModifierFieldType.Cooldown:
            case EffectModifierFieldType.TickInterval:
                return $"{value:0.##}초";
            case EffectModifierFieldType.Chance:
                return $"{Mathf.RoundToInt(value * 100f)}%";
            case EffectModifierFieldType.MaxApplyCount:
                return Mathf.RoundToInt(value).ToString();
            default:
                return value.ToString("0.##");
        }
    }

    private List<SkillStatModifierType> CollectChangedModifierTypes(
        EquipmentSkillSO skillSo,
        IReadOnlyList<SkillStatModifierData> currentModifiers,
        IReadOnlyList<SkillStatModifierData> nextModifiers)
    {
        List<SkillStatModifierType> result = new();
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.BaseDamage, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.AttackPercentDamage, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.Cooldown, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.Range, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.SplitHitCount, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.ProjectileCount, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.ProjectileScale, currentModifiers, nextModifiers, result);
        return result;
    }

    private void AddModifierTypeIfChanged(
        EquipmentSkillSO skillSo,
        SkillStatModifierType modifierType,
        IReadOnlyList<SkillStatModifierData> currentModifiers,
        IReadOnlyList<SkillStatModifierData> nextModifiers,
        List<SkillStatModifierType> result)
    {
        float currentValue = statResolver.ResolveStat(
            skillSo,
            modifierType,
            currentModifiers);
        float nextValue = statResolver.ResolveStat(
            skillSo,
            modifierType,
            nextModifiers);

        if (!Mathf.Approximately(currentValue, nextValue))
        {
            result.Add(modifierType);
        }
    }

    private string GetModifierDisplayName(SkillStatModifierType modifierType)
    {
        return StringManager.Instance.Get(
            $"enum.{nameof(SkillStatModifierType)}.{modifierType}",
            "name");
    }

    private string FormatModifierValue(
        SkillStatModifierType modifierType,
        float value)
    {
        switch (modifierType)
        {
            case SkillStatModifierType.AttackPercentDamage:
                return $"{Mathf.RoundToInt(value * 100f)}%";
            case SkillStatModifierType.Cooldown:
                return $"{value:0.##}초";
            case SkillStatModifierType.Range:
            case SkillStatModifierType.ProjectileScale:
                return value.ToString("0.##");
            case SkillStatModifierType.SplitHitCount:
            case SkillStatModifierType.ProjectileCount:
                return Mathf.RoundToInt(value).ToString();
            default:
                return value.ToString("0.##");
        }
    }

    private EquipmentSkillSO ResolveSkillSo(string equipmentId)
    {
        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            return null;
        }

        EquipmentSkillSO[] skills = Resources.LoadAll<EquipmentSkillSO>(string.Empty);
        for (int i = 0; i < skills.Length; i++)
        {
            EquipmentSkillSO skill = skills[i];
            if (skill != null && skill.EquipmentId == equipmentId)
            {
                return skill;
            }
        }

        return null;
    }

    private void ApplyUpgrade(SkillUpgradeOption option)
    {
        if (option == null || option.CharacterRuntimeData == null || option.SkillInstance == null)
        {
            return;
        }

        int currentLevel = Mathf.Max(1, option.SkillInstance.currentLevel);
        int nextLevel = Mathf.Min(maxSkillLevel, currentLevel + 1);

        bool upgraded = option.SkillManager.TryUpgradeSkill(
            option.SkillInstance,
            maxSkillLevel);

        if (!upgraded)
        {
            SetStatus($"업그레이드 실패: {option.SkillInstance.equipmentId}");
            return;
        }

        SetStatus($"{option.SkillInstance.equipmentId} Lv.{currentLevel} → Lv.{nextLevel}");
        Close();
    }

    private void ResolveReferences()
    {
        if (listRoot == null)
        {
            listRoot = transform.Find("ListRoot");
        }

        if (listRoot != null)
        {
            VerticalLayoutGroup verticalLayout =
                listRoot.GetComponent<VerticalLayoutGroup>();

            if (verticalLayout != null)
            {
                Destroy(verticalLayout);
            }

            HorizontalLayoutGroup layoutGroup =
                listRoot.GetComponent<HorizontalLayoutGroup>();

            if (layoutGroup == null)
            {
                layoutGroup = listRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layoutGroup.spacing = 30f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        }

        if (optionButtonPrefab == null)
        {
            optionButtonPrefab = Resources.Load<Button>("UI/ItemButtonPrefab");
        }

        if (titleText == null)
        {
            titleText = transform.Find("TitleText")?.GetComponent<TMP_Text>();
        }

        if (statusText == null)
        {
            statusText = transform.Find("StatusText")?.GetComponent<TMP_Text>();
        }
    }

    private void PauseTimeIfNeeded()
    {
        if (!pauseTimeOnOpen)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    private void RestoreTimeScaleIfNeeded()
    {
        if (!pauseTimeOnOpen)
        {
            return;
        }

        Time.timeScale = previousTimeScale <= 0f
            ? 1f
            : previousTimeScale;
    }

    private void ClearOptions()
    {
        options.Clear();
        ClearSpawnedButtons();
    }

    private void ClearSpawnedButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();
    }

    private void SetTitle(string message)
    {
        if (titleText != null)
        {
            titleText.text = message;
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log($"[SkillUpgradeUI] {message}", this);
    }

    private readonly struct EffectUpgradeModifierKey
    {
        public EffectUpgradeModifierKey(
            string effectId,
            EffectModifierFieldType fieldType)
        {
            this.effectId = effectId;
            this.fieldType = fieldType;
        }

        public readonly string effectId;
        public readonly EffectModifierFieldType fieldType;

        public bool Equals(EffectUpgradeModifierKey other)
        {
            return effectId == other.effectId &&
                   fieldType == other.fieldType;
        }
    }

    private class SkillUpgradeOption
    {
        public SkillUpgradeOption(
            CharacterRuntimeData characterRuntimeData,
            EquipmentSkillInstanceData skillInstance,
            EquipmentSkillSO skillSo,
            CharacterSkillManager skillManager)
        {
            CharacterRuntimeData = characterRuntimeData;
            SkillInstance = skillInstance;
            SkillSo = skillSo;
            SkillManager = skillManager;
        }

        public CharacterRuntimeData CharacterRuntimeData { get; }
        public EquipmentSkillInstanceData SkillInstance { get; }
        public EquipmentSkillSO SkillSo { get; }
        public CharacterSkillManager SkillManager { get; }
    }
}