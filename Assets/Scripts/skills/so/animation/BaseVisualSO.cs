using UnityEngine;

/// <summary>
/// 무기/스킬의 속성 없는 순수 기본 비주얼을 정의하는 SO.
/// 형태(근거리/원거리/마법)와 기본 애니메이션, 기본 스프라이트를 보관한다.
/// 메인 속성 교체나 서브 속성 효과는 별도 라이브러리/리졸버가 담당한다.
/// </summary>
[CreateAssetMenu(fileName = "BaseVisualSO", menuName = "BS/Skills/Visual/BaseVisualSO")]
public class BaseVisualSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string visualId;
    [SerializeField] private AttackArchetype archetype = AttackArchetype.Melee;

    [Header("Base Sprite")]
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Animation Clips")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip castClip;
    [SerializeField] private AnimationClip attackClip;
    [SerializeField] private AnimationClip projectileLoopClip;
    [SerializeField] private AnimationClip hitClip;

    public string VisualId => visualId;
    public AttackArchetype Archetype => archetype;

    public Sprite BaseSprite => baseSprite;
    public RuntimeAnimatorController AnimatorController => animatorController;

    public AnimationClip IdleClip => idleClip;
    public AnimationClip CastClip => castClip;
    public AnimationClip AttackClip => attackClip;
    public AnimationClip ProjectileLoopClip => projectileLoopClip;
    public AnimationClip HitClip => hitClip;
}
