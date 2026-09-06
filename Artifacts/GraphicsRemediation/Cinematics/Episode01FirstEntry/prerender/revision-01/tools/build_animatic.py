from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageEnhance, ImageChops
import math, random, json, hashlib

ROOT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/Cinematics/Episode01FirstEntry/prerender/revision-01')
KEY=ROOT/'keyframes'; FR=ROOT/'frames-960x540-30fps'; FR.mkdir(parents=True,exist_ok=True)
W,H,FPS,N=960,540,30,240

def font(size):
    for p in ['/System/Library/Fonts/AppleSDGothicNeo.ttc','/System/Library/Fonts/Supplemental/Arial Unicode.ttf']:
        if Path(p).exists(): return ImageFont.truetype(p,size)
    return ImageFont.load_default()

def fit(path):
    im=Image.open(path).convert('RGB')
    scale=max(W/im.width,H/im.height); nw,nh=round(im.width*scale),round(im.height*scale)
    im=im.resize((nw,nh),Image.Resampling.LANCZOS)
    return im.crop(((nw-W)//2,(nh-H)//2,(nw-W)//2+W,(nh-H)//2+H)).convert('RGBA')

bases=[fit(KEY/'beat1-ambience.png'),fit(KEY/'beat2-seojin-entry.png'),fit(KEY/'beat3-enemy-reveal.png'),fit(KEY/'beat4-finisher.png')]
rng=random.Random(917031)
particles=[(rng.uniform(0,W),rng.uniform(H*.42,H*.95),rng.uniform(.15,.55),rng.uniform(1.5,4.0),rng.uniform(20,65)) for _ in range(58)]

def dust_overlay(frame_idx,strength):
    ov=Image.new('RGBA',(W,H),(0,0,0,0)); d=ImageDraw.Draw(ov,'RGBA')
    for x,y,s,r,a in particles:
        xx=(x+frame_idx*s)%W; yy=y+math.sin(frame_idx*.035+x*.01)*2.2
        d.ellipse((xx-r,yy-r*.5,xx+r,yy+r*.5),fill=(91,77,61,int(a*strength)))
    return ov.filter(ImageFilter.GaussianBlur(2.5))

def title_frame(base,t):
    im=ImageEnhance.Brightness(base.convert('RGB')).enhance(.48).convert('RGBA')
    d=ImageDraw.Draw(im,'RGBA'); d.rectangle((0,0,W,H),fill=(4,7,10,75))
    if t<.35: alpha=int(255*t/.35)
    elif t<.70: alpha=255
    elif t<.92: alpha=int(255*(.92-t)/.22)
    else: alpha=0
    if alpha:
        d.polygon([(115,218),(824,194),(864,296),(92,319)],fill=(3,5,8,int(alpha*.84)))
        text='청운촌의 습격'; f=font(66); box=d.textbbox((0,0),text,font=f); x=(W-(box[2]-box[0]))//2
        d.text((x,226),text,font=f,fill=(236,226,204,alpha),stroke_width=1,stroke_fill=(28,19,17,alpha))
    return im

for i in range(N):
    t=i/FPS
    if t<1.5: beat=0; local=t/1.5; im=bases[0].copy(); strength=.45+.25*math.sin(math.pi*local)
    elif t<3.5: beat=1; local=(t-1.5)/2; im=bases[1].copy(); strength=.30+.18*math.sin(math.pi*local)
    elif t<5.5: beat=2; local=(t-3.5)/2; im=bases[2].copy(); strength=.38+.22*math.sin(math.pi*local)
    elif t<7.0: beat=3; local=(t-5.5)/1.5; im=bases[3].copy(); strength=.25+.28*math.sin(math.pi*local)
    else: beat=4; local=t-7.0; im=title_frame(bases[0],local); strength=.08
    im.alpha_composite(dust_overlay(i,strength))
    if beat==3 and abs(t-6.28)<.075:
        phase=(t-6.205)/.15; dx=round(3*math.sin(phase*5*math.pi)*(1-phase)); dy=round(dx*.25)
        im=ImageChops.offset(im,dx,dy)
    if i==N-1: im=Image.new('RGBA',(W,H),(0,0,0,255))
    im.convert('RGB').save(FR/f'frame-{i:04d}.png',compress_level=2)

# review GIF: 10fps derived without interpolation, exact 8s
review=[]
for i in range(0,N,3): review.append(Image.open(FR/f'frame-{i:04d}.png').resize((480,270),Image.Resampling.LANCZOS).convert('P',palette=Image.Palette.ADAPTIVE,colors=256))
gif=ROOT/'episode01-entry-8s-animatic-review.gif'; review[0].save(gif,save_all=True,append_images=review[1:],duration=100,loop=0,disposal=2,optimize=False)

times=[0,1.5,3.5,5.5,6.3,7,7.5,7.99]; thumbs=[]
for t in times:
    idx=min(N-1,int(t*FPS)); im=Image.open(FR/f'frame-{idx:04d}.png'); im.thumbnail((320,180)); c=Image.new('RGB',(336,218),(12,14,18));c.paste(im,(8,8));ImageDraw.Draw(c).text((10,190),f'{t:.2f}s',font=font(17),fill=(236,220,184));thumbs.append(c)
contact=Image.new('RGB',(336*4,218*2),(7,9,12))
for i,c in enumerate(thumbs):contact.paste(c,((i%4)*336,(i//4)*218))
contact.save(ROOT/'contact-8times.png')

data={'status':'LOW_RES_ANIMATIC_REVIEW_ONLY','duration_seconds':8.0,'fps':30,'physical_frames':240,'resolution':[960,540],'cuts_seconds':[1.5,3.5,5.5,7.0],'impact_seconds':6.28,'audio':'none','keyframes':[]}
for p in sorted(KEY.glob('beat[1-4]-*.png')):data['keyframes'].append({'path':str(p.resolve()),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
(ROOT/'animatic-build.json').write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n')
