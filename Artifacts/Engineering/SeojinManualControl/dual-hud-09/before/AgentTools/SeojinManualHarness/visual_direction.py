from pathlib import Path
import subprocess,tempfile,hashlib,json
root=Path(__file__).resolve().parents[2]
out=Path(tempfile.mkdtemp(prefix='projectbs-direction-'))
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
source=(root/'AgentTools/SeojinManualHarness/VisualDirection.template.cs').read_text()
a=(root/'Assets/Scripts/Actor/Party/AnimationMono.cs').read_text()
start=a.index('        private void ApplyFacingLockAfterAnimationSample()');end=a.index('        private bool IsAttackDisabledByCc()',start)
source=source.replace('/* ACTUAL FACING */',a[start:end])
test=out/'Tests.cs';test.write_text(source)
subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-nowarn:0649','-langversion:9.0','-out:'+str(out/'test.exe'),str(test),str(root/'Assets/Scripts/Ability/Skills/Presentation/SkillDirectionPresentation.cs')],check=True)
subprocess.run([str(mono/'bin/mono'),str(out/'test.exe')],check=True)
a=(root/'Assets/Scripts/Actor/Party/AnimationMono.cs').read_text()
assert 'PlayDirectedSkillBodyAction' in a and 'directedBody.Apply();' in a and 'directedBody.Restore();' in a
v=(root/'Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs').read_text()
assert 'data.orientManualPresentation' in v and 'directionWrapper.Restore();' in v and 'data.NormalizedDirection,data.moveRuntime?.applyDirectionRotation==true' in v
s=(root/'Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs').read_text()
assert 'manualAim.Value.Direction' in s and 'PlayDirectedSkillBodyAction' in s
for p,h in json.loads((root/'Artifacts/Engineering/SeojinManualControl/visual-direction-06/morpg-before-sha.json').read_text()).items():
 assert hashlib.sha256((root/p).read_bytes()).hexdigest()==h,p
print('PASS direction integration source audit and unchanged MORPG hashes')
