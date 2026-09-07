"""Run actual spawn service + extracted CharacterManager lifecycle methods with a coroutine-rejecting Unity model."""
from pathlib import Path
import tempfile, subprocess
root=Path('/Users/pvenus/ProjectBS')
out=Path(tempfile.mkdtemp(prefix='projectbs-inactive-spawn.',dir='/private/tmp'))
manager=(root/'Assets/Scripts/Actor/Character/CharacterManager.cs').read_text()
def method(signature):
    start=manager.index(signature);brace=manager.index('{',start);depth=1;end=brace+1
    while depth:
        depth += (manager[end]=='{')-(manager[end]=='}');end+=1
    return manager[start:end]
# Check both real initialization entry points use the lifecycle helper, not direct coroutine launch.
for signature in ['public void InitializeFromSO(', 'public void Initialize(CharacterRuntimeData']:
    body=method(signature)
    assert 'ResetSpawnPresentationForInitialization();' in body and 'RequestSpawnReveal();' in body
    assert 'StartCoroutine(' not in body
methods='\n'.join(method(s) for s in ['private void OnEnable()', 'private void OnDisable()',
    'private void ResetSpawnPresentationForInitialization()', 'private void RequestSpawnReveal()',
    'private void TryStartPendingSpawnReveal()', 'private System.Collections.IEnumerator PlaySpawnRevealNextFrame()'])
stub=(root/'AgentTools/MorpgIntegrationHarness/RuntimeStub.cs').read_text()
stub=stub[:stub.index('public class NpcSpawnService {')]
stub=stub.replace('private float angle;', 'private float angle;public static Quaternion identity=>new();')
stub=stub.replace('public class CharacterManager:','public partial class CharacterManager:')
stub=stub.replace('public class CharacterSO {public string name;}','public class CharacterSO {public string name; public bool FailInitialization;}')
stub=stub.replace('public class MonoBehaviour:Behaviour {}','''public class Coroutine { public System.Collections.IEnumerator routine; }
 public class MonoBehaviour:Behaviour {
  public bool isActiveAndEnabled=>enabled&&gameObject.activeInHierarchy;
  public readonly List<Coroutine> Routines=new();public int Starts;
  public Coroutine StartCoroutine(System.Collections.IEnumerator r){if(!isActiveAndEnabled)throw new Exception("inactive coroutine");Starts++;var c=new Coroutine{routine=r};r.MoveNext();Routines.Add(c);return c;}
  public void StopCoroutine(Coroutine c){Routines.Remove(c);}
  public void NextFrame(){foreach(var c in Routines.ToArray())if(!c.routine.MoveNext())Routines.Remove(c);}
 }''')
stub=stub.replace('public Transform parent; public void SetParent(Transform p,bool world){parent=p;}', 'public Transform parent; public void SetParent(Transform p,bool world){bool was=gameObject.activeInHierarchy;parent=p;gameObject.NotifyLifecycle(was);}')
stub=stub.replace('public void SetActive(bool a){active=a;}', '''public void NotifyLifecycle(bool was){if(was==activeInHierarchy)return;foreach(var c in components){var m=c.GetType().GetMethod(activeInHierarchy?"OnEnable":"OnDisable",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);m?.Invoke(c,null);}}
  public void SetActive(bool a){bool was=activeInHierarchy;active=a;NotifyLifecycle(was);}
  public int GetInstanceID()=>All.IndexOf(this)+1;
  public T GetComponentInChildren<T>() where T:class=>GetComponent<T>();''')
