from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json

ROOT = Path(__file__).parent
SRC = ROOT.parent / "selected" / "captain2-selected-B.png"

# Human-authored clockwise native-canvas silhouette. The sword is followed out to
# its tip and back; it is not inferred from background color.
P = [(502,48),(535,57),(558,82),(567,116),(590,139),(610,205),(667,242),
     (721,291),(765,362),(796,465),(821,590),(809,708),(756,740),(731,806),
     (743,967),(711,1064),(748,1135),(761,1247),(754,1360),(726,1438),
     (659,1434),(638,1378),(629,1289),(592,1214),(565,1098),(533,1055),
     (507,1100),(486,1212),(468,1325),(438,1390),(370,1394),(337,1363),
     (351,1282),(373,1194),(398,1108),(363,1039),(337,944),(329,871),
     (302,838),(286,842),(272,873),(250,920),(224,984),(196,1054),
     (168,1122),(141,1190),(116,1251),(94,1304),(107,1313),(132,1281),
     (160,1230),(190,1168),(220,1101),(251,1030),(278,967),(301,907),
     (314,858),(306,812),(285,785),(274,723),(258,672),(280,599),
     (302,515),(321,415),(342,334),(378,278),(425,242),(446,199),
     (442,151),(463,115),(475,75)]

# Explicit background islands: between legs and the open wedge between sword
# blade and robe. These preserve the composition rather than filling the hull.
HOLES = [
    [(466,1070),(532,1066),(559,1150),(573,1238),(555,1325),(520,1378),
     (486,1325),(474,1237),(455,1150)],
    [(143,1260),(198,1120),(250,990),(291,886),(301,849),(282,846),
     (255,900),(222,977),(187,1065),(152,1150),(118,1235)]
]

def sha(p):
    return hashlib.sha256(Path(p).read_bytes()).hexdigest()

im = Image.open(SRC).convert("RGB")
w,h = im.size
trimap = Image.new("L", (w,h), 0)
d = ImageDraw.Draw(trimap)
d.polygon(P, fill=255)
for hole in HOLES: d.polygon(hole, fill=0)
# Rasterized 2px nominal unknown band (5px centered stroke), no thresholding.
d.line(P+[P[0]], fill=128, width=5, joint="curve")
for hole in HOLES: d.line(hole+[hole[0]], fill=128, width=5, joint="curve")
trimap.save(ROOT/"manual-trimap.png")

svg_path = "M " + " L ".join(f"{x},{y}" for x,y in P) + " Z"
holes = "\n".join("<path d=\"M " + " L ".join(f"{x},{y}" for x,y in q) + " Z\"/>" for q in HOLES)
(ROOT/"manual-contour.svg").write_text(
 f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">\n'
 f'<path d="{svg_path}"/>\n{holes}\n</svg>\n', encoding="utf-8")

srcpx=im.load(); m=trimap.load(); rgba=Image.new("RGBA",(w,h),(0,0,0,0)); out=rgba.load()
for y in range(h):
  for x in range(w):
    a=m[x,y]
    if a==0: continue
    if a==255: out[x,y]=(*srcpx[x,y],255); continue
    # Local inward foreground extrapolation, radius <=3 only.
    samples=[]
    for r in (1,2,3):
      for yy in range(max(0,y-r),min(h,y+r+1)):
        for xx in range(max(0,x-r),min(w,x+r+1)):
          if m[xx,yy]==255: samples.append(srcpx[xx,yy])
      if samples: break
    rgb=tuple(sum(v[i] for v in samples)//len(samples) for i in range(3)) if samples else srcpx[x,y]
    out[x,y]=(*rgb,128)
rgba.save(ROOT/"final-selected-B-alpha.png")
trimap.point(lambda v: 0 if v==0 else (128 if v==128 else 255)).save(ROOT/"alpha-mask.png")

contact=Image.new("RGB",(w*2,h),(216,213,204)); dark=Image.new("RGB",(w,h),(35,38,42))
contact.paste(rgba,(0,0),rgba); dark.paste(rgba,(0,0),rgba); contact.paste(dark,(w,0))
contact.resize((1024,768),Image.Resampling.LANCZOS).save(ROOT/"contact-neutral-dark.png")

vals={0:0,128:0,255:0}
for v in trimap.getdata(): vals[v]=vals.get(v,0)+1
metrics={"source":str(SRC),"source_sha256":sha(SRC),"dimensions":[w,h],
 "trimap_values":vals,"unknown_band_nominal_px":2,"unknown_band_bounds_px":[1,3],
 "method":"human-authored SVG contour + explicit BG islands + 5px centered L8 raster stroke + local inward RGB extrapolation radius<=3",
 "excluded_methods":["global threshold","global distance mask","ImageGen extraction","RGB repaint","geometry transform"],
 "outputs":{n:sha(ROOT/n) for n in ["manual-contour.svg","manual-trimap.png","final-selected-B-alpha.png","alpha-mask.png","contact-neutral-dark.png"]}}
(ROOT/"provenance-metrics.json").write_text(json.dumps(metrics,indent=2),encoding="utf-8")
