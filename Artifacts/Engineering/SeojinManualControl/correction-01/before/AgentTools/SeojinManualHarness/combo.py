from pathlib import Path
import subprocess, tempfile
root=Path(__file__).resolve().parents[2]
def method(path,name):
 s=(root/path).read_text(); start=s.index('        private '+name); a=s.index('{',start); depth=1; end=a+1
 while depth:
  depth+=(s[end]=='{')-(s[end]=='}');end+=1
 return s[start:end]
combo=method('Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs','IEnumerator FireComboRoutine(')
guard=method('Assets/Scripts/Actor/Character/CharacterSkillManager.cs','System.Collections.IEnumerator GuardManualRoutine(')
s=(root/'AgentTools/SeojinManualHarness/Combo.template.cs').read_text().replace('/* COMBO */',combo).replace('/* GUARD */',guard)
out=Path(tempfile.mkdtemp(prefix='projectbs-manual-combo-'));src=out/'Tests.cs';src.write_text(s)
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-langversion:9.0','-out:'+str(out/'test.exe'),str(src)],check=True)
subprocess.run([str(mono/'bin/mono'),str(out/'test.exe')],check=True)
