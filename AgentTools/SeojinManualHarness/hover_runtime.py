from pathlib import Path
import subprocess,tempfile,re,json,hashlib,sys
root=Path(__file__).resolve().parents[2];out=Path(tempfile.mkdtemp(prefix='hover-runtime-'))
def extract(path,signature):
 s=(root/path).read_text();start=s.index(signature);a=s.index('{',start);end=a+1;depth=1
 while depth:depth+=(s[end]=='{')-(s[end]=='}');end+=1
 return s[start:end]
s=(root/'AgentTools/SeojinManualHarness/VisualDirection.template.cs').read_text().split('class ActualFacingProbe')[0]
s=s.replace('public struct Vector2{','public struct Vector2{public static Vector2 zero=>default;public static Vector2 operator +(Vector2 a,Vector2 b)=>new Vector2(a.x+b.x,a.y+b.y);public static Vector2 operator -(Vector2 a,Vector2 b)=>new Vector2(a.x-b.x,a.y-b.y);public static Vector2 operator *(Vector2 a,float b)=>new Vector2(a.x*b,a.y*b);public static implicit operator Vector3(Vector2 a)=>new Vector3(a.x,a.y,0);public static implicit operator Vector2(Vector3 a)=>new Vector2(a.x,a.y);')
s=s.replace('public struct Vector3{','public struct Vector3{public static Vector3 zero=>default;public static Vector3 one=>new Vector3(1,1,1);public static Vector3 forward=>new Vector3(0,0,1);public static Vector3 operator +(Vector3 a,Vector3 b)=>new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);')
s=s.replace('public struct Quaternion{','public struct Quaternion{public static Quaternion identity=>default;public static Quaternion AngleAxis(float a,Vector3 v)=>Euler(0,0,a);')
s=s.replace('public static class Mathf{','public static class Mathf{public const float Epsilon=1e-10f;public static float Max(float a,float b)=>Math.Max(a,b);public static int Max(int a,int b)=>Math.Max(a,b);public static int RoundToInt(float a)=>(int)Math.Round(a);')
s=s.replace('public class Component{','public class Component{public T GetComponentInChildren<T>(bool includeInactive=false) where T:Component{var here=GetComponent<T>();if(here!=null)return here;foreach(var child in transform.children){var c=child.GetComponentInChildren<T>(includeInactive);if(c!=null)return c;}return null;}')
s=s.replace('public class Transform:Component{','public class Transform:Component{public Vector3 position;public Vector3 right=>new Vector3((float)Math.Cos(rotation.angle/Mathf.Rad2Deg),(float)Math.Sin(rotation.angle/Mathf.Rad2Deg),0);public int GetSiblingIndex()=>parent==null?0:parent.children.IndexOf(this);public void SetSiblingIndex(int i){if(parent==null)return;parent.children.Remove(this);parent.children.Insert(Math.Min(i,parent.children.Count),this);}public bool IsChildOf(Transform other){for(var p=parent;p!=null;p=p.parent)if(p==other)return true;return false;}')
s=s.replace('parent=p;p.children.Add(this);','parent?.children.Remove(this);parent=p;p?.children.Add(this);')
s+='\nnamespace UnityEngine{public class Collider2D:Component{}public class Rigidbody2D:Component{}public struct LayerMask{}public static class Debug{public static void LogError(string s){}public static void LogWarning(string s){}}}\n'
s+='\nnamespace Skill{public enum ProjectileMoveType{Linear,Warp,Hover,Orbit,Homing}}\n'
s+='\npublic class SkillUpgradeMono{public class SkillUpgradeData{public float projectileCountAdd;}}\n'
s+='\nnamespace Skills.Dto.Move{public class OrbitProjectileMoveDto:SkillMoveRuntimeDto{public override Skill.ProjectileMoveType MoveType=>Skill.ProjectileMoveType.Orbit;}public class HomingProjectileMoveDto:SkillMoveRuntimeDto{public override Skill.ProjectileMoveType MoveType=>Skill.ProjectileMoveType.Homing;}}\n'
s+='\nclass SkillProjectileOrbitMovement:SkillProjectileLinearMovement{public void SetRuntimeMaxProjectileCount(int i){}}class SkillProjectileHomingMovement:SkillProjectileLinearMovement{}\n'
factory='Assets/Scripts/Ability/Skills/Services/ProjectileFactory.cs'
clone=extract(('Artifacts/Engineering/SeojinManualControl/runtime-hover-08/before/'+factory) if '--before-clone' in sys.argv else factory,'private ProjectileRuntimeData CreateInstanceRuntimeData(')
fields=set(re.findall(r'source\.(\w+)',clone))|{'direction','spawnPosition','moveRuntime','orientManualPresentation'};types={'orientManualPresentation':'bool','spawnPosition':'Vector2','direction':'Vector2','rendererScale':'float','moveRuntime':'Skills.Dto.Move.SkillMoveRuntimeDto'}
s+='\nclass ProjectileRuntimeData{'+''.join('public '+types.get(f,'object')+' '+f+';' for f in sorted(fields))+'public int spawnOrder;public Vector2 NormalizedDirection=>direction.normalized;}\n'
s+='\nclass EquipmentBaseProfileSO{public static float NormalizeRendererScale(float f)=>f;}\n'
s+='\nclass FactoryProbe{public ProjectileRuntimeData Clone(ProjectileRuntimeData s)=>CreateInstanceRuntimeData(s,0);public Quaternion Rotation(ProjectileRuntimeData d)=>ResolveSpawnRotation(d);public Skills.Dto.Move.WarpProjectileMoveDto Warp(Skills.Dto.Move.WarpProjectileMoveDto d)=>CreateInstanceWarpMoveDto(d,default);private Vector2 ResolveSpawnPosition(ProjectileRuntimeData s,int i)=>s.spawnPosition;private Vector2 ResolveProjectileDirection(ProjectileRuntimeData s,int i)=>s.direction;private Skills.Dto.Move.SkillMoveRuntimeDto CreateInstanceMoveRuntimeDto(ProjectileRuntimeData s,int i,Vector2 p)=>s.moveRuntime;'+clone+extract(factory,'private Quaternion ResolveSpawnRotation(')+extract(factory,'private WarpProjectileMoveDto CreateInstanceWarpMoveDto(')+'}\n'
s='using Skills.Dto.Move;\n'+s+(root/'AgentTools/SeojinManualHarness/HoverRuntime.tests.cs').read_text()
test=out/'Test.cs';test.write_text(s)
sources=['Assets/Scripts/Ability/Skills/Presentation/SkillDirectionPresentation.cs','Assets/Scripts/Ability/Skills/Presentation/ProjectileDirectionPresentationLease.cs','Assets/Scripts/Ability/Skills/Projectiles/SkillProjectileMoveController.cs','Assets/Scripts/Ability/Skills/Projectiles/move/ISkillProjectileMovement.cs',*[f'Assets/Scripts/Ability/Skills/Projectiles/move/SkillProjectile{x}Movement.cs' for x in ['Hover','Linear','Warp']],*[f'Assets/Scripts/Ability/Skills/Runtime/DTO/move/{x}.cs' for x in ['SkillMoveRuntimeDto','HoverProjectileMoveDto','LinearProjectileMoveDto','WarpProjectileMoveDto']]]
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-nowarn:0649,0108','-langversion:9.0','-out:'+str(out/'test.exe'),str(test),*[str(root/p) for p in sources]],check=True)
result=subprocess.run([str(mono/'bin/mono'),str(out/'test.exe')],text=True,capture_output=True)
if '--before-clone' in sys.argv:
 assert result.returncode!=0 and 'actual Factory clone keeps manual mode' in result.stderr
 print('PASS negative control: saved production Factory clone loses manual flag; identical hierarchy regression fails before fix')
else:
 print(result.stdout,end='');assert result.returncode==0,result.stderr

