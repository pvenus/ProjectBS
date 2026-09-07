using System;

namespace Character.Control
{
    internal enum ControlReason { Manual, ExplicitAuto, Transition, Death, CrowdControl, Cutscene }
    internal readonly struct ControlVector
    {
        internal readonly float X,Y;
        internal ControlVector(float x,float y){X=x;Y=y;}
        internal bool Valid=>!float.IsNaN(X)&&!float.IsInfinity(X)&&!float.IsNaN(Y)&&!float.IsInfinity(Y);
        internal bool Nonzero=>X*X+Y*Y>.0001f;
        internal ControlVector Normalized {get{float n=(float)Math.Sqrt(X*X+Y*Y);return n>.0001f&&Valid?new ControlVector(X/n,Y/n):default;}}
    }
    internal readonly struct AimSnapshot
    {
        internal readonly ControlVector Point;
        internal AimSnapshot(ControlVector point,float left,float bottom,float right,float top)
        {Point=point.Valid?new ControlVector(Math.Max(left,Math.Min(right,point.X)),Math.Max(bottom,Math.Min(top,point.Y))):new ControlVector(float.NaN,float.NaN);}
    }
    internal struct ManualInputFrame
    {
        internal bool Up,Down,Left,Right,AttackHeld,AttackDown,DashDown;
        internal int SlotDown;
        internal AimSnapshot Aim;
        internal ControlVector CasterPosition;
        internal ControlVector Axes=>new ControlVector((Right?1:0)-(Left?1:0),(Up?1:0)-(Down?1:0)).Normalized;
    }
    internal interface IManualGameplay
    {
        bool Busy {get;}
        bool Available(int slot);
        bool Ready(int slot);
        bool Fire(int slot,AimSnapshot aim,ControlVector dash,Func<bool> chain);
        void Move(ControlVector direction);
        void Clear(bool interrupt);
    }
    internal sealed class ManualControlCore
    {
        internal ControlReason Reason {get;private set;}
        internal bool Suspended {get;private set;}
        internal bool AttackHeld {get;private set;}
        internal bool AllowsAuto=>Reason==ControlReason.ExplicitAuto&&!Suspended;
        private bool initialized,hasPending;
        private int pendingSlot;
        private AimSnapshot pendingAim,basicAim;
        private bool hasBasicAim;
        private ControlVector pendingDash,lastFacing;
        internal void Reset(){initialized=false;Reason=ControlReason.Manual;Suspended=true;AttackHeld=false;hasPending=false;hasBasicAim=false;lastFacing=default;}
        internal static ControlVector DashDirection(AimSnapshot aim,ControlVector origin,ControlVector axes,ControlVector facing)
        {
            var delta=new ControlVector(aim.Point.X-origin.X,aim.Point.Y-origin.Y);
            if(aim.Point.Valid&&origin.Valid&&delta.Nonzero)return delta.Normalized;
            return axes.Valid&&axes.Nonzero?axes.Normalized:facing.Valid&&facing.Nonzero?facing.Normalized:new ControlVector(1,0);
        }
        internal void Tick(ManualInputFrame input,ControlReason reason,bool suspended,ControlVector facing,IManualGameplay game)
        {
            var prior=Reason;
            bool changed=!initialized||Reason!=reason||Suspended!=suspended;
            Reason=reason;Suspended=suspended;initialized=true;
            if(changed)
            {
                AttackHeld=false;hasPending=false;hasBasicAim=false;lastFacing=default;
                game.Clear(reason!=ControlReason.Manual||prior!=ControlReason.Manual);game.Move(default);
                return; // handback frame consumes no input; next frame re-samples physical state.
            }
            if(reason!=ControlReason.Manual||suspended){AttackHeld=false;hasPending=false;hasBasicAim=false;game.Move(default);return;}
            var axes=input.Axes;if(facing.Valid&&facing.Nonzero)lastFacing=facing.Normalized;
            if((input.DashDown||input.SlotDown>0)&&game.Available(input.DashDown?4:input.SlotDown))
            {
                hasPending=true;pendingSlot=input.DashDown?4:input.SlotDown;pendingAim=input.Aim;
                pendingDash=DashDirection(input.Aim,input.CasterPosition,axes,lastFacing);
            }
            bool wasHeld=AttackHeld;
            AttackHeld=(input.AttackHeld||input.AttackDown)&&!hasPending;
            if(!AttackHeld)hasBasicAim=false;
            else if(input.AttackDown||!wasHeld){basicAim=input.Aim;hasBasicAim=true;}
            game.Move(game.Busy?default:axes);
            if(game.Busy)return;
            if(hasPending)
            {
                if(!game.Ready(pendingSlot))return;
                int slot=pendingSlot;var aim=pendingAim;var dash=pendingDash;hasPending=false;
                game.Move(default);game.Fire(slot,aim,dash,()=>false);return;
            }
            var cycleAim=hasBasicAim?basicAim:input.Aim;
            if(AttackHeld&&cycleAim.Point.Valid&&game.Ready(0))
            {game.Move(default);game.Fire(0,cycleAim,default,()=>AttackHeld&&Reason==ControlReason.Manual&&!Suspended);hasBasicAim=false;}
        }
    }
}
