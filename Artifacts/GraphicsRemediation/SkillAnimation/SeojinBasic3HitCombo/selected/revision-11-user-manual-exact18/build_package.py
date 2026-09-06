from pathlib import Path
from PIL import Image
import hashlib, json, shutil
import numpy as np

ROOT=Path(__file__).resolve().parent
SRC=Path('/Users/pvenus/ProjectBS-image-python/downloads/exec-600783ed-96f5-4d18-9ddd-d1dbaa661289_7d56b10b/frames')
DELAYS=[80,80,80,80,120,80,80,80,80,80,120,80,80,80,80,80,120,320]

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def components(mask):
    h,w=mask.shape; seen=np.zeros_like(mask,bool); areas=[]
    for sy,sx in zip(*np.where(mask & ~seen)):
        if seen[sy,sx]: continue
        q=[(sy,sx)]; seen[sy,sx]=1; n=0
        for y,x in q:
            n+=1
            for dy,dx in ((1,0),(-1,0),(0,1),(0,-1)):
                ny,nx=y+dy,x+dx
                if 0<=ny<h and 0<=nx<w and mask[ny,nx] and not seen[ny,nx]:
                    seen[ny,nx]=1; q.append((ny,nx))
        areas.append(n)
    return sorted(areas,reverse=True)

def main():
    out=ROOT/'frames'; out.mkdir(parents=True,exist_ok=True)
    records=[]; images=[]; hashes=set(); hard=[]
    prev=None
    for i in range(18):
        sp=SRC/f'frame_{i+1:03d}.png'; source_im=Image.open(sp); mode=source_im.mode; size=source_im.size
        source_arr=np.asarray(source_im.convert('RGBA')).copy()
        source_arr[source_arr[:,:,3]==0,:3]=0
        clean=Image.fromarray(source_arr,'RGBA')
        im=Image.new('RGBA',(768,512),(0,0,0,0))
        offset=((768-clean.width)//2,(512-clean.height)//2)
        im.alpha_composite(clean,offset)
        arr=np.asarray(im); a=arr[:,:,3]; nz=a>0
        ys,xs=np.where(nz)
        bbox=[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)] if len(xs) else None
        centroid=[float(xs.mean()),float(ys.mean())] if len(xs) else None
        border=np.concatenate([a[0],a[-1],a[:,0],a[:,-1]])
        residue=int(np.count_nonzero((a==0)&np.any(arr[:,:,:3]!=0,axis=2)))
        comps=components(nz); significant=[x for x in comps if x>=max(16,int(nz.sum()*.002))]
        anchor_jump=None if prev is None or centroid is None else round(((centroid[0]-prev[0])**2+(centroid[1]-prev[1])**2)**.5,3)
        prev=centroid
        dp=out/f'frame-{i:02d}.png'; im.save(dp); h=sha(dp); hashes.add(h)
        rec={'frame':i,'source':str(sp),'sourceSha256':sha(sp),'sourceDimensions':list(size),'sourceMode':mode,'normalization':'visible-pixel-preserving center pad to 768x512; alpha0 RGB zeroed','offset':list(offset),'output':str(dp),'sha256':h,'dimensions':list(im.size),'mode':im.mode,'bbox':bbox,'centroid':centroid,'footY':bbox[3] if bbox else None,'bodyHeight':bbox[3]-bbox[1] if bbox else 0,'anchorJump':anchor_jump,'borderAlphaNonzero':int(np.count_nonzero(border)),'alpha0RgbResidue':residue,'significantComponents':len(significant),'componentAreas':comps[:8]}
        records.append(rec); images.append(im.convert('RGBA'))
        if mode!='RGBA' or rec['borderAlphaNonzero'] or residue or not bbox: hard.append({'frame':i,'issues':[k for k,v in [('sourceMode',mode!='RGBA'),('borderAlpha',bool(rec['borderAlphaNonzero'])),('alpha0RgbResidue',bool(residue)),('empty',not bbox)] if v]})
    gif=ROOT/'seojin-basic-g1-continuous3hit-user-final-exact18.gif'
    images[0].save(gif,save_all=True,append_images=images[1:],duration=DELAYS,loop=0,optimize=False,disposal=2)
    contact=Image.new('RGBA',(768*6,512*3),(238,235,228,255))
    for i,im in enumerate(images): contact.alpha_composite(im,((i%6)*768,(i//6)*512))
    contact=contact.resize((1536,512),Image.Resampling.LANCZOS).convert('RGB')
    cp=ROOT/'seojin-basic-g1-continuous3hit-user-final-contact18.png'; contact.save(cp)
    manifest={'status':'PHYSICAL_PASS_PENDING_VISUAL_REVIEW' if not hard and len(hashes)==18 else 'FAIL_CLOSED','sourceAuthority':str(SRC),'copyPolicy':'lossless rename only; visible pixels unchanged','dimensions':[768,512],'physicalFrames':18,'durationMs':1800,'loop':'infinite review','segments':{'hit1':[0,6],'hit2':[6,12],'hit3':[12,18]},'contacts':[4,10,16],'uniqueFrames':len(hashes),'hardFailures':hard,'frames':records,'gif':{'path':str(gif),'sha256':sha(gif)},'contact':{'path':str(cp),'sha256':sha(cp)},'installAuthority':False}
    mp=ROOT/'manifest.json'; mp.write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'manifest':str(mp),'manifestSha256':sha(mp),'status':manifest['status'],'unique':len(hashes),'hardFailures':hard},ensure_ascii=False))

if __name__=='__main__': main()
