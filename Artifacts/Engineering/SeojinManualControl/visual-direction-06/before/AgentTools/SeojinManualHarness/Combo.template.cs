using System; using System.Collections; using System.Collections.Generic; using System.Linq;
enum SkillAimMode{Direction,Target,GroundPoint,Self}
struct ManualSkillAim {public SkillAimMode Mode;public Vector2 Direction;}
struct Vector2 {public float x,y;public Vector2(float a,float b){x=a;y=b;}public float sqrMagnitude=>x*x+y*y;public Vector2 normalized=>new Vector2(x/(float)Math.Sqrt(sqrMagnitude),y/(float)Math.Sqrt(sqrMagnitude));public static Vector2 right=>new Vector2(1,0);public static Vector2 operator -(Vector2 a,Vector2 b)=>new Vector2(a.x-b.x,a.y-b.y);public static float Distance(Vector2 a,Vector2 b)=>1;}
class Transform {public Vector2 position;public Vector2 right=>Vector2.right;public int GetInstanceID()=>1;}
class WaitForSeconds {public float value;public WaitForSeconds(float v){value=v;}}
static class Mathf {public static float Max(float a,float b)=>Math.Max(a,b);}
static class Time {public static float deltaTime=.02f;}
class Cast {public float Range=10;}
class Equipment {public string EquipmentId="skill.character.seojin.1.basic_attack.basic_attack";public Cast CastSo=new();}
class SkillComboStep {public int ComboIndex;public float StartTime,HitTime,RecoveryEnd,LungeDistance=.12f,DamageWeight,MinimumVisualLifetime=.15f;public object Hit=new(),BodyActionClip=new(),BodyPresentationCalibration,VfxClip=new(),VfxProfileOverride,VfxPresentationCalibration;}
class Profile {public bool HasCompleteSegmentedBodyRegistry=true,UseCanonicalBodyChoreography;public object SegmentedBodyActionClip=new();public int SegmentedBodyFrameCount=18;public SkillComboStep[] Steps={new(){ComboIndex=0,StartTime=0,HitTime=.16f,RecoveryEnd=.28f,DamageWeight=.25f},new(){ComboIndex=1,StartTime=.48f,HitTime=.64f,RecoveryEnd=.68f,DamageWeight=.3f},new(){ComboIndex=2,StartTime=.78f,HitTime=.94f,RecoveryEnd=1.06f,DamageWeight=.45f}};}
class EquipmentSkillRuntimeData {public Profile comboProfile=new();public Equipment sourceEquipment=new();}
class CharacterSkillManager {public int Success;public void NotifySkillUseSucceeded(EquipmentSkillRuntimeData r){Success++;}}
class AnimationMono {public int Stops,Fallback,Sync,Starts;public List<Vector2> Directions=new();public void SetDirectionFromVector(Vector2 d){Directions.Add(d);}public void RestartContinuousComboAction(object c,Profile p,object n){Starts++;}public void RestartCanonicalComboChoreography(int i,float a,float b){}public void RestartComboAction(object c,float a,float b,object d){}public void RestartAttack(){Fallback++;}public void SynchronizeContinuousComboContact(object c,SkillComboStep s,int f){Sync++;}public void StopComboAction(){Stops++;}}
static class MainCharacterSkillFocusFeature {public static void NotifySkillExecuting(object a,object b,object c){}public static void CancelPending(object c){}}
class Harness {
 public Dictionary<int,int> comboGenerations=new();public HashSet<int> activeComboCasters=new();int nextComboToken;
 public AnimationMono Anim=new();public float Clock;public bool Held=true;public int Cooldowns,Retargets;public List<(int index,float time,bool suppress,Vector2 point)> Hits=new();public List<float> CooldownTimes=new();public List<SkillAimMode?> Modes=new();public List<bool> Points=new();
 bool RollComboCritical(Transform c)=>false;AnimationMono ResolveAnimation(Transform c)=>Anim;bool CanContinueGeneration(Transform c,int g)=>comboGenerations.TryGetValue(1,out int n)&&n==g;
 bool UseSkill(CharacterSkillManager s,EquipmentSkillRuntimeData r){Cooldowns++;CooldownTimes.Add(Clock);return true;}
 Transform NormalizeComboTarget(Transform t)=>t;bool IsValidComboTarget(Transform c,Transform t,object h)=>t!=null;bool IsComboTargetInRange(Transform c,Transform t,float r)=>t!=null;
 Transform ResolveComboRetarget(Transform c,float r,object h){Retargets++;return null;}/* DIRECTION */
 /* FACING */
 void ApplyCollisionSafeLunge(Transform c,Transform t,Vector2 d,float l){}
 bool UseSkillOnce(CharacterSkillManager s,EquipmentSkillRuntimeData r,Transform c,Transform t,bool up,Vector2 p,int hit,object clip,object profile,object calibration,string token,bool crit,float weight,int index,object h,bool suppress,float life,ManualSkillAim? manualAim=null){Modes.Add(manualAim?.Mode);Points.Add(up);Hits.Add((index,Clock,suppress,p));return true;}
 public IEnumerator Start(EquipmentSkillRuntimeData r=null,bool point=true,ManualSkillAim? aim=null){comboGenerations[1]=1;activeComboCasters.Add(1);return FireComboRoutine(new(),r??new(),new(){position=new Vector2(12,8)},null,point,new Vector2(9,8),()=>Held,1,aim);}
 public void Run(IEnumerator e,Action<float> beforeResume=null){while(e.MoveNext()){Clock+=e.Current is WaitForSeconds w?w.value:Time.deltaTime;beforeResume?.Invoke(Clock);}}
 /* COMBO */
}
class Guard {
 public int manualEpoch;public HashSet<int> manualRoutines=new();
 public IEnumerator Wrap(IEnumerator r,int ticket){manualRoutines.Add(ticket);return GuardManualRoutine(r,manualEpoch,ticket,true);}
 public void Cancel(){manualEpoch++;manualRoutines.Clear();}
 /* GUARD */
}
static class Tests {
 static int count;static void Check(bool b,string n){if(!b)throw new Exception(n);count++;Console.WriteLine("PASS "+n);}static bool Near(float a,float b)=>Math.Abs(a-b)<.0001;
 static int lateHits;static IEnumerator Child(){yield return new WaitForSeconds(.5f);lateHits++;}static IEnumerator Parent(){yield return Child();}
 static void Main(){
 var h=new Harness();h.Run(h.Start(),t=>{if(t>=.1f)h.Held=false;});Check(h.Hits.Count==1&&Near(h.Hits[0].time,.16f)&&h.Cooldowns==0&&h.Anim.Stops==1,"tap-release-completes-current-hit-no-second-step-no-cooldown");
 h=new();h.Run(h.Start());Check(h.Hits.Select(x=>x.index).SequenceEqual(new[]{0,1,2})&&h.Hits.Select(x=>x.suppress).SequenceEqual(new[]{true,true,false})&&Near(h.Hits[2].time,.94f)&&Near(h.Clock,1.06f)&&h.Anim.Sync==1,"held-authoritative-cadence-body-contact-VFX-001");Check(h.Cooldowns==1&&Near(h.CooldownTimes[0],.78f),"cooldown-on-third-step-entry-once");Check(h.Retargets==0&&h.Hits.All(x=>x.point.x==9&&x.point.y==8),"snapshot-all-three-hits-no-target-following");
 Check(h.Anim.Directions.Count==3&&h.Anim.Directions.All(d=>d.x==-1&&d.y==0),"basic-mouse-snapshot-controls-all-three-production-facing-calls");
 h=new();h.Run(h.Start(),t=>{if(t>=.6f)h.Held=false;});Check(h.Hits.Count==2&&h.Cooldowns==0,"release-during-second-hit-finishes-second-only");
 h=new();h.Run(h.Start(),t=>{if(t>=.8f){h.comboGenerations.Remove(1);}});Check(h.Cooldowns==1&&h.Hits.Count==2,"cancel-after-third-entry-no-refund-no-delayed-hit");
 h=new();var old=h.Start();Check(old.MoveNext(),"old-combo-reaches-hit-wait");h.comboGenerations[1]=2;h.Run(old);Check(h.Hits.Count==0&&h.Anim.Stops==0&&h.comboGenerations[1]==2,"old-generation-cannot-hit-stop-or-clear-new-combo");
 var runtime=new EquipmentSkillRuntimeData();runtime.comboProfile.HasCompleteSegmentedBodyRegistry=false;foreach(var step in runtime.comboProfile.Steps){step.BodyActionClip=null;step.VfxClip=null;}h=new();h.Run(h.Start(runtime));Check(h.Hits.Count==3&&h.Anim.Fallback==3,"missing-body-clip-retains-gameplay-fallback-null-VFX-forwarded");
 h=new();h.Run(h.Start(point:false));Check(h.Hits.Count==0&&h.Cooldowns==0&&h.Retargets>0,"legacy-target-mode-retains-validation-fail-closed");
 var g=new Guard();var parent=g.Wrap(Parent(),1);parent.MoveNext();var child=(IEnumerator)parent.Current;child.MoveNext();g.Cancel();var fresh=g.Wrap(Child(),2);fresh.MoveNext();Check(!child.MoveNext()&&lateHits==0,"nested-coroutine-cancel-blocks-old-delayed-hit");parent.MoveNext();Check(g.manualRoutines.Contains(2),"old-coroutine-cleanup-preserves-new-ticket");fresh.MoveNext();Check(lateHits==1&&g.manualRoutines.Count==0,"new-epoch-executes-and-releases-own-ticket");
 h=new();h.Run(h.Start(point:false,aim:new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=new Vector2(-1,0)}));Check(h.Hits.Count==3&&h.Retargets==0&&h.Points.All(x=>!x)&&h.Modes.All(x=>x==SkillAimMode.Direction)&&h.Anim.Directions.All(x=>x.x==-1&&x.y==0),"Direction-basic-three-step-facing-hit-context-no-ground-point");
 Console.WriteLine($"PASS {count}/{count} extracted production combo/guard tests");
 }
}
