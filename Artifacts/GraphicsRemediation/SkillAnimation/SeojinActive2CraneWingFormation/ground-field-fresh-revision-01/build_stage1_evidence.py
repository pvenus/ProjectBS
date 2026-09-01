from pathlib import Path
from PIL import Image,ImageOps,ImageDraw
import hashlib,json,shutil

root=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinActive2CraneWingFormation/ground-field-fresh-revision-01')
src=root/'candidates/candidate-B-joint-contact.png'
sel=root/'selected';ev=root/'evidence';sel.mkdir(exist_ok=True);ev.mkdir(exist_ok=True)
shutil.copyfile(src,sel/'candidate-B-selected-joint-contact.png')
im=Image.open(src).convert('RGBA')
# The alpha-column valley is x574..600. Split at600 without crossing either connected field.
parts={'G2':im.crop((0,0,600,1024)),'G3':im.crop((600,0,1536,1024))}
members={}
for grade,p in parts.items():
    bbox=p.getbbox();q=p.crop(bbox)
    canvas=Image.new('RGBA',(1024,1024),(0,0,0,0));x=(1024-q.width)//2;y=(1024-q.height)//2;canvas.paste(q,(x,y),q)
    path=sel/f'{grade.lower()}-hold-key.png';canvas.save(path,compress_level=9)
    members[grade]={'path':str(path),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'sourceBBox':bbox,'canvasPaste':[x,y,q.width,q.height]}
contacts={}
for size in [200,80,32]:
    color=Image.new('RGB',(size*2,size*2));gray=Image.new('L',(size*2,size*2))
    for col,grade in enumerate(['G2','G3']):
        fg=Image.open(members[grade]['path']).resize((size,size),Image.Resampling.LANCZOS)
        for row,bg in enumerate([(236,231,215),(20,23,29)]):
            base=Image.new('RGBA',(size,size),bg+(255,));comp=Image.alpha_composite(base,fg).convert('RGB');color.paste(comp,(col*size,row*size));gray.paste(ImageOps.grayscale(comp),(col*size,row*size))
    cp=ev/f'joint-{size}-light-dark-color.png';gp=ev/f'joint-{size}-light-dark-gray.png';color.save(cp);gray.save(gp)
    contacts[str(size)]={'color':str(cp),'colorSHA256':hashlib.sha256(cp.read_bytes()).hexdigest(),'gray':str(gp),'graySHA256':hashlib.sha256(gp.read_bytes()).hexdigest()}
# Simulated oblique ground placement, evidence only.
ground=Image.new('RGB',(1200,700),(116,108,88));d=ImageDraw.Draw(ground)
for y in range(80,700,70): d.line((0,y,1200,y+80),fill=(129,120,98),width=2)
for x in range(-300,1500,120): d.line((x,0,x+340,700),fill=(105,98,82),width=2)
for i,grade in enumerate(['G2','G3']):
    fg=Image.open(members[grade]['path']);scale=.48 if grade=='G2' else .56;fg=fg.resize((int(1024*scale),int(1024*scale)),Image.Resampling.LANCZOS)
    ground.paste(fg,(40 if i==0 else 600,130 if i==0 else 80),fg)
gp=ev/'simulated-ground-placement.png';ground.save(gp)
manifest={'authoritySHA256':'2f7add4f8395f127a126950e4cfe9d21298cd90ce281b4f5310f39c25e60e396','selectedCandidate':'B','selectedJointPath':str(sel/'candidate-B-selected-joint-contact.png'),'selectedJointSHA256':hashlib.sha256((sel/'candidate-B-selected-joint-contact.png').read_bytes()).hexdigest(),'members':members,'contacts':contacts,'groundPlacement':str(gp),'groundPlacementSHA256':hashlib.sha256(gp.read_bytes()).hexdigest(),'stage':'hold-key exact2 only','motion':'HOLD','canonical':'HOLD'}
(root/'stage1-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(hashlib.sha256((root/'stage1-manifest.json').read_bytes()).hexdigest())
