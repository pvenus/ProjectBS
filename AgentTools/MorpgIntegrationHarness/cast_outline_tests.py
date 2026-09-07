"""Compile and execute the production cast presenter with material/MPB lifecycle simulation."""
from pathlib import Path
import subprocess,tempfile
root=Path('/Users/pvenus/ProjectBS');out=Path(tempfile.mkdtemp(prefix='projectbs-cast-outline.',dir='/private/tmp'))
(out/'Stub.cs').write_text(r'''
using System;using System.Collections.Generic;using System.Linq;using System.Collections;
namespace UnityEngine {
 public class DisallowMultipleComponent:Attribute {}
 public class Object {public bool destroyed;public static void Destroy(Object o){if(o!=null&&!o.destroyed){o.destroyed=true;if(o is Material)Material.Live--;}}}
 public class GameObject:Object {readonly List<Component> c=new();public bool activeInHierarchy=true;
  public T AddComponent<T>() where T:Component,new(){var x=new T{gameObject=this};c.Add(x);Call(x,"OnEnable");return x;}
  public T GetComponent<T>() where T:class=>c.OfType<T>().FirstOrDefault();
  public T[] GetComponentsInChildren<T>(bool b) where T:class=>c.OfType<T>().ToArray();
  public void SetActive(bool a){activeInHierarchy=a;foreach(var x in c)Call(x,a?"OnEnable":"OnDisable");}
  static void Call(Component x,string name){x.GetType().GetMethod(name,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)?.Invoke(x,null);}
 }
 public class Component:Object {public GameObject gameObject;public T GetComponent<T>() where T:class=>gameObject.GetComponent<T>();public T[] GetComponentsInChildren<T>(bool b) where T:class=>gameObject.GetComponentsInChildren<T>(b);}
 public class Coroutine {}
 public class MonoBehaviour:Component {public bool enabled=true;public bool isActiveAndEnabled=>enabled&&gameObject.activeInHierarchy;public Coroutine StartCoroutine(IEnumerator r){if(!isActiveAndEnabled)throw new Exception("inactive coroutine");return new();}public void StopCoroutine(Coroutine c){} }
 public struct Color {public float r,g,b,a;public Color(float r,float g,float b,float a){this.r=r;this.g=g;this.b=b;this.a=a;}public static Color white=>new(1,1,1,1);public static Color red=>new(1,0,0,1);}
 public struct Color32 {byte r,g,b,a;public Color32(byte r,byte g,byte b,byte a){this.r=r;this.g=g;this.b=b;this.a=a;}public static implicit operator Color(Color32 c)=>new(c.r/255f,c.g/255f,c.b/255f,c.a/255f);}
 public class Shader:Object {static Dictionary<string,int> ids=new();public static int PropertyToID(string n){if(!ids.ContainsKey(n))ids[n]=ids.Count+1;return ids[n];}public static Shader Find(string n)=>new();}
 public class Material:Object {public static int Live;public Shader shader;public string name;public Material(Shader s){shader=s;Live++;}public Material(Material m){shader=m.shader;Live++;}}
 public class MaterialPropertyBlock {Dictionary<int,object> v=new();public void SetFloat(int i,float f){v[i]=f;}public float GetFloat(int i)=>v.TryGetValue(i,out var x)&&x is float f?f:0;public void SetColor(int i,Color c){v[i]=c;}public Color GetColor(int i)=>v.TryGetValue(i,out var x)&&x is Color c?c:default;public void Copy(MaterialPropertyBlock other){v=new(other.v);}}
 public class SpriteRenderer:Component {public Material sharedMaterial;MaterialPropertyBlock block=new();public void GetPropertyBlock(MaterialPropertyBlock b){b.Copy(block);}public void SetPropertyBlock(MaterialPropertyBlock b){block.Copy(b);}}
 public static class Resources {public static T Load<T>(string s) where T:class=>new Shader() as T;}
 public static class Mathf {public static float Clamp01(float f)=>Math.Max(0,Math.Min(1,f));public static float Lerp(float a,float b,float t)=>a+(b-a)*t;}
 public static class Time {public static float unscaledDeltaTime=.02f;}
}
public class ShaderMono:UnityEngine.MonoBehaviour {public void Reload(){} }
namespace Character {
 public class ComboBodyPresentationProxyMarker:UnityEngine.MonoBehaviour {}
 public enum CharacterType {Player,Npc,Boss}
 public class CharacterSO {public string CharacterId;public CharacterType CharacterType=CharacterType.Npc;}
 public class CharacterManager:UnityEngine.MonoBehaviour {public static event Action<CharacterManager> OnAnyCharacterDied;public void Die(){OnAnyCharacterDied?.Invoke(this);}}
}
''')
(out/'Program.cs').write_text(r'''
using System;using UnityEngine;using Character;
class Program {
 static int pass;static int outline=Shader.PropertyToID("_OutlineColor"),cast=Shader.PropertyToID("_CastEnabled"),custom=Shader.PropertyToID("_Unrelated");
 static void Check(bool b,string why){if(!b)throw new Exception(why);}static MaterialPropertyBlock Read(SpriteRenderer r){var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);return b;}
 static void Test(string n,Action a){a();Console.WriteLine("PASS "+n);pass++;}
 static void Main(){foreach(string id in new[]{"character.black_cloth_raider.1","character.chain_axe_enforcer.2"}){
 Test(id+"-white-red-complete-cancel-death-disable-pool",()=>{
 var root=new GameObject();var actor=root.AddComponent<CharacterManager>();var r=root.AddComponent<SpriteRenderer>();var originalShader=new Shader();var material=new Material(originalShader);r.sharedMaterial=material;
 var original=new MaterialPropertyBlock();original.SetColor(outline,Color.red);original.SetFloat(custom,17);r.SetPropertyBlock(original);
 var p=root.AddComponent<CharacterSkillCastPresentationMono>();var so=new CharacterSO{CharacterId=id};p.ConfigureNpcDefaults(so);
 Check(Read(r).GetColor(outline).Equals(Color.white),"default not white");int materials=Material.Live;
 Action begin=()=>{Check(p.BeginPresentation(.4f),"begin failed");Check(Read(r).GetColor(outline).Equals(Color.red),"cast not red");var tint=Read(r).GetColor(Shader.PropertyToID("_CastBaseColor"));Check(tint.r>tint.g&&tint.r>tint.b,"cast shader not red");};
 Action restored=()=>{Check(r.sharedMaterial==material,"material not restored");Check(material.shader==originalShader,"shared material mutated");Check(Read(r).GetColor(outline).Equals(Color.white),"red residue");Check(Read(r).GetFloat(custom)==17,"unrelated MPB lost");Check(Read(r).GetFloat(cast)==0&&!p.IsPresenting,"cast residue");Check(Material.Live==materials,"material leaked");};
 begin();p.CompletePresentation();restored();begin();p.CancelPresentation();restored();begin();actor.Die();restored();begin();root.SetActive(false);restored();root.SetActive(true);
 for(int i=0;i<20;i++){begin();p.ConfigureNpcDefaults(so);restored();}
 p.ConfigureNpcDefaults(new CharacterSO{CharacterId="character.seojin.1",CharacterType=CharacterType.Player});Check(!p.UsesNpcAttackPalette,"pool retained NPC lease");Check(Read(r).GetColor(outline).Equals(Color.red),"pre-NPC default MPB not restored");
 });}
 Test("player-and-unrelated-npc-default-material-MPB-unchanged",()=>{
 foreach(var so in new[]{new CharacterSO{CharacterId="character.seojin.1",CharacterType=CharacterType.Player},new CharacterSO{CharacterId="character.other.1"},new CharacterSO{CharacterId="character.black_cloth_raider.1",CharacterType=CharacterType.Player}}){
 var root=new GameObject();var r=root.AddComponent<SpriteRenderer>();r.sharedMaterial=new Material(new Shader());var b=new MaterialPropertyBlock();b.SetFloat(custom,33);b.SetColor(outline,Color.red);r.SetPropertyBlock(b);var p=root.AddComponent<CharacterSkillCastPresentationMono>();p.ConfigureNpcDefaults(so);Check(!p.UsesNpcAttackPalette&&Read(r).GetColor(outline).Equals(Color.red)&&Read(r).GetFloat(custom)==33,"non NPC modified");p.BeginPresentation(.5f);var color=Read(r).GetColor(Shader.PropertyToID("_CastBaseColor"));Check(color.b>color.r,"legacy palette changed");p.RestoreImmediate();}
 });
 Test("newer-material-owner-preserved",()=>{
 var root=new GameObject();var r=root.AddComponent<SpriteRenderer>();r.sharedMaterial=new Material(new Shader());var p=root.AddComponent<CharacterSkillCastPresentationMono>();p.ConfigureNpcDefaults(new CharacterSO{CharacterId="character.black_cloth_raider.1"});p.BeginPresentation(1);var newer=new Material(new Shader());r.sharedMaterial=newer;p.CancelPresentation();Check(r.sharedMaterial==newer,"new owner clobbered");Check(Read(r).GetColor(outline).Equals(Color.white)&&Read(r).GetFloat(cast)==0,"cue remained");
 });
 Console.WriteLine("PASS "+pass+"/"+pass+" production cast presenter simulations");
 }
}
''')
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-langversion:9.0','-out:'+str(out/'tests.exe'),str(out/'Stub.cs'),str(out/'Program.cs'),str(root/'Assets/Scripts/Actor/Character/presentation/CharacterSkillCastPresentationMono.cs')],check=True,cwd=root)
subprocess.run([str(mono/'bin/mono'),str(out/'tests.exe')],check=True,cwd=root)
# Verify the actual caller's NPC restoration stays after emission, including exceptional exits.
s=(root/'Assets/Scripts/Actor/Character/CharacterSkillManager.cs').read_text()
assert 'finally\n            {\n                if (restoreNpcAfterFire) castPresentation?.RestoreImmediate();' in s
assert 'if (!restoreNpcAfterFire) castPresentation?.CompletePresentation();' in s
shader=(root/'Assets/Resources/Shaders/CharacterCastProgress.shader').read_text()
assert 'if (_NpcCastOutlineEnabled > 0.5 && _CastEnabled > 0.5)' in shader
assert '_NpcCastOutlineEnabled ("NPC Cast Outline", Float) = 0' in shader
print('PASS NPC post-fire/finally ordering and shader opt-in boundaries')
