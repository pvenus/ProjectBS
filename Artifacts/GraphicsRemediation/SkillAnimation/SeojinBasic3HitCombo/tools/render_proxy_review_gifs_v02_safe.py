#!/usr/bin/env python3
import glob,hashlib,json,re,math
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path('/Users/pvenus/ProjectBS')
OUT=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/review/proxy-calibrated-r1-v02-safe-canvas'; OUT.mkdir(parents=True,exist_ok=True)
TIMES={'g1':[[0,.04,.08,.12,.16,.28],[.28,.3175,.355,.3925,.43,.56],[.56,.60,.64,.68,.72,.86]],'g2':[[0,.035,.07,.105,.14,.25],[.25,.2825,.315,.3475,.38,.50],[.50,.5375,.575,.6125,.65,.78]],'g3':[[0,.03,.06,.09,.12,.22],[.22,.25,.28,.31,.34,.44],[.44,.475,.51,.545,.58,.70]]}
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def parse(p):
 d={}
 for s in Path(p).read_text().splitlines():
  m=re.search(r'frameIndex: (\d+), scale: \{x: ([\d.-]+), y: ([\d.-]+)\}, offset: \{x: ([\d.-]+), y: ([\d.-]+)\}',s)
  if m:d[int(m[1])]=tuple(map(float,m.groups()[1:]))
 return d
def paths(p):return sorted(glob.glob(str(ROOT/p)),key=lambda x:int(Path(x).stem.split('-')[-1]))
BODY={h:paths(f'Assets/ImagesGenerated/Character/animation/character.seojin.basic_attack.combo.{h}.body/frame-*.png') for h in ('hit0','hit1','hit2')}
BP={h:parse(ROOT/f'Assets/Contents/Skill/so/skill.character.seojin.basic_attack.combo.body.{h}.calibration.asset') for h in BODY}
def vp(g,h):
 return paths(f'Assets/ImagesGenerated/Skill/animation/skill.character.seojin.{g[-1]}.basic_attack.basic_attack/frame-*.png') if h=='hit0' else paths(f'Assets/ImagesGenerated/Skill/animation/skill.character.seojin.{g[-1]}.basic_attack.basic_attack.combo.{h[-1]}.visual/frame-*.png')
VFX={(g,h):vp(g,h) for g in TIMES for h in BODY}; VP={(g,h):({} if h=='hit0' else parse(ROOT/f'Assets/Contents/Skill/so/skill.character.seojin.{g[-1]}.basic_attack.combo.vfx.{h}.calibration.asset')) for g in TIMES for h in BODY}
def transformed(p,cal):
 im=Image.open(p).convert('RGBA');b=im.getchannel('A').getbbox();fg=im.crop(b);sx,sy,ox,oy=cal
 return fg.resize((max(1,round(fg.width*sx)),max(1,round(fg.height*sy))),Image.Resampling.NEAREST),ox,oy
def placements(g,h,fi):
 fg,ox,oy=transformed(BODY[h][fi],BP[h][fi]); bx=300-fg.width//2+round(ox*100); bb=478-round(oy*100); body=(fg,(bx,bb-fg.height,bx+fg.width,bb))
 cal=(1,1,0,0) if h=='hit0' else VP[(g,h)][fi]; vg,vx,vy=transformed(VFX[(g,h)][fi],cal); xx=585-vg.width//2+round(vx*100); yy=340-vg.height//2-round(vy*100);vfx=(vg,(xx,yy,xx+vg.width,yy+vg.height))
 return body,vfx

diagnostic=[]; allb=[]
for g in TIMES:
 for hi in range(3):
  for fi in range(6):
   for kind,(_,b) in zip(('body','vfx'),placements(g,f'hit{hi}',fi)):
    allb.append(b); sides=[]
    if b[0]<0:sides.append('left')
    if b[1]<0:sides.append('top')
    if b[2]>960:sides.append('right')
    if b[3]>540:sides.append('bottom')
    if sides:diagnostic.append({'grade':g,'hit':hi,'frame':fi,'kind':kind,'bbox960':b,'clipped_sides':sides})
