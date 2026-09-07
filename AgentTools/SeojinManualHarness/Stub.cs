using System;using System.Collections.Generic;using System.Linq;
namespace UnityEngine {
 public struct LayerMask{public int value;public static implicit operator LayerMask(int x)=>new LayerMask{value=x};}
 public class DefaultExecutionOrder:Attribute {public DefaultExecutionOrder(int n){}}public class DisallowMultipleComponent:Attribute {}public class SerializeField:Attribute {}
 public class Object {}public class Component:Object{public GameObject gameObject;public Transform transform=>gameObject.transform;public T GetComponent<T>() where T:class=>gameObject.GetComponent<T>();public T GetComponentInParent<T>() where T:class=>GetComponent<T>();public T GetComponentInChildren<T>(bool includeInactive=false) where T:class=>GetComponent<T>();public T[] GetComponentsInChildren<T>(bool all=false) where T:class=>gameObject.GetComponents<T>();}
 public class Behaviour:Component{public bool enabled=true;public bool isActiveAndEnabled=>enabled&&gameObject.activeInHierarchy;}public class MonoBehaviour:Behaviour {}
 public class Transform:Component{public Transform parent;public Vector3 localPosition,localScale=Vector3.one;public Quaternion localRotation;public void SetParent(Transform p,bool w){parent=p;}public Vector3 position;public Transform root=>this;public Vector2 right=>Vector2.right;public int GetInstanceID()=>gameObject.Id;}
 public class GameObject:Object{static int next;public int Id=++next;public int layer;readonly List<Component> components=new();public bool activeInHierarchy=true;public Transform transform;public string name;public GameObject(string n=""){name=n;transform=new Transform{gameObject=this};components.Add(transform);}public T AddComponent<T>() where T:Component,new(){var t=new T{gameObject=this};components.Add(t);return t;}public T GetComponent<T>() where T:class=>components.OfType<T>().FirstOrDefault();public T[] GetComponents<T>() where T:class=>components.OfType<T>().ToArray();}
 public struct Vector2{public static Vector2 one=>new(1,1);public static Vector2 operator /(Vector2 a,float b)=>new(a.x/b,a.y/b);public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}public float sqrMagnitude=>x*x+y*y;public Vector2 normalized=>sqrMagnitude==0?zero:new Vector2(x/(float)Math.Sqrt(sqrMagnitude),y/(float)Math.Sqrt(sqrMagnitude));public static Vector2 operator -(Vector2 a,Vector2 b)=>new(a.x-b.x,a.y-b.y);public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.x+b.x,a.y+b.y);public static Vector2 operator *(Vector2 a,float b)=>new(a.x*b,a.y*b);public static float Dot(Vector2 a,Vector2 b)=>a.x*b.x+a.y*b.y;public static float Distance(Vector2 a,Vector2 b)=>(float)Math.Sqrt((a-b).sqrMagnitude);public static Vector2 zero=>new(0,0);public static Vector2 right=>new(1,0);public static implicit operator Vector2(Vector3 p)=>new(p.x,p.y);}
 public struct Vector3{public static Vector3 zero=>default;public static Vector3 one=>new(1,1,1);public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}public static Vector3 operator +(Vector3 a,Vector3 b)=>new(a.x+b.x,a.y+b.y,a.z+b.z);public static Vector3 operator *(Vector3 a,float b)=>new(a.x*b,a.y*b,a.z*b);}
 public struct Rect{public Vector2 size=>new(xMax-xMin,yMax-yMin);public float xMin,yMin,xMax,yMax;public static Rect MinMaxRect(float l,float b,float r,float t)=>new Rect{xMin=l,yMin=b,xMax=r,yMax=t};}
 public struct Ray{public Vector3 origin,direction;}
 public class Camera:Behaviour{public static Camera main;public float orthographicSize=5,aspect=16f/9;public int RayReads;public Ray ScreenPointToRay(Vector3 p){RayReads++;return new Ray{origin=new Vector3(p.x,p.y,-10),direction=new Vector3(0,0,1)};}}
 public static class Mathf{public const float Rad2Deg=57.295779513f;public static float Atan2(float y,float x)=>(float)Math.Atan2(y,x);public static float Max(float a,float b)=>Math.Max(a,b);public static int Max(int a,int b)=>Math.Max(a,b);public static float Abs(float n)=>Math.Abs(n);}public static class Time{public static int frameCount;public static float unscaledTime;public static float timeScale=1;}
 public enum KeyCode{W,A,S,D,Alpha1,Alpha2,Alpha3,LeftShift,RightShift,Space}
 public static class Input{public static HashSet<KeyCode> Held=new(),Down=new();public static bool MouseHeld,MouseDown;public static Vector3 mousePosition;public static bool GetKey(KeyCode k)=>Held.Contains(k);public static bool GetKeyDown(KeyCode k)=>Down.Contains(k);public static bool GetMouseButton(int n)=>MouseHeld;public static bool GetMouseButtonDown(int n)=>MouseDown;public static void Clear(){Held.Clear();Down.Clear();MouseHeld=MouseDown=false;}}
 public static class Application{public static bool isFocused=true;}
}
namespace UnityEngine.EventSystems{public class EventSystem{public static EventSystem current;public bool Over;public UnityEngine.GameObject currentSelectedGameObject;public bool IsPointerOverGameObject()=>Over;}}
namespace Skill{
 public static class SkillPoolSlotKeys{public const string BasicAttack="basic",Active1="a1",Active2="a2",Active3="a3",Active4="a4";}
 public class SkillHitSO{public UnityEngine.LayerMask TargetLayerMask=1;}public enum SkillComponentType{Mobility,Damage}public class Profile{public SkillComponentType SkillComponentType;}
 public enum TargetingType{None,Self,AutoTarget,AutoTargetDirection,Directional,Position}
 public class SkillCastSO{public TargetingType TargetingType=TargetingType.AutoTarget;public bool SnapshotTargetPointOnCast;public float Range=100;}
 public class Equipment{public SkillAimMode AimMode=SkillAimMode.GroundPoint;public AimInputSource AimInputSource=AimInputSource.MouseDirection;public SkillCastSO CastSo=new();public Profile BaseProfileSo=new();public SkillHitSO[] HitSos={new SkillHitSO()};}
 public class Combo{public bool Enabled,IsComplete=true;}
 public class EquipmentSkillRuntimeData{public float resolvedRange=100;public int Slot;public Equipment sourceEquipment=new();public Combo comboProfile;}
 public class Pool{public Dictionary<string,EquipmentSkillRuntimeData> Runtimes=new();public EquipmentSkillRuntimeData GetRuntimeByKey(string key)=>Runtimes.TryGetValue(key,out var r)?r:null;}
}
namespace Character{
 public enum CharacterType{Player,Npc}public class CharacterSO{public CharacterType CharacterType;public string CharacterId="character.seojin.1";}
 public class Runtime{public CharacterSO characterSO=new();public bool isDead;}
 public class CharacterManager:UnityEngine.MonoBehaviour{public bool IsTargetable=true;public Runtime RuntimeData=new();public bool IsDying,IsStunned,IsRooted;}
 public class CharacterStateManager:UnityEngine.MonoBehaviour{public bool Forced;public int ClearCount;public void ClearState(){ClearCount++;}public bool TryGetForcedTarget(out UnityEngine.Transform t){t=null;return Forced;}}
 public class CharacterSkillManager:UnityEngine.MonoBehaviour{public HashSet<int> Blocked=new();public bool ManualBusy;public Skill.Pool SkillPool=new();public int Clears;public List<(int slot,UnityEngine.Vector2 point)> Calls=new();public List<Skill.ManualSkillAim> Aims=new();public Func<Skill.ManualSkillAim> StepAim;public int TargetLookups;public UnityEngine.Transform LockedTarget;public UnityEngine.Transform ResolveManualTarget(Skill.EquipmentSkillRuntimeData r,UnityEngine.Vector2 d){TargetLookups++;return LockedTarget;}public bool FireManualAim(Skill.EquipmentSkillRuntimeData r,Skill.ManualSkillAim a,Func<bool> c,Func<Skill.ManualSkillAim> next=null){StepAim=next;if(!a.IsValid)return false;Aims.Add(a);Calls.Add((r.Slot,a.Mode==Skill.SkillAimMode.Direction?a.Direction:a.Point));return true;}public bool ManualReady(Skill.EquipmentSkillRuntimeData r)=>r!=null&&!ManualBusy&&!Blocked.Contains(r.Slot);public bool FireManualAtPoint(Skill.EquipmentSkillRuntimeData r,UnityEngine.Vector2 p,Func<bool> c){Calls.Add((r.Slot,p));return true;}public bool FireManualDash(Skill.EquipmentSkillRuntimeData r,UnityEngine.Vector2 d){Calls.Add((4,d));return true;}public void CancelManualExecution(){Clears++;ManualBusy=false;}}
}
public class PartyMovementMono:UnityEngine.MonoBehaviour{public bool Controlled;public UnityEngine.Vector2 Input;public bool IsMovementControlledByPlayer()=>Controlled;public void SetOwnedManualInput(bool c,UnityEngine.Vector2 i){Controlled=c;Input=i;}public void ReleaseManualControlForTeardown(bool c){Controlled=c;Input=UnityEngine.Vector2.zero;}}
public class MovementMono:UnityEngine.MonoBehaviour{public bool Knockback;public int Stops;public bool IsKnockingBack()=>Knockback;public void StopAllMotion(bool n=true){Stops++;}}
public class SkillBrainMono:UnityEngine.MonoBehaviour{}public class SkillExecutorMono:UnityEngine.MonoBehaviour{public int Clears;public void ClearRequest(){Clears++;}}
public class AnimationMono:UnityEngine.MonoBehaviour{public enum DiagonalDirection{UpLeft,UpRight,DownLeft,DownRight}public DiagonalDirection CurrentDirection;public void BeginSynchronousTeardown(){}public void EndSynchronousTeardown(){}}
namespace Session{public class GameSession{public static GameSession Instance=new();public BattleSession BattleSession=new();}public class BattleSession{public BattleRuntime BattleRuntime=new();}public class BattleRuntime{public bool isCompleted;}}
namespace Battle{public static class BattleMapBoundsContext{public static bool IsActive=true;public static UnityEngine.Rect Arena=UnityEngine.Rect.MinMaxRect(0,-4,32,4);}}
namespace Battle.Morpg{public static class BattleMorpgLiveRoute{public static bool Locked;public static bool IsTransitionLocked(Character.CharacterManager c)=>Locked;}public class MorpgEnvironmentRuntime{public static MorpgEnvironmentRuntime Active;public ZoneData Zone;public class Point{public float x,y;}public class ZoneData{public Point[] walkable;}}}

namespace UnityEngine{
 public struct Quaternion{public float angle;public static Quaternion identity=>default;public static Quaternion Euler(float x,float y,float z)=>new(){angle=z};}
 public struct Color{public static Color white=>default;}
 public class Sprite{public Rect rect=Rect.MinMaxRect(0,0,512,512);public float pixelsPerUnit=100;}
 public class SpriteRenderer:Behaviour{public Sprite sprite;public Color color;public int sortingLayerID,sortingOrder;}
 public static class Resources{public static int Loads;public static bool Missing;public static T Load<T>(string path) where T:new(){Loads++;return Missing?default:new T();}}
}
namespace Battle.Presentation{
 public class BattleCharacterAuraBinding:UnityEngine.MonoBehaviour{public BattleCharacterAuraView AuraView;}
 public class BattleCharacterAuraView:UnityEngine.MonoBehaviour{public UnityEngine.SpriteRenderer BackArcRenderer,FrontArcRenderer;public void SetSelectionActive(bool selected){enabled=selected;}public UnityEngine.Vector3 PositionOffset=new(0,-.25f,0);public UnityEngine.Vector2 VisualScale=new(.2058f,.15435f);}
}
