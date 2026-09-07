// Headless simulation only. Production glue is compiled from Assets, never copied.
using System;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine {
 public class Object {
  public static void Destroy(Object o) { if(o is GameObject g) { g.active=false; g.destroyed=true; } }
 }
 public class Component:Object {
  public GameObject gameObject; public Transform transform=>gameObject.transform;
  public T GetComponent<T>() where T:class=>gameObject.GetComponent<T>();
  public T GetComponentInParent<T>() where T:class=>gameObject.GetComponentInParent<T>();
  public T[] GetComponents<T>() where T:class=>gameObject.GetComponentsInChildren<T>(false);
  public T[] GetComponentsInChildren<T>(bool inactive=false) where T:class=>gameObject.GetComponentsInChildren<T>(inactive);
 }
 public class Behaviour:Component { public bool enabled=true; }
 public class MonoBehaviour:Behaviour {}
 public class Transform:Component { public Vector3 position; public Vector3 localScale=new(1,1,1); public Quaternion rotation; public Transform parent; public bool IsChildOf(Transform other){for(var t=this;t!=null;t=t.parent)if(t==other)return true;return false;} public void SetParent(Transform p,bool world){parent=p;} }
 public class GameObject:Object {
  public static string ThrowOnAddType;
  public static readonly List<GameObject> All=new(); public string name; public bool active=true,destroyed; public bool activeSelf=>active; public bool activeInHierarchy=>active && !destroyed && (transform.parent?.gameObject.activeInHierarchy??true); public Transform transform;
  readonly List<Component> components=new();
  public GameObject(string name){this.name=name;transform=new Transform{gameObject=this};components.Add(transform);All.Add(this);}
  public void SetActive(bool a){active=a;}
  public T AddComponent<T>() where T:Component,new(){if(ThrowOnAddType==typeof(T).Name){ThrowOnAddType=null;throw new Exception("injected component construction failure");}var c=new T{gameObject=this};components.Add(c);return c;}
  public T GetComponent<T>() where T:class=>components.OfType<T>().FirstOrDefault();
  public T GetComponentInParent<T>() where T:class=>GetComponent<T>()??transform.parent?.gameObject.GetComponentInParent<T>();
  public T[] GetComponentsInChildren<T>(bool inactive) where T:class=>components.OfType<T>().Concat(All.Where(g=>g.transform.parent==transform && (inactive||g.activeInHierarchy)).SelectMany(g=>g.GetComponentsInChildren<T>(inactive))).ToArray();
 }
 public struct Vector2 {
 public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}public static Vector2 zero=>new(0,0);public static Vector2 right=>new(1,0);public float sqrMagnitude=>x*x+y*y;public static float Distance(Vector2 a,Vector2 b)=>(a-b).magnitude;
 public static implicit operator Vector2(Vector3 v)=>new(v.x,v.y);public float magnitude=>(float)Math.Sqrt(x*x+y*y);public Vector2 normalized=>magnitude==0?zero:this*(1/magnitude);
 public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.x+b.x,a.y+b.y);public static Vector2 operator -(Vector2 a,Vector2 b)=>new(a.x-b.x,a.y-b.y);public static Vector2 operator *(Vector2 a,float b)=>new(a.x*b,a.y*b);
 public static Vector2 Lerp(Vector2 a,Vector2 b,float t)=>a+(b-a)*t;
 }
 public struct Vector3 {public static Vector3 one=>new(1,1,1);public static Vector3 operator +(Vector3 a,Vector3 b)=>new(a.x+b.x,a.y+b.y,a.z+b.z);public static Vector3 operator *(Vector3 a,float b)=>new(a.x*b,a.y*b,a.z*b);public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}public static Vector3 operator -(Vector3 a,Vector3 b)=>new(a.x-b.x,a.y-b.y,a.z-b.z);public static Vector3 Lerp(Vector3 a,Vector3 b,float t)=>new(a.x+(b.x-a.x)*t,a.y+(b.y-a.y)*t,a.z+(b.z-a.z)*t);}
 public struct Quaternion {private float angle;public static Quaternion Euler(float x,float y,float z)=>new Quaternion{angle=z};public static Vector3 operator *(Quaternion q,Vector3 p){double t=q.angle*Math.PI/180;return new Vector3((float)(p.x*Math.Cos(t)-p.y*Math.Sin(t)),(float)(p.x*Math.Sin(t)+p.y*Math.Cos(t)),p.z);}}
 public struct Bounds {public Vector3 center;public Vector3 size;public Vector3 extents=>new(size.x/2,size.y/2,size.z/2);public bool Intersects(Bounds b)=>Math.Abs(center.x-b.center.x)<=(size.x+b.size.x)/2 && Math.Abs(center.y-b.center.y)<=(size.y+b.size.y)/2;}
 public enum SpriteMeshType {FullRect}
 public class Sprite:Object {public Texture2D texture=new();public static Sprite Create(Texture2D texture,Rect rect,Vector2 pivot,float ppu,uint extrude,SpriteMeshType mesh)=>new Sprite{texture=texture,rect=rect,pivot=new Vector2(rect.width*pivot.x,rect.height*pivot.y),pixelsPerUnit=ppu};public Rect rect=new Rect(0,0,1536,512);public float pixelsPerUnit=100;public Vector2 pivot=new Vector2(768,256);public Bounds bounds=new Bounds{size=new Vector3(10.24f,10.24f,0)};}
 public class Shader:Object {public bool isSupported=true;}
 public class Material:Object { public Shader shader;public Dictionary<string,float> Floats=new();public Material(Shader s){shader=s;}public void SetFloat(string key,float value){Floats[key]=value;} }
 public class SpriteRenderer:Behaviour {public Material sharedMaterial;public Sprite sprite;public int sortingOrder,sortingLayerID;public bool flipX;public Bounds? TestBounds;public Color color=new Color(1,1,1,1);public Bounds bounds=>TestBounds??new Bounds{center=transform.position,size=sprite?.bounds.size??default};}
 public class Collider2D:Behaviour {public Rigidbody2D attachedRigidbody=>GetComponent<Rigidbody2D>();public bool isTrigger;public Bounds bounds;}
 public class CircleCollider2D:Collider2D {public float radius;}
 public class BoxCollider2D:Collider2D {public Vector2 size;}
 public class PolygonCollider2D:Collider2D {public Vector2[] points;public int pathCount;public readonly Dictionary<int,Vector2[]> Paths=new();public void SetPath(int i,Vector2[] p){Paths[i]=p;}}

 public struct Rect {public float x=>xMin;public float y=>yMin;public float xMin,yMin,xMax,yMax;public float width=>xMax-xMin;public float height=>yMax-yMin;public Rect(float x,float y,float w,float h){xMin=x;yMin=y;xMax=x+w;yMax=y+h;}public static Rect MinMaxRect(float a,float b,float c,float d)=>new(a,b,c-a,d-b);}
 public class Rigidbody2D:Component {public Vector2 position,linearVelocity;public float angularVelocity;public Collider2D BlockCast;public int Cast(Vector2 d,ContactFilter2D f,List<RaycastHit2D> hits,float distance){if(BlockCast==null)return 0;hits.Add(new RaycastHit2D{collider=BlockCast,distance=0});return 1;}public int Cast(Vector2 d,ContactFilter2D f,RaycastHit2D[] hits,float distance){if(BlockCast==null)return 0;hits[0]=new RaycastHit2D{collider=BlockCast,distance=0};return 1;}}
 public class Camera:Behaviour {public static Camera main;public float orthographicSize=3,aspect=16f/9f;public bool orthographic=true;}
 public static class Time {public static float timeScale=1;public static int frameCount;}
 public static class Debug {public static readonly List<string> Messages=new();public static void Log(string s){Messages.Add(s);}public static void LogError(string s){}public static void LogWarning(string s){}public static void LogException(System.Exception e){} }
 public class TextAsset {public string text;public TextAsset(string s){text=s;}}
 public static class Resources {public static readonly Dictionary<string,object> Items=new();public static T Load<T>(string name) where T:class=>Items.TryGetValue(name,out var o)?o as T:null;}
 public static class GUI {public static Color color;public static void DrawTexture(Rect r,Texture2D t){}public static void Box(Rect r,string s){}public static void Label(Rect r,string s){} }
}
namespace Stat {public enum StatType {Experience}}
namespace Character {
 public class CharacterSO {public string name;}
 public class CharacterRuntimeData {public bool isDead;public CharacterSO characterSO;}
 public class CharacterManager:UnityEngine.MonoBehaviour {
  public static event Action<CharacterManager> OnAnyCharacterDied;
  public CharacterRuntimeData RuntimeData=new(); public float xp;public bool CanMove=true;
  public float GetStatValue(Stat.StatType t)=>xp;
  public bool ThrowNextStat;public void SetStat(Stat.StatType t,float v){xp=v;if(ThrowNextStat){ThrowNextStat=false;throw new Exception("stat callback");}}
  public void Die(){RuntimeData.isDead=true;OnAnyCharacterDied?.Invoke(this);}
 }
 public class CharacterSkillManager:UnityEngine.MonoBehaviour {public void CancelManualExecution(){}public void CancelCasting(){} }
}
public class MovementMono:UnityEngine.MonoBehaviour {public void StopAllMotion(){} }
namespace Battle {
 public class BattleSO {public string BattleId;}
 public class BattleRuntime {public float rewardExperience=10;public UnityEngine.Sprite backgroundSprite=new();}
 public class BattlePlayerCameraFollowMono:UnityEngine.MonoBehaviour {public void ResetSmoothing(){} }
 public static class BattleMapBoundsContext {
  public static UnityEngine.Rect Zone;
  public static void Activate(UnityEngine.Vector2 size,float inset){}
  public static void Activate(UnityEngine.Rect bounds,float inset){}
  public static void Clear(){}
  public static void SetActorZone(UnityEngine.Rect zone){Zone=zone;}
  public static UnityEngine.Vector2 ClampCameraCenter(UnityEngine.Vector2 p,UnityEngine.Camera c)=>Morpg.MorpgEnvironmentRuntime.Active?.ClampCamera(p)??p;
 }
}
namespace Session {
 public class BattleSession {public Battle.BattleSO BattleSO;public Battle.BattleRuntime BattleRuntime=new();}
 public class GameSession {public static GameSession Instance=new();public StageSession StageSession=new();public ProgressionSession ProgressionSession=new();}
 public class StageSession {public Currency.CurrencyRutimeData CurrencyRuntimeData=new();}
 public class ProgressionSession {public RunId RunId=new();}
 public class RunId {public string Value="run-test";}
}
namespace Party {public class PartyManager {public static PartyManager Instance=new();public List<Character.CharacterManager> Members=new();}}
public enum SpawnUnitRole {Melee}
public class SpawnUnitRequest {public string Key;public SpawnUnitRequest(string key,SpawnUnitRole role){Key=key;}}
public interface ISpawnUnitResolver {Character.CharacterSO Resolve(SpawnUnitRequest request);}
public class EnemyRegistry {public static EnemyRegistry Instance=new();public void UnregisterEnemy(UnityEngine.GameObject root){} }
public class NpcSpawnService {
 public static NpcSpawnService Instance=new(); public bool FailNext; public readonly List<Character.CharacterManager> Spawned=new();
 public UnityEngine.GameObject SpawnNpc(Character.CharacterSO so,UnityEngine.Vector3 position,float rotation,object runtime,UnityEngine.Transform parent,Func<UnityEngine.GameObject,bool> prepare){
  if(FailNext){FailNext=false;return null;}
  var root=new UnityEngine.GameObject(so.name);root.transform.SetParent(parent,false);root.transform.position=position;
  var c=root.AddComponent<Character.CharacterManager>();c.RuntimeData.characterSO=so;
  if(!prepare(root))return null;root.transform.SetParent(null,true);Spawned.Add(c);return root;
 }
}

