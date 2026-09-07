from pathlib import Path
import subprocess,tempfile
root=Path(__file__).resolve().parents[2];out=Path(tempfile.mkdtemp(prefix='seojin-hud-'))
methods,cases=(root/'AgentTools/SeojinManualHarness/Hud.tests.cs').read_text().split('// CASES')
s=(root/'AgentTools/SeojinManualHarness/Tests.cs').read_text().replace(' static void Main(){',methods+'\n static void Main(){').replace('  Console.WriteLine("PASS "+count+',cases+'\n  Console.WriteLine("PASS "+count+');p=out/'Tests.cs';p.write_text(s)
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-nowarn:0649','-langversion:9.0','-out:'+str(out/'test.exe'),str(root/'AgentTools/SeojinManualHarness/Stub.cs'),str(p),*[str(p) for p in (root/'Assets/Scripts/Actor/Character/Control').glob('*.cs')],str(root/'Assets/Scripts/Ability/Skills/Definitions/equipment/SkillAimMode.cs'),str(root/'Assets/Scripts/Battle/Presentation/BattlePresentationSortingPolicy.cs')],check=True)
subprocess.run([str(mono/'bin/mono'),str(out/'test.exe')],check=True)
