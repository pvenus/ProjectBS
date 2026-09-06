from __future__ import annotations
import hashlib,json
from collections import deque
from pathlib import Path
import numpy as np
from PIL import Image,ImageFilter

SRC=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/motion-review/g1-portrait-only-candidates5-revision-01')
ROOT=Path(__file__).parent; (ROOT/'gifs').mkdir(parents=True,exist_ok=True); (ROOT/'contacts').mkdir(parents=True,exist_ok=True)
DELAYS=[80,80,80,80,120,80,80,80,80,80,120,80,80,80,80,80,120,320]
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def main_bbox(rgb):
 a=np.asarray(rgb.resize((384,256),Image.Resampling.BILINEAR),dtype=np.int16)
 border=np.concatenate([a[:4].reshape(-1,3),a[-4:].reshape(-1,3),a[:,:4].reshape(-1,3),a[:,-4:].reshape(-1,3)])
 bg=np.median(border,axis=0); d=np.max(np.abs(a-bg),axis=2); mask=Image.fromarray((d>22).astype('uint8')*255).filter(ImageFilter.MaxFilter(5)); m=np.asarray(mask)>0
 seen=np.zeros(m.shape,bool); best=[]
 for y,x in zip(*np.where(m & ~seen)):
  if seen[y,x]:continue
  q=[(y,x)];seen[y,x]=1;pts=[]
  for yy,xx in q:
   pts.append((yy,xx))
   for dy,dx in ((1,0),(-1,0),(0,1),(0,-1)):
    ny,nx=yy+dy,xx+dx
    if 0<=ny<256 and 0<=nx<384 and m[ny,nx] and not seen[ny,nx]:seen[ny,nx]=1;q.append((ny,nx))
  if len(pts)>len(best):best=pts
 ys=[p[0] for p in best];xs=[p[1] for p in best];return (min(xs)*2,min(ys)*2,(max(xs)+1)*2,(max(ys)+1)*2),tuple(int(v) for v in bg)
def smooth_targets(xs):
 knots=[0,6,12,17]; vals=[xs[0],(xs[5]+xs[6])/2,(xs[11]+xs[12])/2,xs[17]]; out=[]
 for a,b,va,vb in zip(knots[:-1],knots[1:],vals[:-1],vals[1:]):
  for t in range(a,b):out.append(va+(vb-va)*(t-a)/(b-a))
 out.append(vals[-1]);
 for i in range(1,len(out)):
  out[i]=max(out[i-1]-8,min(out[i-1]+8,out[i]))
 return out
records=[];index=Image.new('RGB',(768,512*5),(238,235,228))
for row,name in enumerate('ABCDE'):
 src=Image.open(SRC/'gifs'/f'candidate-{name}-continuous18-768x512.gif');frames=[]
 for i in range(src.n_frames):src.seek(i);frames.append(src.convert('RGB').copy())
 boxes=[];bgs=[]
 for f in frames:b,bg=main_bbox(f);boxes.append(b);bgs.append(bg)
 heights=[b[3]-b[1] for b in boxes];median_h=float(np.median(heights));baseline=float(np.median([b[3] for b in boxes]));xs=[(b[0]+b[2])/2 for b in boxes];targets=smooth_targets(xs)
 aligned=[];before=[];after=[]
 for i,(f,b,bg) in enumerate(zip(frames,boxes,bgs)):
  h=b[3]-b[1];scale=median_h/h;scale=min(scale,(512-48)/(f.height),(768-48)/(f.width)) if False else scale
  # derive a conservative full foreground mask; transform every visible mark together
  arr=np.asarray(f,dtype=np.int16);diff=np.max(np.abs(arr-np.array(bg,dtype=np.int16)),axis=2);mask=Image.fromarray((diff>18).astype('uint8')*255).filter(ImageFilter.MaxFilter(3))
  fg=Image.new('RGBA',f.size,(0,0,0,0));fg.paste(f,(0,0),mask)
  nw,nh=round(768*scale),round(512*scale);q=fg.resize((nw,nh),Image.Resampling.BICUBIC)
  ax=((b[0]+b[2])/2)*scale;foot=b[3]*scale;tx=round(targets[i]-ax);ty=round(baseline-foot)
  canvas=Image.new('RGBA',(768,512),(*bg,255));canvas.alpha_composite(q,(tx,ty))
  # if transformed visible union violates safe padding, uniformly shrink once around target anchor
  rgb=canvas.convert('RGB');nb,_=main_bbox(rgb);left,top,right,bottom=nb;fit=min(1.0,(768-48)/max(1,right-left),(512-48)/max(1,bottom-top))
  if fit<1:
   layer=canvas.copy().resize((round(768*fit),round(512*fit)),Image.Resampling.BICUBIC);canvas=Image.new('RGBA',(768,512),(*bg,255));canvas.alpha_composite(layer,((768-layer.width)//2,(512-layer.height)//2));rgb=canvas.convert('RGB');nb,_=main_bbox(rgb)
  aligned.append(rgb);before.append({'frame':i,'bbox':[int(v) for v in b],'center_x':float(xs[i]),'foot_y':int(b[3]),'height':int(h)});after.append({'frame':i,'bbox':[int(v) for v in nb],'center_x':float((nb[0]+nb[2])/2),'foot_y':int(nb[3]),'height':int(nb[3]-nb[1])})
 gp=ROOT/'gifs'/f'candidate-{name}-aligned-continuous18-768x512.gif';aligned[0].save(gp,save_all=True,append_images=aligned[1:],duration=DELAYS,loop=0,disposal=2)
 cp=ROOT/'contacts'/f'candidate-{name}-aligned-contact18.png';contact=Image.new('RGB',(768,512),bgs[0]);
 for i,f in enumerate(aligned):contact.paste(f.resize((128,171),Image.Resampling.LANCZOS),((i%6)*128,(i//6)*171))
 contact.save(cp);index.paste(contact,(0,row*512));records.append({'candidate':name,'gif':str(gp),'gif_sha256':sha(gp),'contact':str(cp),'contact_sha256':sha(cp),'median_body_height':median_h,'common_foot_baseline':baseline,'target_adjacent_anchor_jump_ceiling_px':8,'before':before,'after':after})
ip=ROOT/'candidate5-aligned-index.png';index.save(ip);m={'status':'REVIEW_ONLY_REGISTERED','method':'main_component_uniform_scale_translation_piecewise_root_trajectory','source_manifest_sha256':'94b0bdb3bb41ad12e27b4b5ede0634f70aba983918ce5a51aea1d4199553a4eb','dimensions':[768,512],'physical_frames':18,'duration_ms':1800,'contacts':[4,10,16],'rotation':0,'warp':0,'crop':0,'install':0,'candidates':records,'index':{'path':str(ip),'sha256':sha(ip)}};(ROOT/'manifest.json').write_text(json.dumps(m,indent=2)+'\n')
print(ROOT/'manifest.json')
