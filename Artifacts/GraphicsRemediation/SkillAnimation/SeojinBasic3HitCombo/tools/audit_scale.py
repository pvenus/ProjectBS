#!/usr/bin/env python3
import glob,json,statistics
from pathlib import Path
from PIL import Image

ROOT=Path('/Users/pvenus/ProjectBS')
def metrics(paths):
 out=[]
 for p in paths:
  im=Image.open(p).convert('RGBA'); b=im.getchannel('A').getbbox()
  if not b: continue
  w,h=im.size; bw=b[2]-b[0]; bh=b[3]-b[1]
  out.append({'path':str(p),'canvas':[w,h],'bbox':list(b),'bbox_wh':[bw,bh],
   'bbox_ratio':[bw/w,bh/h],'centroid_norm':[(b[0]+b[2])/(2*w),(b[1]+b[3])/(2*h)],
   'padding':[b[0],b[1],w-b[2],h-b[3]]})
 return out
def summary(rows):
 keys=('bbox_ratio','centroid_norm')
 s={'count':len(rows)}
 for k in keys:
  for i,n in enumerate(('x','y')):
   vals=[r[k][i] for r in rows]; s[f'{k}_{n}']={'min':min(vals),'median':statistics.median(vals),'max':max(vals)}
 return s

groups={
 'reference_body_idle_attack': [Path(p) for p in glob.glob(str(ROOT/'Assets/ImagesGenerated/Character/animation/character.seojin.*/*/frame-*.png')) if '.attack.' in p or '.idle.' in p],
 'reference_vfx_hit0': [Path(p) for p in glob.glob(str(ROOT/'Assets/ImagesGenerated/Skill/animation/skill.character.seojin.*.basic_attack.basic_attack/frame-*.png'))],
 'new_body18': [Path(p) for p in glob.glob(str(ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-01/body/**/*.png'),recursive=True)],
 'new_vfx36': [Path(p) for p in glob.glob(str(ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-01/vfx/**/*.png'),recursive=True)],
 'normalized_body18': [Path(p) for p in glob.glob(str(ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-02-scale-normalized/body/**/*.png'),recursive=True)],
 'normalized_vfx36': [Path(p) for p in glob.glob(str(ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-02-scale-normalized/vfx/**/*.png'),recursive=True)],
}
report={'groups':{}}
for name,paths in groups.items():
 rows=metrics(paths); report['groups'][name]={'summary':summary(rows),'frames':rows}
out=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-01/scale-audit.json'
out.write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v['summary'] for k,v in report['groups'].items()},indent=2))
