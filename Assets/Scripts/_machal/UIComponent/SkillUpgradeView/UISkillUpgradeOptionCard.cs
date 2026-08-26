using System;
using Presentation;
using TMPro;
using UI;
using UIFramework.Data;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 업그레이드 선택지 카드
/// SkillUpgradeView 내에서 Instantiate되며, 하나의 업그레이드 옵션을 표시한다.
/// UIContentInfoView가 스킬의 아이콘, 이름, 설명, 태그를 표시한다.
/// Body는 공용 상세 그룹 대신 업그레이드 수치 비교만 표시한다.
/// </summary>
[AutoBindPrefix("Bind")]
public class UISkillUpgradeButton : UIComponent
{
    [Header("Button")]
    [AutoBind] [SerializeField] private Button button;

    [Header("Visual")]
    [AutoBind] [SerializeField] private Image backgroundImage;
    [AutoBind] [SerializeField] private UIContentInfoView contentInfoView;

    [Header("Character")]
    [AutoBind] [SerializeField] private Image characterPortraitImage;
    [AutoBind] [SerializeField] private TMP_Text memberNameText;

    [Header("Skill")]
    [AutoBind] [SerializeField] private TMP_Text levelText;
    [AutoBind] [SerializeField] private TMP_Text statComparisonText;

    // ── 런타임 상태 ──────────────────────────────────────────────
    private Action onClickCallback;

    // ── 생명주기 ─────────────────────────────────────────────────
    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClicked);
    }

    // ── 공개 API ─────────────────────────────────────────────────

    /// <summary>
    /// 카드에 데이터를 주입하고 클릭 콜백을 등록한다.
    /// </summary>
    public void Bind(SkillUpgradeOptionData data, Action onClick)
    {
        onClickCallback = onClick;

        SetCharacterPortrait(data?.characterPortrait);
        SetCharacterName(data?.characterName ?? string.Empty);
        SetLevelText(data?.currentLevel ?? 1, data?.nextLevel ?? 2);
        SetStatComparison(data?.statComparisonText ?? string.Empty);
        SetContent(data?.content);
    }

    // ── 내부 구현 ─────────────────────────────────────────────────

    private void SetCharacterPortrait(Sprite portrait)
    {
        if (characterPortraitImage == null)
        {
            return;
        }

        characterPortraitImage.sprite = portrait;
        characterPortraitImage.preserveAspect = true;
        characterPortraitImage.enabled = portrait != null;
    }

    private void SetCharacterName(string characterName)
    {
        if (memberNameText != null)
			memberNameText.text = characterName;
    }

    private void SetContent(ContentPresentationData content)
    {
        if (contentInfoView == null)
            return;

        contentInfoView.gameObject.SetActive(true);
        contentInfoView.SetFormatter(
            PresentationTextFormatter.CreatePlayerFormatter(
                PresentationLocalizedTextResolver.ResolveLabel));
        contentInfoView.Bind(content);
    }

    private void SetLevelText(int current, int next)
    {
        if (levelText != null)
            levelText.text = $"Lv.{current} → Lv.{next}";
    }

    private void SetStatComparison(string comparisonText)
    {
        if (statComparisonText == null)
            return;

        statComparisonText.text = comparisonText;
        statComparisonText.gameObject.SetActive(!string.IsNullOrWhiteSpace(comparisonText));
    }

    private void OnClicked()
    {
        onClickCallback?.Invoke();
    }
}
