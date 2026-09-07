from pathlib import Path
import hashlib,json,difflib,subprocess
root=Path(__file__).resolve().parents[2]; out=root/'Artifacts/Engineering/SeojinManualControl'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
protected=json.loads((out/'same-path-before.json').read_text())
authorized=json.loads((out/'authorized-aim-assets.json').read_text()) if (out/'authorized-aim-assets.json').exists() else {}
for name,h in authorized.items():
 original=out/'aim-mode-03/before'/name
 assert sha(original)==h==protected[name]
 import re
 assert re.sub(r'^  (aimMode|aimInputSource): [0-9]+\n','',(root/name).read_text(),flags=re.M)==original.read_text(),name
 json_name=name.replace('/so/','/json/').replace('.asset','.json')
 current_json=json.loads((root/json_name).read_text());current_json.pop('aimMode');current_json.pop('aimInputSource')
 baseline_json=json.loads((out/'aim-mode-03/before'/json_name).read_text())
 if '.basic_attack.' in name:
  assert current_json['cast']['cooldown']==1.0
  current_json['cast']['cooldown']=baseline_json['cast']['cooldown']
 assert current_json==baseline_json,json_name
cooldowns=json.loads((out/'authorized-cooldown-assets.json').read_text())
for name,h in cooldowns.items():
 original=out/'input-source-04/before'/name
 assert sha(original)==h==protected[name]
 assert re.sub(r'^  cooldown: [0-9.]+','  cooldown: 1.000',original.read_text(),flags=re.M)==(root/name).read_text(),name
authorized.update(cooldowns)
changed=[p for p,h in protected.items() if p not in authorized and (not (root/p).exists() or sha(root/p)!=h)]
(out/'same-path-result.json').write_text(json.dumps({'checked':len(protected),'unchanged':len(protected)-len(authorized),'authorized_design_fields':list(authorized),'changed':changed},indent=2)+'\n')
assert not changed,changed
before=json.loads((out/'before-sha.json').read_text())
new=[root/'Assets/Scripts/Ability/Skills/Presentation/SkillDirectionPresentation.cs',root/'Assets/Scripts/Ability/Skills/Presentation/SkillDirectionPresentation.cs.meta',root/'Assets/Scripts/Ability/Skills/Definitions/equipment/SkillAimMode.cs',root/'Assets/Scripts/Ability/Skills/Definitions/equipment/SkillAimMode.cs.meta',root/'Assets/Scripts/Actor/Character/Control.meta',*sorted((root/'Assets/Scripts/Actor/Character/Control').glob('*')),*sorted((root/'AgentTools/SeojinManualHarness').glob('*'))]
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
print(f'PASS protected {len(protected)-len(authorized)} unchanged + {len(authorized)} authorized design-field assets; reverse patch and diff checks; {len(before)} existing / {len(new)} new files')
