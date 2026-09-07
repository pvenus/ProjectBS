using System;using System.Collections;using System.Collections.Generic;
struct Vector2 {public float x,y;public Vector2(float a,float b){x=a;y=b;}public float sqrMagnitude=>x*x+y*y;public Vector2 normalized=>sqrMagnitude==0?zero:new Vector2(x/(float)Math.Sqrt(sqrMagnitude),y/(float)Math.Sqrt(sqrMagnitude));public static Vector2 zero=>new(0,0);public static Vector2 ClampMagnitude(Vector2 x,float m)=>x.sqrMagnitude>m*m?x.normalized:x;}
static class Time {public static float time=100;}
class GameObject {public bool activeInHierarchy=true;}
class Behaviour {public GameObject gameObject=new();public bool enabled=true;public bool isActiveAndEnabled=>enabled&&gameObject.activeInHierarchy;}
class Coroutine {}
class AnimationClip {}
class DummyDirectionLease {public void Restore(){}}
class AnimationMono:Behaviour {
 DummyDirectionLease directedBody=new();
 enum AnimationState{None,Idle,Move,Attack,Death}
 AnimationState _currentState,_previousLocomotionState;int _currentDirection;bool _isDead,_isPlayingOneShot,_playingAttackDisabledCc,_holdingSkillCastPose;object _facingLockOwner;AnimationClip _currentClip;Coroutine _playRoutine,_oneShotRoutine;float _currentLocomotionPlaybackRate;int _presentationTeardownDepth;
 public int Starts,Stops;AnimationClip clip=new();public bool Clean=>_currentState==AnimationState.None&&_currentClip==null&&_playRoutine==null&&_oneShotRoutine==null&&!_isPlayingOneShot&&!_holdingSkillCastPose&&_facingLockOwner==null;
 public void Seed(){_currentState=AnimationState.Attack;_currentClip=clip;_playRoutine=new();_oneShotRoutine=new();_isPlayingOneShot=true;_holdingSkillCastPose=true;_facingLockOwner=new();}
 public void Disable(){ResetInactivePresentation();}public void Destroy(){ResetInactivePresentation();}
 public void DirectState(){PlayState(AnimationState.Idle,true);}public void AdjacentStart(){StartPresentationRoutine(PlayLoopClipRoutine(clip));}
 public bool IsPlayingAttack()=>_isPlayingOneShot;public void SetDirectionFromVector(Vector2 v){}bool IsAttackDisabledByCc()=>false;float ResolveLocomotionPlaybackRate()=>1;void UpdateDirectionFromMovementController(){}AnimationClip GetClip(AnimationState s,int d)=>clip;bool CanPlayClip(AnimationClip c,AnimationState s,int d)=>c!=null;
 IEnumerator PlayLoopClipRoutine(AnimationClip c){yield return null;}
 Coroutine StartCoroutine(IEnumerator e){if(!isActiveAndEnabled||!gameObject.activeInHierarchy)throw new Exception("inactive coroutine start");Starts++;return new();}
 void StopCoroutine(Coroutine c){Stops++;}void EndComboPresentation(){}void RestoreFacingMirror(){}
 /* ANIMATION */
}
class PartyMovementMono:Behaviour {
 enum MovePhase{ComputePosition}bool _isMovementControlledByPlayer=true;Vector2 _manualMoveInput=new(1,0);float _lastManualInputTime=100,_phaseTimer=5;MovePhase _phase;public AnimationMono _animationMono;public Vector2 Velocity=new(2,0);public int PresentationCalls;
 public bool Cleared=>_manualMoveInput.sqrMagnitude==0&&Velocity.sqrMagnitude==0&&float.IsNegativeInfinity(_lastManualInputTime)&&_phaseTimer==0&&!_isMovementControlledByPlayer;
 void StopMovement(){Velocity=Vector2.zero;}public void NextFrame(Vector2 v){UpdateMovementAnimation(v);}
 /* MOVEMENT */
}
class Adapter {public Action Callback;public void Clear(bool x){Callback?.Invoke();}}
class Core {public bool Cleared;public void Reset(){Cleared=true;}}
class State {public Action Callback;public void ClearState(){Callback?.Invoke();}}
class SkillExecutorMono {public void ClearRequest(){}}
class Owner {
 public AnimationMono Animation;public PartyMovementMono movement;public Adapter adapter=new();public Core core=new();public State state=new();public Dictionary<Behaviour,bool> ai=new();bool baselineMovement,aiAllowed;
 T GetComponentInChildren<T>(bool b) where T:class=>Animation as T;T GetComponent<T>() where T:class=>new SkillExecutorMono() as T;
 public void Disable(){OnDisable();}
 /* OWNER */
}
static class Tests {
 static int count;static void Test(string name,Action a){a();count++;Console.WriteLine("PASS "+name);}static void Check(bool b){if(!b)throw new Exception("assertion");}
 static (AnimationMono a,PartyMovementMono m,Owner o) Rig(){var a=new AnimationMono();var m=new PartyMovementMono{_animationMono=a,gameObject=a.gameObject};var o=new Owner{Animation=a,movement=m};o.adapter.Callback=()=>a.PlayIdle();o.state.Callback=()=>a.PlayMove();return(a,m,o);}
 static void Main(){
 Test("active-component-disable-scope-zero-presentation-starts",()=>{var x=Rig();x.a.Seed();x.o.Disable();Check(x.a.Starts==0&&x.a.Clean&&x.m.Cleared&&x.o.core.Cleared);});
 Test("parent-inactive-owner-teardown-no-exception-no-coroutine",()=>{var x=Rig();x.a.Seed();x.a.gameObject.activeInHierarchy=false;x.o.Disable();Check(x.a.Starts==0&&x.a.Clean&&x.m.Cleared);});
 Test("animation-disabled-direct-idle-move-state-final-guard",()=>{var x=Rig();x.a.Seed();x.a.enabled=false;x.a.PlayIdle();x.a.PlayMove();x.a.DirectState();Check(x.a.Starts==0&&x.a.Clean);});
 Test("inactive-parent-direct-idle-and-adjacent-coroutine-final-guard",()=>{var x=Rig();x.a.Seed();x.a.gameObject.activeInHierarchy=false;x.a.PlayIdle();x.a.AdjacentStart();Check(x.a.Starts==0&&x.a.Clean);});
 Test("repeat-disable-destroy-idempotent-without-start",()=>{var x=Rig();x.a.Seed();for(int i=0;i<20;i++){x.o.Disable();x.a.Disable();x.a.Destroy();}Check(x.a.Starts==0&&x.a.Clean&&x.m.Cleared);});
 Test("reenable-first-idle-one-start-no-stale-clip-dedup",()=>{var x=Rig();x.a.Seed();x.a.gameObject.activeInHierarchy=false;x.o.Disable();x.a.gameObject.activeInHierarchy=true;x.m.SetOwnedManualInput(true,Vector2.zero);x.m.NextFrame(Vector2.zero);Check(x.a.Starts==1&&!x.a.IsPlayingAttack());});
 Test("reenable-first-move-one-start-no-stale-oneshot",()=>{var x=Rig();x.a.Seed();x.a.enabled=false;x.a.Disable();x.a.enabled=true;x.m.NextFrame(new Vector2(1,0));x.m.NextFrame(new Vector2(1,0));Check(x.a.Starts==1&&!x.a.IsPlayingAttack());});
 Test("teardown-scope-balanced-after-callback-exception",()=>{var x=Rig();x.o.adapter.Callback=()=>throw new Exception("callback");try{x.o.Disable();}catch(Exception){}x.a.PlayIdle();Check(x.a.Starts==1);});
 Test("nested-teardown-blocks-until-outer-release",()=>{var x=Rig();x.a.BeginSynchronousTeardown();x.o.Disable();x.a.PlayIdle();Check(x.a.Starts==0);x.a.EndSynchronousTeardown();x.a.PlayIdle();Check(x.a.Starts==1);});
 Test("legacy-inactive-movement-toggle-does-not-call-presentation",()=>{var x=Rig();x.m.gameObject.activeInHierarchy=false;x.m.SetOwnedManualInput(false,Vector2.zero);Check(x.a.Starts==0);});
 Console.WriteLine($"PASS {count}/{count} production teardown-method simulations");
 }
}
