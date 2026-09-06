from __future__ import annotations
import hashlib,json,shutil
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).parent
SOURCES='''
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-06bb71f7-5fe1-4e13-bf0a-0b53bd0ff5e7.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-d4eb391f-b307-4a9b-82af-8cd5a50bdba0.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-cb5a4e46-087f-4599-92ae-5bbda98379d0.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-268f2443-6935-495e-8e5f-7fe7b2d20ceb.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-a0aa2717-a919-44e6-aa9a-028644bc6ac7.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-5485008a-fc25-44bf-b8bd-86c26b205795.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-314d951e-8e8f-45bb-aeda-ad21b760572e.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-d9d88361-1579-4211-a1fe-2a603f3028e3.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-3579b718-3d2f-468c-be47-0fec7ec4bf6d.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-63f9667c-1a8a-4741-9112-5840de305f8e.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-46dee24a-1d30-4669-8c94-224e8a1e5197.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-642da3a5-770d-4e27-a8b9-a0515f7a1464.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-d1fb027b-0d37-40f8-8db1-71ff2ef668cd.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-cc4ee341-16ab-4a2d-ab0b-97c500c06539.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-40c16f47-d734-4f47-9773-8bfa4f88ede9.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-1d2aedd5-d33d-45b3-8e39-61013b8e9372.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-63e28599-fb94-4472-b281-b501979bd2d3.png
/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-56ceb42d-b120-4c16-87af-099f114f17a0.png
'''.strip().splitlines(); DELAYS=[80,80,80,80,120,80,80,80,80,80,120,80,80,80,80,80,120,320]
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def contain(im,size,pad=0):
 w,h=size;s=min((w-2*pad)/im.width,(h-2*pad)/im.height);nw=round(im.width*s);nh=round(im.height*s);q=im.resize((nw,nh),Image.Resampling.LANCZOS);c=Image.new('RGBA',size,(0,0,0,0));c.alpha_composite(q,((w-nw)//2,(h-nh)//2));return c
raw=ROOT/'raw-native-1536x1024';review=ROOT/'review-768x512';runtime=ROOT/'runtime-256x256';raw.mkdir(parents=True,exist_ok=True);review.mkdir(parents=True,exist_ok=True);runtime.mkdir(parents=True,exist_ok=True)
frames=[];rec=[]
for i,s in enumerate(SOURCES):
 p=Path(s);rp=raw/f'frame-{i:02d}.png';shutil.copyfile(p,rp);im=Image.open(rp)
 if im.mode!='RGBA' or im.size!=(1536,1024) or im.getchannel('A').getextrema()[0]!=0:raise RuntimeError((i,im.mode,im.size))
 rv=contain(im,(768,512));rvp=review/f'frame-{i:02d}.png';rv.save(rvp);frames.append(rv)
 rt=contain(im,(256,256),10);rtp=runtime/f'frame-{i:02d}.png';rt.save(rtp);a=rt.getchannel('A');rec.append({'frame':i,'source_sha256':sha(p),'review_sha256':sha(rvp),'runtime_sha256':sha(rtp),'runtime_alpha_bbox':list(a.getbbox() or (0,0,0,0)),'alpha_extrema':list(a.getextrema())})
gif=ROOT/'C3-like-character-free-vfx-continuous18.gif';frames[0].save(gif,save_all=True,append_images=frames[1:],duration=DELAYS,loop=0,disposal=2)
contact=Image.new('RGB',(2304,768),(36,42,50))
for i,im in enumerate(frames):
 bg=Image.new('RGBA',im.size,(36,42,50,255));bg.alpha_composite(im);contact.paste(bg.convert('RGB').resize((384,256),Image.Resampling.LANCZOS),((i%6)*384,(i//6)*256))
cp=ROOT/'C3-like-character-free-vfx-contact18.png';contact.save(cp)
m={'status':'PHYSICAL_PASS_VISUAL_REVIEW_PENDING','attempt':2,'method':'independent_native_rgba_fixed_composition_prompts','physical_frames':18,'review_dimensions':[768,512],'runtime_dimensions':[256,256],'runtime_padding':10,'duration_ms':1800,'delays_ms':DELAYS,'segments':{'hit1':[0,6],'hit2':[6,12],'hit3':[12,18]},'contacts':[4,10,16],'frames':rec,'gif':{'path':str(gif),'sha256':sha(gif)},'contact':{'path':str(cp),'sha256':sha(cp)},'install':0}
(ROOT/'manifest.json').write_text(json.dumps(m,indent=2)+'\n')
print(json.dumps({'gif':str(gif),'contact':str(cp),'manifest':str(ROOT/'manifest.json')},indent=2))
