from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageEnhance
import hashlib, json

ROOT = Path('/Users/pvenus/ProjectBS')
OUT = ROOT/'Artifacts/GraphicsRemediation/Cinematics/Episode01FirstEntry/revision-01'
BEATS = OUT/'beats'
BEATS.mkdir(parents=True, exist_ok=True)

assets = {
    'village': ROOT/'Assets/ImagesGenerated/Stage/popup_main/node.act1.chapter01.episode01.village_arrival.main.png',
    'raid': ROOT/'Assets/ImagesGenerated/Stage/popup_main/node.act1.chapter01.episode01.black_cloth_raid.main.png',
    'charge': ROOT/'Assets/ImagesGenerated/Stage/popup_main/node.act1.chapter01.episode01.rescue_charge.main.png',
    'vfx': ROOT/'Assets/ImagesGenerated/Skill/animation/skill.character.seojin.2.active_1.charge/frame-3.png',
}

W,H=960,540
durations=[1400,1300,1400,2200,1700]
titles=['1 AMBIENCE  0.0–1.4','2 SEOJIN ENTRY  1.4–2.7','3 ENEMY MASS  2.7–4.1','4 FINISHER  4.1–6.3','5 INK TITLE / GAMEPLAY  6.3–8.0']

def font(size):
    choices=['/System/Library/Fonts/AppleSDGothicNeo.ttc','/System/Library/Fonts/Supplemental/Arial Unicode.ttf']
    for p in choices:
        if Path(p).exists(): return ImageFont.truetype(p,size)
    return ImageFont.load_default()

def crop(path,y):
    im=Image.open(path).convert('RGB')
    return im.crop((0,y,W,y+H)).convert('RGBA')

def grade(im,brightness=1.0,contrast=1.0):
    x=ImageEnhance.Brightness(im.convert('RGB')).enhance(brightness)
    x=ImageEnhance.Contrast(x).enhance(contrast)
    return x.convert('RGBA')

def bar(im,label):
    d=ImageDraw.Draw(im,'RGBA'); d.rectangle((0,0,W,42),fill=(8,10,14,205)); d.text((16,9),label,font=font(21),fill=(238,224,190,255))
    return im

frames=[]
frames.append(bar(grade(crop(assets['village'],390),.86,1.08),titles[0]))
frames.append(bar(grade(crop(assets['raid'],410),.92,1.10),titles[1]))
frames.append(bar(grade(crop(assets['raid'],135),.77,1.16),titles[2]))

fin=grade(crop(assets['charge'],330),.92,1.16)
vfx=Image.open(assets['vfx']).convert('RGBA').resize((285,285),Image.Resampling.NEAREST)
fin.alpha_composite(vfx,(170,150))
frames.append(bar(fin,titles[3]))

last=grade(crop(assets['charge'],330),.42,1.25)
d=ImageDraw.Draw(last,'RGBA')
d.rectangle((0,0,W,H),fill=(4,7,12,75))
d.polygon([(70,220),(800,185),(880,275),(110,310)],fill=(5,8,12,205))
d.text((110,205),'청운촌의 습격',font=font(62),fill=(240,231,210,255))
d.text((114,282),'전투 개시',font=font(28),fill=(172,91,60,255))
frames.append(bar(last,titles[4]))

rows=[]
for i,(im,ms) in enumerate(zip(frames,durations),1):
    p=BEATS/f'beat-{i}.png'; im.convert('RGB').save(p,optimize=True)
    rows.append({'beat':i,'path':str(p.resolve()),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'duration_ms':ms,'dimensions':[W,H]})

gif=OUT/'episode01-first-entry-storyboard-8s.gif'
frames[0].convert('P',palette=Image.Palette.ADAPTIVE,colors=256).save(gif,save_all=True,append_images=[x.convert('P',palette=Image.Palette.ADAPTIVE,colors=256) for x in frames[1:]],duration=durations,loop=0,disposal=2,optimize=False)

thumbs=[]
for i,im in enumerate(frames):
    t=im.copy(); t.thumbnail((320,180)); c=Image.new('RGBA',(336,222),(14,16,20,255)); c.alpha_composite(t,(8,8)); ImageDraw.Draw(c).text((10,192),f'{i+1}. {durations[i]/1000:.1f}s',font=font(18),fill=(235,220,185,255)); thumbs.append(c)
contact=Image.new('RGBA',(336*5,222),(8,10,13,255))
for i,t in enumerate(thumbs):contact.alpha_composite(t,(336*i,0))
cp=OUT/'storyboard-contact.png'; contact.convert('RGB').save(cp,optimize=True)

manifest={'status':'PREPRODUCTION_REVIEW_ONLY','duration_ms':sum(durations),'camera':'fixed compositions; instant cuts only','zoom_pan_follow_slow':False,'source_assets':{k:{'path':str(v),'sha256':hashlib.sha256(v.read_bytes()).hexdigest()} for k,v in assets.items()},'beats':rows,'gif':{'path':str(gif.resolve()),'sha256':hashlib.sha256(gif.read_bytes()).hexdigest(),'physical_frames':5,'delays_ms':durations,'loop':'infinite'},'contact':{'path':str(cp.resolve()),'sha256':hashlib.sha256(cp.read_bytes()).hexdigest()}}
(OUT/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(manifest,ensure_ascii=False,indent=2))
