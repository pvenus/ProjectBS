from pathlib import Path
from PIL import Image
import hashlib,json,shutil
import numpy as np

ROOT=Path(__file__).resolve().parent
BOARDS={
'Move-A':'exec-22609bb5-216f-459e-961e-fbbe0fc35f50.png','Move-B':'exec-56f1939e-4c58-4a8a-a5e8-3371f3fb3fdf.png','Move-C':'exec-5be6563a-317d-4e23-b1e3-b8bb48a94ba8.png',
'Idle-A':'exec-6ad405d7-ef36-4395-9e91-daa471794ece.png','Idle-B':'exec-e726bb68-f5e0-496e-bf95-ef099480f2ba.png','Idle-C':'exec-eeb73c1b-736b-4568-b659-5f702f9af9b3.png',
'Dash-A':'exec-393cd5bc-60d1-4db1-9cec-8ea0cb5f3a65.png','Dash-B':'exec-7c9376df-6925-4113-973d-7c75971e967d.png','Dash-C':'exec-35c4ed8f-82fb-4546-b382-ae7301443138.png',
'Charge-A':'exec-a7d6085b-abcb-462d-ab30-26fad9631a3d.png','Charge-B':'exec-ddb61a0c-47e4-46fd-9c98-ed687eb31881.png','Charge-C':'exec-a0ede08a-4e9f-4fd4-ac92-3522a427bf1f.png',
'CraneCast-A':'exec-155bf644-3430-46a9-9cec-018bb8cd5ee1.png','CraneCast-B':'exec-510362fe-6e9f-4539-9a89-5a038a8c4fb7.png','CraneCast-C':'exec-f279b5db-9cb8-43e5-9bb9-a907d126ddfe.png',
'TurtleSummon-A':'exec-323e74d1-66be-49a7-b78a-ccec08352a33.png','TurtleSummon-B':'exec-ff6795b7-b15a-4672-af98-94ea908cb8ca.png','TurtleSummon-C':'exec-5551b571-348d-48dc-8cf2-502ee04a967b.png'}
GEN=Path('/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93')

def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def frame(cell):
    dst=Image.new('RGB',(568,340),(235,233,228)); s=min(540/cell.width,312/cell.height); q=cell.resize((round(cell.width*s),round(cell.height*s)),Image.Resampling.LANCZOS);dst.paste(q,((568-q.width)//2,(340-q.height)//2));return dst

def main():
    raw=ROOT/'raw-boards';gifs=ROOT/'gifs';contacts=ROOT/'contacts';framesroot=ROOT/'review-frames';
    for p in(raw,gifs,contacts,framesroot):p.mkdir(parents=True,exist_ok=True)
    recs=[]; overview=Image.new('RGB',(568*3,340*6),(220,218,212))
    for idx,(name,file) in enumerate(BOARDS.items()):
        src=GEN/file; bp=raw/f'{name}.png'; shutil.copy2(src,bp); im=Image.open(bp).convert('RGB'); cw=im.width/6
        frames=[]; touches=[]; fd=framesroot/name;fd.mkdir(exist_ok=True)
        for i in range(6):
            x0=round(i*cw);x1=round((i+1)*cw); cell=im.crop((x0,0,x1,im.height)); fr=frame(cell); fp=fd/f'frame-{i}.png';fr.save(fp);frames.append(fr)
            a=np.asarray(cell); bg=np.median(np.concatenate([a[:4].reshape(-1,3),a[-4:].reshape(-1,3)]),axis=0); fg=np.max(np.abs(a.astype(np.int16)-bg.astype(np.int16)),axis=2)>18
            touches.append({'frame':i,'left':int(fg[:,0].sum()),'right':int(fg[:,-1].sum()),'top':int(fg[0].sum()),'bottom':int(fg[-1].sum())})
        gp=gifs/f'{name}.gif';frames[0].save(gp,save_all=True,append_images=frames[1:],duration=[100]*6,loop=0,optimize=False,disposal=2)
        cp=contacts/f'{name}-contact6.png'; contact=Image.new('RGB',(568*6,340),(235,233,228));
        for i,f in enumerate(frames):contact.paste(f,(i*568,0))
        contact=contact.resize((1136,113),Image.Resampling.LANCZOS);contact.save(cp)
        row=idx//3;col=idx%3;overview.paste(contact.resize((568,57),Image.Resampling.LANCZOS),(col*568,row*340+142))
        recs.append({'name':name,'rawBoard':str(bp),'rawSha256':sha(bp),'rawDimensions':list(im.size),'rawMode':Image.open(bp).mode,'gif':str(gp),'gifSha256':sha(gp),'contact':str(cp),'contactSha256':sha(cp),'cellBoundaryTouchPixels':touches,'warning':'Review crop may contain overlap/clipping when boundary touch is nonzero; raw board remains authority.'})
    ip=ROOT/'six-actions-variants-overview.png';overview.save(ip)
    m={'status':'REVIEW_ONLY_NOT_INSTALL_AUTHORITY','generator':'built-in ImageGen','reference':'/Users/pvenus/ProjectBS-image-python/downloads/exec-600783ed-96f5-4d18-9ddd-d1dbaa661289_7d56b10b/frames/frame_001.png','actions':6,'variantsPerAction':3,'animations':18,'visualFrames':108,'reviewGifFrames':6,'rawBoardsPreserved':True,'alphaGateDeferredByUser':True,'normalizationDeferredByUser':True,'CraneTurtleBindingInferred':False,'records':recs,'overview':{'path':str(ip),'sha256':sha(ip)},'projectInstall':False};mp=ROOT/'manifest.json';mp.write_text(json.dumps(m,indent=2)+'\n');print(mp)
if __name__=='__main__':main()