public class MovementController:UnityEngine.MonoBehaviour {}
public class KnockbackController:UnityEngine.MonoBehaviour {}
public class PartyMovementMono:UnityEngine.MonoBehaviour {public bool TryAcquireExternalMovement(object o,bool b)=>true;public void ReleaseExternalMovement(object o){}}
public class SkillBrainMono:UnityEngine.MonoBehaviour {}
public class SkillExecutorMono:UnityEngine.MonoBehaviour {}
namespace UnityEngine {public struct Color {public float a;public Color(float r,float g,float b,float a){this.a=a;}}public static class Screen {public static int width=800,height=600;}public class Texture2D {public static Texture2D whiteTexture=new();}public static class Mathf {public static int RoundToInt(float f)=>(int)Math.Round(f);public static float Clamp01(float f)=>Math.Max(0,Math.Min(1,f));public static float Max(float a,float b)=>Math.Max(a,b);public static float Abs(float v)=>Math.Abs(v);public static float Clamp(float v,float a,float b)=>Math.Max(a,Math.Min(b,v));public static float MoveTowards(float v,float target,float d)=>v<target?Math.Min(target,v+d):Math.Max(target,v-d);}}

namespace Battle.Morpg {
 internal class MorpgRewardHudMono:UnityEngine.MonoBehaviour,IMorpgRewardPresentation {
  internal static bool EnableForTests;internal static MorpgRewardHudMono Last;
  internal readonly System.Collections.Generic.List<MorpgRewardVisualGroup> Groups=new();
  internal int Receipts;internal int? FinalGold;internal bool Available=true, ThrowTick, HoldArrivals;private System.Action missing;
  internal static MorpgRewardHudMono TryCreate(UnityEngine.Transform p,System.Action missing,int gold,float xp){
   if(!EnableForTests)return null;var view=new UnityEngine.GameObject("reward HUD").AddComponent<MorpgRewardHudMono>();view.missing=missing;Last=view;return view;
  }
  public bool isActiveAndEnabled=>Available;
  public bool IsAvailable=>Available;
  public bool TryShow(MorpgRewardVisualGroup g){Groups.Add(g);return true;}
  public void Refresh(MorpgRewardVisualGroup g){if(HoldArrivals)g.Age=0f;} public void Release(MorpgRewardVisualGroup g){Groups.Remove(g);}
  internal void NotifyCredited(int g,int q,int total,float xp){Receipts++;}internal void TickHud(float d,int gold,float xp){if(ThrowTick)throw new System.Exception("HUD anchor missing");} internal void DisposeView(){Available=false;}internal void SnapToAuthoritative(int gold,float xp){FinalGold=gold;}
  internal void LosePresentation(){Available=false;missing?.Invoke();}
 }
}

