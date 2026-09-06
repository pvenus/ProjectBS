from __future__ import annotations
import hashlib, json, shutil
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).parent
SOURCES='''
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-9f28c27d-f40d-4b0b-b168-a1bb864156a8.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-0e1facc4-876a-40ca-a4f1-6e4c00ca2c77.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-b4064f1b-cef5-4df8-a65e-d71135b45fa1.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-c8d47eaa-f994-4986-ad55-2c01fab4982e.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-5d755873-579d-41aa-b8d4-af5e094b5fd9.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-cd0b5402-64a0-47bb-afbb-480f38579282.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-e7ec42d2-af9f-442a-abda-8030501c51bf.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-5acca078-8801-404b-b76d-ce498dfd21e6.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-f271bd6f-98d5-43d3-9320-85c68c295b3c.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-1f1de9dc-ad20-416a-a24e-0cd9f3d21366.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-af677b19-3f75-4bc0-8338-647baaef4c2a.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-55443b1c-3ee5-44ac-b07b-25ecb8a3cf8a.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-d257f151-8f5b-455e-a194-0bf6a45d1943.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-5ae85507-1d92-4679-a187-a4f04e48e0a1.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-90c9e620-07e2-4d86-9365-1678a436543e.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-17fc0656-2bf0-49cc-9ec7-e8e2b3bd9d1e.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-6ffd7175-dfca-452a-9de6-f38497faeb00.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-86322fdd-17a9-4867-a8ef-8f68c9c6829b.png
'''.strip().splitlines()
DELAYS=[80,80,80,80,120,80,80,80,80,80,120,80,80,80,80,80,120,320]
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
raw=ROOT/'raw-native-1536x1024'; out=ROOT/'frames-768x512'; raw.mkdir(parents=True,exist_ok=True); out.mkdir(parents=True,exist_ok=True)
frames=[]; records=[]
for i,s in enumerate(SOURCES):
 p=Path(s); rp=raw/f'frame-{i:02d}.png'; shutil.copyfile(p,rp); im=Image.open(rp)
 if im.mode!='RGBA' or im.size!=(1536,1024) or im.getchannel('A').getextrema()[0]!=0: raise RuntimeError((i,im.mode,im.size))
 q=im.resize((768,512),Image.Resampling.LANCZOS); op=out/f'frame-{i:02d}.png'; q.save(op); frames.append(q)
 a=q.getchannel('A'); records.append({'frame':i,'source':s,'source_sha256':sha(p),'output_sha256':sha(op),'alpha_extrema':list(a.getextrema()),'alpha_bbox':list(a.getbbox() or (0,0,0,0))})
gif=ROOT/'C3-like-continuous18-clean-review.gif'; frames[0].save(gif,save_all=True,append_images=frames[1:],duration=DELAYS,loop=0,disposal=2)
contact=Image.new('RGB',(2304,768),(36,42,50))
for i,im in enumerate(frames):
 bg=Image.new('RGBA',im.size,(36,42,50,255)); bg.alpha_composite(im); thumb=bg.convert('RGB').resize((384,256),Image.Resampling.LANCZOS); contact.paste(thumb,((i%6)*384,(i//6)*256))
cp=ROOT/'C3-like-contact18-unmarked.png'; contact.save(cp)
m={'status':'PHYSICAL_PASS_VISUAL_REVIEW_PENDING','method':'C3_fixed_anchor_plus_previous_pose_sequential_native_RGBA','attempt':3,'reference_c3_sha256':'3c1d9186a9aa1abac8b05659533671ea5f8a58b2ace585a8f439849e70723dbf','physical_frames':18,'dimensions':[768,512],'duration_ms':1800,'delays_ms':DELAYS,'segments':{'hit1':[0,6],'hit2':[6,12],'hit3':[12,18]},'contacts':[4,10,16],'frames':records,'gif':{'path':str(gif),'sha256':sha(gif)},'contact':{'path':str(cp),'sha256':sha(cp)},'project_install':0}
(ROOT/'manifest.json').write_text(json.dumps(m,indent=2)+'\n')
print(json.dumps({'gif':str(gif),'contact':str(cp),'manifest':str(ROOT/'manifest.json')},indent=2))
