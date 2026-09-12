from pathlib import Path
from PIL import Image, ImageOps
import hashlib, json

SRC=Path('/Users/pvenus/ProjectBS/Artifacts/candidates/M3-COMBO-REV2-v20260909')
PREV=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinMouse3Exact3/selected/revision-01')
OUT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinMouse3Exact3/selected/revision-02-combo-rework')
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def sanitize(im):
    im=im.convert('RGBA'); d=bytearray(im.tobytes())
    for i in range(0,len(d),4):
        if d[i+3]==0: d[i]=d[i+1]=d[i+2]=0
    return Image.frombytes('RGBA',im.size,bytes(d))
def fit(path,size,pad,bottom=False):
    im=sanitize(Image.open(path)); box=im.getchannel('A').getbbox(); c=im.crop(box)
    sc=min((size[0]-2*pad)/c.width,(size[1]-2*pad)/c.height); wh=(max(1,round(c.width*sc)),max(1,round(c.height*sc)))
    c=c.resize(wh,Image.Resampling.LANCZOS); o=Image.new('RGBA',size); x=(size[0]-wh[0])//2; y=(size[1]-pad-wh[1] if bottom else (size[1]-wh[1])//2); o.alpha_composite(c,(x,y)); return sanitize(o)
def save_unit(skill,kind,count,size,pad):
    d=OUT/skill/kind; d.mkdir(parents=True,exist_ok=True); frames=[]; rows=[]
    for i in range(count):
        src=SRC/skill/kind/f'frame_{i+1:02d}.png'; im=fit(src,size,pad,kind=='body'); p=d/f'frame-{i:02d}.png'; im.save(p); frames.append(im)
        rows.append({'skill':skill,'kind':kind,'frame':i,'source':str(src),'sourceSha256':sha(src),'path':str(p),'sha256':sha(p),'size':list(size),'mode':'RGBA','alphaBbox':list(im.getchannel('A').getbbox())})
    delay=50 if count==12 else 80; gp=OUT/'evidence'/f'{skill}-{kind}.gif'; gp.parent.mkdir(parents=True,exist_ok=True); frames[0].save(gp,save_all=True,append_images=frames[1:],duration=delay,loop=0,disposal=2)
    rows.append({'skill':skill,'kind':kind+'Gif','path':str(gp),'sha256':sha(gp),'physicalFrames':count,'delayMs':delay})
    for s in (200,80,32):
        tw=round(size[0]*s/size[1])
        for bgname,bg,gray in [('dark',(20,24,31),False),('light',(236,238,241),False),('gray',(128,128,128),True)]:
            ims=[]
            for im in frames:
                t=im.resize((tw,s),Image.Resampling.LANCZOS); b=Image.new('RGBA',(tw,s),bg+(255,)); b.alpha_composite(t)
                ims.append(ImageOps.grayscale(b.convert('RGB')).convert('RGBA') if gray else b)
            sh=Image.new('RGBA',(tw*count,s),bg+(255,))
            for i,im in enumerate(ims): sh.alpha_composite(im,(tw*i,0))
            p=OUT/'evidence'/f'{skill}-{kind}-contact-{s}-{bgname}.png'; sh.save(p); rows.append({'skill':skill,'kind':'contact','subject':kind,'scale':s,'background':bgname,'path':str(p),'sha256':sha(p)})
    return rows
def main():
    rows=[]; rows+=save_unit('active_6','body',12,(568,340),12); rows+=save_unit('active_6','vfx',12,(256,256),12); rows+=save_unit('active_7','vfx',6,(256,256),12)
    manifest={'task':'M3-COMBO-REV2','status':'DELTA_ART_CANDIDATE','authorityMeaning':{'active_5':'retain revision-01 all','active_6':'replace body/VFX; icon retained','active_7':'replace VFX only; body/icon retained'},'runtimeTiming':{'active_6':{'contact1':.16,'contact2L4':.38,'bodyTotalSingle':.42,'bodyTotalDouble':.56},'active_7':{'fanDegrees':60,'gatherTarget':'aim point inside fan'}},'sourceManifest':str(SRC/'manifest.json'),'sourceManifestSha256':sha(SRC/'manifest.json'),'previousManifest':str(PREV/'manifest.json'),'previousManifestSha256':sha(PREV/'manifest.json'),'assets':rows,'install':False,'unity':False,'rollback':'remove revision-02-combo-rework; revision-01 unchanged'}
    OUT.mkdir(parents=True,exist_ok=True); p=OUT/'manifest.json'; p.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n'); print(p); print(sha(p))
if __name__=='__main__': main()
