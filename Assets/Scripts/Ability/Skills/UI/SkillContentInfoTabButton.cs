using System;
using Skill;
using UnityEngine;
using UnityEngine.UI;

public sealed class SkillContentInfoTabButton : UIComponent
{
    [Header("References")]
    [AutoBind("UISkillIconSlot")]
    [SerializeField] private Button button;

    [AutoBind("Bind_SkillIconImage")]
    [SerializeField] private Image skillIconImage;

    [SerializeField] private GameObject selectedVisual;

    private Action onClick;

    public EquipmentSkillSO Skill { get; private set; }

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError(
                "[SkillContentInfoTabButton] Button is not assigned.",
                this);
            return;
        }

        button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    public void Bind(EquipmentSkillSO skill, Action clickAction)
    {
        Skill = skill;
        onClick = clickAction;

        if (skillIconImage != null)
        {
            skillIconImage.sprite = skill != null ? skill.Icon : null;
            skillIconImage.enabled = skillIconImage.sprite != null;
        }
        else
        {
            Debug.LogWarning(
                "[SkillContentInfoTabButton] Skill icon Image is not assigned.",
                this);
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedVisual != null)
        {
            selectedVisual.SetActive(selected);
        }

        if (button != null)
        {
            button.interactable = !selected;
        }
    }

    private void HandleClick()
    {
        onClick?.Invoke();
    }
}
