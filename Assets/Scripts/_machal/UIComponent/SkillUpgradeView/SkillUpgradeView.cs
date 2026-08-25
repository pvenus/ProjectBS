using System;
using System.Collections.Generic;
using TMPro;
using UIFramework.Data;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 중 스킬 업그레이드 선택 UI — View 레이어.
/// Passive View 패턴 준수:
///  - 데이터는 SetData() 를 통해 외부에서 주입받는다.
///  - 선택 결과는 OnOptionClicked 이벤트를 통해 단순 index로 전달한다.
///  - 비즈니스 로직은 포함하지 않는다.
/// </summary>
[AutoBindPrefix("Bind")]
public class SkillUpgradeView : UIView
{
    [Header("Panel")]
    [AutoBind] [SerializeField] private GameObject panelRoot;

    [Header("Header")]
    [AutoBind] [SerializeField] private TMP_Text titleText;
    [AutoBind] [SerializeField] private TMP_Text statusText;

    [Header("Option List")]
    [AutoBind] [SerializeField] private Transform optionContainer;
    [SerializeField] private UISkillUpgradeButton optionCardPrefab;

    [Header("Footer")]
    [AutoBind] [SerializeField] private Button closeButton;

    public event Action<int> OnOptionClicked;

    private readonly List<UISkillUpgradeButton> spawnedCards = new();

    // ── 생명주기 ─────────────────────────────────────────────────
    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    // ── 공개 API ─────────────────────────────────────────────────

    /// <summary>
    /// View 데이터를 설정한다.
    /// </summary>
    public void SetData(SkillUpgradeViewData data)
    {
        ClearCallbacks();

        SetTitle(data?.title ?? "스킬 업그레이드 선택");

        ClearCards();

        if (data == null || data.options == null || data.options.Count == 0)
        {
            SetStatus("업그레이드 가능한 스킬이 없습니다.");
        }
        else
        {
            SetStatus("업그레이드할 스킬을 선택하세요.");
            BuildOptionCards(data.options);
        }
    }

    public void SetCloseButtonVisible(bool visible)
    {
        if (closeButton != null)
            closeButton.gameObject.SetActive(visible);
    }

    public override void Show()
    {
        gameObject.SetActive(true);

        if (panelRoot != null)
            panelRoot.SetActive(true);
        else
            base.Show();
    }

    public override void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        
        base.Hide();
    }

    public override void Clear()
    {
        ClearCards();
    }

    public override void ClearCallbacks()
    {
        OnOptionClicked = null;
    }

    // ── 내부 구현 ─────────────────────────────────────────────────

    private void BuildOptionCards(IReadOnlyList<SkillUpgradeOptionData> options)
    {
        if (optionCardPrefab == null || optionContainer == null)
            return;

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            SkillUpgradeOptionData option = options[index];

            UISkillUpgradeButton card = Instantiate(optionCardPrefab, optionContainer);
            card.Bind(option, () => OnOptionClicked?.Invoke(index));
            card.gameObject.SetActive(true);
            spawnedCards.Add(card);
        }
    }

    private void OnCloseClicked()
    {
        Hide();
    }

    private void SetTitle(string message)
    {
        if (titleText != null)
            titleText.text = message;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void ClearCards()
    {
        for (int i = spawnedCards.Count - 1; i >= 0; i--)
        {
            if (spawnedCards[i] != null)
            {
                Destroy(spawnedCards[i].gameObject);
            }
        }
        spawnedCards.Clear();

        if (optionContainer != null)
        {
            for (int i = optionContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(optionContainer.GetChild(i).gameObject);
            }
        }
    }
}
