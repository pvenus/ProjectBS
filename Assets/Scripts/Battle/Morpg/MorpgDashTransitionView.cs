using System;
using System.Collections.Generic;
using Character;
using UnityEngine;

namespace Battle.Morpg
{
    // This animation owner never invokes a gameplay skill, consumes cooldown or applies root motion.
    internal sealed class MorpgDashTransitionView : IDisposable
    {
        private sealed class Snapshot { internal SpriteRenderer Renderer; internal Color Color; internal Sprite Sprite; internal bool Flip; }
        private readonly CharacterManager actor;
        private readonly MorpgEnvironmentRuntime environment;
        private readonly int sourceZone, destinationZone;
        private readonly Vector3 sourcePosition, stagingPosition, targetPosition;
        private readonly Camera camera;
        private readonly Vector3 sourceCameraPosition;
        private Vector3 fixedCameraPosition;
        private readonly Quaternion sourceCameraRotation;
        private readonly BattlePlayerCameraFollowMono follow;
        private readonly bool followEnabled;
        private readonly AnimationMono animation;
        private readonly SpriteRenderer bodyRenderer;
        private readonly MorpgDashExitPath exitPath;
        private readonly AnimationClip clip;
        private readonly Animator animator;
        private readonly bool rootMotion;
        private readonly List<Snapshot> renderers=new();
        private readonly List<RaycastHit2D> sweepHits=new(16);
        private readonly MorpgTransitionLateView late;
        private bool disposed, bodyReleased, warped, startLogged;
        private int waveFrame=-1;
        private MorpgDashPhase previousPhase;
        internal MorpgTransitionClock Clock { get; }
        internal bool CanLand { get; }
        internal string FallbackReason { get; private set; }
        internal Vector3 WarpPosition => Clock.Fallback ? targetPosition : stagingPosition;
        internal Vector2 CameraTarget { get; }
        internal bool Finished => disposed;
        internal Action Cancelled;

