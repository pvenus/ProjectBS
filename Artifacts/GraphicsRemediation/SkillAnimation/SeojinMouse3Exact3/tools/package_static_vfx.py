from pathlib import Path
from PIL import Image, ImageOps, ImageDraw
import hashlib, json

SRC=Path('/Users/pvenus/ProjectBS/Artifacts/candidates/M3-ART-V1')
OUT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinMouse3Exact3/selected/revision-01')
SKILLS=['command_chain','thunder_command','blockade_cut']

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def clean(im):
    im=im.convert('RGBA')
    px=im.load()
    for y in range(im.height):
        for x in range(im.width):
            if px[x,y][3]==0: px[x,y]=(0,0,0,0)
    return im

def contain(im,size,pad):
    a=im.getchannel('A'); box=a.getbbox()
    if not box: return Image.new('RGBA',size)
    crop=im.crop(box)
    scale=min((size[0]-2*pad)/crop.width,(size[1]-2*pad)/crop.height)
    wh=(max(1,round(crop.width*scale)),max(1,round(crop.height*scale)))
    crop=crop.resize(wh,Image.Resampling.LANCZOS)
    dst=Image.new('RGBA',size)
    dst.alpha_composite(crop,((size[0]-wh[0])//2,(size[1]-wh[1])//2))
    return clean(dst)

def contact(frames,scale,bg,gray=False):
    thumbs=[]
    for im in frames:
        th=im.resize((scale,scale),Image.Resampling.LANCZOS)
        base=Image.new('RGBA',(scale,scale),bg+(255,))
        base.alpha_composite(th)
        if gray: base=ImageOps.grayscale(base.convert('RGB')).convert('RGBA')
        thumbs.append(base)
    sheet=Image.new('RGBA',(scale*6,scale),bg+(255,))
    for i,th in enumerate(thumbs): sheet.alpha_composite(th,(i*scale,0))
    return sheet

def main():
    (OUT/'icons').mkdir(parents=True,exist_ok=True)
    for k in ('vfx','evidence'): (OUT/k).mkdir(parents=True,exist_ok=True)
    rows=[]
    layouts={'command_chain':(3,2),'thunder_command':(3,2),'blockade_cut':(3,2)}
    for skill in SKILLS:
        icon=contain(clean(Image.open(SRC/'icons'/f'{skill}.png')),(512,512),24)
        ip=OUT/'icons'/f'{skill}.png'; icon.save(ip)
        rows.append({'skill':skill,'kind':'icon','frame':None,'path':str(ip),'sha256':sha(ip),'size':[512,512],'mode':'RGBA'})
        board=clean(Image.open(SRC/'vfx'/f'{skill}-board.png'))
        cols,rs=layouts[skill]; cw=board.width//cols; ch=board.height//rs
        frames=[]
        d=OUT/'vfx'/skill; d.mkdir(parents=True,exist_ok=True)
        for i in range(6):
            x=(i%cols)*cw; y=(i//cols)*ch
            cell=board.crop((x,y,x+cw,y+ch))
            frame=contain(cell,(256,256),10)
            fp=d/f'frame-{i:02d}.png'; frame.save(fp); frames.append(frame)
            rows.append({'skill':skill,'kind':'vfx','frame':i,'path':str(fp),'sha256':sha(fp),'size':[256,256],'mode':'RGBA'})
        durations={'command_chain':[70,70,40,120,60,60],'thunder_command':[80,80,80,120,70,70],'blockade_cut':[50,50,40,140,90,90]}[skill]
        gp=OUT/'evidence'/f'{skill}-vfx.gif'
        frames[0].save(gp,save_all=True,append_images=frames[1:],duration=durations,loop=0,disposal=2)
        rows.append({'skill':skill,'kind':'vfxGif','path':str(gp),'sha256':sha(gp),'physicalFrames':6,'durationsMs':durations})
        for s in (200,80,32):
            for name,bg in [('light',(236,238,241)),('dark',(20,24,31))]:
                cp=OUT/'evidence'/f'{skill}-vfx-{s}-{name}.png'; contact(frames,s,bg).save(cp)
                rows.append({'skill':skill,'kind':'contact','scale':s,'background':name,'path':str(cp),'sha256':sha(cp)})
            cp=OUT/'evidence'/f'{skill}-vfx-{s}-gray.png'; contact(frames,s,(128,128,128),True).save(cp)
            rows.append({'skill':skill,'kind':'contact','scale':s,'background':'gray','path':str(cp),'sha256':sha(cp)})
    manifest={'task':'M3-ART-V1','status':'STATIC_VFX_ICONS_COMPLETE_BODY_PENDING','authoritySha256':'15cb5535c40f4a584172932d15d7935ffd0012dbfd913e3408d7db4e88dab9dc','timing':{'command_chain':{'bodyDuration':.42,'contact':.18},'thunder_command':{'bodyDuration':.50,'contact':.24},'blockade_cut':{'bodyDuration':.46,'contact':.14}},'assets':rows,'install':False,'unity':False}
    mp=OUT/'static-vfx-icons-manifest.json'; mp.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
    print(mp); print(sha(mp))

if __name__=='__main__': main()
