using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Skill;
namespace Character.Control
{
    internal static class SeojinControlPolicy
    {
        internal const string IdPrefix="character.seojin.";
        internal static readonly string[] Slots={SkillPoolSlotKeys.BasicAttack,SkillPoolSlotKeys.Active1,SkillPoolSlotKeys.Active2,SkillPoolSlotKeys.Active3,SkillPoolSlotKeys.Active4,SkillPoolSlotKeys.Active5,SkillPoolSlotKeys.Active6,SkillPoolSlotKeys.Active7,SkillPoolSlotKeys.Active8};
        internal static readonly KeyCode[] Skills={KeyCode.Alpha1,KeyCode.Alpha2,KeyCode.Alpha3};
        internal const string ChargeShortcutSlotKey=SkillPoolSlotKeys.Active1;
        internal const string DashShortcutSlotKey=SkillPoolSlotKeys.Active4;
        internal const string SecondarySlotKey=SkillPoolSlotKeys.Active5;
        internal const string WheelUpSlotKey=SkillPoolSlotKeys.Active6;
        internal const string WheelDownSlotKey=SkillPoolSlotKeys.Active7;
        internal static int ShortcutSlot(string key)
        {
            for(int i=0;i<Slots.Length;i++)if(string.Equals(Slots[i],key,System.StringComparison.Ordinal))return i;
            return -1;
        }
        internal static bool Matches(CharacterSO so)=>so!=null&&so.CharacterType==CharacterType.Player&&so.CharacterId!=null&&so.CharacterId.StartsWith(IdPrefix,System.StringComparison.Ordinal);
    }
    internal static class SeojinInputReader
    {
        internal static bool UiBlocked
        {
            get
            {
                var ui=EventSystem.current;
                return !Application.isFocused ||
                    (UIPopupViewController.Instance!=null&&UIPopupViewController.Instance.HasOpenPopup) ||
                    (ui!=null&&(ui.IsPointerOverGameObject()||ui.currentSelectedGameObject!=null));
            }
        }
        internal static ManualInputFrame Read(Camera camera,Rect bounds,Vector2 casterPosition)
        {
            float wheel=UiOwnsWheel()?0f:Input.mouseScrollDelta.y;
            var f=new ManualInputFrame{Up=Input.GetKey(KeyCode.W),Down=Input.GetKey(KeyCode.S),Left=Input.GetKey(KeyCode.A),Right=Input.GetKey(KeyCode.D),AttackHeld=Input.GetMouseButton(0),AttackDown=Input.GetMouseButtonDown(0),DashDown=Input.GetKeyDown(KeyCode.Space),ChargeDown=Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift),SecondaryDown=Input.GetMouseButtonDown(1),ActiveSkillActionDown=Input.GetKeyDown(KeyCode.Q),WheelDelta=wheel,UnscaledTime=Time.unscaledTime};
            for(int i=0;i<SeojinControlPolicy.Skills.Length;i++)if(Input.GetKeyDown(SeojinControlPolicy.Skills[i])){f.SlotDown=i+1;break;}
            // Capture one raw world point; only GroundPoint consumes its bounds projection.
            f.CasterPosition=new ControlVector(casterPosition.x,casterPosition.y);
            Vector2 point=new Vector2(float.NaN,float.NaN);
            if(camera!=null)
            {
                Ray ray=camera.ScreenPointToRay(Input.mousePosition);
                if(Mathf.Abs(ray.direction.z)>.0001f){float t=-ray.origin.z/ray.direction.z;if(t>=0)point=ray.origin+ray.direction*t;}
            }
            f.Aim=new AimSnapshot(new ControlVector(point.x,point.y),bounds.xMin,bounds.yMin,bounds.xMax,bounds.yMax);return f;
        }
        private static bool UiOwnsWheel()
        {
            var ui=EventSystem.current;
            if(ui==null)return false;
            var pointer=new PointerEventData(ui){position=Input.mousePosition};
            var hits=new List<RaycastResult>();ui.RaycastAll(pointer,hits);
            for(int i=0;i<hits.Count;i++)
            {
                var go=hits[i].gameObject;
                if(go!=null&&(go.GetComponentInParent<ScrollRect>()!=null||go.GetComponentInParent<UICarouselWidget>()!=null))return true;
            }
            return false;
        }
    }
}