        internal MorpgDashTransitionView(CharacterManager actor, MorpgEnvironmentRuntime environment, int source, bool reducedMotion)
        {
            this.actor=actor;this.environment=environment;sourceZone=source;destinationZone=source+1;
            sourcePosition=actor.transform.position;
            try
            {
            var from=environment.RuntimeZone(source);var to=environment.RuntimeZone(destinationZone);
            targetPosition=new Vector3(to.entry[0],to.entry[1],sourcePosition.z);
            var staging=to.transitionStaging;
            stagingPosition=staging?.Length==2 ? new Vector3(staging[0],staging[1],sourcePosition.z) : targetPosition;
            CameraTarget=environment.CameraTarget(destinationZone,targetPosition);
            camera=Camera.main;
            if(camera!=null)
            {
                sourceCameraPosition=fixedCameraPosition=camera.transform.position;sourceCameraRotation=camera.transform.rotation;
                follow=camera.GetComponent<BattlePlayerCameraFollowMono>();
                if(follow!=null){followEnabled=follow.enabled;follow.enabled=false;follow.ResetSmoothing();}
            }
            animation=actor.GetComponent<AnimationMono>();
            bodyRenderer=animation?.TransitionBodyRenderer ?? actor.GetComponent<SpriteRenderer>();
            animator=actor.GetComponent<Animator>();
            if(animator!=null){rootMotion=animator.applyRootMotion;animator.applyRootMotion=false;}
            foreach(var renderer in actor.GetComponentsInChildren<SpriteRenderer>(true))
                renderers.Add(new Snapshot{Renderer=renderer,Color=renderer.color,Sprite=renderer.sprite,Flip=renderer.flipX});
            clip=Resources.Load<AnimationClip>("battle/morpg/transitions/transition-dash");
            float radius=MorpgEnvironmentRuntime.ActorRadius(actor.GetComponent<Rigidbody2D>());
            CanLand=MorpgEnvironmentGeometry.Free(to,targetPosition.x,targetPosition.y,Math.Max(1.5f,radius+.15f));
            exitPath=MorpgDashExitPath.Plan(from,sourcePosition,radius);
            bool path=exitPath!=null && staging?.Length==2 &&
                Math.Abs(targetPosition.x-stagingPosition.x-3f)<.001f && Math.Abs(targetPosition.y-stagingPosition.y)<.001f &&
                Corridor(to,stagingPosition,targetPosition,Math.Max(1.4f,radius)+.15f);
            bool nonDash=reducedMotion || clip==null || bodyRenderer==null;
            FallbackReason=reducedMotion?"reduced motion":clip==null||bodyRenderer==null?"body clip/renderer unavailable":!path?"authored dash corridor blocked":null;
            Clock=new MorpgTransitionClock(exitPath?.Length??0,nonDash,!path);
            late=new GameObject("MORPG transition presentation owner").AddComponent<MorpgTransitionLateView>();late.Owner=this;
            }
            catch { Abort(); throw; }
        }
        private static bool Corridor(MorpgEnvironmentProfile.Zone zone, Vector3 from, Vector3 to,float inset)
        {
            int samples=Math.Max(1,(int)Math.Ceiling(Vector2.Distance(from,to)/.05f));
            for(int i=0;i<=samples;i++)
            {var p=Vector3.Lerp(from,to,(float)i/samples);if(!MorpgEnvironmentGeometry.Free(zone,p.x,p.y,inset))return false;}
            return true;
        }
        internal bool Tick(float delta,bool rewardsReady,out string error)
        {
            error=null;
            if(disposed){error="transition owner disposed";return false;}
            if(actor==null || !actor.gameObject.activeInHierarchy || actor.RuntimeData.isDead){error="transition actor unavailable";return false;}
            if(!CanLand){error="transition landing invalid";return false;}
            if(waveFrame>=0 && Time.frameCount>waveFrame){Dispose();return true;}
            previousPhase=Clock.Phase;float previousAlpha=Clock.Alpha;
            Clock.Tick(delta,Time.frameCount,rewardsReady,actor.CanMove);
            if(!startLogged && Clock.Phase!=MorpgDashPhase.Cleanup)
            {
                if(Clock.Mode==MorpgDashMode.AssistedDash && FallbackReason==null)FallbackReason="motion status remains after wait";
                startLogged=true;
                Debug.Log($"[MORPG transition] zone={sourceZone}->{destinationZone} mode={Clock.Mode} reason={FallbackReason??"none"} clear=({sourcePosition.x:F2},{sourcePosition.y:F2}) exitLength={exitPath?.Length??0:F2}");
            }
            if(Clock.Phase==MorpgDashPhase.Exit)
            {
                float t=Clock.Progress;var next=exitPath.Sample(t*t*t);
                if(!TryMove(next))Assist("structural exit obstruction",previousAlpha);
            }
            if(Clock.Phase==MorpgDashPhase.Invisible && previousPhase==MorpgDashPhase.Exit)
                if(!TryMove(exitPath.End))Assist("structural exit obstruction",previousAlpha);
            if(Clock.Phase==MorpgDashPhase.Invisible && previousPhase==MorpgDashPhase.FallbackOut && warped)
            {
                if(!TargetClear()){error="fallback landing dynamically blocked";return false;}
                actor.transform.position=targetPosition;var body=actor.GetComponent<Rigidbody2D>();
                if(body!=null){body.position=targetPosition;body.linearVelocity=Vector2.zero;body.angularVelocity=0;}
            }
            if(Clock.Phase==MorpgDashPhase.Entry)
            {
                float t=Clock.Progress;var next=Vector3.Lerp(stagingPosition,targetPosition,1-(1-t)*(1-t)*(1-t));
                if(!TryMove(next))Assist("structural entry obstruction",previousAlpha);
            }
            if(Clock.Phase==MorpgDashPhase.Settle && previousPhase!=Clock.Phase)
            {
                if(!TryMove(targetPosition)){error="landing became blocked";return false;}
                if(bodyRenderer!=null)foreach(var state in renderers)if(state.Renderer==bodyRenderer)bodyRenderer.sprite=state.Sprite;
                animation?.PlayIdle();
            }
            ApplyPresentation();return true;
        }
        internal bool TargetClear()
        {
            var body=actor.GetComponent<Rigidbody2D>();
            float radius=MorpgEnvironmentRuntime.ActorRadius(body)+.15f;
            foreach(var collider in Physics2D.OverlapCircleAll(targetPosition,radius))
                if(environment.OwnsTransitionBlocker(collider))return false;
            return CanLand;
        }
        private bool TryMove(Vector3 target)
        {
            var body=actor.GetComponent<Rigidbody2D>();Vector2 from=actor.transform.position;
            Vector2 safe=environment.Sweep(from,target,MorpgEnvironmentRuntime.ActorRadius(body));
            if(Vector2.Distance(safe,target)>.001f)return false;
            Vector2 delta=(Vector2)target-from;
            if(body!=null && delta.sqrMagnitude>.000001f)
            {
                sweepHits.Clear();var filter=new ContactFilter2D{useTriggers=false,useLayerMask=false};
                int count=body.Cast(delta.normalized,filter,sweepHits,delta.magnitude+.15f);
                for(int i=0;i<count;i++)if(environment.OwnsTransitionBlocker(sweepHits[i].collider) && sweepHits[i].distance<delta.magnitude+.15f)return false;
            }
            actor.transform.position=target;
            if(body!=null){body.position=target;body.linearVelocity=Vector2.zero;body.angularVelocity=0;}
            return true;
        }
        private void Assist(string reason,float previousAlpha)
        {
            if(!Clock.UseFallback(previousAlpha))return;
            FallbackReason=reason;
            Debug.Log($"[MORPG transition assistance] zone={sourceZone}->{destinationZone} mode={Clock.Mode} reason={reason}");
        }
        internal void ApplyPresentation()
        {
            if(disposed)return;
            if(actor==null || !actor.gameObject.activeInHierarchy){Cancelled?.Invoke();return;}
            if(!bodyReleased && Clock.DashEnabled && bodyRenderer!=null && clip!=null &&
               Clock.Phase!=MorpgDashPhase.Cleanup && Clock.Phase!=MorpgDashPhase.Settle &&
               Clock.Phase!=MorpgDashPhase.AwaitUnlock && Clock.Phase!=MorpgDashPhase.WaveDelay && Clock.Phase!=MorpgDashPhase.Ready)
            {
                float progress=Clock.Phase==MorpgDashPhase.Anticipation?0:
                    Clock.Phase==MorpgDashPhase.Exit?Clock.Progress:
                    Clock.Phase==MorpgDashPhase.Entry?Clock.Progress:
                    Clock.Phase==MorpgDashPhase.Invisible?1:Mathf.Clamp01(Clock.Age/(Clock.Phase==MorpgDashPhase.FallbackOut?.32f:.36f));
                // Dedicated late visual sampling ignores skill cooldowns, CC and another facing owner.
                // The selected clip has only sprite/flip curves; gameplay and animation state are untouched.
                clip.SampleAnimation(bodyRenderer.gameObject,progress*Math.Max(0,clip.length-.0001f));
                bodyRenderer.flipX=false;
            }
            if(!bodyReleased)foreach(var state in renderers)
                if(state.Renderer!=null){var color=state.Color;color.a*=Clock.Alpha;state.Renderer.color=color;}
            if(camera!=null){camera.transform.position=fixedCameraPosition;camera.transform.rotation=sourceCameraRotation;}
        }
        internal void MarkWarped()
        {warped=true;Clock.MarkWarped();if(camera!=null)fixedCameraPosition=camera.transform.position;ApplyPresentation();}
        internal void UnlockBody(){ReleaseBody(false);Clock.MarkUnlocked();}
        internal void WaveStarted(){waveFrame=Time.frameCount;}
        private void ReleaseBody(bool abort)
        {
            if(bodyReleased)return;bodyReleased=true;
            foreach(var state in renderers)if(state.Renderer!=null)
            {state.Renderer.color=state.Color;state.Renderer.flipX=state.Flip;if(abort)state.Renderer.sprite=state.Sprite;}
            if(animator!=null)animator.applyRootMotion=rootMotion;
        }
        internal void Abort()
        {
            if(disposed)return;
            if(actor!=null)
            {
                actor.transform.position=sourcePosition;var body=actor.GetComponent<Rigidbody2D>();
                if(body!=null){body.position=sourcePosition;body.linearVelocity=Vector2.zero;body.angularVelocity=0;}
            }
            environment.CompleteWarp(sourceZone);
            fixedCameraPosition=sourceCameraPosition;
            if(camera!=null){camera.transform.position=sourceCameraPosition;camera.transform.rotation=sourceCameraRotation;}
            ReleaseBody(true);Dispose();
        }
        public void Dispose()
        {
            if(disposed)return;ReleaseBody(false);disposed=true;
            if(follow!=null){follow.ResetSmoothing();follow.enabled=followEnabled;}
            if(late!=null){late.Owner=null;UnityEngine.Object.Destroy(late.gameObject);}
        }
    }
}
