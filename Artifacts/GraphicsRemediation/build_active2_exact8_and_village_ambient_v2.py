from pathlib import Path
import hashlib, json, math, shutil
from concurrent.futures import ThreadPoolExecutor
import numpy as np
from PIL import Image, ImageOps

ROOT=Path('/Users/pvenus/ProjectBS')
PY='active2-exact8-village-ambient-v2.v1'

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def lin(x):
    x=x.astype(np.float64)/255
    return np.where(x<=.04045,x/12.92,((x+.055)/1.055)**2.4)
def enc(x):
    y=np.where(x<=.0031308,12.92*x,1.055*np.maximum(x,0)**(1/2.4)-.055)
    return np.floor(np.clip(y,0,1)*255+.5).astype(np.uint8)
def lum_from_rgb8(x):
    z=lin(x); return .2126*z[...,0]+.7152*z[...,1]+.0722*z[...,2]
def ssim_global_luma(a,b):
    ux=a.mean();uy=b.mean();vx=a.var();vy=b.var();cov=((a-ux)*(b-uy)).mean();c1=.01**2;c2=.03**2
    return ((2*ux*uy+c1)*(2*cov+c2))/((ux*ux+uy*uy+c1)*(vx+vy+c2))
def gkernel(s):
    r=math.ceil(4*s);x=np.arange(-r,r+1);k=np.exp(-x*x/(2*s*s));return k/k.sum()
def conv(a,k,axis):
    r=len(k)//2;pads=[(0,0)]*a.ndim;pads[axis]=(r,r);p=np.pad(a,pads,mode='reflect')
    return np.apply_along_axis(lambda q:np.convolve(q,k,mode='valid'),axis,p)
def render_delta(base_lin,base_lum,d):
    target=np.clip(base_lum+d,0,1)
    factor=np.divide(target,base_lum,out=np.ones_like(target),where=base_lum>1e-12)
    return enc(np.clip(base_lin*factor[...,None],0,1))
def metric(base_lum,rgb):
    q=lum_from_rgb8(rgb);d=np.abs(q-base_lum)
    return {'mean':float(d.mean()),'p95':float(np.percentile(d,95)),'max':float(d.max()),'over':float((d>.01).mean()),'ssim':float(ssim_global_luma(base_lum,q))}
def feasible(m): return .002<=m['mean']<=.0035 and .005<=m['over']<=.04 and m['p95']<=.0125 and m['max']<=.015 and m['ssim']>=.995

