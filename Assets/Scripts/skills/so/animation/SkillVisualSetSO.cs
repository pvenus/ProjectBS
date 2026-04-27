

using UnityEngine;

/// <summary>
/// 스킬 비주얼 리소스를 묶어두는 최상위 허브 SO.
/// 실제 최종 비주얼 선택(무기 타입 + 등급 + 메인 속성 + 서브 속성)은
/// 별도 VisualResolver가 담당하고,
/// 이 SO는 필요한 라이브러리와 기본 프로필 참조만 보관한다.
/// </summary>
[CreateAssetMenu(fileName = "SkillVisualSetSO", menuName = "BS/Skills/Visual/SkillVisualSetSO")]
public class SkillVisualSetSO : ScriptableObject
{
    [Header("Base Visual")]
    [SerializeField] private BaseVisualSO baseVisualSo;

    [Header("Main Element Visual Library")]
    [SerializeField] private MainElementVisualLibrarySO mainElementVisualLibrarySo;

    [Header("Sub Element Effect Library")]
    [SerializeField] private SubElementEffectLibrarySO subElementEffectLibrarySo;

    [Header("Impact / State Visual")]
    [SerializeField] private ImpactVisualSO impactVisualSo;

    public BaseVisualSO BaseVisualSo => baseVisualSo;
    public MainElementVisualLibrarySO MainElementVisualLibrarySo => mainElementVisualLibrarySo;
    public SubElementEffectLibrarySO SubElementEffectLibrarySo => subElementEffectLibrarySo;
    public ImpactVisualSO ImpactVisualSo => impactVisualSo;
}