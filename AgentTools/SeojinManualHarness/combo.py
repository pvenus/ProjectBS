from pathlib import Path
import subprocess, tempfile, sys
root=Path(__file__).resolve().parents[2]
def method(path,name):
 s=(root/path).read_text(); start=s.index('        private '+name); a=s.index('{',start); depth=1; end=a+1
 while depth:
  depth+=(s[end]=='{')-(s[end]=='}');end+=1
 return s[start:end]
combo=method('Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs','IEnumerator FireComboRoutine(')
guard=method('Assets/Scripts/Actor/Character/CharacterSkillManager.cs','System.Collections.IEnumerator GuardManualRoutine(')
s=(root/'AgentTools/SeojinManualHarness/Combo.template.cs').read_text().replace('/* COMBO */',combo).replace('/* GUARD */',guard).replace('/* DIRECTION */',method('Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs','Vector2 ResolveDirection(')).replace('/* FACING */',method('Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs','void ApplyAnimationDirection('))
if '--step-aim' in sys.argv:
 extra=(root/'AgentTools/SeojinManualHarness/StepAim.tests.cs').read_text()
 s=s.replace(' Console.WriteLine($"PASS {count}/{count}',extra+'\n Console.WriteLine($"PASS {count}/{count}')
 core=(root/'Assets/Scripts/Actor/Character/Control/ManualControlCore.cs').read_text()
 start=core.index('    internal readonly struct ControlVector');end=core.index('    internal struct AimSnapshot',start)
 s+='\n'+core[start:end]
 for file,signature,cls in [('Assets/Scripts/Actor/Character/Control/ManualControlCore.cs','internal static ControlVector KeyboardMoveDirection','ManualControlCore'),('Assets/Scripts/Actor/Character/Control/SeojinManualControl.cs','internal static ControlVector ResolveComboStepDirection','ManualGameplayAdapter')]:
  code=(root/file).read_text();start=code.index(signature);end=code.index(';',start)+1
  s+='\nclass '+cls+' {'+code[start:end]+'}'
out=Path(tempfile.mkdtemp(prefix='projectbs-manual-combo-'));src=out/'Tests.cs';src.write_text(s)
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-langversion:9.0','-out:'+str(out/'test.exe'),str(src)],check=True)
subprocess.run([str(mono/'bin/mono'),str(out/'test.exe')],check=True)
