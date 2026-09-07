"""Prepare exact9 completely, verify selected hashes, then atomically publish the resource directory."""
from pathlib import Path
import json,hashlib,uuid,shutil,tempfile,os,sys
from PIL import Image
R=Path('/Users/pvenus/ProjectBS');O=R/'Artifacts/Engineering/MorpgWaveDressingInstall'
B=R/'Artifacts/GraphicsRemediation/MORPGWaveEnvironment/selected/revision-02-foreground-empty';V=B.parent/'revision-01'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assert sha(B/'manifest.json')=='a02e23e9b812768cdbb80b1aec473d1e5cdcca8019b9d43eeb6d36e7cd18338b'
a=json.loads((B/'manifest.json').read_text());v=json.loads((V/'manifest.json').read_text());assert sha(V/'manifest.json')==a['preservedDependencies']['barrierManifestSha256']
dest=R/'Assets/Resources/battle/morpg/wave-environment-v1';stage=Path(tempfile.mkdtemp(prefix='projectbs-wave-assets-',dir='/private/tmp'));assets=[];rows=[]
guid=lambda n:uuid.uuid5(uuid.NAMESPACE_URL,'projectbs/morpg/wave-environment-v1/'+n).hex
metaTemplate=(R/'Assets/Resources/battle/morpg/environment/z1-sapling.png.meta').read_text()
for i in range(3):
 x=36*i;entry={'zoneId':f"zone.act1.chapter01.01.rescue_villagers.{['left','center','right'][i]}",'assets':[]}
 for kind in ['background','top','bottom']:
  if kind=='background':source=B/a['selected'][i]['path'];expected=a['selected'][i]['sha256'];pivot=[.5,.5];w=32.5;h=w/3;rect=[x-.25,-4.5,x+32.25,-4.5+h];order=-1000
  else:
   name=kind+'Barrier';source=V/v['waves'][i][name][0];expected=v['waves'][i][name][1];pivot=[.5,.5];rect=[x-.25,2.5 if kind=='top' else -5.7,x+32.25,5.7 if kind=='top' else -2.5];order=-900 if kind=='top' else 50
  assert sha(source)==expected
  name=f'wave{i+1}-{kind}.png';shutil.copyfile(source,stage/name)
  im=Image.open(source);assert im.size==(1536,512) and im.mode==('RGB' if kind=='background' else 'RGBA')
  meta=metaTemplate.replace('spriteAlignment: 9','spriteAlignment: 0').replace('spriteMeshType: 1','spriteMeshType: 0').replace('c2ffba5d73dd4bb1a80b56cc1fa44bde',guid(name)).replace('c0dda6c412834b238ec4e0155046527d',guid(name+'/sprite')).replace('maxTextureSize: 1024','maxTextureSize: 2048').replace('spritePivot: {x: 0.5, y: 0.046875}',f'spritePivot: {{x: 0.5, y: {pivot[1]}}}')
  (stage/(name+'.meta')).write_text(meta)
  boxes=[]
  if kind!='background':
   # Conservative pixel-supported silhouette: no polygon bridges an alpha gap.
   alpha=im.getchannel('A');sx=(rect[2]-rect[0])/1536;sy=(rect[3]-rect[1])/512
   for y in range(0,512,8):
    run=None
    for px in range(0,1536+32,32):
     wy0=rect[3]-(y+8)*sy;wy1=rect[3]-y*sy
     outside=wy0>=4.35 if kind=='top' else wy1<=-4.35
     solid=px<1536 and outside and alpha.crop((px,y,px+32,y+8)).getextrema()[0]>=192
     if solid and run is None:run=px
     if not solid and run is not None:
      boxes.append([round(rect[0]+run*sx,6),round(wy0,6),round(rect[0]+px*sx,6),round(wy1,6)]);run=None
   assert boxes,(i,kind,'empty supported silhouette')
  row=dict(id=f'wave{i+1}.{kind}',kind=kind,resource='battle/morpg/wave-environment-v1/'+name[:-4],guid=guid(name),sha256=expected,pivot=pivot,rect=rect,sortingOrder=order,colliderRects=[{'rect':b} for b in boxes])
  entry['assets'].append(row);assets.append(dict(path=str((dest/name).relative_to(R)),source=str(source.relative_to(R)),sha256=expected,guid=guid(name),colliderPatches=len(boxes)))
 rows.append(entry)
profile=dict(schemaVersion='morpg-wave-dressing.v1',backgroundManifestSha256=sha(B/'manifest.json'),barrierManifestSha256=sha(V/'manifest.json'),waves=rows)
(stage/'binding.json').write_text(json.dumps(profile,indent=2)+'\n');(stage/'binding.json.meta').write_text('fileFormatVersion: 2\nguid: '+guid('binding.json')+'\nTextScriptImporter:\n  externalObjects: {}\n')
foldermeta=Path(str(dest)+'.meta');text='fileFormatVersion: 2\nguid: '+guid('folder')+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n'
if foldermeta.exists():assert foldermeta.read_text()==text
else:foldermeta.write_text(text)
if dest.exists() and 'alphaRect' in (dest/'binding.json').read_text():
 shutil.rmtree(stage)
 raise SystemExit('Viewport rootfix binding active. Use reframe.py; legacy installer is frozen.')
if dest.exists():
 old={p.name:sha(p) for p in dest.iterdir()};new={p.name:sha(p) for p in stage.iterdir()}
 if old!=new:
  assert '--replace-binding' in sys.argv and {k:v for k,v in old.items() if k!='binding.json'}=={k:v for k,v in new.items() if k!='binding.json'},'Existing assets differ; do not overwrite'
  os.replace(stage/'binding.json',dest/'binding.json')
 shutil.rmtree(stage)
else:os.replace(stage,dest)
(O/'installed-assets.json').write_text(json.dumps(assets,indent=2)+'\n');(O/'binding.json').write_text(json.dumps(profile,indent=2)+'\n')
print('PASS exact3 backgrounds + exact6 independent barriers; atomic resource bundle published; source byte exact9/9')
print('Silhouette patches:',[r['colliderPatches'] for r in assets if r['colliderPatches']])
