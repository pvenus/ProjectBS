#!/usr/bin/env python3
import csv,hashlib,json,glob
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw
ROOT=Path('/Users/pvenus/ProjectBS')
ART=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-01'
OUT=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/audit/frame-contamination-v01';OUT.mkdir(parents=True,exist_ok=True)
M=json.loads((ART/'manifest.json').read_text())
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def comps(mask):
 h,w=mask.shape;seen=np.zeros_like(mask,bool);out=[]
 for y,x in zip(*np.nonzero(mask)):
  if seen[y,x]:continue
  st=[(y,x)];seen[y,x]=1;pts=[]
  while st:
   yy,xx=st.pop();pts.append((yy,xx))
   for ny,nx in ((yy-1,xx),(yy+1,xx),(yy,xx-1),(yy,xx+1)):
    if 0<=ny<h and 0<=nx<w and mask[ny,nx] and not seen[ny,nx]:seen[ny,nx]=1;st.append((ny,nx))
  ys=[p[0] for p in pts];xs=[p[1] for p in pts];out.append({'area':len(pts),'bbox':[min(xs),min(ys),max(xs)+1,max(ys)+1],'pts':pts})
 return sorted(out,key=lambda c:c['area'],reverse=True)
def installed(row):
 reg=row['registry'];u=row['unit'];i=row['frame']
 if reg=='body':return ROOT/f'Assets/ImagesGenerated/Character/animation/character.seojin.basic_attack.combo.{u}.body/frame-{i}.png'
 g,h=u.split('/');return ROOT/f'Assets/ImagesGenerated/Skill/animation/skill.character.seojin.{g[-1]}.basic_attack.basic_attack.combo.{h[-1]}.visual/frame-{i}.png'

# Cache full-strip components and which proportional cells each occupies.
strip_cache={}
for r in M['rows']:
 p=r['source']
 if p in strip_cache:continue
 im=Image.open(p).convert('RGBA');a=np.array(im)[:,:,3]>2;w=im.width;xs=[round(k*w/6) for k in range(7)];cs=comps(a);cross=[]
 for ci,c in enumerate(cs):
  occ=[]
  for k in range(6):
   n=sum(1 for y,x in c['pts'] if xs[k]<=x<xs[k+1])
   if n:occ.append((k,n))
  if len(occ)>1:cross.append({'component':ci,'area':c['area'],'bbox':c['bbox'],'cells':occ})
 strip_cache[p]={'size':[im.width,im.height],'xs':xs,'cross':cross}

rows=[]
for r in M['rows']:
 p=Path(r['path']);ip=installed(r);im=Image.open(ip).convert('RGBA');a=np.array(im)[:,:,3]>2;b=im.getchannel('A').getbbox();cc=comps(a)
 border={'left':int(a[:,0].sum()),'right':int(a[:,-1].sum()),'top':int(a[0,:].sum()),'bottom':int(a[-1,:].sum())}
 k=r['frame'];sc=strip_cache[r['source']];cross=[]
 for c in sc['cross']:
  d=dict(c);mine=next(n for cell,n in c['cells'] if cell==k) if any(cell==k for cell,n in c['cells']) else 0
  if not mine:continue
  donors=[{'frame':cell,'area':n} for cell,n in c['cells'] if cell!=k]
  side=[]
  if any(x<k for x,n in c['cells']):side.append('left')
  if any(x>k for x,n in c['cells']):side.append('right')
  d.update(area_in_frame=mine,probable_donors=donors,sides=side);cross.append(d)
 flags=[]
 if any(border.values()):flags.append('installed_png_border_touch')
 if cross:flags.append('source_component_crossed_cell_edge')
 if r['registry']=='body' and cross:flags.append('weapon_or_body_half_shape_risk')
 if r['registry']=='vfx' and cross:flags.append('vfx_gutter_or_neighbor_shape_risk')
 rows.append({'registry':r['registry'],'unit':r['unit'],'frame':k,'installed':str(ip),'installed_sha':sha(ip),'artifact_sha':r['sha256'],'installed_matches_r1':sha(ip)==r['sha256'],'bbox':list(b) if b else None,'border_touch_pixels':border,'components':len(cc),'largest_component_area':cc[0]['area'] if cc else 0,'small_component_area':sum(c['area'] for c in cc[1:]),'source_rect':r['source_rect'],'source_crossings':cross,'flags':flags,'fail':bool(flags)})

