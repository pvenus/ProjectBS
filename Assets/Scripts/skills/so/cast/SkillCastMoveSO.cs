using UnityEngine;

namespace Skill
{
    /// <summary>
    /// 스킬 시전 시 시전자 본체에 적용되는 이동 정의.
    /// Projectile 이동과 분리된 개념이다.
    ///
    /// 현재는 DashForward 같은 타입 중심 데이터로 시작하고,
    /// 필요해질 때만 세부 변수를 추가한다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "skill.cast_move",
        menuName = "Skill/Cast Move")]
    public class SkillCastMoveSO : ScriptableObject
    {
        [SerializeField] private string castMoveId;
        [SerializeField] private CastMoveType moveType = CastMoveType.None;

        [Header("Basic Movement")]
        [SerializeField] private float distance = 0f;
        [SerializeField] private float duration = 0f;

        [Header("Options")]
        [SerializeField] private bool stopOnWall = true;
        [SerializeField] private bool ignoreDuringStun = false;

        public string CastMoveId => castMoveId;
        public CastMoveType MoveType => moveType;
        public float Distance => distance;
        public float Duration => duration;
        public bool StopOnWall => stopOnWall;
        public bool IgnoreDuringStun => ignoreDuringStun;
        public bool HasMove => moveType != CastMoveType.None && distance > 0f && duration > 0f;
    }
}