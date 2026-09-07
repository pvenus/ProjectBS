from pathlib import Path
import subprocess,tempfile,json
root=Path(__file__).resolve().parents[2];out=root/'Artifacts/Engineering/SeojinManualControl/rootfix-05'
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
results={}
for phase,source in [('before',out/'before/Assets/Scripts/Actor/Character/Control/ManualControlCore.cs'),('after',root/'Assets/Scripts/Actor/Character/Control/ManualControlCore.cs')]:
 exe=Path(tempfile.mkdtemp(prefix='projectbs-rootfix-'))/'replay.exe'
 subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-nowarn:0649,0414','-langversion:9.0','-out:'+str(exe),str(source),str(root/'AgentTools/SeojinManualHarness/Stub.cs'),str(root/'AgentTools/SeojinManualHarness/RootfixReplay.cs'),str(root/'Assets/Scripts/Ability/Skills/Definitions/equipment/SkillAimMode.cs')],check=True,capture_output=True,text=True)
 results[phase]=json.loads(subprocess.check_output([str(mono/'bin/mono'),str(exe)],text=True))
assert results['before']==dict(held=False,pending=True,chain=False,basicCalls=1,cancelCount=0),results
assert results['after']==dict(held=True,pending=False,chain=True,basicCalls=2,cancelCount=0),results
(out/'root-cause-replay.json').write_text(json.dumps(results,indent=2)+'\n')
print('PASS 1/1 before/after production-core cooldown starvation replay')
