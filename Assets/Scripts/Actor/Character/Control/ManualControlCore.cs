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
    internal struct AimSnapshot
    {
        internal ControlVector RawPoint {get;private set;}
        internal ControlVector Origin {get;private set;}
        internal ControlVector Direction {get;private set;}
        internal ControlVector KeyboardDirection {get;private set;}
        internal global::Skill.AimInputSource InputSource {get;private set;}
        internal ControlVector Point {get;private set;}
        internal global::Skill.SkillAimMode Mode {get;private set;}
        internal object LockedTarget {get;private set;}
        internal AimSnapshot(ControlVector point,float left,float bottom,float right,float top)
        {
            RawPoint=point;Origin=default;Direction=default;KeyboardDirection=default;InputSource=global::Skill.AimInputSource.Legacy;Mode=global::Skill.SkillAimMode.Legacy;LockedTarget=null;
            Point=point.Valid?new ControlVector(Math.Max(left,Math.Min(right,point.X)),Math.Max(bottom,Math.Min(top,point.Y))):new ControlVector(float.NaN,float.NaN);
        }
        internal AimSnapshot AtOrigin(ControlVector origin)
        {
            var copy=this;copy.Origin=origin;
            copy.Direction=RawPoint.Valid&&origin.Valid?new ControlVector(RawPoint.X-origin.X,RawPoint.Y-origin.Y).Normalized:default;
            return copy;
        }
        internal AimSnapshot WithKeyboardDirection(ControlVector direction)
        {var copy=this;copy.KeyboardDirection=direction;return copy;}
        internal AimSnapshot WithInputSource(global::Skill.AimInputSource source)
        {var copy=this;copy.InputSource=source;return copy;}
        internal AimSnapshot Resolve(global::Skill.SkillAimMode mode,ControlVector direction,ControlVector point,object target=null)
        {var copy=this;copy.Mode=mode;copy.Direction=direction;copy.Point=point;copy.LockedTarget=target;return copy;}
        internal ControlVector GroundPoint(float range)
        {
            // Origin is inside playable bounds: shortening the segment stays inside them.
            if(!Point.Valid||!Origin.Valid||float.IsNaN(range)||float.IsInfinity(range)||range<0)return new ControlVector(float.NaN,float.NaN);
            var delta=new ControlVector(Point.X-Origin.X,Point.Y-Origin.Y);
            float distance=(float)Math.Sqrt(delta.X*delta.X+delta.Y*delta.Y);
            return distance>range?new ControlVector(Origin.X+delta.Normalized.X*range,Origin.Y+delta.Normalized.Y*range):Point;
        }
    }
    internal struct ManualInputFrame
    {
        internal bool Up,Down,Left,Right,AttackHeld,AttackDown,DashDown,ChargeDown,SecondaryDown,ActiveSkillActionDown;
        internal float WheelDelta,UnscaledTime;
        internal int SlotDown;
        internal AimSnapshot Aim;
        internal ControlVector CasterPosition;
        internal ControlVector Axes=>new ControlVector((Right?1:0)-(Left?1:0),(Up?1:0)-(Down?1:0)).Normalized;
    }
    internal interface IManualGameplay
    {
        AimSnapshot Capture(int slot,AimSnapshot aim);
        bool Busy {get;}
        bool Available(int slot);
        bool Ready(int slot);
        bool CanReserve(int slot)=>Ready(slot);
        bool CanCancelForReservedSkill=>false;
        int ExecutionId=>0;
        void CancelForReservedSkill(int slot)=>Clear(true);
        bool Fire(int slot,AimSnapshot aim,ControlVector dash,Func<bool> chain);
        bool TryActiveSkillAction(AimSnapshot aim)=>false;
        void Move(ControlVector direction);
        void Clear(bool interrupt);
    }
    internal sealed class ManualControlCore
    {
        internal ControlReason Reason {get;private set;}
        internal bool Suspended {get;private set;}
        internal bool AttackHeld {get;private set;}
        internal bool AllowsAuto=>Reason==ControlReason.ExplicitAuto&&!Suspended;
        private bool initialized;
        private int pendingSlot;
        private AimSnapshot pendingAim;
        private float pendingExpiresAt;
        private int pendingExecutionId;
        internal int PendingSlot=>pendingSlot;
        internal event Action<int> PendingSkillChanged;
        private AimSnapshot basicAim;
        private bool hasBasicAim;
        private bool basicPressPending;
        private bool wheelArmed=true;
        private float wheelDebounceUntil;
        private ControlVector lastMovement;
        internal void Reset(){initialized=false;Reason=ControlReason.Manual;Suspended=true;AttackHeld=false;hasBasicAim=false;basicPressPending=false;wheelArmed=true;wheelDebounceUntil=0;lastMovement=default;ClearPending();}
        private void ClearPending(){bool changed=pendingSlot!=0;pendingSlot=0;pendingAim=default;pendingExpiresAt=0f;pendingExecutionId=0;if(changed)PendingSkillChanged?.Invoke(0);}
        internal static ControlVector KeyboardMoveDirection(ControlVector axes,ControlVector last,ControlVector facing)
            =>axes.Valid&&axes.Nonzero?axes.Normalized:last.Valid&&last.Nonzero?last.Normalized:facing.Valid&&facing.Nonzero?facing.Normalized:new ControlVector(1,0);
        internal void Tick(ManualInputFrame input,ControlReason reason,bool suspended,ControlVector facing,IManualGameplay game)
        {
            var prior=Reason;
            bool changed=!initialized||Reason!=reason||Suspended!=suspended;
            Reason=reason;Suspended=suspended;initialized=true;
            if(changed)
            {
                AttackHeld=false;hasBasicAim=false;basicPressPending=false;lastMovement=default;ClearPending();
                game.Clear(suspended||reason!=ControlReason.Manual||prior!=ControlReason.Manual);game.Move(default);
                return; // handback frame consumes no input; next frame re-samples physical state.
            }
            if(reason!=ControlReason.Manual||suspended){AttackHeld=false;hasBasicAim=false;basicPressPending=false;ClearPending();game.Move(default);return;}
            var snapshot=input.Aim.AtOrigin(input.CasterPosition);
            var axes=input.Axes;if(axes.Valid&&axes.Nonzero)lastMovement=axes.Normalized;
            snapshot=snapshot.WithKeyboardDirection(KeyboardMoveDirection(axes,lastMovement,facing));
            if(input.ActiveSkillActionDown)
            {
                game.TryActiveSkillAction(snapshot);
                return;
            }
            bool wasHeld=AttackHeld;
            AttackHeld=input.AttackHeld||input.AttackDown;
            bool newBasicPress=input.AttackDown||(input.AttackHeld&&!wasHeld);
            if(newBasicPress)
            {
                basicAim=game.Capture(0,snapshot);
                hasBasicAim=true;
                basicPressPending=true;
            }
            game.Move(game.Busy?default:axes);
            // Shortcut priority is deterministic: Shift/Charge wins over Space/Dash.
            // Resolve through stable pool keys rather than coupling input to array indexes.
            int wheelSign=input.WheelDelta>=.10f?1:input.WheelDelta<=-.10f?-1:0;
            if(Math.Abs(input.WheelDelta)<=.02f)wheelArmed=true;
            bool wheelEdge=wheelSign!=0&&wheelArmed&&input.UnscaledTime>=wheelDebounceUntil;
            if(wheelEdge){wheelArmed=false;wheelDebounceUntil=input.UnscaledTime+.12f;}
            int requested=input.ChargeDown?SeojinControlPolicy.ShortcutSlot(SeojinControlPolicy.ChargeShortcutSlotKey):
                input.DashDown?SeojinControlPolicy.ShortcutSlot(SeojinControlPolicy.DashShortcutSlotKey):
                input.SecondaryDown?SeojinControlPolicy.ShortcutSlot(SeojinControlPolicy.SecondarySlotKey):
                wheelEdge&&wheelSign>0?SeojinControlPolicy.ShortcutSlot(SeojinControlPolicy.WheelUpSlotKey):
                wheelEdge&&wheelSign<0?SeojinControlPolicy.ShortcutSlot(SeojinControlPolicy.WheelDownSlotKey):
                input.SlotDown;
            if(input.DashDown)ClearPending();
            if(requested>0&&game.Available(requested)&&game.CanReserve(requested))
            {
                var requestedAim=game.Capture(requested,snapshot);
                if(game.Busy)
                {
                    bool pendingChanged=pendingSlot!=requested;
                    pendingSlot=requested;pendingAim=requestedAim;pendingExecutionId=game.ExecutionId;
                    pendingExpiresAt=input.UnscaledTime+.35f;
                    if(pendingChanged)PendingSkillChanged?.Invoke(requested);
                    basicPressPending=false;hasBasicAim=false;
                    if(game.CanCancelForReservedSkill)
                    {
                        game.CancelForReservedSkill(requested);
                        bool fired=game.Ready(requested)&&
                            game.Fire(requested,requestedAim,snapshot.KeyboardDirection,()=>false);
                        ClearPending();
                        if(fired){game.Move(default);return;}
                    }
                    return;
                }
                ClearPending();
            }
            // Basic is an edge-triggered offensive request too. It may use the
            // finisher bridge only when the ordinary readiness gate accepts it;
            // a held button never manufactures this edge.
            if(game.Busy&&basicPressPending&&game.CanCancelForReservedSkill)
            {
                var reservedBasicAim=hasBasicAim?basicAim:game.Capture(0,snapshot);
                game.CancelForReservedSkill(0);
                bool fired=game.Ready(0)&&
                    game.Fire(0,reservedBasicAim,snapshot.KeyboardDirection,()=>false);
                basicPressPending=false;hasBasicAim=false;
                if(fired){game.Move(default);return;}
            }
            if(game.Busy)return;
            if(pendingSlot>0)
            {
                int reserved=pendingSlot;var reservedAim=pendingAim;float expires=pendingExpiresAt;int reservedExecutionId=pendingExecutionId;
                ClearPending();
                if(input.UnscaledTime<=expires&&(reservedExecutionId==0||reservedExecutionId==game.ExecutionId)&&game.Available(reserved)&&game.Ready(reserved)&&
                    game.Fire(reserved,reservedAim,snapshot.KeyboardDirection,()=>false))
                {basicPressPending=false;hasBasicAim=false;game.Move(default);return;}
            }
            // Reject unavailable/busy/cooldown/resource failures without touching Basic.
            // Fire=false is also non-preempting: held Basic may run in this same tick.
            if(requested>0&&game.Available(requested)&&game.Ready(requested))
            {
                var aim=game.Capture(requested,snapshot);
                if(game.Fire(requested,aim,snapshot.KeyboardDirection,()=>false))
                {
                    // Preserve the sampled physical held state so a successful skill
                    // cannot manufacture a new Basic rising edge on the next frame.
                    hasBasicAim=false;basicPressPending=false;game.Move(default);return;
                }
            }
            if(game.Busy)return;
            // A held button that outlives comboIndex2's recovery does not retain a
            // request through cooldown. It creates a fresh step-0 intent only when
            // the ordinary readiness gate opens again.
            if(!basicPressPending&&AttackHeld&&game.Ready(0))
            {
                basicAim=game.Capture(0,snapshot);
                hasBasicAim=true;
                basicPressPending=true;
            }
            if(!basicPressPending)return;
            if(!game.Ready(0))
            {
                // Buffer only across the active recovery. A press rejected by cooldown
                // must not fire later without a fresh release/press edge.
                basicPressPending=false;hasBasicAim=false;return;
            }
            var cycleAim=hasBasicAim?basicAim:game.Capture(0,snapshot);
            game.Move(default);
            if(game.Fire(0,cycleAim,default,()=>false))
            {basicPressPending=false;hasBasicAim=false;}
            else
            {basicPressPending=false;hasBasicAim=false;}
        }
    }
}
