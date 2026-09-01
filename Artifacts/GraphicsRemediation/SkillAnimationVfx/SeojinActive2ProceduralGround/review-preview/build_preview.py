from pathlib import Path
import hashlib,json,math
import numpy as np
from PIL import Image,ImageDraw,ImageOps

ROOT=Path(__file__).resolve().parent
PROJECT=Path('/Users/pvenus/ProjectBS')
SHADER=PROJECT/'Assets/Shaders/SkillAnimationVfx.shader'
G2P=PROJECT/'Assets/Contents/Skill/vfx/vfx-seojin-active2-ground-g2.asset'
G3P=PROJECT/'Assets/Contents/Skill/vfx/vfx-seojin-active2-ground-g3.asset'
IMPL=Path('/private/tmp/projectbs-current-hangyeol-seojin-active2-procedural-ground-vfx-implementation.txt')
FPS=15; FRAMES=75; SIZE=512; WORLD_HALF=4.5
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def frac(x):return x-np.floor(x)
def hsh(px,py,seed):return frac(np.sin(px*127.1+py*311.7+seed)*43758.5453)
def noise(x,y,seed):
 cx=np.floor(x);cy=np.floor(y);fx=frac(x);fy=frac(y);fx=fx*fx*(3-2*fx);fy=fy*fy*(3-2*fy)
 a=hsh(cx,cy,seed);b=hsh(cx+1,cy,seed);d=hsh(cx,cy+1,seed);e=hsh(cx+1,cy+1,seed)
 return (a*(1-fx)+b*fx)*(1-fy)+(d*(1-fx)+e*fx)*fy
def smooth(a,b,x):t=np.clip((x-a)/(b-a),0,1);return t*t*(3-2*t)
def pulse(phase,center,width):d=np.abs(frac(phase-center+.5)-.5);x=np.clip(1-d/max(.001,width),0,1);return x*x*(3-2*x)
def ground():
 im=Image.new('RGB',(SIZE,SIZE),(118,111,91));d=ImageDraw.Draw(im)
 for k in range(-8,9):
  y=SIZE//2+k*28;d.line((0,y,SIZE,y),fill=(126,119,98),width=1)
  x=SIZE//2+k*36;d.line((x,0,x,SIZE),fill=(105,100,84),width=1)
 return np.array(im)
def render(profile,phase):
 y,x=np.mgrid[0:SIZE,0:SIZE];wx=(x/(SIZE-1)*2-1)*WORLD_HALF;wy=(y/(SIZE-1)*2-1)*WORLD_HALF
 pX=wx/profile['radius'];pY=wy/profile['radius'];aspect=.62
 npx=pX+np.sin(phase*2*np.pi)*.015;npy=pY+np.sin(phase*2*np.pi)*.006;seed=float(profile['seed']&0x00ffffff)
 low=noise(npx*.85+.13,npy*.85+.37,seed);high=noise(npx*3.4+.71,npy*3.4+.19,seed)
 irregular=(low-.5)*2*profile['edgeIrregularity'];r=np.sqrt(pX*pX+(pY/aspect)**2);signed=1-r+irregular*smooth(.82,1,r);foot=smooth(0,.18,signed);interior=smooth(1,.74,r);foot=np.maximum(foot,interior*.82)
 supportPulse=.85+(1-.85)*pulse(phase,.4,.25);pressurePulse=.55+(1-.55)*pulse(phase,.6,.24)
 support=np.exp(-((pX+.24)**2+(pY-.08)**2)/.24)+np.exp(-((pX-.18)**2+(pY+.10)**2)/.27)
 if profile['lobes']>=3:support+=np.exp(-((pX+.02)**2+(pY-.22)**2)/.25)
 support=np.clip(support*.52,0,1)*foot
 scar=np.exp(-((pX-.47)**2/.018+(pY-.02)**2/.055))
 scar=np.maximum(scar,np.exp(-((pX-.33)**2/.012+(pY-.23)**2/.035))*.7)
 scar=np.maximum(scar,np.exp(-((pX-.38)**2/.014+(pY+.24)**2/.04))*.62)
 if profile['scars']>=4:scar=np.maximum(scar,np.exp(-((pX-.12)**2/.012+(pY+.32)**2/.035))*.55)
 scar*=foot*pressurePulse;densityPulse=.94+(1.03-.94)*pulse(phase,.6,.25);grain=low*(1-.18)+high*.18
 alpha=foot*np.clip((profile['ink']+grain*.14)*densityPulse,0,1);alpha=np.where(alpha>=.01,alpha,0)
 sig=np.array([.16,.29,.41]);aux=np.array([.78,.34,.26]);navy=sig[None,None,:]*(.52+(.78-.52)*grain[...,None]);supportColor=navy*(1-(profile['support']*supportPulse*support)[...,None])+np.array([.36,.45,.52])[None,None,:]*(profile['support']*supportPulse*support)[...,None]
 rust=scar*np.clip(profile['rust']/.04,0,1);rgb=supportColor*(1-rust[...,None])+aux[None,None,:]*rust[...,None]
 prem=rgb*alpha[...,None];bg=ground().astype(float)/255.;out=prem+bg*(1-alpha[...,None]);return np.clip(np.floor(out*255+.5),0,255).astype(np.uint8),np.clip(np.floor(alpha*255+.5),0,255).astype(np.uint8)
