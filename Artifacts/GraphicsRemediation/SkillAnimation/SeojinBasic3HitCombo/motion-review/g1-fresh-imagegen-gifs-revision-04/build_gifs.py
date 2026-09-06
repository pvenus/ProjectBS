from pathlib import Path
from PIL import Image, ImageFilter
import numpy as np
import hashlib, json, shutil

ROOT = Path(__file__).resolve().parent
SRC = {
    "A": Path("/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-ef166094-8e57-4eb2-aede-5d2cc87fd8f0.png"),
    "B": Path("/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-882ce500-3584-4f62-bd2e-c793b8e66af0.png"),
    "C": Path("/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-abc257ae-9c00-4e8e-b1ee-0f042d3316f5.png"),
    "D": Path("/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-600783ed-96f5-4d18-9ddd-d1dbaa661289.png"),
    "E": Path("/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-f4406309-8375-4e1a-97c5-32f5f2db6101.png"),
}
DELAYS = [80,80,80,80,120,80, 80,80,80,80,120,80, 80,80,80,80,120,320]

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main_component(cell):
    arr=np.asarray(cell,dtype=np.int16)
    border=np.concatenate([arr[:6].reshape(-1,3),arr[-6:].reshape(-1,3),arr[:,:6].reshape(-1,3),arr[:,-6:].reshape(-1,3)])
    bg=np.median(border,axis=0).astype(np.uint8)
    raw=np.max(np.abs(arr-bg.astype(np.int16)),axis=2)>16
    grown=np.asarray(Image.fromarray(raw.astype(np.uint8)*255).filter(ImageFilter.MaxFilter(5)))>0
    h,w=grown.shape; seen=np.zeros_like(grown); best=[]; score=-1
    for sy,sx in zip(*np.where(grown & ~seen)):
        if seen[sy,sx]: continue
        q=[(sy,sx)]; seen[sy,sx]=1; pts=[]
        for y,x in q:
            pts.append((y,x))
            for dy,dx in ((1,0),(-1,0),(0,1),(0,-1)):
                ny,nx=y+dy,x+dx
                if 0<=ny<h and 0<=nx<w and grown[ny,nx] and not seen[ny,nx]:
                    seen[ny,nx]=1; q.append((ny,nx))
        ys=np.array([p[0] for p in pts]); xs=np.array([p[1] for p in pts])
        dist=((xs.mean()-w/2)**2+(ys.mean()-h/2)**2)**.5
        s=len(pts)/(1+dist*.7)
        if s>score: score=s; best=pts
    keep=np.zeros((h,w),dtype=np.uint8)
    for y,x in best: keep[y,x]=255
    keep=np.asarray(Image.fromarray(keep).filter(ImageFilter.MaxFilter(3)))>0
    alpha=(raw & keep).astype(np.uint8)*255
    rgba=Image.new('RGBA',cell.size,(0,0,0,0)); rgba.paste(cell,(0,0),Image.fromarray(alpha))
    box=Image.fromarray(alpha).getbbox()
    return rgba.crop(box) if box else cell.convert('RGBA')

def fit(cell):
    cell=main_component(cell)
    canvas = Image.new("RGB", (768,512), (238,235,228))
    scale = min(720/cell.width, 464/cell.height)
    size = (round(cell.width*scale), round(cell.height*scale))
    part = cell.resize(size, Image.Resampling.LANCZOS)
    canvas.paste(part, ((768-size[0])//2, (512-size[1])//2), part)
    return canvas

def main():
    (ROOT/'sources').mkdir(parents=True, exist_ok=True)
    (ROOT/'frames').mkdir(exist_ok=True)
    (ROOT/'gifs').mkdir(exist_ok=True)
    (ROOT/'contacts').mkdir(exist_ok=True)
    manifest = {"status":"REVIEW_ONLY_NOT_INSTALL_AUTHORITY", "canvas":[768,512], "physicalFrames":18, "durationMs":1800, "candidates":{}}
    contact_tiles=[]
    for name, src in SRC.items():
        local = ROOT/'sources'/f'candidate-{name}-fresh-board.png'
        shutil.copy2(src, local)
        im = Image.open(local).convert('RGB')
        cw, ch = im.width//6, im.height//3
        frames=[]; entries=[]
        frame_dir=ROOT/'frames'/name; frame_dir.mkdir(exist_ok=True)
        for i in range(18):
            r,c=divmod(i,6)
            cell=im.crop((c*cw,r*ch,(c+1)*cw,(r+1)*ch))
            fr=fit(cell)
            fp=frame_dir/f'frame-{i:02d}.png'; fr.save(fp)
            frames.append(fr); entries.append({"frame":i,"sha256":sha(fp),"delayMs":DELAYS[i]})
        gif=ROOT/'gifs'/f'candidate-{name}-fresh-continuous18-768x512.gif'
        frames[0].save(gif,save_all=True,append_images=frames[1:],duration=DELAYS,loop=0,optimize=False,disposal=2)
        contact=Image.new('RGB',(768*6,512*3),(238,235,228))
        for i,fr in enumerate(frames): contact.paste(fr,((i%6)*768,(i//6)*512))
        contact=contact.resize((1536,512),Image.Resampling.LANCZOS)
        cp=ROOT/'contacts'/f'candidate-{name}-contact18.png'; contact.save(cp)
        contact_tiles.append(contact.resize((768,256),Image.Resampling.LANCZOS))
        manifest['candidates'][name]={"source":str(local),"sourceSha256":sha(local),"gif":str(gif),"gifSha256":sha(gif),"contact":str(cp),"contactSha256":sha(cp),"frames":entries}
    idx=Image.new('RGB',(768,256*5),(225,222,216))
    for i,t in enumerate(contact_tiles): idx.paste(t,(0,i*256))
    index=ROOT/'candidate5-fresh-index.png'; idx.save(index)
    manifest['index']=str(index); manifest['indexSha256']=sha(index)
    mp=ROOT/'manifest.json'; mp.write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    print(mp)

if __name__ == '__main__': main()
