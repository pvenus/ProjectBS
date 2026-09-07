using System;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Session;
using Skill;
namespace Character.Control
{
    [DefaultExecutionOrder(-20000)]
    [DisallowMultipleComponent]
    public sealed class SeojinManualControl:MonoBehaviour
    {
        [SerializeField] private bool explicitAutoMode;
        private bool scriptedCutscene;
        private CharacterManager actor;
        private PartyMovementMono movement;
        private CharacterSkillManager skills;
        private CharacterStateManager state;
        private ManualGameplayAdapter adapter;
        private readonly ManualControlCore core=new();
        private readonly Dictionary<Behaviour,bool> ai=new();
        private bool aiAllowed,baselineCaptured,baselineMovement;
        private bool ForcedBlocked=>actor==null||actor.RuntimeData==null||actor.RuntimeData.isDead||actor.IsDying||actor.IsStunned||actor.IsRooted||
            (GetComponent<MovementMono>()?.IsKnockingBack()??false)||(state!=null&&state.TryGetForcedTarget(out _))||
            Battle.Morpg.BattleMorpgLiveRoute.IsTransitionLocked(actor)||scriptedCutscene||Time.timeScale<=0||
            GameSession.Instance?.BattleSession?.BattleRuntime==null||GameSession.Instance.BattleSession.BattleRuntime.isCompleted;
        internal bool ManualAuthorized=>isActiveAndEnabled&&!ForcedBlocked&&core.Reason==ControlReason.Manual&&!core.Suspended;
        internal bool AutoAuthorized=>!ForcedBlocked&&(!isActiveAndEnabled||core.AllowsAuto);
        public void SetAutoMode(bool value)=>explicitAutoMode=value;
        public void SetScriptedCutscene(bool active)=>scriptedCutscene=active;
        internal static bool Bound(GameObject go)=>go!=null&&go.GetComponent<SeojinManualControl>() is SeojinManualControl c&&c.isActiveAndEnabled;
        internal static void Bind(CharacterManager owner)
        {
            if(owner==null||!SeojinControlPolicy.Matches(owner.RuntimeData?.characterSO))return;
            var control=owner.GetComponent<SeojinManualControl>()??owner.gameObject.AddComponent<SeojinManualControl>();control.Configure(owner);
        }
        private void Configure(CharacterManager owner)
        {
            actor=owner;skills=GetComponent<CharacterSkillManager>();movement=GetComponent<PartyMovementMono>()??gameObject.AddComponent<PartyMovementMono>();state=GetComponent<CharacterStateManager>();
            if(!baselineCaptured){baselineCaptured=true;baselineMovement=movement.IsMovementControlledByPlayer();}
            core.Reset();adapter=new ManualGameplayAdapter(actor,movement,skills);
            foreach(var b in GetComponentsInChildren<MonoBehaviour>(true))
                if(b is CharacterStateManager||b is SkillBrainMono||b is SkillExecutorMono)
                {if(!ai.ContainsKey(b))ai[b]=b.enabled;b.enabled=false;}
            state?.ClearState();movement.SetOwnedManualInput(true,Vector2.zero);aiAllowed=false;
        }
        private void Update()
        {
            if(actor==null||adapter==null)return;
            var battle=GameSession.Instance?.BattleSession?.BattleRuntime;
            bool end=battle==null||battle.isCompleted;
            bool cc=actor.IsStunned||actor.IsRooted||(GetComponent<MovementMono>()?.IsKnockingBack()??false)||(state!=null&&state.TryGetForcedTarget(out _));
            ControlReason reason=actor.RuntimeData==null||actor.RuntimeData.isDead||actor.IsDying?ControlReason.Death:
                Battle.Morpg.BattleMorpgLiveRoute.IsTransitionLocked(actor)?ControlReason.Transition:cc?ControlReason.CrowdControl:scriptedCutscene?ControlReason.Cutscene:explicitAutoMode?ControlReason.ExplicitAuto:ControlReason.Manual;
            bool blocked=end||Time.timeScale<=0||SeojinInputReader.UiBlocked;
            Rect bounds=BattleMapBoundsContext.Arena;
            var viewCamera=Camera.main;
            if(!BattleMapBoundsContext.IsActive&&viewCamera!=null){float hh=viewCamera.orthographicSize,hw=hh*viewCamera.aspect;var c=viewCamera.transform.position;bounds=Rect.MinMaxRect(c.x-hw,c.y-hh,c.x+hw,c.y+hh);}
            var env=Battle.Morpg.MorpgEnvironmentRuntime.Active;
            if(env?.Zone!=null){var z=env.Zone;float l=z.walkable[0].x,r=l,b=z.walkable[0].y,t=b;foreach(var v in z.walkable){l=Math.Min(l,v.x);r=Math.Max(r,v.x);b=Math.Min(b,v.y);t=Math.Max(t,v.y);}bounds=Rect.MinMaxRect(l,b,r,t);}
            var facing=GetComponentInChildren<AnimationMono>();Vector2 dir=Vector2.right;
            if(facing!=null){var f=facing.CurrentDirection;dir=new Vector2(f==AnimationMono.DiagonalDirection.UpLeft||f==AnimationMono.DiagonalDirection.DownLeft?-1:1,f==AnimationMono.DiagonalDirection.UpLeft||f==AnimationMono.DiagonalDirection.UpRight?1:-1);}
            var input=blocked?default:SeojinInputReader.Read(Camera.main,bounds,actor.transform.position);
            core.Tick(input,reason,blocked,new ControlVector(dir.x,dir.y),adapter);
            if(end)skills?.CancelManualExecution();
            bool allow=core.AllowsAuto;
            if(aiAllowed!=allow){state?.ClearState();GetComponent<SkillExecutorMono>()?.ClearRequest();foreach(var pair in ai)if(pair.Key!=null)pair.Key.enabled=allow&&pair.Value;aiAllowed=allow;}
            movement.SetOwnedManualInput(!allow,adapter.MoveInput);
        }
        private void OnEnable(){if(actor!=null)Configure(actor);}
        private void OnDisable()
        {
            var animation=GetComponentInChildren<AnimationMono>(true);
            animation?.BeginSynchronousTeardown();
            try
            {
                adapter?.Clear(true);core.Reset();if(state!=null)state.ClearState();GetComponent<SkillExecutorMono>()?.ClearRequest();
                if(movement!=null)movement.ReleaseManualControlForTeardown(baselineMovement);
                foreach(var pair in ai)if(pair.Key!=null)pair.Key.enabled=pair.Value;
                aiAllowed=false;
            }
            finally { if(animation!=null)animation.EndSynchronousTeardown(); }
        }
    }
    internal sealed class ManualGameplayAdapter:IManualGameplay
    {
        private readonly CharacterManager actor;private readonly PartyMovementMono movement;private readonly CharacterSkillManager skills;
        internal Vector2 MoveInput {get;private set;}
        internal ManualGameplayAdapter(CharacterManager a,PartyMovementMono m,CharacterSkillManager s){actor=a;movement=m;skills=s;}
        public bool Busy=>skills==null||skills.ManualBusy;
        private EquipmentSkillRuntimeData Runtime(int slot)=>slot>=0&&slot<SeojinControlPolicy.Slots.Length?skills?.SkillPool?.GetRuntimeByKey(SeojinControlPolicy.Slots[slot]):null;
        public bool Available(int slot)
        {
            var r=Runtime(slot);if(r?.sourceEquipment?.CastSo==null||r.sourceEquipment.BaseProfileSo==null||r.sourceEquipment.AimMode==SkillAimMode.Invalid)return false;
            if(r.comboProfile!=null&&r.comboProfile.Enabled)return r.comboProfile.IsComplete;
            return r.sourceEquipment.BaseProfileSo.SkillComponentType==SkillComponentType.Mobility || (r.sourceEquipment.HitSos!=null&&Array.Exists(r.sourceEquipment.HitSos,h=>h!=null));
        }
        public bool Ready(int slot)=>Available(slot)&&skills!=null&&skills.ManualReady(Runtime(slot));
        public AimSnapshot Capture(int slot,AimSnapshot aim)
        {
            var runtime=Runtime(slot);var mode=runtime?.sourceEquipment?.AimMode??SkillAimMode.Invalid;
            object target=null;
            if(mode==SkillAimMode.Target&&aim.Direction.Nonzero&&skills!=null)
                target=skills.ResolveManualTarget(runtime,new Vector2(aim.Direction.X,aim.Direction.Y));
            var point=mode==SkillAimMode.GroundPoint?aim.GroundPoint(runtime.resolvedRange):default;
            return aim.Resolve(mode,aim.Direction,point,target);
        }
        public bool Fire(int slot,AimSnapshot aim,ControlVector dash,Func<bool> chain)
        {
            var runtime=Runtime(slot);if(runtime==null||runtime.sourceEquipment.AimMode!=aim.Mode)return false;
            var resolved=new ManualSkillAim(aim.Mode,new Vector2(aim.Direction.X,aim.Direction.Y),new Vector2(aim.Point.X,aim.Point.Y),aim.LockedTarget as Transform);
            if(!resolved.IsValid)return false;
            actor.GetComponent<MovementMono>()?.StopAllMotion(false);
            return skills.FireManualAim(runtime,resolved,slot==0?chain:null);
        }
        public void Move(ControlVector v){MoveInput=new Vector2(v.X,v.Y);}
        public void Clear(bool interrupt)
        {
            Move(default);
            if(actor!=null)actor.GetComponent<MovementMono>()?.StopAllMotion(false);
            if(interrupt&&skills!=null)skills.CancelManualExecution();
        }
    }
}
