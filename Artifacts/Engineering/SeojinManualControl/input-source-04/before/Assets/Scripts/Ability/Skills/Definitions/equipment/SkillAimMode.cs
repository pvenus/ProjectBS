using System;
using UnityEngine;
namespace Skill
{
    // Zero preserves serialized assets predating aimMode. Unknown values fail closed.
    public enum SkillAimMode { Legacy=0, Direction=1, Target=2, GroundPoint=3, Self=4, Invalid=255 }
    public static class SkillAimPolicy
    {
        public static SkillAimMode Parse(string value)
        {
            if(value==null)return SkillAimMode.Legacy;
            switch(value){case "Direction":return SkillAimMode.Direction;case "Target":return SkillAimMode.Target;case "GroundPoint":return SkillAimMode.GroundPoint;case "Self":return SkillAimMode.Self;default:return SkillAimMode.Invalid;}
        }
        public static SkillAimMode Resolve(SkillAimMode mode,SkillCastSO cast)
        {
            if(mode!=SkillAimMode.Legacy)return mode>=SkillAimMode.Direction&&mode<=SkillAimMode.Self?mode:SkillAimMode.Invalid;
            if(cast==null)return SkillAimMode.Invalid;
            switch(cast.TargetingType)
            {
                case TargetingType.None:case TargetingType.Self:return SkillAimMode.Self;
                case TargetingType.AutoTarget:return cast.SnapshotTargetPointOnCast?SkillAimMode.GroundPoint:SkillAimMode.Target;
                case TargetingType.AutoTargetDirection:case TargetingType.Directional:return SkillAimMode.Direction;
                case TargetingType.Position:return SkillAimMode.GroundPoint;
                default:return SkillAimMode.Invalid;
            }
        }
    }
    public readonly struct ManualSkillAim
    {
        public readonly SkillAimMode Mode;
        public readonly Vector2 Direction,Point;
        public readonly Transform Target;
        public ManualSkillAim(SkillAimMode mode,Vector2 direction,Vector2 point,Transform target=null){Mode=mode;Direction=direction;Point=point;Target=target;}
        public bool IsValid=>Mode==SkillAimMode.Self || Mode==SkillAimMode.Target&&Target!=null ||
            Mode==SkillAimMode.Direction&&Finite(Direction)&&Direction.sqrMagnitude>.0001f || Mode==SkillAimMode.GroundPoint&&Finite(Point);
        private static bool Finite(Vector2 v)=>!float.IsNaN(v.x)&&!float.IsInfinity(v.x)&&!float.IsNaN(v.y)&&!float.IsInfinity(v.y);
    }
}
