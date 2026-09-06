#!/usr/bin/env python3
import hashlib, json, os
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np

ROOT = Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-01')
SRC = Path('/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93')

BODY = {
    'hit0': 'exec-56e2d0b1-7e13-42b1-a6d1-c600d756e3cf.png',
    'hit1': 'exec-9bca6561-cea7-4d21-9113-9605cd4fc19b.png',
    'hit2': 'exec-e198083d-cfa0-4150-9634-855a1264e92d.png',
}
VFX = {
    ('g1','hit1'): 'exec-de0335ca-98f0-444c-9b53-fa9f3028dca7.png',
    ('g1','hit2'): 'exec-d867a43f-cd78-4032-8b84-8b4384b81d6b.png',
    ('g2','hit1'): 'exec-ba178993-721d-4638-9609-538c62f8a1fa.png',
    ('g2','hit2'): 'exec-08dac8fd-d9ae-4f9f-a63a-d215100f6df8.png',
    ('g3','hit1'): 'exec-a26a0687-7639-4e58-be5b-48ac795815a8.png',
    ('g3','hit2'): 'exec-4c535677-6b14-4d5c-bb13-137765009f0c.png',
}

def sha(p):
    return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def split6(path):
    im = Image.open(path).convert('RGBA')
    w,h = im.size
    # Exact proportional cells; rounding makes the nonstandard 2038px strip exhaustive.
    xs = [round(i*w/6) for i in range(7)]
    return [im.crop((xs[i],0,xs[i+1],h)) for i in range(6)], xs

def normalize(cell, kind):
    # Strip generations occasionally let a neighbor's disconnected fragment cross
    # a cell boundary. Keep the main connected subject and discard only smaller
    # alpha components that physically touch a vertical cut edge.
    arr=np.array(cell)
    mask=arr[:,:,3]>2
    seen=np.zeros(mask.shape,bool); comps=[]
    h,w=mask.shape
    for yy,xx in zip(*np.nonzero(mask)):
        if seen[yy,xx]: continue
        stack=[(yy,xx)]; seen[yy,xx]=1; pts=[]; edge=False
        while stack:
            y,x=stack.pop(); pts.append((y,x)); edge |= x==0 or x==w-1
            for ny,nx in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
                if 0<=ny<h and 0<=nx<w and mask[ny,nx] and not seen[ny,nx]:
                    seen[ny,nx]=1; stack.append((ny,nx))
        comps.append((len(pts),edge,pts))
    largest=max((c[0] for c in comps),default=0)
    for n,edge,pts in comps:
        if edge and n<largest:
            for y,x in pts: arr[y,x]=(0,0,0,0)
    cell=Image.fromarray(arr,'RGBA')
    a = cell.getchannel('A')
    box = a.getbbox()
    if not box: raise ValueError('empty cell')
    fg = cell.crop(box)
    maxbox = (224, 232) if kind == 'body' else (232, 208)
    scale = min(maxbox[0]/fg.width, maxbox[1]/fg.height, 1.0)
    size = (max(1,round(fg.width*scale)), max(1,round(fg.height*scale)))
    fg = fg.resize(size, Image.Resampling.LANCZOS)
    out = Image.new('RGBA',(256,256),(0,0,0,0))
    x=(256-size[0])//2
    y=244-size[1] if kind=='body' else (256-size[1])//2
    out.alpha_composite(fg,(x,y))
    px=out.load()
    for yy in range(256):
        for xx in range(256):
            if px[xx,yy][3] == 0: px[xx,yy]=(0,0,0,0)
    return out

def save_frames(registry, kind, source_map):
    rows=[]
    for key,name in source_map.items():
        cells,xs=split6(SRC/name)
        parts = key if isinstance(key,tuple) else (key,)
        unit='/'.join(parts)
        d=ROOT/registry
        for p in parts: d=d/p
        d.mkdir(parents=True,exist_ok=True)
        for i,c in enumerate(cells):
            out=normalize(c,kind)
            p=d/f'frame-{i}.png'; out.save(p,optimize=False)
            rows.append({'registry':registry,'unit':unit,'frame':i,'path':str(p),'sha256':sha(p),
                         'size':[256,256],'mode':'RGBA','pivot':[0.5,0.5],
                         'source':str(SRC/name),'source_sha256':sha(SRC/name),
                         'source_rect':[xs[i],0,xs[i+1],c.height]})
    return rows

def contact(rows, name, bg):
    subset=[r for r in rows if r['registry']==name]
    units=[]
    for r in subset:
        if r['unit'] not in units: units.append(r['unit'])
    sheet=Image.new('RGB',(6*200,len(units)*220),bg)
    dr=ImageDraw.Draw(sheet)
    for y,u in enumerate(units):
        ur=[r for r in subset if r['unit']==u]
        for x,r in enumerate(ur):
            im=Image.open(r['path']).convert('RGBA').resize((200,200),Image.Resampling.LANCZOS)
            tile=Image.new('RGBA',(200,200),bg+(255,)); tile.alpha_composite(im)
            sheet.paste(tile.convert('RGB'),(x*200,y*220))
        dr.text((4,y*220+202),u,fill=(230,120,70) if sum(bg)<100 else (25,45,70))
    p=ROOT/'contacts'/f'{name}-{("dark" if sum(bg)<100 else "light")}-200.png'
    p.parent.mkdir(parents=True,exist_ok=True); sheet.save(p)
    return {'path':str(p),'sha256':sha(p)}

def main():
    ROOT.mkdir(parents=True,exist_ok=True)
    rows=save_frames('body','body',BODY)+save_frames('vfx','vfx',VFX)
    contacts=[]
    for reg in ('body','vfx'):
        contacts += [contact(rows,reg,(18,21,27)),contact(rows,reg,(232,226,211))]
    manifest={'status':'CANDIDATE_ONLY_ATOMIC_HANDOFF','body_frame_count':18,'vfx_fresh_frame_count':36,
              'existing_hit0_vfx':'retained canonical exact18; not copied here',
              'frame_semantics':['anticipation','onset','build','contact','decay','recovery'],
              'timing':'runtime-owned by comboSteps; loop=false',
              'rows':rows,'contacts':contacts}
    p=ROOT/'manifest.json'; p.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
    receipt=ROOT/'receipt.json'; receipt.write_text(json.dumps({'manifest':str(p),'manifest_sha256':sha(p),'counts':{'body':18,'fresh_vfx':36,'total_new':54},'contacts':contacts},indent=2)+'\n')
    print(json.dumps({'manifest':str(p),'manifest_sha256':sha(p),'receipt':str(receipt),'receipt_sha256':sha(receipt)},indent=2))
if __name__=='__main__': main()
