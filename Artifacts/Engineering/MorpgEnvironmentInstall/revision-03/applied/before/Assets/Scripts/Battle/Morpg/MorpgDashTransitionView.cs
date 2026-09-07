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
        private readonly Vector3 sourcePosition, exitPosition, stagingPosition, targetPosition;
        private readonly Camera camera;
        private readonly Vector3 sourceCameraPosition;
        private Vector3 fixedCameraPosition;
        private readonly Quaternion sourceCameraRotation;
        private readonly BattlePlayerCameraFollowMono follow;
        private readonly bool followEnabled;
        private readonly AnimationMono animation;
        private readonly AnimationMono.DiagonalDirection facing;
        private readonly AnimationClip clip;
        private readonly Animator animator;
        private readonly bool rootMotion;
        private readonly List<Snapshot> renderers=new();
        private readonly MorpgTransitionLateView late;
        private bool disposed, bodyReleased, warped, ownsFacing, ownsBody;
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
            var from=environment.Profile.zones[source];var to=environment.Profile.zones[destinationZone];
            targetPosition=new Vector3(to.entry[0],to.entry[1],sourcePosition.z);
            var staging=to.transitionStaging;
            stagingPosition=staging?.Length==2 ? new Vector3(staging[0],staging[1],sourcePosition.z) : targetPosition;
            var exit=from.transitionExit;
            exitPosition=exit?.Length==2 ? new Vector3(exit[0],exit[1],sourcePosition.z) : sourcePosition;
            CameraTarget=new Vector2((to.cameraClamp[0]+to.cameraClamp[2])/2,(to.cameraClamp[1]+to.cameraClamp[3])/2);
            camera=Camera.main;
            if(camera!=null)
            {
                sourceCameraPosition=fixedCameraPosition=camera.transform.position;sourceCameraRotation=camera.transform.rotation;
                follow=camera.GetComponent<BattlePlayerCameraFollowMono>();
                if(follow!=null){followEnabled=follow.enabled;follow.enabled=false;follow.ResetSmoothing();}
            }
            animation=actor.GetComponent<AnimationMono>();
            if(animation!=null)facing=animation.CurrentDirection;
            animator=actor.GetComponent<Animator>();
            if(animator!=null){rootMotion=animator.applyRootMotion;animator.applyRootMotion=false;}
            foreach(var renderer in actor.GetComponentsInChildren<SpriteRenderer>(true))
                renderers.Add(new Snapshot{Renderer=renderer,Color=renderer.color,Sprite=renderer.sprite,Flip=renderer.flipX});
            clip=Resources.Load<AnimationClip>("battle/morpg/transitions/transition-dash");
            float radius=MorpgEnvironmentRuntime.ActorRadius(actor.GetComponent<Rigidbody2D>());
            CanLand=MorpgEnvironmentGeometry.Free(to,targetPosition.x,targetPosition.y,Math.Max(1.5f,radius+.15f));
            bool path=exit?.Length==2 && staging?.Length==2 && exitPosition.x>sourcePosition.x &&
                Math.Abs(targetPosition.x-stagingPosition.x-3f)<.001f && Math.Abs(targetPosition.y-stagingPosition.y)<.001f &&
                Corridor(from,sourcePosition,exitPosition,Math.Max(1.4f,radius)+.15f) &&
                Corridor(to,stagingPosition,targetPosition,Math.Max(1.4f,radius)+.15f);
            bool fallback=reducedMotion || clip==null || animation==null || renderers.Count==0 || !path;
            FallbackReason=reducedMotion?"reduced motion":clip==null||animation==null||renderers.Count==0?"body clip/renderer unavailable":!path?"authored dash corridor blocked":null;
            Clock=new MorpgTransitionClock(Vector2.Distance(sourcePosition,exitPosition),fallback);
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
            if(Clock.Phase==MorpgDashPhase.Anticipation && previousPhase!=Clock.Phase)
            {
                ownsFacing=animation.AcquireFacingLock(this,Vector2.right);
                ownsBody=ownsFacing && animation.PlaySkillBodyAction(clip,.08f+Clock.ExitDuration,false);
                if(!ownsBody){FallbackReason="body action unavailable";Clock.UseFallback(previousAlpha);}
            }
            if(Clock.Phase==MorpgDashPhase.Exit)
            {
                float t=Clock.Progress;var next=Vector3.Lerp(sourcePosition,exitPosition,t*t*t);
                if(!TryMove(next)){FallbackReason="dynamic exit obstruction";Clock.UseFallback(previousAlpha);}
            }
            if(Clock.Phase==MorpgDashPhase.Invisible && previousPhase==MorpgDashPhase.Exit)
                if(!TryMove(exitPosition)){FallbackReason="dynamic exit obstruction";Clock.UseFallback(previousAlpha);}
            if(Clock.Phase==MorpgDashPhase.Invisible && previousPhase==MorpgDashPhase.FallbackOut && warped)
            {
                if(!TargetClear()){error="fallback landing dynamically blocked";return false;}
                actor.transform.position=targetPosition;var body=actor.GetComponent<Rigidbody2D>();
                if(body!=null){body.position=targetPosition;body.linearVelocity=Vector2.zero;body.angularVelocity=0;}
            }
            if(Clock.Phase==MorpgDashPhase.Entry && previousPhase!=Clock.Phase)
                if(!animation.PlaySkillBodyAction(clip,.36f,false)){FallbackReason="entry body unavailable";Clock.UseFallback(previousAlpha);}
            if(Clock.Phase==MorpgDashPhase.Entry)
            {
                float t=Clock.Progress;var next=Vector3.Lerp(stagingPosition,targetPosition,1-(1-t)*(1-t)*(1-t));
                if(!TryMove(next)){FallbackReason="dynamic entry obstruction";Clock.UseFallback(previousAlpha);}
            }
            if(Clock.Phase==MorpgDashPhase.Settle && previousPhase!=Clock.Phase)
            {
                if(!TryMove(targetPosition)){error="landing became blocked";return false;}
                if(ownsBody){animation.StopSkillBodyAction();animation.PlayIdle();}
            }
            ApplyPresentation();return true;
        }
        internal bool TargetClear()
        {
            var body=actor.GetComponent<Rigidbody2D>();
            float radius=MorpgEnvironmentRuntime.ActorRadius(body)+.15f;
            foreach(var collider in Physics2D.OverlapCircleAll(targetPosition,radius))
                if(!collider.isTrigger && (body==null || collider.attachedRigidbody!=body) && !collider.transform.IsChildOf(actor.transform))return false;
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
                var hits=new RaycastHit2D[16];var filter=new ContactFilter2D{useTriggers=false,useLayerMask=false};
                int count=body.Cast(delta.normalized,filter,hits,delta.magnitude+.15f);
                for(int i=0;i<count;i++)if(hits[i].collider!=null && hits[i].collider.attachedRigidbody!=body && hits[i].distance<delta.magnitude+.15f)return false;
            }
            actor.transform.position=target;
            if(body!=null){body.position=target;body.linearVelocity=Vector2.zero;body.angularVelocity=0;}
            return true;
        }
        internal void ApplyPresentation()
        {
            if(disposed)return;
            if(actor==null || !actor.gameObject.activeInHierarchy){Cancelled?.Invoke();return;}
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
            if(ownsBody)animation?.StopSkillBodyAction();
            if(ownsFacing){animation.ReleaseFacingLock(this);animation.SetDirection(facing);}
            if(ownsBody && actor!=null && !actor.RuntimeData.isDead)animation.PlayIdle();
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
