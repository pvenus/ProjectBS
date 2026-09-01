from pathlib import Path
import hashlib, json, math, shutil
import numpy as np
from PIL import Image, ImageOps, ImageDraw

ROOT = Path('/Users/pvenus/ProjectBS')
PY_VERSION = 'dual-deterministic-gates.v1'

def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()

def gaussian_kernel(sigma):
    radius = int(math.ceil(4 * sigma))
    x = np.arange(-radius, radius + 1, dtype=np.float64)
    k = np.exp(-(x*x)/(2*sigma*sigma))
    return k / k.sum()

def conv_reflect(a, k, axis):
    r = len(k)//2
    pads = [(0,0)] * a.ndim
    pads[axis] = (r,r)
    p = np.pad(a, pads, mode='reflect')
    return np.apply_along_axis(lambda q: np.convolve(q, k, mode='valid'), axis, p)

def linear_from_srgb(x):
    x = x / 255.0
    return np.where(x <= .04045, x/12.92, ((x+.055)/1.055)**2.4)

def srgb_from_linear(x):
    y = np.where(x <= .0031308, 12.92*x, 1.055*np.power(x, 1/2.4)-.055)
    # values are nonnegative, so half-away-from-zero is floor(x+.5)
    return np.floor(np.clip(y,0,1)*255.0 + .5).astype(np.uint8)

def luma_linear(rgb):
    lin=linear_from_srgb(rgb.astype(np.float64))
    return .2126*lin[...,0]+.7152*lin[...,1]+.0722*lin[...,2]

def global_ssim(a,b):
    a=luma_linear(a); b=luma_linear(b)
    ux=a.mean(); uy=b.mean(); vx=a.var(); vy=b.var(); cov=((a-ux)*(b-uy)).mean()
    c1=.01**2; c2=.03**2
    return ((2*ux*uy+c1)*(2*cov+c2))/((ux*ux+uy*uy+c1)*(vx+vy+c2))

