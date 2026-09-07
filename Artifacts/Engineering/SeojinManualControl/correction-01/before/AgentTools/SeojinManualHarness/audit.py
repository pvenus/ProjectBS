from pathlib import Path
import hashlib,json,difflib,subprocess
root=Path(__file__).resolve().parents[2]; out=root/'Artifacts/Engineering/SeojinManualControl'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
protected=json.loads((out/'same-path-before.json').read_text())
changed=[p for p,h in protected.items() if not (root/p).exists() or sha(root/p)!=h]
(out/'same-path-result.json').write_text(json.dumps({'checked':len(protected),'changed':changed},indent=2)+'\n')
assert not changed,changed
before=json.loads((out/'before-sha.json').read_text())
new=[root/'Assets/Scripts/Actor/Character/Control.meta',*sorted((root/'Assets/Scripts/Actor/Character/Control').glob('*')),*sorted((root/'AgentTools/SeojinManualHarness').glob('*'))]
new=[p for p in new if p.is_file()]; (out/'new-files.json').write_text(json.dumps([str(p.relative_to(root)) for p in new],indent=2)+'\n')
patch=[]; hashes={}
for name,h in before.items():
 original=out/'before'/name;assert sha(original)==h
 current=root/name;hashes[name]=sha(current)
 patch.extend(difflib.unified_diff(original.read_text().splitlines(True),current.read_text().splitlines(True),fromfile='a/'+name,tofile='b/'+name))
for p in new:
 name=str(p.relative_to(root)); hashes[name]=sha(p)
 patch.extend(difflib.unified_diff([],p.read_text().splitlines(True),fromfile='/dev/null',tofile='b/'+name))
(out/'scoped.patch').write_text(''.join(patch));(out/'after-sha.json').write_text(json.dumps(hashes,indent=2)+'\n')
for args,file in [(['git','apply','--reverse','--check',str(out/'scoped.patch')],'reverse-check.log'),(['git','diff','--check'],'diff-check.log')]:
 p=subprocess.run(args,cwd=root,text=True,capture_output=True);(out/file).write_text(p.stdout+p.stderr+f'exit={p.returncode}\n');assert p.returncode==0,(file,p.stderr)
print(f'PASS protected hashes {len(protected)}/{len(protected)}; reverse patch and diff checks; {len(before)} existing / {len(new)} new files')
