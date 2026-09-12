from pathlib import Path
from PIL import Image, ImageOps
import hashlib, json

SRC=Path('/Users/pvenus/ProjectBS/Artifacts/candidates/M3-ART-V1-body-recovery-v20260909')
OUT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinMouse3Exact3/selected/revision-01')
SKILLS=['command_chain','thunder_command','blockade_cut']
TIMES={'command_chain':[0,.07,.14,.18,.30,.42], 'thunder_command':[0,.08,.16,.24,.36,.50], 'blockade_cut':[0,.05,.10,.14,.28,.46]}
GIF_MS={'command_chain':[70,70,40,120,60,60], 'thunder_command':[80,80,80,120,70,70], 'blockade_cut':[50,50,40,140,90,90]}

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def sanitize(im):
    im=im.convert('RGBA'); d=bytearray(im.tobytes())
    for i in range(0,len(d),4):
        if d[i+3]==0: d[i]=d[i+1]=d[i+2]=0
    return Image.frombytes('RGBA',im.size,bytes(d))
def fit(im):
    box=im.getchannel('A').getbbox(); crop=im.crop(box)
    s=min(544/crop.width,316/crop.height)
    wh=(round(crop.width*s),round(crop.height*s))
    crop=crop.resize(wh,Image.Resampling.LANCZOS)
    dst=Image.new('RGBA',(568,340)); x=(568-wh[0])//2; y=328-wh[1]
    dst.alpha_composite(crop,(x,y)); return sanitize(dst)
def comp(im,size,bg,gray=False):
    thumb=im.resize((round(568*size/340),size),Image.Resampling.LANCZOS)
    base=Image.new('RGBA',thumb.size,bg+(255,)); base.alpha_composite(thumb)
    return ImageOps.grayscale(base.convert('RGB')).convert('RGBA') if gray else base

def main():
    rows=[]; (OUT/'body').mkdir(parents=True,exist_ok=True); (OUT/'evidence').mkdir(parents=True,exist_ok=True)
    for skill in SKILLS:
        frames=[]; d=OUT/'body'/skill; d.mkdir(parents=True,exist_ok=True)
        for i in range(6):
            src=SRC/skill/f'frame_{i+1:02d}.png'; im=fit(sanitize(Image.open(src)))
            fp=d/f'frame-{i:02d}.png'; im.save(fp); frames.append(im)
            box=im.getchannel('A').getbbox(); rows.append({'skill':skill,'kind':'body','frame':i,'time':TIMES[skill][i],'source':str(src),'sourceSha256':sha(src),'path':str(fp),'sha256':sha(fp),'size':[568,340],'mode':'RGBA','alphaBbox':list(box)})
        gp=OUT/'evidence'/f'{skill}-body.gif'; frames[0].save(gp,save_all=True,append_images=frames[1:],duration=GIF_MS[skill],loop=0,disposal=2)
        rows.append({'skill':skill,'kind':'bodyGif','path':str(gp),'sha256':sha(gp),'physicalFrames':6,'durationsMs':GIF_MS[skill]})
        for size in (200,80,32):
            for name,bg in [('light',(236,238,241)),('dark',(20,24,31))]:
                ims=[comp(f,size,bg) for f in frames]; sheet=Image.new('RGBA',(ims[0].width*6,size),bg+(255,))
                for i,im in enumerate(ims): sheet.alpha_composite(im,(i*im.width,0))
                p=OUT/'evidence'/f'{skill}-body-{size}-{name}.png'; sheet.save(p); rows.append({'skill':skill,'kind':'bodyContact','scale':size,'background':name,'path':str(p)})
            ims=[comp(f,size,(128,128,128),True) for f in frames]; sheet=Image.new('RGBA',(ims[0].width*6,size),(128,128,128,255))
            for i,im in enumerate(ims): sheet.alpha_composite(im,(i*im.width,0))
            p=OUT/'evidence'/f'{skill}-body-{size}-gray.png'; sheet.save(p); rows.append({'skill':skill,'kind':'bodyContact','scale':size,'background':'gray','path':str(p)})
    mp=OUT/'body-manifest.json'; mp.write_text(json.dumps({'task':'M3-ART-V1','status':'BODY18_PACKAGED_CANDIDATE','sourceManifest':str(SRC/'manifest.json'),'sourceManifestSha256':sha(SRC/'manifest.json'),'assets':rows,'install':False,'unity':False},ensure_ascii=False,indent=2)+'\n'); print(mp); print(sha(mp))
if __name__=='__main__': main()
