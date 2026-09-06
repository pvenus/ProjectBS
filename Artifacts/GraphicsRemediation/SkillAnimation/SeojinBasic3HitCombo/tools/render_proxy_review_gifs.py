#!/usr/bin/env python3
import bisect,glob,hashlib,json,re
from pathlib import Path
from PIL import Image,ImageDraw

ROOT=Path('/Users/pvenus/ProjectBS')
OUT=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/review/proxy-calibrated-r1-v01'
OUT.mkdir(parents=True,exist_ok=True)
TIMES={
'g1':[[0,.04,.08,.12,.16,.28],[.28,.3175,.355,.3925,.43,.56],[.56,.60,.64,.68,.72,.86]],
'g2':[[0,.035,.07,.105,.14,.25],[.25,.2825,.315,.3475,.38,.50],[.50,.5375,.575,.6125,.65,.78]],
'g3':[[0,.03,.06,.09,.12,.22],[.22,.25,.28,.31,.34,.44],[.44,.475,.51,.545,.58,.70]]}

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def parse_profile(p):
 rows={}
 for line in Path(p).read_text().splitlines():
  m=re.search(r'frameIndex: (\d+), scale: \{x: ([\d.-]+), y: ([\d.-]+)\}, offset: \{x: ([\d.-]+), y: ([\d.-]+)\}',line)
  if m: rows[int(m[1])]=tuple(map(float,m.groups()[1:]))
 return rows
def frame_paths(pattern): return sorted(glob.glob(str(ROOT/pattern)),key=lambda p:int(Path(p).stem.split('-')[-1]))

BODY={h:frame_paths(f'Assets/ImagesGenerated/Character/animation/character.seojin.basic_attack.combo.{h}.body/frame-*.png') for h in ('hit0','hit1','hit2')}
BODYPROF={h:parse_profile(ROOT/f'Assets/Contents/Skill/so/skill.character.seojin.basic_attack.combo.body.{h}.calibration.asset') for h in BODY}
def vfx_paths(g,h):
 if h=='hit0': return frame_paths(f'Assets/ImagesGenerated/Skill/animation/skill.character.seojin.{g[-1]}.basic_attack.basic_attack/frame-*.png')
 return frame_paths(f'Assets/ImagesGenerated/Skill/animation/skill.character.seojin.{g[-1]}.basic_attack.basic_attack.combo.{h[-1]}.visual/frame-*.png')
VFX={(g,h):vfx_paths(g,h) for g in TIMES for h in ('hit0','hit1','hit2')}
VFXPROF={(g,h):({} if h=='hit0' else parse_profile(ROOT/f'Assets/Contents/Skill/so/skill.character.seojin.{g[-1]}.basic_attack.combo.vfx.{h}.calibration.asset')) for g in TIMES for h in ('hit0','hit1','hit2')}

def transformed(path,cal,kind):
 im=Image.open(path).convert('RGBA'); b=im.getchannel('A').getbbox(); fg=im.crop(b)
 sx,sy,ox,oy=cal
 fg=fg.resize((max(1,round(fg.width*sx)),max(1,round(fg.height*sy))),Image.Resampling.NEAREST)
 return fg,ox,oy
def compose(g,hit,fi,mode):
 c=Image.new('RGBA',(960,540),(18,21,27,255)); d=ImageDraw.Draw(c)
 # fixed neutral gameplay-like ground bands
 d.rectangle((0,410,960,540),fill=(27,31,36,255)); d.line((0,444,960,444),fill=(48,52,55,255),width=2)
 if mode in ('combined','body'):
  cal=BODYPROF[hit][fi]; fg,ox,oy=transformed(BODY[hit][fi],cal,'body')
  x=300-fg.width//2+round(ox*100); bottom=478-round(oy*100); c.alpha_composite(fg,(x,bottom-fg.height))
 if mode in ('combined','vfx'):
  if hit=='hit0': cal=(1,1,0,0)
  else: cal=VFXPROF[(g,hit)][fi]
  fg,ox,oy=transformed(VFX[(g,hit)][fi],cal,'vfx')
  x=585-fg.width//2+round(ox*100); y=340-fg.height//2-round(oy*100); c.alpha_composite(fg,(x,y))
 d.text((18,16),f'SIMULATED PROXY REVIEW  {g.upper()}  {hit} F{fi}',fill=(220,226,232,255))
 d.text((18,36),'fixed camera / no root motion / runtime PASS not inferred',fill=(125,138,150,255))
 return c.convert('P',palette=Image.Palette.ADAPTIVE,colors=255)

def sequence(g,mode):
 events=[]
 for hi,ts in enumerate(TIMES[g]):
  for fi,t in enumerate(ts):
   # F5 at hit0/hit1 is a zero-time transition receipt; next F0 owns display.
   if fi==5 and hi<2: continue
   events.append((t,hi,fi))
 events.sort(key=lambda x:(x[0],x[1],x[2]))
 end=TIMES[g][-1][-1]; hold=.30
 frames=[];dur=[];meta=[]
 for n,(t,hi,fi) in enumerate(events):
  nt=events[n+1][0] if n+1<len(events) else end+hold
  ms=max(10,round((nt-t)*1000/10)*10)
  frames.append(compose(g,f'hit{hi}',fi,mode));dur.append(ms);meta.append({'time':t,'hit':hi,'frame':fi,'delay_ms':ms})
 return frames,dur,meta

manifest={'status':'REVIEW_ONLY_RUNTIME_PASS_NOT_INFERRED','authority_receipt_sha':'3aade3b3e486c04a866e85db8f670ad6dcaf6de59a8ab722f5a9be6512c97374','canvas':[960,540],'interpolation':'none; NEAREST/point','loop':0,'outputs':[],'sources':{'body':BODY,'vfx':{f'{g}/{h}':p for (g,h),p in VFX.items()}}}
for g in TIMES:
 for mode in ('combined','vfx'):
  frames,durs,meta=sequence(g,mode); p=OUT/f'seojin-basic-3hit-{g}-{mode}.gif'; frames[0].save(p,save_all=True,append_images=frames[1:],duration=durs,loop=0,disposal=2,optimize=False)
  manifest['outputs'].append({'path':str(p),'sha256':sha(p),'grade':g,'mode':mode,'physical_frames':len(frames),'delays_ms':durs,'events':meta})
frames,durs,meta=sequence('g2','body'); p=OUT/'seojin-basic-3hit-body-shared.gif'; frames[0].save(p,save_all=True,append_images=frames[1:],duration=durs,loop=0,disposal=2,optimize=False)
manifest['outputs'].append({'path':str(p),'sha256':sha(p),'grade':'shared-g2-timing','mode':'body','physical_frames':len(frames),'delays_ms':durs,'events':meta})
m=OUT/'manifest.json'; m.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
print(json.dumps({'manifest':str(m),'sha256':sha(m),'outputs':[(o['path'],o['sha256']) for o in manifest['outputs']]},indent=2))
