from pathlib import Path
import hashlib,json,re,struct,base64,math
root=Path(__file__).resolve().parents[2];out=root/'Artifacts/Engineering/SeojinManualControl/dual-hud-09';src=root/'Artifacts/GraphicsRemediation/HUD/SeojinDualDirectionCrescent/selected/revision-01/manifest.json';manifest=json.loads(src.read_text());assert hashlib.sha256(src.read_bytes()).hexdigest()=='99737281257a6c67b77eb4ac418ff97689d1d2b2850db11096e282d34703a5dd'
dest=root/'Assets/Resources/battle/Presentation/SeojinDualDirectionCrescent/revision-01';assert len(list(dest.glob('*.png')))==len(manifest['records'])==2
rows=[]
for rec in manifest['records']:
 p=dest/Path(rec['path']).name;b=p.read_bytes();assert hashlib.sha256(b).hexdigest()==rec['sha256']==hashlib.sha256(Path(rec['path']).read_bytes()).hexdigest();w,h,depth,mode=struct.unpack('>IIBB',b[16:26]);assert (w,h,depth,mode)==(512,512,8,6)
 meta=Path(str(p)+'.meta').read_text();assert all(x in meta for x in ['spriteMode: 1','spritePixelsToUnits: 100','spritePivot: {x: 0.5, y: 0.5}','textureType: 8','alphaIsTransparency: 1','textureCompression: 0']);guid=re.search(r'guid: (\w+)',meta).group(1);rows.append({'path':str(p.relative_to(root)),'sha256':rec['sha256'],'guid':guid,'RGBA':[w,h,depth],'PPU':100,'pivot':[.5,.5],'Sprite':'Single'})
for row in rows:
 matches=[p for p in (root/'Assets').rglob('*.meta') if ('guid: '+row['guid']) in p.read_text(errors='replace')];assert len(matches)==1
for n,h in json.loads((out/'aura-before-sha.json').read_text()).items():assert hashlib.sha256((root/n).read_bytes()).hexdigest()==h,n
code=(root/'Assets/Scripts/Actor/Character/Control/SeojinDualDirectionHud.cs').read_text();late=code[code.index('private void LateUpdate()'):code.index('internal static void UpdateArc')];assert 'new GameObject' not in late and 'Resources.Load' not in late and 'GetComponents' not in late
(out/'asset-audit.json').write_text(json.dumps({'manifest_verified':True,'exact2':rows,'existing_aura_unchanged':True,'Unity_import':False,'per_frame_managed_collection_or_object_creation':False},indent=2)+'\n')
def uri(p):return 'data:image/png;base64,'+base64.b64encode(p.read_bytes()).decode()
red=uri(dest/'attack-mouse-red.png');blue=uri(dest/'movement-keyboard-blue.png');back=uri(root/'Assets/ImagesGenerated/Battle/character/player-character-aura-loop-f/back/frame_01.png');front=uri(root/'Assets/ImagesGenerated/Battle/character/player-character-aura-loop-f/front/frame_01.png')
aw,ah=struct.unpack('>II',(root/'Assets/ImagesGenerated/Battle/character/player-character-aura-loop-f/back/frame_01.png').read_bytes()[16:24]);width=aw/100*.2058*400;height=ah/100*.15435*400;ratio=height/width
svg=['<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="640" viewBox="0 0 1200 640"><rect width="1200" height="640" fill="#151d28"/><text x="40" y="45" fill="#e7ecf2" font-size="24" font-family="sans-serif">Seojin • dual direction HUD / static composition</text><text x="40" y="75" fill="#abb8c8" font-size="15" font-family="sans-serif">Approved source pixels unchanged · red outer lane / blue inner lane · Unity not run</text>']
for index,(a,b,label) in enumerate([(0,0,'Same direction'),(180,0,'Opposite directions'),(45,-45,'Independent diagonals'),(90,180,'Mouse up / WASD left')]):
 x=300+(index%2)*600;y=215+(index//2)*275;svg.append(f'<g transform="translate({x} {y})"><text x="0" y="-85" text-anchor="middle" fill="#e7ecf2" font-size="18" font-family="sans-serif">{label}</text>')
 for img in [back,front]:svg.append(f'<image href="{img}" x="{-width/2}" y="{-height/2}" width="{width}" height="{height}"/>')
 svg.append(f'<g transform="scale(1 {ratio})">')
 for img,angle,size in [(red,math.degrees(math.atan2(math.sin(math.radians(a))/ratio,math.cos(math.radians(a)))),width*1.2*1.15),(blue,math.degrees(math.atan2(math.sin(math.radians(b))/ratio,math.cos(math.radians(b)))),width*1.2)]:svg.append(f'<g transform="rotate({-angle})"><image href="{img}" x="{-size/2}" y="{-size/2}" width="{size}" height="{size}"/></g>')
 svg.append('</g><path d="M -4 0 H 4 M 0 -4 V 4" stroke="#e7ecf2" opacity=".5"/></g>')
svg.append('</svg>');(out/'static-preview.svg').write_text(''.join(svg));print('PASS exact2 source/hash/RGBA512/metadata/GUID uniqueness, existing aura unchanged, allocation source audit; static preview generated')
