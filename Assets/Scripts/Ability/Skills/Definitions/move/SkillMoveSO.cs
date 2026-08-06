using UnityEngine;
using Skills.Move.Config;
using Skill;

[CreateAssetMenu(
    fileName = "SkillMove",
    menuName = "BS/Skills/Move/Skill Move SO",
    order = 15)]
public class SkillMoveSO : ScriptableObject
{
    [Header("Move")]
    [SerializeField] private string moveId;
    [Header("Rotation")]
    [SerializeField] private bool applyDirectionRotation = true;
    [SerializeField] private float rotationOffset;
    [Header("Move Type")]
    [SerializeField] private ProjectileMoveType moveType = ProjectileMoveType.Linear;
    [Header("Config")]
    [SerializeReference] private SkillMoveConfig config;
    public string MoveId => moveId;
    public ProjectileMoveType MoveType => moveType;
    public SkillMoveConfig Config => config;
    public bool ApplyDirectionRotation => applyDirectionRotation;
    public float RotationOffset => rotationOffset;

#if UNITY_EDITOR
    public void ApplyEditorData(
        string moveId,
        ProjectileMoveType moveType,
        bool applyDirectionRotation,
        float rotationOffset)
    {
        this.moveId = moveId;
        this.moveType = moveType;
        this.applyDirectionRotation = applyDirectionRotation;
        this.rotationOffset = rotationOffset;
    }

    public void ApplyEditorConfig(
        SkillMoveConfig config)
    {
        this.config = config;
    }
#endif
}