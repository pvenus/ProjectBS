from pathlib import Path
import subprocess,tempfile
root=Path(__file__).resolve().parents[2]
def extract(path,signature):
 s=(root/path).read_text();start=s.index(signature);a=s.index('{',start);end=a+1;depth=1
 while depth:depth+=(s[end]=='{')-(s[end]=='}');end+=1
 return s[start:end]
animation='Assets/Scripts/Actor/Party/AnimationMono.cs';movement='Assets/Scripts/Actor/Party/PartyMovementMono.cs';owner='Assets/Scripts/Actor/Character/Control/SeojinManualControl.cs'
methods={
 'ANIMATION':'\n'.join(extract(animation,s) for s in ['public void PlayIdle()','public void PlayMove()','private void PlayState(','private Coroutine StartPresentationRoutine(','private bool EnsurePresentationActive()','internal void BeginSynchronousTeardown()','internal void EndSynchronousTeardown()','private void ResetInactivePresentation()','private void StopPlayRoutine()','private void StopOneShotRoutine()']),
 'MOVEMENT':'\n'.join(extract(movement,s) for s in ['internal void SetOwnedManualInput(','internal void ReleaseManualControlForTeardown(','private void ApplyMovementControl(','private void ApplyManualMoveInput(','private void UpdateMovementAnimation(']),
 'OWNER':extract(owner,'private void OnDisable()')}
s=(root/'AgentTools/SeojinManualHarness/Teardown.template.cs').read_text()
for key,body in methods.items():s=s.replace('/* '+key+' */',body)
# Audit all actual animation coroutine call sites, including adjacent recovery/CC entry points.
a=(root/animation).read_text();assert a.count('StartCoroutine(')==1 and 'return StartCoroutine(routine);' in a
assert 'private void OnDisable() => ResetInactivePresentation();' in a
assert 'private void OnDestroy() => ResetInactivePresentation();' in a
out=Path(tempfile.mkdtemp(prefix='projectbs-teardown-'));src=out/'Test.cs';src.write_text(s)
mono=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge')
subprocess.run([str(mono/'bin/mono'),str(mono/'lib/mono/msbuild/Current/bin/Roslyn/csc.exe'),'-nologo','-nowarn:0649,0414','-langversion:9.0','-out:'+str(out/'test.exe'),str(src)],check=True)
subprocess.run([str(mono/'bin/mono'),str(out/'test.exe')],check=True)
print('PASS all animation coroutine call sites use final active/teardown gate; disable/destroy share synchronous reset')
