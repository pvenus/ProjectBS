from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json

ROOT=Path(__file__).parent
SRC=ROOT.parent/'selected'/'captain2-selected-B.png'

# Locally tightened, human-authored native silhouette. Body and sword are
# authored as separate overlapping contours so the former matte wedge is BG.
BODY=[(501,51),(526,57),(548,75),(557,108),(574,132),(588,178),(621,221),
 (676,252),(718,291),(752,349),(779,427),(797,520),(808,612),(797,684),
 (760,704),(735,756),(724,811),(731,895),(725,977),(697,1031),(719,1093),
 (738,1168),(742,1248),(734,1344),(713,1414),(671,1429),(644,1404),
 (633,1340),(629,1264),(604,1197),(575,1099),(545,1064),(521,1092),
 (498,1163),(484,1241),(469,1320),(449,1376),(413,1395),(374,1387),
 (352,1352),(355,1300),(371,1235),(391,1170),(404,1112),(377,1060),
 (349,1002),(337,935),(334,876),(319,843),(294,831),(280,805),(274,758),
 (272,701),(285,649),(294,590),(306,523),(319,450),(331,376),(350,319),
 (382,273),(421,245),(446,210),(445,166),(459,125),(477,95),(482,67)]

SWORD=[(296,813),(307,819),(304,846),(287,900),(264,958),(240,1020),
 (216,1085),(192,1151),(168,1215),(145,1271),(124,1313),(105,1320),
 (112,1298),(133,1248),(157,1188),(180,1124),(204,1059),(228,993),
 (251,930),(274,870),(286,830)]

LEG_GAP=[(454,1062),(520,1064),(549,1135),(562,1212),(551,1289),(524,1362),
 (492,1325),(479,1248),(466,1167)]

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
im=Image.open(SRC).convert('RGB'); w,h=im.size
t=Image.new('L',(w,h),0); d=ImageDraw.Draw(t)
for p in (BODY,SWORD): d.polygon(p,fill=255)
d.polygon(LEG_GAP,fill=0)
for p in (BODY,SWORD): d.line(p+[p[0]],fill=128,width=5,joint='curve')
d.line(LEG_GAP+[LEG_GAP[0]],fill=128,width=5,joint='curve')
t.save(ROOT/'manual-trimap.png')

def path(p): return 'M '+' L '.join(f'{x},{y}' for x,y in p)+' Z'
(ROOT/'manual-contour.svg').write_text(
 f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">\n'
 f'<path d="{path(BODY)}"/><path d="{path(SWORD)}"/><path d="{path(LEG_GAP)}"/>\n</svg>\n',encoding='utf-8')

src=im.load(); m=t.load(); out=Image.new('RGBA',(w,h),(0,0,0,0)); op=out.load()
for y in range(h):
 for x in range(w):
  a=m[x,y]
  if a==0: continue
  if a==255: op[x,y]=(*src[x,y],255); continue
  samples=[]
  for r in (1,2,3):
   for yy in range(max(0,y-r),min(h,y+r+1)):
    for xx in range(max(0,x-r),min(w,x+r+1)):
     if m[xx,yy]==255: samples.append(src[xx,yy])
   if samples: break
  rgb=tuple(sum(v[i] for v in samples)//len(samples) for i in range(3)) if samples else src[x,y]
  op[x,y]=(*rgb,128)
out.save(ROOT/'final-selected-B-alpha.png'); t.save(ROOT/'alpha-mask.png')
neutral=Image.new('RGB',(w,h),(216,213,204)); dark=Image.new('RGB',(w,h),(35,38,42))
neutral.paste(out,(0,0),out); dark.paste(out,(0,0),out)
c=Image.new('RGB',(w*2,h)); c.paste(neutral,(0,0)); c.paste(dark,(w,0))
c.resize((1024,768),Image.Resampling.LANCZOS).save(ROOT/'contact-neutral-dark.png')

vals={0:0,128:0,255:0}
for v in t.getdata(): vals[v]=vals.get(v,0)+1
metrics={'source':str(SRC),'source_sha256':sha(SRC),'dimensions':[w,h],
 'trimap_values':vals,'unknown_band_nominal_px':2,'unknown_band_bounds_px':[1,3],
 'method':'versioned human-authored tight body contour + separate sword contour + explicit leg BG island; 5px centered L8 edge; local inward RGB extrapolation radius<=3',
 'correction_reason':'remove broad former-matte enclosure from coarse revision02 contour',
 'outputs':{n:sha(ROOT/n) for n in ['manual-contour.svg','manual-trimap.png','final-selected-B-alpha.png','alpha-mask.png','contact-neutral-dark.png']}}
(ROOT/'provenance-metrics.json').write_text(json.dumps(metrics,indent=2),encoding='utf-8')
