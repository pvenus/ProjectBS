from __future__ import annotations
import hashlib, json, shutil
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).parent
SOURCES='''
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-4a970881-81a9-4e72-a754-518631b5e5b0.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-137ac80e-9c3d-49e6-9ea5-a26414049fd5.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-c438d764-eea9-438e-ad14-58ff45266901.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-1cd9357d-a0ac-40b9-a7ee-660d1cd14d58.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-1fe324f8-d2a4-463f-8dce-63a9605265c1.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-86149569-6eee-4fec-964c-dea0fe6518ab.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-b9798af5-5a94-48fc-8dbf-3b76db93706c.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-a58a245f-276a-4cea-83fa-cd8df7f4132e.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-f861d7fa-dd20-496f-aa88-c576d39aef51.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-47ceedb7-e276-4f7d-b494-0316d03ce364.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-09588042-3e7e-4788-b856-12cbb6463d79.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-bf284838-1730-4ea3-a0c2-c2ca618d6029.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-5acbb115-1513-4ca8-a7b9-2acdc6bc53fb.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-b5a49b07-2173-4b59-8f94-f0eeddd3dbc9.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-24349cfd-3a3d-45ad-b1ec-91e666e83eae.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-a5a87e5d-4f76-41fd-a4b2-eb88dcf27edc.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-c6f0bda9-d3dc-4b96-b72b-255b0c695b14.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-7e85eff7-56b5-48ea-91e6-6abccfe2909c.png
'''.strip().splitlines()
DELAYS=[80,80,80,80,120,80,80,80,80,80,120,80,80,80,80,80,120,320]
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def contain(im,size,pad=0):
 w,h=size; scale=min((w-2*pad)/im.width,(h-2*pad)/im.height); nw=max(1,round(im.width*scale)); nh=max(1,round(im.height*scale)); q=im.resize((nw,nh),Image.Resampling.LANCZOS); c=Image.new('RGBA',size,(0,0,0,0)); c.alpha_composite(q,((w-nw)//2,(h-nh)//2)); return c
raw=ROOT/'raw-native'; review=ROOT/'review-768x512'; runtime=ROOT/'runtime-256x256'; raw.mkdir(parents=True,exist_ok=True); review.mkdir(parents=True,exist_ok=True); runtime.mkdir(parents=True,exist_ok=True)
reviews=[]; records=[]
for i,s in enumerate(SOURCES):
 p=Path(s); rp=raw/f'frame-{i:02d}.png'; shutil.copyfile(p,rp); im=Image.open(rp)
 if im.mode!='RGBA' or im.getchannel('A').getextrema()[0]!=0: raise RuntimeError((i,im.mode,im.size))
 rv=contain(im,(768,512),0); rvp=review/f'frame-{i:02d}.png'; rv.save(rvp); reviews.append(rv)
 rt=contain(im,(256,256),10); rtp=runtime/f'frame-{i:02d}.png'; rt.save(rtp)
 a=rt.getchannel('A'); records.append({'frame':i,'source':s,'source_size':list(im.size),'source_sha256':sha(p),'review_sha256':sha(rvp),'runtime_sha256':sha(rtp),'runtime_alpha_bbox':list(a.getbbox() or (0,0,0,0)),'runtime_alpha_extrema':list(a.getextrema())})
gif=ROOT/'C3-like-character-free-vfx-continuous18.gif'; reviews[0].save(gif,save_all=True,append_images=reviews[1:],duration=DELAYS,loop=0,disposal=2)
contact=Image.new('RGB',(2304,768),(36,42,50))
for i,im in enumerate(reviews):
 bg=Image.new('RGBA',im.size,(36,42,50,255)); bg.alpha_composite(im); contact.paste(bg.convert('RGB').resize((384,256),Image.Resampling.LANCZOS),((i%6)*384,(i//6)*256))
cp=ROOT/'C3-like-character-free-vfx-contact18.png'; contact.save(cp)
m={'status':'PHYSICAL_PASS_VISUAL_REVIEW_PENDING','method':'built_in_imagegen_character_free_sequential_native_rgba','attempt':1,'reference':'C3 ink rhythm only; no reference image uploaded','physical_frames':18,'review_dimensions':[768,512],'runtime_dimensions':[256,256],'runtime_contain_padding_px':10,'duration_ms':sum(DELAYS),'delays_ms':DELAYS,'segments':{'hit1':[0,6],'hit2':[6,12],'hit3':[12,18]},'contacts':[4,10,16],'frames':records,'gif':{'path':str(gif),'sha256':sha(gif)},'contact':{'path':str(cp),'sha256':sha(cp)},'project_install':0}
(ROOT/'manifest.json').write_text(json.dumps(m,indent=2)+'\n')
print(json.dumps({'gif':str(gif),'contact':str(cp),'manifest':str(ROOT/'manifest.json')},indent=2))
