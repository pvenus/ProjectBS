"""Static raster audit from actual runtime-harness transforms; no Unity/GPU execution."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,re
R=Path(__file__).resolve().parents[2];O=R/'Artifacts/Engineering/MorpgNativeTiling';O.mkdir(exist_ok=True)
d=json.loads((R/'Assets/Resources/battle/morpg/wave-environment-v1/binding.json').read_text());e=json.loads((R/'Assets/Resources/battle/morpg/environment/environment.v1.json').read_text());frames=json.loads((O/'runtime-frames.json').read_text());L=d['layout'];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for n,h in json.loads((O/'same-path-before.json').read_text()).items():assert sha(R/n)==h,n
images={};propassets={a['id']:a for a in e['assets']};specs={a['id']:a for w in d['waves'] for a in w['assets']};props={a['id']:a for z in e['zones'] for a in z['props']+z['decorations']}
for a in list(specs.values())+list(propassets.values()):
 p=R/('Assets/Resources/'+a['resource']+'.png');images[a['id']]=Image.open(p).convert('RGBA');assert sha(p)==a['sha256']
patches=0
for a in specs.values():
 if a['kind']=='background':continue
 l,b,r,t=a['rect'];assert abs(r-l-15.36)<1e-6 and abs(t-b-5.12)<1e-6
 for q in a['colliderRects']:
  x0,y0,x1,y1=q['rect'];crop=tuple(map(round,(x0*100,(t-y1)*100,x1*100,(t-y0)*100)))
  assert images[a['id']].getchannel('A').crop(crop).getextrema()[0]>=192;patches+=1
W,H=960,540;hh=L['cameraHalfHeight'];hw=hh*16/9;scale=H/(hh*2);rows=[];previews=[]
def drawframe(frame,overlay=False):
 i=frame['wave']-1;origin=i*(L['zoneWidth']+L['zoneGap']);cx,cy=frame['camera'];vl,vb,vr,vt=cx-hw,cy-hh,cx+hw,cy+hh
 out=Image.new('RGBA',(W,H));bottom=Image.new('RGBA',(W,H));top=Image.new('RGBA',(W,H));items=[]
 for g in frame['rendered']:
  name=g['name'];x,y=g['position'];sx,sy=g['scale']
  if name.startswith('wave'):
   a=specs[name.split('.tile.')[0]];raw=images[a['id']];crop=g['crop'];xx,yy,ww,h=crop
   raw=raw.crop((round(xx),round(512-yy-h),round(xx+ww),round(512-yy)))
   width,height=ww/100*sx,h/100*sy;rect=[x-width/2,y-height/2,x+width/2,y+height/2];order=a['sortingOrder']
   if a['kind']!='background':assert rect[0]>=origin-1e-4 and rect[2]<=origin+L['zoneWidth']+1e-4 and sx==sy==1
  else:
   a=props[name];raw=images[a['asset']];meta=(R/('Assets/Resources/'+propassets[a['asset']]['resource']+'.png.meta')).read_text();px,py=map(float,re.search(r'spritePivot: \{x: ([^,]+), y: ([^}]+)',meta).groups())
   if a.get('flipX'):raw=raw.transpose(Image.Transpose.FLIP_LEFT_RIGHT);px=1-px
   rw,rh=raw.size;ox=(.5-px)*rw/100*sx;oy=(.5-py)*rh/100*sy;rot=a.get('visualRotation',0)
   if rot==90:raw=raw.transpose(Image.Transpose.ROTATE_90);ox,oy=-oy,ox;width,height=rh/100*sy,rw/100*sx
   elif rot==-90:raw=raw.transpose(Image.Transpose.ROTATE_270);ox,oy=oy,-ox;width,height=rh/100*sy,rw/100*sx
   else:width,height=rw/100*sx,rh/100*sy
   rect=[x+ox-width/2,y+oy-height/2,x+ox+width/2,y+oy+height/2];order=-900
  items.append((order,raw,rect,name))
 for order,raw,(l,b,r,t),name in sorted(items,key=lambda a:a[0]):
  im=raw.resize((max(1,round((r-l)*scale)),max(1,round((t-b)*scale))),Image.Resampling.BILINEAR);pos=(round((l-vl)*scale),round((vt-t)*scale));out.alpha_composite(im,pos)
  if '.bottom.tile.' in name:bottom.alpha_composite(im,pos)
  if '.top.tile.' in name:top.alpha_composite(im,pos)
 assert out.getchannel('A').getextrema()==(255,255),'uncovered pixels'
 # Every viewport column has visible opaque bottom barrier in the lower 0.65 world-unit band.
 band=bottom.getchannel('A').crop((0,H-round(L['bottomInset']*scale),W,H));missing=sum(max(band.crop((x,0,x+1,band.height)).getdata())<192 for x in range(W));assert missing==0,('bottom missing columns',frame['wave'],frame['view'],missing)
 if overlay:
  draw=ImageDraw.Draw(out)
  for g in frame['rendered']:
   if '.tile.' not in g['name']:continue
   a=specs[g['name'].split('.tile.')[0]];x,y=g['position'];crop=g['crop'];start=x-crop[2]/200-crop[0]/100
   left=max(origin,start);right=min(origin+L['zoneWidth'],start+15.36)
   draw.line((round((left-vl)*scale),0,round((left-vl)*scale),H),fill=(240,170,20,220),width=2)
   for q in a['colliderRects']:
    l,b,r,t=q['rect'];l=max(left,l+start);r=min(right,r+start)
    if r>l:draw.rectangle((round((l-vl)*scale),round((vt-t)*scale),round((r-vl)*scale),round((vt-b)*scale)),outline=(250,70,80,130))
 visible={}
 for label,layer in [('top',top),('bottom',bottom)]:
  solid=layer.getchannel('A').point(lambda v:255 if v>=192 else 0);count=solid.histogram()[255];bbox=solid.getbbox();assert count>W*5,(label,'not visibly present',count)
  visible[label]=dict(opaqueVisiblePixels=count,opaqueVisibleBBox=list(bbox))
 visible['bottomMissingColumns']=missing
 return out,visible
for f in frames:
 out,visible=drawframe(f);path=O/f"wave{f['wave']}-{f['view']}.png";out.convert('RGB').save(path)
 if f['view']!='center-return':previews.append((f,out));rows.append(dict(wave=f['wave'],view=f['view'],uncoveredPixels=0,visibility=visible,camera=f['camera']))
 if f['view']=='center':drawframe(f,True)[0].convert('RGB').save(O/f"wave{f['wave']}-seams-collider-overlay.png")
contact=Image.new('RGB',(1440,882),(238,232,215));draw=ImageDraw.Draw(contact)
for f,im in previews:
 col=f['wave']-1;row=['center','left','right'].index(f['view']);contact.paste(im.convert('RGB').resize((480,270)),(col*480,row*294+24));draw.text((col*480+8,row*294+6),f"W{f['wave']} / {f['view']} / native tiles",fill=(25,25,25))
contact.save(O/'viewport-contact.png')
# Show actual destination/source fixed views separated by a fully hidden cut, never a camera pan across the gap.
strip=Image.new('RGB',(1440,300),(25,25,25));draw=ImageDraw.Draw(strip)
for col,i in [(0,0),(2,1)]:
 f=next(f for f in frames if f['wave']==i+1 and f['view']==('right' if i==0 else 'left'));strip.paste(drawframe(f)[0].convert('RGB').resize((480,270)),(col*480,30))
draw.text((8,8),'Source camera fixed',fill='white');draw.text((520,110),'INVISIBLE CUT / GAP '+str(L['zoneGap']),fill='white');draw.text((970,8),'Destination camera fixed + origin reset',fill='white');strip.save(O/'transition-gap.png')
# A feature marker shows world-follow coefficient directly against a static center image.
par=Image.new('RGB',(1440,310),(238,232,215));draw=ImageDraw.Draw(par)
for col,view in enumerate(['left','center','right']):
 f=next(f for f in frames if f['wave']==1 and f['view']==view);im=drawframe(f)[0].convert('RGB').resize((480,270));par.paste(im,(col*480,40));delta=f['camera'][0]-L['zoneWidth']/2;draw.text((col*480+8,8),f'camera delta {delta:+.3f} / background delta {delta*L["parallaxFactor"]:+.3f}',fill=(25,25,25))
par.save(O/'parallax-before-after.png')
report=dict(method='Static exact source compositing from runtime-stub transforms; no Unity/GPU execution',views=rows,worldFollowCoefficient=L['parallaxFactor'],gap=L['zoneGap'],origins=[i*(L['zoneWidth']+L['zoneGap']) for i in range(3)],nativeTileCount=18,opaqueSupportedTemplatePatches=patches,protectedSamePathDiff=0,uncoveredPixels=0,bottomMissingColumns=0)
(O/'audit.json').write_text(json.dumps(report,indent=2)+'\n');(O/'preview-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
