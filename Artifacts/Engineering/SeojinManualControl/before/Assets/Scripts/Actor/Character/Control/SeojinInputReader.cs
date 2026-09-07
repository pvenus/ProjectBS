using UnityEngine;
using UnityEngine.EventSystems;
using Skill;
namespace Character.Control
{
    internal static class SeojinControlPolicy
    {
        internal const string IdPrefix="character.seojin.";
        internal static readonly string[] Slots={SkillPoolSlotKeys.BasicAttack,SkillPoolSlotKeys.Active1,SkillPoolSlotKeys.Active2,SkillPoolSlotKeys.Active3,SkillPoolSlotKeys.Active4};
        internal static readonly KeyCode[] Skills={KeyCode.Alpha1,KeyCode.Alpha2,KeyCode.Alpha3};
        internal static bool Matches(CharacterSO so)=>so!=null&&so.CharacterType==CharacterType.Player&&so.CharacterId!=null&&so.CharacterId.StartsWith(IdPrefix,System.StringComparison.Ordinal);
    }
    internal static class SeojinInputReader
    {
        internal static bool UiBlocked
        {
            get
            {
                var ui=EventSystem.current;
                return !Application.isFocused || (ui!=null&&(ui.IsPointerOverGameObject()||ui.currentSelectedGameObject!=null));
            }
        }
        internal static ManualInputFrame Read(Camera camera,Rect bounds,Vector2 casterPosition)
        {
            var f=new ManualInputFrame{Up=Input.GetKey(KeyCode.W),Down=Input.GetKey(KeyCode.S),Left=Input.GetKey(KeyCode.A),Right=Input.GetKey(KeyCode.D),AttackHeld=Input.GetMouseButton(0),AttackDown=Input.GetMouseButtonDown(0),DashDown=Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift)};
            for(int i=0;i<SeojinControlPolicy.Skills.Length;i++)if(Input.GetKeyDown(SeojinControlPolicy.Skills[i])){f.SlotDown=i+1;break;}
            // Basic, skills and Dash share the same clamped world-point snapshot.
            f.CasterPosition=new ControlVector(casterPosition.x,casterPosition.y);
            Vector2 point=new Vector2(float.NaN,float.NaN);
            if(camera!=null)
            {
                Ray ray=camera.ScreenPointToRay(Input.mousePosition);
                if(Mathf.Abs(ray.direction.z)>.0001f){float t=-ray.origin.z/ray.direction.z;if(t>=0)point=ray.origin+ray.direction*t;}
            }
            f.Aim=new AimSnapshot(new ControlVector(point.x,point.y),bounds.xMin,bounds.yMin,bounds.xMax,bounds.yMax);return f;
        }
    }
}
