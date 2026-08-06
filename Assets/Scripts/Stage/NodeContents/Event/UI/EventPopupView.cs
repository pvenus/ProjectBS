using System;
using System.Collections.Generic;
using TMPro;
using UIFramework.Data;
using UnityEngine;
using UnityEngine.UI;

[AutoBindPrefix("Bind")]
public class EventPopupView : UIView
{
    [Header("UI Components")]
    [AutoBind] [SerializeField] private GameObject panelRoot;
    [AutoBind] [SerializeField] private Image eventImage;
    [AutoBind] [SerializeField] private TMP_Text titleText;
    [AutoBind] [SerializeField] private TMP_Text bodyText;
    [AutoBind] [SerializeField] private TMP_Text resultText;
    
    [Header("Choices")]
    [AutoBind] [SerializeField] private Transform choiceContainer;
    [SerializeField] private UITextButton choiceButtonPrefab;

    public event Action<string> OnChoiceSelected;
    public event Action OnChoiceConfirmed;

    private readonly List<UITextButton> spawnedButtons = new();

    public void SetData(EventPopupViewData data)
    {
        ClearCallbacks();

        if (titleText != null) titleText.text = data.Title;
        if (bodyText != null) bodyText.text = data.Description;
        
        if (eventImage != null)
        {
            if (data.Illustration != null)
            {
                eventImage.sprite = data.Illustration;
                eventImage.gameObject.SetActive(true);
            }
            else
            {
                eventImage.gameObject.SetActive(false);
            }
        }

        // Hide result text by default
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        ClearChoices();

        if (choiceButtonPrefab != null && choiceContainer != null && data.Choices != null)
        {
            for (int i = 0; i < data.Choices.Count; i++)
            {
                var choice = data.Choices[i];
                UITextButton btn = Instantiate(choiceButtonPrefab, choiceContainer);
                btn.Bind(
                    choice.Text,
                    () => OnChoiceSelectedInternal(choice));
                btn.gameObject.SetActive(true);
                spawnedButtons.Add(btn);
            }
        }
    }

    private void OnChoiceSelectedInternal(EventChoiceViewData choice)
    {
        // 콜백에서 다음 Popup이 동기적으로 열릴 수 있으므로
        // 현재 선택지 정리는 Manager 호출보다 먼저 끝낸다.
        ClearChoices();
        OnChoiceSelected?.Invoke(choice.Id);

        if (choice.HasResult)
        {
            // 뷰 내부적으로 결과 화면으로 즉시 전환
            if (bodyText != null)
            {
                bodyText.text = choice.ResultText;
            }

            // "확인" 버튼을 한 개 추가하여, 이것을 누를 때 비로소 매니저로 이벤트를 최종 발행함
            if (choiceButtonPrefab != null && choiceContainer != null)
            {
                UITextButton btn = Instantiate(choiceButtonPrefab, choiceContainer);
                btn.Bind(
                    "확인",
                    () => OnChoiceConfirmed?.Invoke());
                btn.gameObject.SetActive(true);
                spawnedButtons.Add(btn);
            }
        }
        else
        {
            OnChoiceConfirmed?.Invoke();
        }
    }

    public override void Show()
    {
        gameObject.SetActive(true);
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
        else
        {
            base.Show();
        }
    }

    public override void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        base.Hide();
    }

    public override void Clear()
    {
        ClearChoices();
    }

    public override void ClearCallbacks()
    {
        OnChoiceSelected = null;
        OnChoiceConfirmed = null;
    }

    private void ClearChoices()
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        spawnedButtons.Clear();

        if (choiceContainer != null)
        {
            for (int i = choiceContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(choiceContainer.GetChild(i).gameObject);
            }
        }
    }
}
