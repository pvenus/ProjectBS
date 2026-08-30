from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json

ROOT=Path(__file__).resolve().parent
PROJECT=ROOT.parents[5]
SETS={
 'Seojin Charge':[
 PROJECT/'Assets/ImagesGenerated/Skill/icon/skill.character.seojin.1.active_1.active_1.icon.png',
  ROOT/'selected/seojin-charge-G2-selected-v3-concrete.png', ROOT/'selected/seojin-charge-G3-selected-v3-concrete.png'],
 'Jihan Medicine Prescription':[
  PROJECT/'Assets/ImagesGenerated/Skill/icon/skill.character.jihan.1.active_1.medicine_prescription.icon.png',
  ROOT/'selected/jihan-medicine-G2-selected.png', ROOT/'selected/jihan-medicine-G3-selected.png'],
 'Yujin Multi Shot':[
 PROJECT/'Assets/ImagesGenerated/Skill/icon/skill.character.yujin.1.active_1.multi_shot.icon.png',
  ROOT/'selected/yujin-multishot-G2-selected-v2.png', ROOT/'selected/yujin-multishot-G3-selected-v2.png'],
}
font=ImageFont.load_default(size=22); small=ImageFont.load_default(size=17)
manifest={}
for title,paths in SETS.items():
 c=Image.new('RGB',(1664,800),(216,213,204)); d=ImageDraw.Draw(c)
 d.text((28,18),title+' — calibration strip',fill=(24,27,30),font=font)
 rows=[]
 for i,p in enumerate(paths):
  im=Image.open(p).convert('RGBA'); x=32+i*544
  tile=Image.new('RGB',(480,480),(216,213,204)); fit=im.copy(); fit.thumbnail((460,460),Image.Resampling.LANCZOS)
  # raw G2/G3 are RGB matte selection authorities; G1 uses alpha composite.
  if im.getextrema()[3] == (255,255): tile.paste(fit.convert('RGB'),((480-fit.width)//2,(480-fit.height)//2))
  else: tile.paste(fit,((480-fit.width)//2,(480-fit.height)//2),fit)
  c.paste(tile,(x,70)); d.text((x,560),f'G{i+1}',fill=(24,27,30),font=font)
  # 80/32 readability previews, light/dark. Raw matte is retained as calibration source.
  for size,sx in ((80,x),(32,x+210)):
   tiny=im.resize((size,size),Image.Resampling.LANCZOS)
   for bg,bx in (((216,213,204),sx),((18,19,21),sx+size+8)):
    t=Image.new('RGBA',(size,size),bg+(255,)); t.alpha_composite(tiny); c.paste(t.convert('RGB'),(bx,610))
  rows.append({'grade':i+1,'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
 d.text((32,735),'Full composition + 80px and synthetic32px light/dark. G2/G3 are style/structure selection raws; alpha qualification is not part of this gate.',fill=(55,58,62),font=small)
 slug=title.lower().replace(' ','-')
 out=ROOT/f'contact-{slug}.png'; c.save(out,compress_level=9)
 manifest[title]={'contact':str(out),'contactSha256':hashlib.sha256(out.read_bytes()).hexdigest(),'grades':rows}
(ROOT/'calibration3-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
