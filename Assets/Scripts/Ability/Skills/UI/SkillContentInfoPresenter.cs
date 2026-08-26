using Presentation;
using Skill;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class SkillContentInfoPresenter : UIComponent
{
    [Header("View")]
    [AutoBind("UIContentInfoView")]
    [SerializeField] private UIContentInfoView contentView;

    [Header("Skill")]
    [SerializeField] private EquipmentSkillSO skill;
    [SerializeField] private bool useRuntimeValues;
    [SerializeField, Min(1)] private int currentLevel = 1;
    [SerializeField, Min(0)] private int upgradeLevel;

    [ContextMenu("Build Presentation")]
    public void BuildPresentation()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[SkillContentInfoPresenter] Enter Play Mode before building presentation data.",
                this);
            return;
        }

        if (contentView == null)
        {
            Debug.LogError(
                "[SkillContentInfoPresenter] UIContentInfoView is not assigned.",
                this);
            return;
        }

        if (skill == null)
        {
            Debug.LogError(
                "[SkillContentInfoPresenter] EquipmentSkillSO is not assigned.",
                this);
            return;
        }

        if (EventSystem.current == null)
        {
            Debug.LogWarning(
                "[SkillContentInfoPresenter] No active EventSystem was found. " +
                "The content can be displayed, but ScrollRect input will not work.",
                this);
        }

        SkillPresentationResolver resolver = new();
        SkillPresentationData presentation = useRuntimeValues
            ? ResolveRuntime(resolver)
            : resolver.Resolve(skill, PresentationContext.Preview);

        ContentPresentationData content =
            new SkillPresentationGroupResolver().ResolveForPlayerDisplay(presentation);
        contentView.SetFormatter(
            PresentationTextFormatter.CreatePlayerFormatter(
                PresentationLocalizedTextResolver.ResolveLabel));
        contentView.Bind(content);
    }

    public void SetSkill(EquipmentSkillSO value)
    {
        skill = value;
    }

    public void ShowSkill(EquipmentSkillSO value)
    {
        ShowSkill(value, null);
    }

    public void ShowSkill(EquipmentSkillSO value, EquipmentSkillInstanceData instanceData)
    {
        skill = value;

        if (instanceData != null)
        {
            useRuntimeValues = true;
            currentLevel = Mathf.Max(1, instanceData.currentLevel);
            upgradeLevel = Mathf.Max(0, instanceData.upgradeLevel);
        }
        else
        {
            useRuntimeValues = false;
        }

        if (skill == null)
        {
            ClearPresentation();
            return;
        }

        BuildPresentation();
    }

    public void ClearPresentation()
    {
        if (contentView == null)
        {
            return;
        }

        contentView.Bind(null);
    }

    private SkillPresentationData ResolveRuntime(SkillPresentationResolver resolver)
    {
        EquipmentSkillRuntimeData runtime = new EquipmentSkillResolver().Resolve(
            skill,
            new EquipmentSkillInstanceData
            {
                equipmentId = skill.EquipmentId,
                currentLevel = Mathf.Max(1, currentLevel),
                upgradeLevel = Mathf.Max(0, upgradeLevel),
            });

        return resolver.Resolve(runtime, PresentationContext.Runtime);
    }

}