stub=stub.replace('public static class Debug {public static void LogError(string s){} }','public static class Debug {public static void LogError(string s){}public static void LogException(Exception e){} }')
stub=stub.replace('public class EnemyRegistry {public static EnemyRegistry Instance=new();public void UnregisterEnemy(UnityEngine.GameObject root){} }','''public class EnemyRegistry {public static EnemyRegistry Instance=new();public bool ThrowAfterRegister;public readonly List<UnityEngine.GameObject> ActiveEnemies=new();public void RegisterEnemy(UnityEngine.GameObject root){ActiveEnemies.Add(root);if(ThrowAfterRegister)throw new Exception("registry observer");}public void UnregisterEnemy(UnityEngine.GameObject root){ActiveEnemies.Remove(root);}}''')
stub+='''
namespace UnityEngine {public struct Color {public Color(float r,float g,float b,float a){}} public class Texture2D {}}
public class StatusStub {public void SuspendIndomitableProjection(Character.CharacterManager c){}}
public class ShaderControllerMono:UnityEngine.MonoBehaviour {public int Reveals;public void PlaySpawnReveal(){Reveals++;}}
public class AnimationMono:UnityEngine.MonoBehaviour {public void SetDirectionFromVector(UnityEngine.Vector2 v){}}
namespace Character {public partial class CharacterManager {
 private UnityEngine.Coroutine spawnRevealRoutine,npcDeathPresentationRoutine;
 private bool spawnRevealPending,isDying;private CharacterManager lastHitAttacker;private StatusStub statusTickService;
 public bool IsDying=>isDying;
 public void SeedDying(){isDying=true;}
 public void InitializeFromSO(CharacterSO so){ResetSpawnPresentationForInitialization();if(so.FailInitialization)throw new Exception("initialization failure");RuntimeData=new CharacterRuntimeData{characterSO=so};RequestSpawnReveal();}
'''+methods+'''
}}
namespace Character.Helper {public static class CharacterBuilder {
 public static UnityEngine.GameObject Pooled,Last;
 public static UnityEngine.GameObject CreateOrBuildNpcObject(object prefab,string name,UnityEngine.Transform parent,UnityEngine.Vector3 pos,UnityEngine.Quaternion rotation,string layer,object sprite,bool collider){
 var root=Pooled??new UnityEngine.GameObject(name);Pooled=null;root.transform.SetParent(parent,false);root.transform.position=pos;Last=root;return root;
 }
}}
public class SpawnSequenceRuntime {public readonly HashSet<int> Tracked=new();public void AddEnemyTracking(int id){Tracked.Add(id);}public void RemoveEnemyTracking(int id){Tracked.Remove(id);}public List<Step> StepRuntimes=new();public SpawnSequenceRuntime(SpawnSequenceSO s){} }
public class Step {public UnityEngine.Vector3 AnchorPosition;public UnityEngine.Vector2 AnchorOffset;public bool IsCanvasCoordinate;}
public class SpawnContentSO {}
public class SpawnSequenceSO {}
public class SpawnContentRuntime {public SpawnContentRuntime(SpawnContentSO s){}public UnityEngine.Vector3 AnchorPosition;public UnityEngine.Vector2 AnchorOffset;public bool IsCanvasCoordinate;}
public class SpawnContentRunner {public void Start(SpawnContentRuntime r,SpawnSequenceRuntime s,ISpawnUnitResolver u){}}
public class SpawnSequenceRunner {public void StartSequence(SpawnSequenceRuntime r,Action a,ISpawnUnitResolver u){}}
public static class SpawnCoordinateUtility {public static UnityEngine.Vector2 GetLookVector(float z)=>new();}
'''
(out/'Stub.cs').write_text(stub)
(out/'Program.cs').write_text(r'''
using System;using UnityEngine;using Character;using Character.Helper;
class Program {
 static int pass;static void Check(bool b,string s){if(!b)throw new Exception(s);}
 static void Test(string s,Action a){EnemyRegistry.Instance=new EnemyRegistry();a();pass++;Console.WriteLine("PASS "+s);}
 static void Main(){
 foreach(string role in new[]{"black","chain"}){
 Test(role+"-direct-inactive-initialize-defers-reveal",()=>{
 var root=new GameObject("inactive direct");var cm=root.AddComponent<CharacterManager>();root.SetActive(false);
 cm.InitializeFromSO(new CharacterSO{name=role});Check(cm.Starts==0&&cm.Routines.Count==0,"inactive direct coroutine");
 root.SetActive(true);Check(cm.Starts==1&&cm.Routines.Count==1,"deferred reveal not unique");
 });
 Test(role+"-inactive-initialize-then-enable-once",()=>{
 var stage=new GameObject("staging");stage.SetActive(false);var so=new CharacterSO{name=role};bool prepared=false;
 var root=NpcSpawnService.Instance.SpawnNpc(so,new Vector3(3,4,0),0,null,stage.transform,g=>{
 var c=g.GetComponent<CharacterManager>();Check(!g.activeInHierarchy&&c.Starts==0,"coroutine started under inactive parent");Check(EnemyRegistry.Instance.ActiveEnemies.Count==0,"ownership published living NPC early");prepared=true;return true;});
 Check(root!=null&&prepared&&root.activeInHierarchy,"spawn failed");var cm=root.GetComponent<CharacterManager>();Check(cm.RuntimeData.characterSO==so,"returned NPC not initialized");Check(cm.Starts==1,"enable did not schedule exactly once");Check(EnemyRegistry.Instance.ActiveEnemies.Count==1,"living registry");cm.NextFrame();Check(cm.Routines.Count==0,"reveal not completed");
 });
 Test(role+"-pooled-reuse-cancels-old-reveal-and-death-state",()=>{
 var pooled=new GameObject("pool");var cm=pooled.AddComponent<CharacterManager>();var shader=pooled.AddComponent<ShaderControllerMono>();cm.InitializeFromSO(new CharacterSO{name="prior"});cm.SeedDying();pooled.SetActive(false);CharacterBuilder.Pooled=pooled;
 var stage=new GameObject("stage");stage.SetActive(false);var so=new CharacterSO{name=role};
 var root=NpcSpawnService.Instance.SpawnNpc(so,new Vector3(8,9,0),0,null,stage.transform,g=>true);
 Check(root==pooled&&cm.RuntimeData.characterSO==so&&!cm.IsDying,"pooled state stale");Check(cm.Routines.Count==1,"duplicate or missing reveal");cm.NextFrame();Check(shader.Reveals==1,"old reveal survived reuse");Check(root.transform.position.x==8,"pooled position stale");
 });
 }
 Test("active-legacy-spawn-still-reveals",()=>{
 var root=NpcSpawnService.Instance.SpawnNpc(new CharacterSO{name="legacy"},new Vector3(),0,null);Check(root!=null&&root.GetComponent<CharacterManager>().Starts==1,"legacy changed");
 });
 Test("initialization-rejection-never-enters-living-registry",()=>{
 var stage=new GameObject("stage");stage.SetActive(false);
 var root=NpcSpawnService.Instance.SpawnNpc(new CharacterSO{name="bad",FailInitialization=true},new Vector3(),0,null,stage.transform,g=>true);
 Check(root==null&&EnemyRegistry.Instance.ActiveEnemies.Count==0&&CharacterBuilder.Last.destroyed,"partial initialization published");
 });
 Test("ownership-rejection-never-enters-living-registry",()=>{
 var stage=new GameObject("stage");stage.SetActive(false);var root=NpcSpawnService.Instance.SpawnNpc(new CharacterSO{name="black"},new Vector3(),0,null,stage.transform,g=>false);
 Check(root==null&&EnemyRegistry.Instance.ActiveEnemies.Count==0&&CharacterBuilder.Last.destroyed,"rejected root published");
 });
 Test("registry-observer-fault-removes-sequence-and-living-tracking",()=>{
 EnemyRegistry.Instance.ThrowAfterRegister=true;var sequence=new SpawnSequenceRuntime(new SpawnSequenceSO());
 var root=NpcSpawnService.Instance.SpawnNpc(new CharacterSO{name="chain"},new Vector3(),0,sequence);
 Check(root==null&&sequence.Tracked.Count==0&&EnemyRegistry.Instance.ActiveEnemies.Count==0,"failed publication leaked tracking");
 });
 Console.WriteLine("PASS "+pass+"/"+pass+" inactive spawn lifecycle simulations");
 }
}
''')
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
cmd=[str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-langversion:9.0','-out:'+str(out/'tests.exe'),str(out/'Stub.cs'),str(out/'Program.cs'),str(root/'Assets/Scripts/Battle/Spawn/Sequence/service/NpcSpawnService.cs'),str(root/'Assets/Scripts/Collection/Currency/CurrencyRutimeData.cs')]
subprocess.run(cmd,check=True,cwd=root)
subprocess.run([str(mono/'bin/mono'),str(out/'tests.exe')],check=True,cwd=root)
