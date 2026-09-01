from pathlib import Path
from PIL import Image,ImageOps,ImageDraw
import numpy as np, hashlib, json, math

root=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinActive2CraneWingFormation/fresh-d-native-rgba')
AUTH='d6360d2193d49d0733a9d150e4f02f3608f692e218f8e211aaddbbdcb7646a7f'

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def lin(x):
    x=x.astype(np.float64)/255
    return np.where(x<=.04045,x/12.92,((x+.055)/1.055)**2.4)
def enc(x):
    y=np.where(x<=.0031308,12.92*x,1.055*np.maximum(x,0)**(1/2.4)-.055)
    return np.floor(np.clip(y,0,1)*255+.5).astype(np.uint8)
def sinc(x): return np.sinc(x)
def table(src,dst):
    scale=dst/src;support=3/scale if scale<1 else 3
    rows=[]
    for j in range(dst):
        c=(j+.5)/scale-.5;lo=math.ceil(c-support);hi=math.floor(c+support);idx=np.arange(lo,hi+1)
        d=c-idx;z=d*scale if scale<1 else d;w=sinc(z)*sinc(z/3)
        idx=np.clip(idx,0,src-1);acc={}
        for ii,ww in zip(idx,w): acc[int(ii)]=acc.get(int(ii),0)+float(ww)
        ii=np.array(sorted(acc));ww=np.array([acc[k] for k in ii]);ww/=ww.sum();rows.append((ii,ww))
    return rows
def axis_resize(a,dst,axis):
    src=a.shape[axis];t=table(src,dst);outshape=list(a.shape);outshape[axis]=dst;out=np.empty(outshape,np.float64)
    for j,(idx,w) in enumerate(t):
        vals=np.take(a,idx,axis=axis);v=np.tensordot(w,vals,axes=(0,axis))
        sl=[slice(None)]*a.ndim;sl[axis]=j;out[tuple(sl)]=v
    return out
def normalize(src,size,offset,out):
    rgba=np.array(Image.open(src).convert('RGBA'));alpha=rgba[...,3:4].astype(np.float64)/255
    prem=np.concatenate([lin(rgba[...,:3])*alpha,alpha],2)
    q=axis_resize(axis_resize(prem,size[0],1),size[1],0);a=np.clip(q[...,3:4],0,1);rgb=np.divide(q[...,:3],a,out=np.zeros_like(q[...,:3]),where=a>0)
    rr=np.zeros((size[1],size[0],4),np.uint8);rr[...,:3]=enc(rgb);rr[...,3]=np.floor(a[...,0]*255+.5).astype(np.uint8);rr[rr[...,3]==0,:3]=0
    canvas=np.zeros((1024,1024,4),np.uint8);x,y=offset;canvas[y:y+size[1],x:x+size[0]]=rr;Image.fromarray(canvas,'RGBA').save(out,compress_level=9)
def metrics(p):
    a=np.array(Image.open(p).convert('RGBA'));al=a[...,3];border=np.concatenate([al[0],al[-1],al[:,0],al[:,-1]]);rgb0=a[al==0,:3]
    ys,xs=np.where(al>0);return {'dimensions':[a.shape[1],a.shape[0]],'borderNonzero':int((border>0).sum()),'corners':[int(al[0,0]),int(al[0,-1]),int(al[-1,0]),int(al[-1,-1])],'alpha0RgbResiduePixels':int(np.any(rgb0!=0,axis=1).sum()),'nonzeroBounds':[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)]}

specs=[('attempt-01','g2-hold.png',(928,619),(48,202)),('attempt-01','g3-hold.png',(928,928),(48,48)),('attempt-02','g2-hold.png',(928,928),(48,48)),('attempt-02','g3-hold.png',(928,928),(48,48))]
members=[]
for att,name,size,off in specs:
    src=root/att/name;od=root/att/'normalized';od.mkdir(exist_ok=True);out=od/name;normalize(src,size,off,out);first=sha(out);normalize(src,size,off,out);second=sha(out)
    members.append({'attempt':att,'grade':name[:2].upper(),'source':str(src),'sourceSHA256':sha(src),'sourceDimensions':list(Image.open(src).size),'normalized':str(out),'normalizedSHA256':first,'rerunSHA256':second,'targetContentSize':list(size),'offset':list(off),'metrics':metrics(out)})

contacts={}
for att in ['attempt-01','attempt-02']:
    contacts[att]={}
    for size in [200,80,32]:
        col=Image.new('RGB',(size*2,size*2));gry=Image.new('L',(size*2,size*2))
        for x,grade in enumerate(['g2','g3']):
            fg=Image.open(root/att/'normalized'/f'{grade}-hold.png').resize((size,size),Image.Resampling.LANCZOS)
            for y,bg in enumerate([(236,231,215),(20,23,29)]):
                c=Image.alpha_composite(Image.new('RGBA',(size,size),bg+(255,)),fg).convert('RGB');col.paste(c,(x*size,y*size));gry.paste(ImageOps.grayscale(c),(x*size,y*size))
        od=root/att/'evidence';od.mkdir(exist_ok=True);cp=od/f'joint-{size}-color.png';gp=od/f'joint-{size}-gray.png';col.save(cp);gry.save(gp);contacts[att][str(size)]={'color':str(cp),'colorSHA256':sha(cp),'gray':str(gp),'graySHA256':sha(gp)}
    ground=Image.new('RGB',(1200,700),(116,108,88));d=ImageDraw.Draw(ground)
    for y in range(70,700,70):d.line((0,y,1200,y+80),fill=(130,121,100),width=2)
    for x in range(-300,1500,120):d.line((x,0,x+340,700),fill=(103,96,80),width=2)
    for x,grade,scale,pos in [(0,'g2',.45,(60,140)),(1,'g3',.54,(610,80))]:
        fg=Image.open(root/att/'normalized'/f'{grade}-hold.png').resize((int(1024*scale),int(1024*scale)),Image.Resampling.LANCZOS);ground.paste(fg,pos,fg)
    p=root/att/'evidence/ground-placement.png';ground.save(p);contacts[att]['ground']={'path':str(p),'sha256':sha(p)}
manifest={'authoritySHA256':AUTH,'algorithm':'linear-light premultiplied RGBA separable Lanczos3, antialiased downscale, half-away rounding','members':members,'contacts':contacts,'pairMixing':0,'selection':'PENDING_VISUAL_REVIEW','motion':'HOLD','canonical':'HOLD'}
p=root/'normalization-manifest.json';p.write_text(json.dumps(manifest,indent=2)+'\n');print(sha(p))