namespace UnityEngine {public class SerializeField:Attribute{}public class DisallowMultipleComponent:Attribute{}public class MinAttribute:Attribute{public MinAttribute(int value){}}}
namespace Character.UI {public class CharacterSkillCooldownSlot:UnityEngine.MonoBehaviour{}}
namespace Party.UI {public class CharacterBattleHudUI:UnityEngine.MonoBehaviour{}}
namespace Battle.Presentation {public class BattleCharacterAuraView:UnityEngine.MonoBehaviour{}}

namespace UnityEngine {
 public class AnimationClip {public float length=.5f;public int Samples;public float LastSampleTime;public void SampleAnimation(GameObject go,float time){Samples++;LastSampleTime=time;var sr=go.GetComponent<SpriteRenderer>();if(sr!=null)sr.sprite=new Sprite();}}
 public class Animator:Behaviour {public bool applyRootMotion;}
 public struct ContactFilter2D {public bool useTriggers,useLayerMask;}
 public struct RaycastHit2D {public Collider2D collider;public float distance;}
 public class DefaultExecutionOrder:Attribute {public DefaultExecutionOrder(int value){}}
}
namespace Character {
 public class AnimationMono:UnityEngine.MonoBehaviour {
  public enum DiagonalDirection {UpRight,UpLeft,DownRight,DownLeft}
  public DiagonalDirection CurrentDirection;public bool BodyAvailable=true;public int BodyPlays,Idles;
  public UnityEngine.SpriteRenderer TransitionBodyRenderer=>GetComponent<UnityEngine.SpriteRenderer>();
  public bool AcquireFacingLock(object o,UnityEngine.Vector2 d){CurrentDirection=DiagonalDirection.DownRight;return true;}
  public bool PlaySkillBodyAction(UnityEngine.AnimationClip c,float d,bool mirror){BodyPlays++;return BodyAvailable;}
  public void StopSkillBodyAction(){}public void PlayIdle(){Idles++;}public void ReleaseFacingLock(object o){}public void SetDirection(DiagonalDirection d){CurrentDirection=d;}
 }
}

namespace UnityEngine {public static class Physics2D {public static Collider2D[] Overlaps=System.Array.Empty<Collider2D>();public static Collider2D[] OverlapCircleAll(Vector2 p,float r)=>Overlaps;}}
