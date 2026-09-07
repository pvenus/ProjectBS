"""Static raster audit from actual runtime-harness transforms; no Unity/GPU execution."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,re
R=Path(__file__).resolve().parents[2];O=R/'Artifacts/Engineering/MorpgDepthTiling';O.mkdir(exist_ok=True)
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
   if a['kind']!='background':assert rect[0]>origin-L['zoneGap'] and rect[2]<origin+L['zoneWidth']+L['zoneGap'] and sx==sy==1 and ww==1536
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
   fixedstart=g['physicsBase'][0]-15.36/2;left=max(origin,fixedstart);right=min(origin+L['zoneWidth'],fixedstart+15.36)
   draw.line((round((start-vl)*scale),0,round((start-vl)*scale),H),fill=(240,170,20,220),width=2)
   for q in a['colliderRects']:
    l,b,r,t=q['rect'];l=max(left,l+fixedstart);r=min(right,r+fixedstart)
    if r>l:draw.rectangle((round((l-vl)*scale),round((vt-t)*scale),round((r-vl)*scale),round((vt-b)*scale)),outline=(250,70,80,130))
 visible={}
 for label,layer in [('top',top),('bottom',bottom)]:
  solid=layer.getchannel('A').point(lambda v:255 if v>=192 else 0);count=solid.histogram()[255];bbox=solid.getbbox();assert count>W*5,(label,'not visibly present',count)
  visible[label]=dict(opaqueVisiblePixels=count,opaqueVisibleBBox=list(bbox))
 visible['bottomMissingColumns']=missing
 return out,visible
def annotated(frame,raw):
 canvas=Image.new('RGB',(W,H+90),(239,233,218));canvas.paste(raw.convert('RGB'),(0,90));draw=ImageDraw.Draw(canvas)
 i=frame['wave']-1;origin=i*(L['zoneWidth']+L['zoneGap']);dx=frame['camera'][0]-origin-L['zoneWidth']/2;vl=frame['camera'][0]-hw
 draw.text((10,6),f"W{i+1} {frame['view']} / T0,T1 + B0,B1,B2,B3 / native scale 1 / uncovered 0",fill=(20,20,20))
 for row,(name,factor,color) in enumerate([('BG',L['parallaxFactor'],(80,90,180)),('TOP',L['topParallaxFactor'],(20,110,60)),('BOTTOM',L['bottomParallaxFactor'],(190,65,20))]):
  y=28+row*19;delta=dx*factor;draw.text((10,y-5),f'{name} world {factor:+.2f} / delta {delta:+.3f}',fill=color);x=640;end=x+round(delta*scale);draw.line((x,y,end,y),fill=color,width=3)
  if delta:sign=1 if delta>0 else -1;draw.polygon([(end,y),(end-sign*8,y-4),(end-sign*8,y+4)],fill=color)
 for g in frame['rendered']:
  if '.tile.' not in g['name']:continue
  x=g['position'][0];left=x-7.68;right=x+7.68
  for edge in [left,right]:
   px=round((edge-vl)*scale)
   if 0<=px<W:draw.line((px,90,px,H+90),fill=(212,153,35),width=1)
  if right>vl and left<vl+2*hw:
   px=max(2,min(W-150,round((x-vl)*scale)-65));y=96 if '.top.' in g['name'] else H+68
   draw.rectangle((px-2,y-2,px+147,y+12),fill=(240,233,215));draw.text((px,y),g['name'],fill=(30,30,30))
 return canvas
for f in frames:
 out,visible=drawframe(f);path=O/f"wave{f['wave']}-{f['view']}.png";out.convert('RGB').save(path)
 annotated(f,out).save(O/f"wave{f['wave']}-{f['view']}-annotated.png")
 if f['view'] in ['center','left','right']:previews.append((f,out))
 if f['view']!='center-return':rows.append(dict(wave=f['wave'],view=f['view'],uncoveredPixels=0,visibility=visible,camera=f['camera'],instances=[g for g in f['rendered'] if '.tile.' in g['name']]))
 if f['view']=='center':drawframe(f,True)[0].convert('RGB').save(O/f"wave{f['wave']}-seams-collider-overlay.png")
contact=Image.new('RGB',(1440,882),(238,232,215));draw=ImageDraw.Draw(contact)
for f,im in previews:
 col=f['wave']-1;row=['center','left','right'].index(f['view']);contact.paste(im.convert('RGB').resize((480,270)),(col*480,row*294+24));draw.text((col*480+8,row*294+6),f"W{f['wave']} / {f['view']} / native tiles",fill=(25,25,25))
contact.save(O/'viewport-contact.png')
annotated_contact=Image.new('RGB',(1440,990),(239,233,218))
for f,im in previews:
 col=f['wave']-1;row=['center','left','right'].index(f['view']);annotated_contact.paste(annotated(f,im).resize((480,315)),(col*480,row*330))
annotated_contact.save(O/'depth-annotated-contact.png')
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
report=dict(method='Static exact source compositing from runtime-stub transforms; no Unity/GPU execution',views=rows,worldFollowCoefficients=dict(background=L['parallaxFactor'],top=L['topParallaxFactor'],bottom=L['bottomParallaxFactor']),gap=L['zoneGap'],origins=[i*(L['zoneWidth']+L['zoneGap']) for i in range(3)],nativeTileCount=18,topCount=6,bottomCount=12,maxVisualColliderSeparation=dict(top=(L['zoneWidth']/2-hw)*L['topParallaxFactor'],bottom=abs((L['zoneWidth']/2-hw)*L['bottomParallaxFactor'])),movingVisualBand=dict(topMinY=4+L['collisionBandInset'],bottomMaxY=-4-L['collisionBandInset']),opaqueSupportedTemplatePatches=patches,protectedSamePathDiff=0,uncoveredPixels=0,bottomMissingColumns=0)
(O/'audit.json').write_text(json.dumps(report,indent=2)+'\n');(O/'preview-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
