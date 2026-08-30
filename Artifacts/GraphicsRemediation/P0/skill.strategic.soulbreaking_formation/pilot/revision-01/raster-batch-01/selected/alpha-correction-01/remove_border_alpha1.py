from collections import deque
from pathlib import Path
import hashlib, json
import numpy as np
from PIL import Image

SRC=Path('Artifacts/GraphicsRemediation/P0/skill.strategic.soulbreaking_formation/pilot/revision-01/raster-batch-01/selected/selected-D.png')
ROOT=Path(__file__).parent; ROOT.mkdir(parents=True,exist_ok=True)
arr=np.asarray(Image.open(SRC).convert('RGBA')).copy(); a=arr[...,3]; h,w=a.shape
eligible=a==1; remove=np.zeros_like(eligible); q=deque()
for x in range(w):
    for y in (0,h-1):
        if eligible[y,x] and not remove[y,x]: remove[y,x]=1;q.append((y,x))
for y in range(h):
    for x in (0,w-1):
        if eligible[y,x] and not remove[y,x]: remove[y,x]=1;q.append((y,x))
while q:
    y,x=q.popleft()
    for yy,xx in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
        if 0<=yy<h and 0<=xx<w and eligible[yy,xx] and not remove[yy,xx]: remove[yy,xx]=1;q.append((yy,xx))
arr[remove]=0
out=ROOT/'skill.strategic.soulbreaking_formation.icon.selected-D-alpha-r1.png'
Image.fromarray(arr,'RGBA').save(out,compress_level=9)
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
ys,xs=np.where(arr[...,3]>0)
m={'source':str(SRC),'source_sha256':sha(SRC),'output':str(out),'output_sha256':sha(out),'removed_border_connected_alpha1_pixels':int(remove.sum()),'remaining_alpha1_pixels':int((arr[...,3]==1).sum()),'alpha_positive_bbox':[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)],'corner_alpha':[int(arr[0,0,3]),int(arr[0,-1,3]),int(arr[-1,0,3]),int(arr[-1,-1,3])],'transparent_rgb_residue':int(np.any(arr[arr[...,3]==0,:3]!=0,axis=1).sum()),'rgb_or_alpha_changes_outside_removed_component':0,'rule':'set only 4-connected alpha==1 components touching canvas border to RGBA zero'}
(ROOT/'metrics.json').write_text(json.dumps(m,ensure_ascii=False,indent=2)+'\n');print(json.dumps(m,ensure_ascii=False,indent=2))
