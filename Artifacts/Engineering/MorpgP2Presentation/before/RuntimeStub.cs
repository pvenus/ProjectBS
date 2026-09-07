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
  public T[] GetComponentsInChildren<T>(bool inactive=false) where T:class=>gameObject.GetComponentsInChildren<T>(inactive);
 }
 public class Behaviour:Component { public bool enabled=true; }
 public class MonoBehaviour:Behaviour {}
 public class Transform:Component { public Vector3 position; public Transform parent; public void SetParent(Transform p,bool world){parent=p;} }
 public class GameObject:Object {
  public static readonly List<GameObject> All=new(); public string name; public bool active=true,destroyed; public Transform transform;
  readonly List<Component> components=new();
  public GameObject(string name){this.name=name;transform=new Transform{gameObject=this};components.Add(transform);All.Add(this);}
  public void SetActive(bool a){active=a;}
  public T AddComponent<T>() where T:Component,new(){var c=new T{gameObject=this};components.Add(c);return c;}
  public T GetComponent<T>() where T:class=>components.OfType<T>().FirstOrDefault();
  public T GetComponentInParent<T>() where T:class=>GetComponent<T>()??transform.parent?.gameObject.GetComponentInParent<T>();
  public T[] GetComponentsInChildren<T>(bool inactive) where T:class=>components.OfType<T>().ToArray();
 }
 public struct Vector2 {public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}public static Vector2 zero=>new(0,0);public static implicit operator Vector2(Vector3 v)=>new(v.x,v.y);}
 public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}}
 public struct Rect {public float xMin,yMin,xMax,yMax;public Rect(float x,float y,float w,float h){xMin=x;yMin=y;xMax=x+w;yMax=y+h;}public static Rect MinMaxRect(float a,float b,float c,float d)=>new(a,b,c-a,d-b);}
 public class Rigidbody2D:Component {public Vector2 position,linearVelocity;public float angularVelocity;}
 public class Camera:Behaviour {public static Camera main;}
 public static class Time {public static float timeScale=1;public static int frameCount;}
 public static class Debug {public static void LogError(string s){} }
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
  public CharacterRuntimeData RuntimeData=new(); public float xp;
  public float GetStatValue(Stat.StatType t)=>xp;
  public bool ThrowNextStat;public void SetStat(Stat.StatType t,float v){xp=v;if(ThrowNextStat){ThrowNextStat=false;throw new Exception("stat callback");}}
  public void Die(){RuntimeData.isDead=true;OnAnyCharacterDied?.Invoke(this);}
 }
 public class CharacterSkillManager:UnityEngine.MonoBehaviour {public void CancelCasting(){} }
}
public class MovementMono:UnityEngine.MonoBehaviour {public void StopAllMotion(){} }
namespace Battle {
 public class BattleSO {public string BattleId;}
 public class BattleRuntime {public float rewardExperience=10;}
 public class BattlePlayerCameraFollowMono:UnityEngine.MonoBehaviour {public void ResetSmoothing(){} }
 public static class BattleMapBoundsContext {
  public static UnityEngine.Rect Zone;
  public static void Activate(UnityEngine.Vector2 size,float inset){}
  public static void SetActorZone(UnityEngine.Rect zone){Zone=zone;}
  public static UnityEngine.Vector2 ClampCameraCenter(UnityEngine.Vector2 p,UnityEngine.Camera c)=>p;
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
namespace UnityEngine {public struct Color {public Color(float r,float g,float b,float a){}}public static class Screen {public static int width=800,height=600;}public class Texture2D {public static Texture2D whiteTexture=new();}public static class Mathf {public static float Clamp01(float f)=>Math.Max(0,Math.Min(1,f));}}