# TSV summary
with (OUT/'failures.tsv').open('w',newline='') as f:
 w=csv.writer(f,delimiter='\t');w.writerow(['registry','unit','frame','side','bbox','component_area_in_cell','donor_neighbor','flags'])
 for r in rows:
  if not r['fail']:continue
  if r['source_crossings']:
   for c in r['source_crossings']:w.writerow([r['registry'],r['unit'],f"F{r['frame']}",','.join(c['sides']),r['bbox'],c['area_in_frame'],','.join(f"F{x['frame']}:{x['area']}px" for x in c['probable_donors']),','.join(r['flags'])])
  else:w.writerow([r['registry'],r['unit'],f"F{r['frame']}",'png-border',r['bbox'],'','',','.join(r['flags'])])

# contacts, nine units x six frames; cell outlines are explicit.
units=[]
for r in rows:
 key=(r['registry'],r['unit'])
 if key not in units:units.append(key)
for style in ('checker','solid'):
 tile=180;label=22;sheet=Image.new('RGB',(6*tile,len(units)*(tile+label)),(25,28,34));d=ImageDraw.Draw(sheet)
 for y,key in enumerate(units):
  ur=[r for r in rows if (r['registry'],r['unit'])==key]
  for x,r in enumerate(ur):
   if style=='checker':
    bg=Image.new('RGBA',(tile,tile),(220,220,220,255));bd=ImageDraw.Draw(bg)
    for yy in range(0,tile,16):
     for xx in range(0,tile,16):
      if (xx//16+yy//16)%2:bd.rectangle((xx,yy,xx+15,yy+15),fill=(165,165,165,255))
   else:bg=Image.new('RGBA',(tile,tile),(18,21,27,255))
   im=Image.open(r['installed']).convert('RGBA').resize((tile,tile),Image.Resampling.NEAREST);bg.alpha_composite(im);sheet.paste(bg.convert('RGB'),(x*tile,y*(tile+label)))
   d.rectangle((x*tile,y*(tile+label),x*tile+tile-1,y*(tile+label)+tile-1),outline=(255,70,70) if r['fail'] else (80,180,110),width=2)
   d.text((x*tile+4,y*(tile+label)+4),f"F{r['frame']}",fill=(255,210,90))
  d.text((4,y*(tile+label)+tile+2),f'{key[0]} {key[1]}',fill=(220,130,80))
 sheet.save(OUT/f'contact-{style}-cell-outlines.png')

report={'status':'READ_ONLY_CONTAMINATION_AUDIT','total':len(rows),'fail_count':sum(r['fail'] for r in rows),'installed_hash_match_count':sum(r['installed_matches_r1'] for r in rows),'gif_canvas_issue_separate':'v01 GIF top clipping was composition-canvas only; this report concerns source strip/cell provenance.','rows':rows,'remedy':{'preferred':'fresh standalone full-canvas native RGBA frames; no shared strip gutters','conditional':'re-extract original full strip only if overlap-aware segmentation can recover complete connected subjects without donor pixels; current proportional cell cuts are not safe','partial_install':False}}
p=OUT/'report.json';p.write_text(json.dumps(report,indent=2,default=lambda value: value.item() if hasattr(value,'item') else str(value))+'\n')
receipt=OUT/'RECEIPT.md';receipt.write_text(f"# Basic combo54 frame contamination audit\n\n- Exact54 inspected: {len(rows)}\n- Installed byte match to r1: {report['installed_hash_match_count']}/54\n- Flagged frames: {report['fail_count']}/54\n- GIF safe-canvas issue is separate from source cell contamination.\n- See `failures.tsv` for exact sides, component areas, and probable neighbor donors.\n- No repaint/crop/repair/install performed.\n")
print(json.dumps({'report':str(p),'report_sha':sha(p),'receipt':str(receipt),'receipt_sha':sha(receipt),'failures':report['fail_count'],'hash_matches':report['installed_hash_match_count']},indent=2))
