from pathlib import Path
import re,json,hashlib
root=Path(__file__).resolve().parents[2];out=root/'Artifacts/Engineering/SeojinManualControl/runtime-hover-08'
read=lambda p:(root/p).read_text()
prefab='Assets/Resources/skill/ProjectileEntity.prefab';p=read(prefab)
blocks={m.group(2):m.group(3) for m in re.finditer(r'^--- !u!(\d+) &(\d+)\n(.*?)(?=^--- |\Z)',p,re.M|re.S)}
assert 'm_GameObject: {fileID: 100001}' in blocks['9500000'] and 'm_GameObject: {fileID: 100001}' in blocks['21200000']
assert 'm_Father: {fileID: 8472752214521235128}' in blocks['400001']
assert 'm_Father: {fileID: 400000}' in blocks['8472752214521235128']
rows=[]
for grade,key in [(1,'active_1'),(2,'charge'),(3,'charge')]:
 equipment=f'Assets/Contents/Skill/so/skill.character.seojin.{grade}.active_1.{key}.asset';e=read(equipment);guid=re.search(r'moveSo: .*guid: (\w+)',e).group(1)
 matches=[m for m in (root/'Assets/Contents/Skill/so').glob('*.asset.meta') if f'guid: {guid}' in m.read_text()];assert len(matches)==1
 move=matches[0].with_suffix('');m=move.read_text();assert 'applyDirectionRotation: 1' in m and 'moveType: 3' in m
 clip=f'Assets/AnimationClips/Skill/skill.character.seojin.{grade}.active_1.{key}.visual.loop.anim';c=read(clip)
 assert 'm_RotationCurves: []' in c and 'm_EulerCurves: []' in c and 'attribute: m_FlipX' in c
 rows.append({'grade':grade,'equipment':equipment,'move':str(move.relative_to(root)),'applyDirectionRotation':True,'rotationOffset':float(re.search(r'rotationOffset: (.+)',m).group(1)),'moveType':'Hover','clip':clip,'current_clip_rotation_curves':0,'root_flip_curve':True})
v=read('Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs');assert 'directedVisual' not in v and 'directionWrapper.Begin(animator.transform,transform,spriteRenderer' in v
assert v.index('directionWrapper.Begin(')<v.index('OnSpawn();',v.index('directionWrapper.Begin('))
assert 'data.NormalizedDirection,data.moveRuntime?.applyDirectionRotation==true' in v
assert v.count('directionWrapper.Restore();')==4
movement=read('Assets/Scripts/Ability/Skills/Projectiles/ProjectileMovement.cs');assert 'authoritativeDirection = data.NormalizedDirection' in movement and 'hasAuthoritativeDirection = data.orientManualPresentation' in movement and 'visualOnlyDirectionRotation = data.orientManualPresentation' in movement
assert 'if(data.orientManualPresentation)transform.rotation=Quaternion.identity;' in movement
for n,h in json.loads((out/'morpg-before-sha.json').read_text()).items():assert hashlib.sha256((root/n).read_bytes()).hexdigest()==h
result={'prefab':prefab,'hierarchy_before':'ProjectileEntity -> Scaler -> Renderer (Animator + SpriteRenderer)','hierarchy_manual':'ProjectileEntity -> Scaler -> __ProjectileDirectionRotation -> Renderer (Animator + SpriteRenderer)','charge_move_readback':rows,'root_cause':['Factory CreateInstanceRuntimeData dropped orientManualPresentation','Hover ignored runtime direction and inferred from owner/target/spawn','Factory and MoveController used gameplay-root rotation; old visual child ignored applyDirectionRotation and rotationOffset','Warp clone dropped applyDirectionRotation/rotationOffset'],'morpg_unchanged':True,'Unity_import':False}
(out/'hierarchy-audit.json').write_text(json.dumps(result,indent=2)+'\n');print('PASS production prefab hierarchy, Charge G1-3 referenced MoveSO true, clip curves, runtime direction plumbing, restore sites, MORPG SHA')