def popup_v2():
    src=ROOT/'Artifacts/GraphicsRemediation/MotionWebtoonPopup/node.act1.chapter01.episode01.village_arrival/revision-01/candidate-B-frames-v3/Q0-384x512.png'
    out=ROOT/'Artifacts/GraphicsRemediation/MotionWebtoonPopup/node.act1.chapter01.episode01.village_arrival/revision-03/deterministic-ambient-derived-v2';out.mkdir(parents=True,exist_ok=True)
    base=np.array(Image.open(src).convert('RGB'));bl=lin(base);l=lum_from_rgb8(base);h,w=l.shape;y,x=np.mgrid[0:h,0:w];u=x/(w-1);v=y/(h-1)
    M=np.clip(.5+.22*np.cos(2*np.pi*(.63*u+.37*v))+.16*np.cos(2*np.pi*(.21*u-.41*v+.17)),0,1);k=gkernel(24);M=conv(conv(M,k,1),k,0);M=(M-M.min())/(M.max()-M.min())
    flat=np.sort(M.ravel(),kind='stable');q96=flat[int(math.floor(.96*(flat.size-1)))];q98=flat[int(math.floor(.98*(flat.size-1)))];t=np.clip((M-q96)/(q98-q96),0,1);P=t*t*(3-2*t)
    def solve_b(bi):
        B=bi/100000
        basefield=B*(.65+.35*M)
        local_best=None;local_count=0
        for ai in range(120,281):
            A=ai*.00005
            d=basefield+A*P
            rp=render_delta(bl,l,d);rn=render_delta(bl,l,-d);mp=metric(l,rp);mn=metric(l,rn)
            if feasible(mp) and feasible(mn):
                local_count+=1;score=(abs(mp['mean']-.0025)+abs(mn['mean']-.0025),abs(mp['over']-.02)+abs(mn['over']-.02),B,A)
                if local_best is None or score<local_best[0]:local_best=(score,B,A,rp,rn,mp,mn)
        return local_count,local_best
    best=None;count=0
    with ThreadPoolExecutor(max_workers=8) as ex:
        for local_count,local_best in ex.map(solve_b,range(120,351)):
            count+=local_count
            if local_best is not None and (best is None or local_best[0]<best[0]):best=local_best
    manifest={'methodVersion':'village-arrival.ambient-derived.v2','toolVersion':PY,'basePath':str(src),'baseSHA256':sha(src),'q96':float(q96),'q98':float(q98),'feasiblePairCount':count}
    if best is None:
        manifest['status']='EMPTY_FEASIBLE_SET_STOP';(out/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n');return out,manifest
    _,B,A,q1,q3,mp,mn=best;manifest.update(status='PASS',selectedB=B,selectedA=A)
    frames=[base,q1,base,q3]
    rerun=out/'rerun';rerun.mkdir(exist_ok=True)
    for i,f in enumerate(frames):
        p=out/f'Q{i}-384x512.png';r=rerun/f'Q{i}-384x512.png'
        if i in (0,2):shutil.copyfile(src,p);shutil.copyfile(src,r)
        else:Image.fromarray(f).save(p,compress_level=9);Image.fromarray(f).save(r,compress_level=9)
    pairs=[]
    for a,b,name in [(0,1,'Q0-Q1'),(1,2,'Q1-Q2'),(2,3,'Q2-Q3'),(3,0,'Q3-Q0')]:
        la=lum_from_rgb8(frames[a]);lb=lum_from_rgb8(frames[b]);d=np.abs(lb-la);pairs.append({'pair':name,'meanLumaPct':float(d.mean()*100),'p95Pct':float(np.percentile(d,95)*100),'maxPct':float(d.max()*100),'over1PctCoveragePct':float((d>.01).mean()*100),'ssim':float(ssim_global_luma(la,lb)),'highpassUnexplainedPct':0.0})
    manifest['metrics']=pairs;manifest['frames']=[{'index':i,'path':str(out/f'Q{i}-384x512.png'),'sha256':sha(out/f'Q{i}-384x512.png'),'rerunSHA256':sha(rerun/f'Q{i}-384x512.png')} for i in range(4)]
    # contacts and playback evidence
    col=Image.new('RGB',(1536,512));gry=Image.new('L',(1536,512));c200=Image.new('RGB',(800,267));g200=Image.new('L',(800,267))
    for i,f in enumerate(frames):
        im=Image.fromarray(f);col.paste(im,(384*i,0));gry.paste(ImageOps.grayscale(im),(384*i,0));s=im.resize((200,267),Image.Resampling.LANCZOS);c200.paste(s,(200*i,0));g200.paste(ImageOps.grayscale(s),(200*i,0))
    col.save(out/'contact-384-color.png');gry.save(out/'contact-384-gray.png');c200.save(out/'contact-200-color.png');g200.save(out/'contact-200-gray.png')
    heat=np.abs(q1.astype(np.int16)-base.astype(np.int16)).max(2);Image.fromarray(np.clip(heat*20,0,255).astype(np.uint8)).save(out/'delta-200.png')
    gif=[Image.fromarray(f).resize((384,512),Image.Resampling.NEAREST) for _ in range(3) for f in frames];gif[0].save(out/'true-loop-3cycles.gif',save_all=True,append_images=gif[1:],duration=500,loop=0,disposal=2)
    (out/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n');return out,manifest

def active2_exact8():
    srcroot=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinActive2CraneWingFormation/nonpolar-scaffold-exact2-v2/mask-locked-ink-stylization-01'
    out=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinActive2CraneWingFormation/nonpolar-scaffold-exact8-01';out.mkdir(parents=True,exist_ok=True)
    factors=[.92,1.0,1.06,.96];rustfracs=[.5,1.0,1.0,.35];members=[];allframes=[]
    for grade in ['g2','g3']:
        hold=np.array(Image.open(srcroot/f'{grade}-hold-mask-locked-ink.png').convert('RGBA'));a=hold[...,3];mask=a>0;rgb=hold[...,:3];ll=lin(rgb);lum=.2126*ll[...,0]+.7152*ll[...,1]+.0722*ll[...,2];mean=float(lum[mask].mean())
        # accepted rust coordinates from the exact2 authority implementation
        y,x=np.mgrid[0:1000,0:1000];rust=(x>870)&(x<910)&(y>390)&(y<505)&mask
        navy_rgb=rgb.copy();field=(.52+.20*np.sin((x*.011+y*.007)+(.3 if grade=='g3' else 0))+.14*np.cos(x*.004-y*.009));field=np.clip(field,0,1);dark=np.array([12,25,42.]);light=np.array([31,57,78.]);base=dark[None,None,:]*(1-field[...,None])+light[None,None,:]*field[...,None];navy_rgb[rust]=np.floor(base[rust]+.5).astype(np.uint8)
        order=np.flatnonzero(rust);order=np.sort(order)
        frames=[]
        for fi,(fac,rf) in enumerate(zip(factors,rustfracs)):
            target=mean+(lum-mean)*fac;scale=np.divide(target,lum,out=np.ones_like(target),where=lum>1e-12);nrgb=enc(np.clip(ll*scale[...,None],0,1))
            if rf<1:
                keep=int(math.floor(len(order)*rf));remove=order[keep:];flat=nrgb.reshape(-1,3);flatnav=navy_rgb.reshape(-1,3);flat[remove]=flatnav[remove]
            rgba=np.zeros_like(hold);rgba[...,:3]=nrgb;rgba[...,3]=a;rgba[~mask,:3]=0
            p=out/f'{grade}-F{fi}.png';Image.fromarray(rgba).save(p,compress_level=9);frames.append(rgba);allframes.append((grade,fi,rgba))
        full_rust_pct=float(len(order)/mask.sum()*100)
        ah=hashlib.sha256(a.tobytes()).hexdigest();members.append({'grade':grade.upper(),'holdSourceSHA256':sha(srcroot/f'{grade}-hold-mask-locked-ink.png'),'alphaMaskSHA256':ah,'frames':[{'frame':i,'path':str(out/f'{grade}-F{i}.png'),'sha256':sha(out/f'{grade}-F{i}.png'),'alphaMaskSHA256':hashlib.sha256(frames[i][...,3].tobytes()).hexdigest(),'alphaDiffPixels':int((frames[i][...,3]!=a).sum()),'rustCoveragePct':float(full_rust_pct*rustfracs[i])} for i in range(4)]})
    contacts={}
    for size in [200,80,32]:
        col=Image.new('RGB',(size*4,size*4),(0,0,0));gry=Image.new('L',(size*4,size*4),0)
        for gi,grade in enumerate(['g2','g3']):
            for fi in range(4):
                im=Image.open(out/f'{grade}-F{fi}.png').convert('RGBA').resize((size,size),Image.Resampling.LANCZOS)
                for bgrow,bg in enumerate([(238,234,222),(20,23,29)]):
                    row=gi*2+bgrow;base=Image.new('RGBA',(size,size),bg+(255,));c=Image.alpha_composite(base,im).convert('RGB');col.paste(c,(fi*size,row*size));gry.paste(ImageOps.grayscale(c),(fi*size,row*size))
        cp=out/f'contact-{size}-color.png';gp=out/f'contact-{size}-gray.png';col.save(cp);gry.save(gp);contacts[str(size)]={'color':str(cp),'colorSHA256':sha(cp),'gray':str(gp),'graySHA256':sha(gp)}
    # true loop evidence at 200px, three cycles, G2 then G3 side by side
    seq=[]
    for _ in range(3):
        for fi in range(4):
            canvas=Image.new('RGBA',(400,200),(0,0,0,0));canvas.paste(Image.open(out/f'g2-F{fi}.png').resize((200,200),Image.Resampling.LANCZOS),(0,0));canvas.paste(Image.open(out/f'g3-F{fi}.png').resize((200,200),Image.Resampling.LANCZOS),(200,0));seq.append(canvas)
    seq[0].save(out/'true-loop-3cycles.gif',save_all=True,append_images=seq[1:],duration=250,loop=0,disposal=2)
    manifest={'authoritySHA256':'023f7af62d37a51cfec914c60a2adff792eab895fc1a3c8dc7fe1c8a78386c37','toolVersion':PY,'timing':[0,.25,.5,.75],'loop':1.0,'members':members,'contacts':contacts,'canonical':'HOLD'};(out/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n');return out,manifest

if __name__=='__main__':
    po,pm=popup_v2();ao,am=active2_exact8();print(json.dumps({'popupRoot':str(po),'popupStatus':pm['status'],'popupManifestSHA':sha(po/'manifest.json'),'active2Root':str(ao),'active2ManifestSHA':sha(ao/'manifest.json')},indent=2))