minx=min(b[0] for b in allb);miny=min(b[1] for b in allb);maxx=max(b[2] for b in allb);maxy=max(b[3] for b in allb)
PAD=48;shiftx=PAD-min(0,minx);shifty=PAD-min(0,miny); W=max(960+shiftx,maxx+shiftx+PAD);H=max(540+shifty,maxy+shifty+PAD);W=math.ceil(W/2)*2;H=math.ceil(H/2)*2
def compose(g,h,fi,mode):
 c=Image.new('RGBA',(W,H),(18,21,27,255));d=ImageDraw.Draw(c);ground=410+shifty;d.rectangle((0,ground,W,H),fill=(27,31,36,255));d.line((0,444+shifty,W,444+shifty),fill=(48,52,55,255),width=2)
 body,vfx=placements(g,h,fi)
 for kind,(fg,b) in (('body',body),('vfx',vfx)):
  if mode=='combined' or mode==kind:c.alpha_composite(fg,(b[0]+shiftx,b[1]+shifty))
 d.text((18,16),f'SIMULATED PROXY REVIEW SAFE  {g.upper()} {h} F{fi}',fill=(220,226,232,255));d.text((18,36),'fixed camera / pixel scale preserved / runtime PASS not inferred',fill=(125,138,150,255))
 return c.convert('P',palette=Image.Palette.ADAPTIVE,colors=255)
def seq(g,mode):
 ev=[]
 for hi,ts in enumerate(TIMES[g]):
  for fi,t in enumerate(ts):
   if fi==5 and hi<2:continue
   ev.append((t,hi,fi))
 ev.sort();end=TIMES[g][-1][-1];frames=[];ds=[];meta=[]
 for n,(t,hi,fi) in enumerate(ev):
  nt=ev[n+1][0] if n+1<len(ev) else end+.30;ms=max(10,round((nt-t)*1000/10)*10);frames.append(compose(g,f'hit{hi}',fi,mode));ds.append(ms);meta.append({'time':t,'hit':hi,'frame':fi,'delay_ms':ms})
 return frames,ds,meta
manifest={'status':'REVIEW_ONLY_RUNTIME_PASS_NOT_INFERRED','source_review':'proxy-calibrated-r1-v01','canvas':[W,H],'safe_padding_required':48,'union_bbox_original':[minx,miny,maxx,maxy],'union_bbox_safe':[minx+shiftx,miny+shifty,maxx+shiftx,maxy+shifty],'art_clearance_safe':[minx+shiftx,miny+shifty,W-(maxx+shiftx),H-(maxy+shifty)],'clearance_gate_32px':min(minx+shiftx,miny+shifty,W-(maxx+shiftx),H-(maxy+shifty))>=32,'global_shift':[shiftx,shifty],'clipped_in_v01':diagnostic,'interpolation':'NEAREST/point; no shrink','outputs':[]}
for g in TIMES:
 for mode in ('combined','vfx'):
  fs,ds,meta=seq(g,mode);p=OUT/f'seojin-basic-3hit-{g}-{mode}-safe.gif';fs[0].save(p,save_all=True,append_images=fs[1:],duration=ds,loop=0,disposal=2,optimize=False);manifest['outputs'].append({'path':str(p),'sha256':sha(p),'grade':g,'mode':mode,'frames':len(fs),'delays_ms':ds,'events':meta})
fs,ds,meta=seq('g2','body');p=OUT/'seojin-basic-3hit-body-shared-safe.gif';fs[0].save(p,save_all=True,append_images=fs[1:],duration=ds,loop=0,disposal=2,optimize=False);manifest['outputs'].append({'path':str(p),'sha256':sha(p),'grade':'shared-g2-timing','mode':'body','frames':len(fs),'delays_ms':ds,'events':meta})
m=OUT/'manifest.json';m.write_text(json.dumps(manifest,indent=2)+'\n');print(json.dumps({'manifest':str(m),'sha256':sha(m),'canvas':[W,H],'union':[minx,miny,maxx,maxy],'shift':[shiftx,shifty],'clipped_count':len(diagnostic),'outputs':[(o['path'],o['sha256']) for o in manifest['outputs']]},indent=2))
