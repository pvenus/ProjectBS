from pathlib import Path
from PIL import Image, ImageOps
import hashlib,json

SRC=Path('/Users/pvenus/ProjectBS/Artifacts/candidates/JANGDAN-SKILLSET-V1-v20260909')
REC=SRC/'standalone-recovery'
OUT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinJangdanSkillset/selected/revision-01')
UNITS={'active5_dung_kung':12,'active6_gi':6,'active7_deok':6,'active8_deoreoreoreo':12}
ICONS={'active5_dung_kung':'active5-jangdan-dung-kung.png','active6_gi':'active6-gi.png','active7_deok':'active7-deok.png','active8_deoreoreoreo':'active8-deoreoreoreo.png'}
REPLACE={
('active6_gi','body',6),('active6_gi','vfx',1),('active7_deok','body',5),('active7_deok','body',6),('active7_deok','vfx',2),('active7_deok','vfx',3),
('active8_deoreoreoreo','body',1),*[('active8_deoreoreoreo','body',i) for i in range(4,13)],*[('active8_deoreoreoreo','vfx',i) for i in range(1,13)]}
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def clean(im):
 im=im.convert('RGBA'); d=bytearray(im.tobytes())
 for i in range(0,len(d),4):
  if d[i+3]==0:d[i]=d[i+1]=d[i+2]=0
 return Image.frombytes('RGBA',im.size,bytes(d))
def fit(src,size,pad,bottom=False):
 im=clean(Image.open(src)); box=im.getchannel('A').getbbox(); c=im.crop(box); s=min((size[0]-2*pad)/c.width,(size[1]-2*pad)/c.height); wh=(round(c.width*s),round(c.height*s)); c=c.resize(wh,Image.Resampling.LANCZOS); o=Image.new('RGBA',size); x=(size[0]-wh[0])//2; y=size[1]-pad-wh[1] if bottom else (size[1]-wh[1])//2; o.alpha_composite(c,(x,y)); return clean(o)
def contact(frames,size,bg,gray):
 ims=[]
 for f in frames:
  tw=round(f.width*size/f.height); t=f.resize((tw,size),Image.Resampling.LANCZOS); b=Image.new('RGBA',(tw,size),bg+(255,)); b.alpha_composite(t); ims.append(ImageOps.grayscale(b.convert('RGB')).convert('RGBA') if gray else b)
 o=Image.new('RGBA',(ims[0].width*len(ims),size),bg+(255,))
 for i,im in enumerate(ims):o.alpha_composite(im,(i*im.width,0))
 return o
def main():
 rows=[];(OUT/'evidence').mkdir(parents=True,exist_ok=True)
 for u,icon in ICONS.items():
  p=OUT/'icons'/f'{u}.png';p.parent.mkdir(parents=True,exist_ok=True);im=fit(SRC/'icons'/icon,(512,512),24);im.save(p);rows.append({'skill':u,'kind':'icon','path':str(p),'sha256':sha(p),'size':[512,512]})
 for u,count in UNITS.items():
  for kind,size,pad in [('body',(568,340),12),('vfx',(256,256),12)]:
   frames=[];d=OUT/kind/u;d.mkdir(parents=True,exist_ok=True)
   for n in range(1,count+1):
    src=(REC/u/kind/f'frame_{n:02d}.png') if (u,kind,n) in REPLACE else (SRC/u/kind/f'frame_{n:02d}.png')
    im=fit(src,size,pad,kind=='body');p=d/f'frame-{n-1:02d}.png';im.save(p);frames.append(im);rows.append({'skill':u,'kind':kind,'frame':n-1,'source':str(src),'sourceSha256':sha(src),'path':str(p),'sha256':sha(p),'size':list(size),'alphaBbox':list(im.getchannel('A').getbbox())})
   delay=50 if count==12 else 62 if u=='active6_gi' else 75
   gp=OUT/'evidence'/f'{u}-{kind}.gif';frames[0].save(gp,save_all=True,append_images=frames[1:],duration=delay,loop=0,disposal=2);rows.append({'skill':u,'kind':kind+'Gif','path':str(gp),'sha256':sha(gp),'frames':count,'delayMs':delay})
   for s in (200,80,32):
    for name,bg,g in [('light',(236,238,241),False),('dark',(20,24,31),False),('gray',(128,128,128),True)]:
     cp=OUT/'evidence'/f'{u}-{kind}-{s}-{name}.png';contact(frames,s,bg,g).save(cp);rows.append({'skill':u,'kind':'contact','subject':kind,'scale':s,'background':name,'path':str(cp),'sha256':sha(cp)})
 timing={'bpm':120,'beat':.5,'subdivision':.125,'active5_dung_kung':{'contacts':[.25,.75],'total':1.0},'active6_gi':{'contacts':[.125],'total':.25},'active7_deok':{'contacts':[.25],'total':.375},'active8_deoreoreoreo':{'contacts':[.125,.25,.375,.625],'total':.75}}
 m={'task':'JANGDAN-SKILLSET-V1','status':'SELECTED_SOURCE_CANDIDATE_QA_PENDING','counts':{'icons':4,'body':36,'vfx':36,'runtimePng':76},'timing':timing,'recoveredStandaloneRows':28,'assets':rows,'install':False,'unity':False,'rollback':'remove revision-01 only; prior M3 revisions unchanged'};p=OUT/'manifest.json';p.write_text(json.dumps(m,ensure_ascii=False,indent=2)+'\n');print(p);print(sha(p))
if __name__=='__main__':main()
