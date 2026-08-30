from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json

ROOT=Path(__file__).resolve().parent
PROJECT=ROOT.parents[4]
SOURCES={
 "Yujin": Path('/private/tmp/projectbs-yujin13-final-art-feed.json'),
 "Jihan": Path('/private/tmp/projectbs-jihan12-promotion-records.json'),
 "Seojin": Path('/private/tmp/projectbs-seojin13-promotion-records.json'),
}
font=ImageFont.load_default(size=16); titlefont=ImageFont.load_default(size=24)
manifest={}
for char,p in SOURCES.items():
 data=json.loads(p.read_text())
 if char=='Yujin':
  entries=[(e['assetId'],e.get('canonicalPngPath') or e.get('canonicalPng') or e.get('png')) for e in data['entries']]
 else:
  entries=[(e['canonicalAssetId'],e['png']) for e in data]
 entries.sort()
 cols=4; cellw=360; cellh=405; rows=(len(entries)+cols-1)//cols
 sheet=Image.new('RGB',(cols*cellw,55+rows*cellh),(211,208,198)); d=ImageDraw.Draw(sheet)
 d.text((20,15),f'{char} promoted canonical — exact{len(entries)}',fill=(24,27,30),font=titlefont)
 out_entries=[]
 for i,(aid,rel) in enumerate(entries):
  src=PROJECT/rel; im=Image.open(src).convert('RGBA')
  icon=im.resize((260,260),Image.Resampling.LANCZOS)
  x=(i%cols)*cellw+50; y=55+(i//cols)*cellh+10
  tile=Image.new('RGBA',(260,260),(229,227,220,255)); tile.alpha_composite(icon)
  sheet.paste(tile.convert('RGB'),(x,y))
  # 80 and 32 light/dark authority samples
  for size,sx in ((80,x),(32,x+190)):
   tiny=im.resize((size,size),Image.Resampling.LANCZOS)
   for bg,bx in (((229,227,220),sx),((18,19,21),sx+size+6)):
    t=Image.new('RGBA',(size,size),bg+(255,)); t.alpha_composite(tiny); sheet.paste(t.convert('RGB'),(bx,y+268))
  label=aid.replace('skill.character.','')
  d.text((x,y+355),label[:43],fill=(34,37,40),font=font)
  d.text((x,y+374),label[43:86],fill=(34,37,40),font=font)
  out_entries.append({'assetId':aid,'canonicalPng':str(src),'sha256':hashlib.sha256(src.read_bytes()).hexdigest()})
 out=ROOT/f'main3-{char.lower()}-exact{len(entries)}-contact.png'; sheet.save(out,compress_level=9)
 manifest[char]={'count':len(entries),'contact':str(out),'contactSha256':hashlib.sha256(out.read_bytes()).hexdigest(),'entries':out_entries}
(ROOT/'main3-exact38-contact-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