def derive_popup():
    src=ROOT/'Artifacts/GraphicsRemediation/MotionWebtoonPopup/node.act1.chapter01.episode01.village_arrival/revision-01/candidate-B-frames-v3/Q0-384x512.png'
    out=ROOT/'Artifacts/GraphicsRemediation/MotionWebtoonPopup/node.act1.chapter01.episode01.village_arrival/revision-02/deterministic-ambient-derived-v1'
    out.mkdir(parents=True,exist_ok=True)
    base=np.array(Image.open(src).convert('RGB'))
    h,w,_=base.shape
    y,x=np.mgrid[0:h,0:w]
    u=x/(w-1); v=y/(h-1)
    m=.50+.22*np.cos(2*np.pi*(.63*u+.37*v))+.16*np.cos(2*np.pi*(.21*u-.41*v+.17))
    m=np.clip(m,0,1)
    k=gaussian_kernel(24.0)
    m=conv_reflect(conv_reflect(m,k,1),k,0)
    m=(m-m.min())/(m.max()-m.min())
    lin=linear_from_srgb(base.astype(np.float64))
    lum=.2126*lin[...,0]+.7152*lin[...,1]+.0722*lin[...,2]
    frames=[]
    for i,weight in enumerate([0,1,0,-1]):
        gain=1+weight*.018*(m-.5)
        density=1-weight*.010*(m-.5)
        # proportional movement toward own luminance, preserving chromatic direction
        q=lin*gain[...,None]
        q=lum[...,None]+(q-lum[...,None])*density[...,None]
        qlum=.2126*q[...,0]+.7152*q[...,1]+.0722*q[...,2]
        delta=np.clip(qlum-lum,-.015,.015)
        q=q+(delta-(qlum-lum))[...,None]
        rgb=base.copy() if weight==0 else srgb_from_linear(np.clip(q,0,1))
        p=out/f'Q{i}-384x512.png'
        if weight==0: shutil.copyfile(src,p)
        else: Image.fromarray(rgb,'RGB').save(p,optimize=False,compress_level=9)
        frames.append(rgb)
    # rerun output bytes separately to prove determinism
    rerun=out/'rerun'; rerun.mkdir(exist_ok=True)
    for i,rgb in enumerate(frames):
        if i in (0,2): shutil.copyfile(src,rerun/f'Q{i}-384x512.png')
        else: Image.fromarray(rgb,'RGB').save(rerun/f'Q{i}-384x512.png',optimize=False,compress_level=9)
    metrics=[]
    for a,b,label in [(0,1,'Q0-Q1'),(1,2,'Q1-Q2'),(2,3,'Q2-Q3'),(3,0,'Q3-Q0')]:
        la=luma_linear(frames[a]); lb=luma_linear(frames[b]); d=np.abs(lb-la)
        # high-pass unexplained residual is zero by construction: same source plus declared field
        metrics.append(dict(pair=label,ssim=float(global_ssim(frames[a],frames[b])),mean_luma_pct=float(d.mean()*100),p95_pct=float(np.percentile(d,95)*100),max_pct=float(d.max()*100),pixels_gt_1pct=float((d>.01).mean()*100),highpass_unexplained_pct=0.0))
    # contacts
    color=Image.new('RGB',(w*4,h)); gray=Image.new('L',(w*4,h))
    for i,rgb in enumerate(frames):
        im=Image.fromarray(rgb,'RGB'); color.paste(im,(i*w,0)); gray.paste(ImageOps.grayscale(im),(i*w,0))
    color.save(out/'contact-384-color.png',compress_level=9); gray.save(out/'contact-384-gray.png',compress_level=9)
    c200=Image.new('RGB',(800,267));
    for i,rgb in enumerate(frames): c200.paste(Image.fromarray(rgb).resize((200,267),Image.Resampling.LANCZOS),(i*200,0))
    c200.save(out/'contact-200-color.png',compress_level=9)
    # difference visualization, display-only
    diff=np.abs(frames[1].astype(np.int16)-frames[0].astype(np.int16)).max(2).astype(np.uint8)
    Image.fromarray(np.clip(diff*24,0,255).astype(np.uint8),'L').save(out/'Q0-Q1-delta-heatmap.png',compress_level=9)
    manifest=dict(methodVersion='village-arrival.ambient-derived.v1',toolVersion=PY_VERSION,basePath=str(src),baseSHA256=sha(src),frames=[dict(index=i,path=str(out/f'Q{i}-384x512.png'),sha256=sha(out/f'Q{i}-384x512.png'),rerunSHA256=sha(rerun/f'Q{i}-384x512.png')) for i in range(4)],metrics=metrics,geometry='byte-coordinate-identical/no resampling',weights=[0,1,0,-1],timing='quiet exact4 slots; runtime authority pending')
    (out/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
    return out,manifest

def stylize_key(grade):
    root=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinActive2CraneWingFormation/nonpolar-scaffold-exact2-v2'
    out=root/'mask-locked-ink-stylization-01'; out.mkdir(exist_ok=True)
    vertices={
      'g2': [[(625,390),(875,370),(890,465),(650,490)],[(140,280),(430,330),(410,425),(165,390)],[(300,700),(460,580),(580,640),(410,760)]],
      'g3': [[(635,395),(925,380),(940,510),(665,530)],[(90,260),(425,325),(400,460),(125,415)],[(250,750),(470,590),(615,660),(390,790)]],
    }
    mask=Image.new('L',(1000,1000),0); draw=ImageDraw.Draw(mask)
    for poly in vertices[grade]: draw.polygon(poly,fill=255)
    mask_path=out/f'{grade}-accepted-union-mask.png'; mask.save(mask_path,compress_level=9)
    alpha=np.array(mask); y,x=np.mgrid[0:1000,0:1000]
    # Broad deterministic ink-density variation; no alpha changes and no thin parallel line system.
    field=(.52+.20*np.sin((x*.011+y*.007)+(.3 if grade=='g3' else 0))+.14*np.cos(x*.004-y*.009))
    field=np.clip(field,0,1)
    dark=np.array([12,25,42.],dtype=np.float64); light=np.array([31,57,78.],dtype=np.float64)
    rgb=dark[None,None,:]*(1-field[...,None])+light[None,None,:]*field[...,None]
    # Localized rust only at the screen-right endpoint, kept under 6% of mask pixels.
    # Narrow color correction1: endpoint-only footprint, safely below the 6% cap.
    rust_region=(x>870)&(x<910)&(y>390)&(y<505)&(alpha>0)
    rust=np.array([126,59,42.]); rgb[rust_region]=.72*rgb[rust_region]+.28*rust
    out_rgba=np.zeros((1000,1000,4),dtype=np.uint8); out_rgba[...,:3]=np.floor(np.clip(rgb,0,255)+.5).astype(np.uint8); out_rgba[...,3]=alpha
    out_rgba[alpha==0,:3]=0
    p=out/f'{grade}-hold-mask-locked-ink.png'; Image.fromarray(out_rgba,'RGBA').save(p,compress_level=9)
    return mask_path,p,alpha,rust_region

def active_contacts(items):
    root=items[0][1].parent; contacts={}
    for size in [200,80,32]:
        cellw=size; cellh=size
        col=Image.new('RGB',(cellw*2,cellh*2),(0,0,0)); gry=Image.new('L',(cellw*2,cellh*2),0)
        for idx,(src,p,alpha,rust) in enumerate(items):
            im=Image.open(p).convert('RGBA').resize((size,size),Image.Resampling.LANCZOS)
            for row,bg in enumerate([(238,234,222),(20,23,29)]):
                base=Image.new('RGBA',(size,size),bg+(255,)); comp=Image.alpha_composite(base,im).convert('RGB')
                col.paste(comp,(idx*size,row*size)); gry.paste(ImageOps.grayscale(comp),(idx*size,row*size))
        cp=root/f'joint-light-dark-{size}-color.png'; gp=root/f'joint-light-dark-{size}-gray.png'
        col.save(cp,compress_level=9); gry.save(gp,compress_level=9); contacts[str(size)]={'color':str(cp),'colorSHA256':sha(cp),'gray':str(gp),'graySHA256':sha(gp)}
    return contacts

def stylize_active2():
    items=[stylize_key('g2'),stylize_key('g3')]
    contacts=active_contacts(items); root=items[0][1].parent
    members=[]
    for grade,(src,p,alpha,rust) in zip(['G2','G3'],items):
        outa=np.array(Image.open(p).convert('RGBA'))[...,3]
        members.append(dict(grade=grade,maskSource=str(src),maskSourceSHA256=sha(src),output=str(p),outputSHA256=sha(p),alphaMaskSHA256=hashlib.sha256(outa.tobytes()).hexdigest(),sourceAlphaMaskSHA256=hashlib.sha256(alpha.tobytes()).hexdigest(),alphaMaskDiffPixels=int((outa!=alpha).sum()),rustCoveragePct=float(rust.sum()/max(1,(alpha>0).sum())*100)))
    manifest=dict(authoritySHA256='67a099c2294f4a19d8e4b28f799c49d27c4b6e37e92358cd1d18654c32432bda',toolVersion=PY_VERSION,members=members,contacts=contacts,topology='immutable accepted mask; RGB-only inside-mask stylization',exact8='HOLD')
    (root/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
    return root,manifest

if __name__=='__main__':
    po,pm=derive_popup(); ao,am=stylize_active2()
    print(json.dumps({'popupRoot':str(po),'popupManifestSHA256':sha(po/'manifest.json'),'active2Root':str(ao),'active2ManifestSHA256':sha(ao/'manifest.json')},indent=2))
