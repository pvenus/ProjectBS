from pathlib import Path
import subprocess,tempfile
root=Path(__file__).resolve().parents[2]
def method(path,sig):
 s=(root/path).read_text();a=s.index(sig);indent=s[s.rfind('\n',0,a)+1:a];b=s.index('{',a)
 end=s.index('\n'+indent+'}',b)+len(indent)+2
 return s[a:end]
g='Assets/Editor/tools/skill/EquipmentSkillJsonGenerator.cs';h='Assets/Scripts/Ability/Skills/Services/Helpers/SkillUseHelper.cs';a='Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs'
s=(root/'AgentTools/SeojinManualHarness/AimPipeline.template.cs').read_text()
for key,path,sigs in [('GENERATOR',g,['public class EquipmentSkillJson','private class EquipmentSkillRootJson','private static EquipmentSkillJson ParseEquipmentSkillJson(','private static string ExtractJsonValue(','private static int FindTopLevelProperty(','private static string ExtractBalancedJson(','private static void ApplySkillFields(']),('HELPER',h,['public static bool UseSkillProjectilesAndSelfEffects(','public static bool FireProjectiles(','public static Vector2 ResolveDirection(','public static Vector2 ResolveTargetPoint(']),('TARGET',a,['private Transform NormalizeComboTarget(','private bool IsValidComboTarget(','private Transform ResolveComboRetarget(','internal Transform ResolveManualTarget('])]:
 s=s.replace('/* '+key+' */','\n'.join(method(path,sig) for sig in sigs))
out=Path(tempfile.mkdtemp(prefix='projectbs-aim-pipeline-'));(out/'Tests.cs').write_text(s)
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-nowarn:0649,0414','-langversion:9.0','-r:'+str(mono/'lib/mono/4.5/System.Web.Extensions.dll'),'-out:'+str(out/'test.exe'),str(out/'Tests.cs'),str(root/'AgentTools/SeojinManualHarness/Stub.cs'),str(root/'Assets/Scripts/Ability/Skills/Definitions/equipment/SkillAimMode.cs'),str(root/'Assets/Scripts/Ability/Skills/Definitions/equipment/EquipmentSkillSO.cs')],check=True)
subprocess.run([str(mono/'bin/mono'),str(out/'test.exe')],cwd=root,check=True)
