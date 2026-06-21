using System.Collections.Generic;
using System.Text;
using Character;
using Skill;
using TMPro;
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

    public void Open(
        IReadOnlyList<CharacterRuntimeData> characterRuntimeDatas)
    {
        ResolveReferences();
        ClearOptions();
        BuildRandomOptions(characterRuntimeDatas);

        isOpen = true;
        gameObject.SetActive(true);

        PauseTimeIfNeeded();
        Refresh();
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
        IReadOnlyList<CharacterRuntimeData> characterRuntimeDatas)
    {
        List<SkillUpgradeOption> candidates = new();

        if (characterRuntimeDatas != null)
        {
            for (int characterIndex = 0; characterIndex < characterRuntimeDatas.Count; characterIndex++)
            {
                CharacterRuntimeData characterRuntimeData = characterRuntimeDatas[characterIndex];
                if (characterRuntimeData == null || characterRuntimeData.skillInstances == null)
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
                            skillSo));
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
            ? option.CharacterRuntimeData.characterSO.name
            : "Character";

        string skillName = option.SkillSo != null
            ? option.SkillSo.name
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

        List<SkillStatModifierRuntimeData> currentModifiers =
            EquipmentUpgradeRuntimeData.FromEntries(
                currentLevel,
                option.SkillSo.UpgradeTableSo.Entries,
                option.SkillSo.EquipmentId).statModifiers;

        List<SkillStatModifierRuntimeData> nextModifiers =
            EquipmentUpgradeRuntimeData.FromEntries(
                nextLevel,
                option.SkillSo.UpgradeTableSo.Entries,
                option.SkillSo.EquipmentId).statModifiers;

        List<SkillStatModifierType> changedTypes = CollectChangedModifierTypes(
            option.SkillSo,
            currentModifiers,
            nextModifiers);

        if (changedTypes.Count == 0)
        {
            return string.Empty;
        }

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

        return builder.ToString();
    }

    private List<SkillStatModifierType> CollectChangedModifierTypes(
        EquipmentSkillSO skillSo,
        IReadOnlyList<SkillStatModifierRuntimeData> currentModifiers,
        IReadOnlyList<SkillStatModifierRuntimeData> nextModifiers)
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
        IReadOnlyList<SkillStatModifierRuntimeData> currentModifiers,
        IReadOnlyList<SkillStatModifierRuntimeData> nextModifiers,
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
        switch (modifierType)
        {
            case SkillStatModifierType.BaseDamage:
                return "데미지";
            case SkillStatModifierType.AttackPercentDamage:
                return "공격력 퍼센트 데미지";
            case SkillStatModifierType.Cooldown:
                return "쿨타임";
            case SkillStatModifierType.Range:
                return "사정거리";
            case SkillStatModifierType.SplitHitCount:
                return "타격 수";
            case SkillStatModifierType.ProjectileCount:
                return "투사체 수";
            case SkillStatModifierType.ProjectileScale:
                return "범위 크기";
            default:
                return modifierType.ToString();
        }
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

        option.CharacterRuntimeData.SetSkillLevel(
            option.SkillInstance.equipmentId,
            nextLevel);

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

    private class SkillUpgradeOption
    {
        public SkillUpgradeOption(
            CharacterRuntimeData characterRuntimeData,
            EquipmentSkillInstanceData skillInstance,
            EquipmentSkillSO skillSo)
        {
            CharacterRuntimeData = characterRuntimeData;
            SkillInstance = skillInstance;
            SkillSo = skillSo;
        }

        public CharacterRuntimeData CharacterRuntimeData { get; }
        public EquipmentSkillInstanceData SkillInstance { get; }
        public EquipmentSkillSO SkillSo { get; }
    }
}