def gif(frames,p):
 ims=[Image.fromarray(f) for f in frames];ims[0].save(p,save_all=True,append_images=ims[1:],duration=round(1000/FPS),loop=0,optimize=False,disposal=2)
def main():
 ROOT.mkdir(parents=True,exist_ok=True)
 profiles={'G2':dict(radius=3.,edgeIrregularity=.10,ink=.62,support=.14,lobes=2,scars=3,rust=.025,seed=0xA2C20002),'G3':dict(radius=3.5,edgeIrregularity=.12,ink=.68,support=.17,lobes=3,scars=4,rust=.035,seed=0xA2C20003)}
 allf={};alph={};rows={}
 for name,p in profiles.items():
  fs=[];aa=[]
  for i in range(FRAMES):f,a=render(p,(i%15)/15);fs.append(f);aa.append(a)
  allf[name]=fs;alph[name]=aa;gp=ROOT/f'active2-{name.lower()}-topdown-simulated-5s.gif';gif(fs,gp)
  contacts=[]
  for phase in [0,.2,.4,.6,.8]:contacts.append(Image.fromarray(render(p,phase)[0]).resize((256,256),Image.Resampling.LANCZOS))
  cp=ROOT/f'active2-{name.lower()}-t0-t4-contact.png';sheet=Image.new('RGB',(1280,256));
  for i,im in enumerate(contacts):sheet.paste(im,(i*256,0))
  sheet.save(cp,compress_level=9);rows[name]=dict(gif=str(gp),gifSHA256=sha(gp),contact=str(cp),contactSHA256=sha(cp),profile=p)
 both=[]
 for i in range(FRAMES):
  im=Image.new('RGB',(SIZE*2,SIZE));im.paste(Image.fromarray(allf['G2'][i]),(0,0));im.paste(Image.fromarray(allf['G3'][i]),(SIZE,0));both.append(np.array(im))
 bp=ROOT/'active2-g2-g3-comparison-simulated-5s.gif';gif(both,bp)
 man=dict(status='SIMULATED_SHADER_PREVIEW_NOT_RUNTIME_CAPTURE',implementationReceiptSHA256=sha(IMPL),shader=dict(path=str(SHADER),sha256=sha(SHADER)),profiles=dict(G2=dict(path=str(G2P),sha256=sha(G2P)),G3=dict(path=str(G3P),sha256=sha(G3P))),render=dict(size=[SIZE,SIZE],fps=FPS,frames=FRAMES,durationSeconds=5,loopsOfPresentationClock=5,ground='neutral grid',blend='premultiplied source-over'),outputs=rows,comparison=dict(path=str(bp),sha256=sha(bp)),deterministicSeeds={'G2':'0xA2C20002','G3':'0xA2C20003'},unityHeadless=0,runtimeCapture=False,projectCanonicalWrite=0)
 mp=ROOT/'manifest.json';mp.write_text(json.dumps(man,indent=2)+'\n');print(json.dumps({'manifest':str(mp),'manifestSHA':sha(mp),'G2':rows['G2']['gif'],'G2SHA':rows['G2']['gifSHA256'],'G3':rows['G3']['gif'],'G3SHA':rows['G3']['gifSHA256'],'comparison':str(bp),'comparisonSHA':sha(bp)}))
if __name__=='__main__':main()
