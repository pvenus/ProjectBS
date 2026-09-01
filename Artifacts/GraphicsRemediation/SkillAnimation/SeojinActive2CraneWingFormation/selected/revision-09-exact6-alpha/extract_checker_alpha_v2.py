from pathlib import Path
from PIL import Image, ImageOps
import numpy as np, hashlib, json

SRC=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinActive2CraneWingFormation/raw-review/revision-07-independent-exact6-clean-review')
OUT=Path(__file__).resolve().parent
TIMES=[0,.167,.333,.5,.667,.833]; DELAYS=[170,160,170,170,160,170]
BG=250.0; D0=12.0; D1=100.0
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def extract(rgb):
    x=rgb.astype(np.float32); mn=x.min(2); mx=x.max(2); chroma=mx-mn
    d=np.maximum(BG-mn,1.6*chroma)
    t=np.clip((d-D0)/(D1-D0),0,1); a=np.rint((t*t*(3-2*t))*255).astype(np.uint8)
    # Physical canvas border is background by construction; exact four-border cleanup.
    a[0,:]=a[-1,:]=0; a[:,0]=a[:,-1]=0
    out=np.dstack([rgb.copy(),a]); out[:,:,:3][a==0]=0
    return out
def metrics(src,rgba):
    a=rgba[:,:,3]; nz=a>0; z=~nz
    border=np.concatenate([a[0],a[-1],a[:,0],a[:,-1]])
    strong=(src.min(2)<190)|((src.max(2)-src.min(2))>24)
    ys,xs=np.nonzero(nz); bbox=[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)] if len(xs) else None
    return {'alphaNonzero':int(nz.sum()),'alphaOpaque':int((a==255).sum()),'borderNonzero':int((border>0).sum()),'corners':[int(a[0,0]),int(a[0,-1]),int(a[-1,0]),int(a[-1,-1])],'alpha0RgbResidue':int(np.count_nonzero(rgba[:,:,:3][z])),'foregroundRgbMismatch':int(np.count_nonzero(rgba[:,:,:3][nz]!=src[nz])),'strongForegroundLoss':int(np.count_nonzero(strong&z)),'bbox':bbox}

manifest={'status':'EXACT12_ALPHA_PHYSICAL_PASS_T0_HANDOFF','authority':{'path':'/private/tmp/projectbs-current-byeori-active2-exact12-deterministic-alpha-authority.txt','sha256':'6d84a11bb7c8223d375b408e763fd108429971a537123e114219264025f3ac90'},'sourceManifest':{'path':str(SRC/'manifest.json'),'sha256':'6660badea04da9583d45fd3b92ce252605eef573e9a3ed5d02e0efcd36cfc5af'},'method':{'bgRgb':[250,250,250],'distance':'max(250-minRGB,1.6*chroma)','d0':D0,'d1':D1,'alpha':'smoothstep','foregroundRgb':'original bytes unchanged where alpha>0','alpha0Rgb0':True,'outerBorderAlpha0':True},'timing':{'timestamps':TIMES,'gifDelaysMs':DELAYS,'durationMs':1000},'grades':{},'contacts':[],'rollback':'delete revision-09-exact6-alpha artifact root; canonical not written by Hwagam'}
for grade in ('G2','G3'):
    frames=[]; rows=[]; rerun=[]
    for i,t in enumerate(TIMES):
        sp=SRC/f'seojin-active2-{grade.lower()}-f{i}-{t:.3f}-rgb-review.png'; src=np.array(Image.open(sp).convert('RGB'))
        rgba=extract(src); rgba2=extract(src); assert np.array_equal(rgba,rgba2)
        p=OUT/f'seojin-active2-{grade.lower()}-f{i}-{t:.3f}-rgba.png'; Image.fromarray(rgba,'RGBA').save(p,'PNG',compress_level=9)
        frames.append(Image.fromarray(rgba,'RGBA')); rerun.append(hashlib.sha256(rgba2.tobytes()).hexdigest())
        rows.append({'index':i,'timestamp':t,'sourcePath':str(sp),'sourceSha256':sha(sp),'path':str(p),'sha256':sha(p),'decodedRgbaSha256':hashlib.sha256(rgba.tobytes()).hexdigest(),'dimensions':[362,724],**metrics(src,rgba)})
    gif=OUT/f'seojin-active2-{grade.lower()}-exact6-alpha-loop-1s.gif'; frames[0].save(gif,save_all=True,append_images=frames[1:],duration=DELAYS,loop=0,disposal=2,optimize=False)
    manifest['grades'][grade]={'frames':rows,'uniqueFrames':len(set(r['decodedRgbaSha256'] for r in rows)),'deterministicRerunDiff0':all(r['decodedRgbaSha256']==h for r,h in zip(rows,rerun)),'gif':{'path':str(gif),'sha256':sha(gif),'physicalFrames':6,'durationMs':1000}}

for size in (200,80,32):
  for bgname,bg in (('light',(224,221,212)),('dark',(24,30,38))):
    color=Image.new('RGB',(size*6,size*2),bg); gray=Image.new('L',(size*6,size*2),Image.new('RGB',(1,1),bg).convert('L').getpixel((0,0)))
    for row,grade in enumerate(('G2','G3')):
      for i,t in enumerate(TIMES):
        fr=Image.open(OUT/f'seojin-active2-{grade.lower()}-f{i}-{t:.3f}-rgba.png').convert('RGBA'); fr.thumbnail((size,size),Image.Resampling.LANCZOS)
        cell=Image.new('RGBA',(size,size),(0,0,0,0));cell.alpha_composite(fr,((size-fr.width)//2,(size-fr.height)//2));base=Image.new('RGBA',(size,size),(*bg,255));base.alpha_composite(cell)
        color.paste(base.convert('RGB'),(i*size,row*size));gray.paste(base.convert('L'),(i*size,row*size))
    for kind,img in (('color',color),('gray',gray)):
      p=OUT/f'active2-exact12-{size}px-{bgname}-{kind}.png';img.save(p,'PNG',compress_level=9);manifest['contacts'].append({'size':size,'background':bgname,'kind':kind,'path':str(p),'sha256':sha(p)})

script=Path(__file__);manifest['script']={'path':str(script),'sha256':sha(script)}
mp=OUT/'manifest.json';mp.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n');print(json.dumps({'manifestPath':str(mp),'manifestSha256':sha(mp),**manifest},ensure_ascii=False,indent=2))
