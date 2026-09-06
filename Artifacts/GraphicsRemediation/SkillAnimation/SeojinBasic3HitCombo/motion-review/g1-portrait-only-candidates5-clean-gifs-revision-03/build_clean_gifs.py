from __future__ import annotations
import hashlib,json,shutil
from pathlib import Path
import numpy as np
from PIL import Image,ImageFilter

ROOT=Path(__file__).parent
BOARDS={
'A':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-194fee63-dab9-4b16-baed-290cf0f581bf.png',
'B':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-9a38a7ca-cddb-435c-a9aa-9fde519922b3.png',
'C':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-e9765dc0-c7ca-48a3-b61a-408c2a7eaeeb.png',
'D':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-e54f1e46-42b0-490f-80cf-e5e03391ef4f.png',
'E':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-a0a8e7b5-108a-4106-9ea1-6a8065fdafb1.png'}
DELAYS=[80,80,80,80,120,80,80,80,80,80,120,80,80,80,80,80,120,320]
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def component_mask(mask,cx,cy):
 a=np.asarray(mask)>0;h,w=a.shape;seen=np.zeros_like(a);best=None;bestscore=-1
 for sy,sx in zip(*np.where(a & ~seen)):
  if seen[sy,sx]:continue
  q=[(sy,sx)];seen[sy,sx]=1;pts=[]
  for y,x in q:
   pts.append((y,x))
   for dy,dx in ((1,0),(-1,0),(0,1),(0,-1)):
    ny,nx=y+dy,x+dx
    if 0<=ny<h and 0<=nx<w and a[ny,nx] and not seen[ny,nx]:seen[ny,nx]=1;q.append((ny,nx))
  ys=np.array([p[0] for p in pts]);xs=np.array([p[1] for p in pts]);dist=((xs.mean()-cx)**2+(ys.mean()-cy)**2)**.5;score=len(pts)/(1+dist*.8)
  if score>bestscore:bestscore=score;best=pts
 out=np.zeros((h,w),dtype=np.uint8)
 for y,x in best or []:out[y,x]=255
 return Image.fromarray(out).filter(ImageFilter.MaxFilter(3))
boards=ROOT/'boards';gifs=ROOT/'gifs';contacts=ROOT/'contacts';boards.mkdir(parents=True,exist_ok=True);gifs.mkdir(parents=True,exist_ok=True);contacts.mkdir(parents=True,exist_ok=True)
records=[];index=Image.new('RGB',(768,512*5),(238,235,228))
for rr,(name,src) in enumerate(BOARDS.items()):
 bp=boards/f'candidate-{name}-motion-board.png';shutil.copyfile(src,bp);board=Image.open(bp).convert('RGB');bg=np.median(np.concatenate([np.asarray(board)[:8].reshape(-1,3),np.asarray(board)[-8:].reshape(-1,3)]),axis=0).astype(np.uint8)
 isolated=[];metrics=[]
 for i in range(18):
  col=i%6;row=i//6;nomx0=round(col*1536/6);nomx1=round((col+1)*1536/6);nomy0=round(row*1024/3);nomy1=round((row+1)*1024/3)
  x0=max(0,nomx0-56);x1=min(1536,nomx1+56);y0=max(0,nomy0-16);y1=min(1024,nomy1+16);roi=board.crop((x0,y0,x1,y1));arr=np.asarray(roi,dtype=np.int16);diff=np.max(np.abs(arr-bg.astype(np.int16)),axis=2);raw=Image.fromarray((diff>16).astype(np.uint8)*255);grown=raw.filter(ImageFilter.MaxFilter(5));cm=component_mask(grown,(nomx0+nomx1)/2-x0,(nomy0+nomy1)/2-y0);keep=np.minimum(np.asarray(raw),np.asarray(cm)).astype(np.uint8);alpha=Image.fromarray(keep)
  rgba=Image.new('RGBA',roi.size,(0,0,0,0));rgba.paste(roi,(0,0),alpha);bbox=alpha.getbbox()
  if not bbox:raise RuntimeError((name,i,'empty'))
  subject=rgba.crop(bbox);isolated.append(subject);metrics.append({'frame':i,'source_roi':[x0,y0,x1,y1],'isolated_bbox':list(bbox),'subject_size':list(subject.size)})
 heights=[im.height for im in isolated];target_h=float(np.median(heights));foot=464;frames=[];centers=[]
 for i,sub in enumerate(isolated):
  scale=target_h/sub.height;nw=round(sub.width*scale);nh=round(sub.height*scale);q=sub.resize((nw,nh),Image.Resampling.BICUBIC)
  # retain gentle three-segment progression while removing cell-center jitter
  seg=i//6;phase=i%6;rootx=352+seg*12+phase*3
  x=round(rootx-nw/2);y=round(foot-nh);canvas=Image.new('RGBA',(768,512),(*bg,255));canvas.alpha_composite(q,(x,y));frames.append(canvas.convert('RGB'));centers.append({'frame':i,'root_x':rootx,'foot_y':foot,'placed_bbox':[x,y,x+nw,y+nh]})
 gp=gifs/f'candidate-{name}-clean-continuous18-768x512.gif';frames[0].save(gp,save_all=True,append_images=frames[1:],duration=DELAYS,loop=0,disposal=2)
 cp=contacts/f'candidate-{name}-clean-contact18.png';contact=Image.new('RGB',(768,512),tuple(bg));
 for i,f in enumerate(frames):contact.paste(f.resize((128,171),Image.Resampling.LANCZOS),((i%6)*128,(i//6)*171))
 contact.save(cp);index.paste(contact,(0,rr*512));records.append({'candidate':name,'gif':str(gp),'gif_sha256':sha(gp),'contact':str(cp),'contact_sha256':sha(cp),'frames':metrics,'registration':centers})
ip=ROOT/'candidate5-clean-index.png';index.save(ip);m={'status':'REVIEW_ONLY_CLEAN_SEPARATED','method':'expanded_cell_main_component_isolation_plus_uniform_registration','source_generation':0,'dimensions':[768,512],'physical_frames':18,'duration_ms':1800,'segments':{'hit1':[0,6],'hit2':[6,12],'hit3':[12,18]},'contacts':[4,10,16],'neighbor_component_removed':True,'rotation':0,'warp':0,'install':0,'candidates':records,'index':{'path':str(ip),'sha256':sha(ip)}};(ROOT/'manifest.json').write_text(json.dumps(m,indent=2)+'\n')
print(ROOT/'manifest.json')
