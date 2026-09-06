#!/usr/bin/env python3
import json,hashlib
from pathlib import Path
from PIL import Image
GEN=Path('/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93')
OUT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/candidates/revision-04-exact-size-attempt-d')
BODY_NAMES=['31a70258-3dcc-4f53-b66c-8c87989239d5','1c22cf06-8da2-428b-8af6-b62700887209','43a9760a-8570-49c9-a8b6-fe1981466e43','88e34d5a-819e-4917-9a45-77c09b70421c','28a99dc0-1043-42ce-866b-a6ed4ca94fd5','c97c79f2-cec0-4891-819c-14696af07824','0ad6d289-dec3-43da-b6fd-3fddb63feda6','a18d8276-9e26-47e4-9cf6-c525d239f03a','3fc9d931-437c-4e80-b15b-067e4f7c96ca','d562223d-1d3a-4189-b6ff-64950d8673cc','c3ffcd2d-6659-4f55-aff7-73780a8e4b6c','6f242456-6e50-4dbb-be0a-41057051cf3c','3bae0d71-25fc-4b16-9933-1ad2b16fdbe4','f649d674-a0cf-4f8b-b19b-9841226e3c62','78a2f277-5de5-4c32-9525-6950c50586d1','86231618-65b2-4061-b6fc-23e045ff5b9b','0e0f0365-b70c-454f-8b7d-966edbeca5f9','c264efd5-ead6-43de-ba60-841bea9d4588']
VFX_NAMES=['cc649854-c78e-4453-a0d4-d0f7a100074e','6cb6aaa6-8ef4-4728-af14-1099cd7d80fd','18245b04-a856-42fd-a1ee-81fa848001bd','8d9ebda4-0e73-4d9f-9627-637c2447c163','a61b697b-4aa9-41ee-ad4f-198b630db2a2','c61c51e3-dd51-4d75-ac21-ffa2f975dbad','1a86b11c-3ec9-41ec-bc59-e076268435f3','c5937106-b3ef-4c04-b3a7-dde7e6880c48','3b2912f9-bbb7-41ca-9d32-a897e2919c32','f46bf1e8-1040-4015-8ed0-5255ca7216ae','50603e31-9c5b-4434-b911-b7bddd7d0b4d','b9ca570e-8d46-4a2c-9975-824d4566b0ab','c82a6746-ce12-4b54-9432-3cf93113617b','731cc018-5742-47f9-b5b0-c95678d794a7','3f3ae5a8-5c36-41d3-a612-354a664ac400','a198fc8f-d82b-46ac-8a47-3f520d417cf0','87b0b7d2-6890-433a-8298-f80dae4312db','b4529070-5f55-47d4-88cb-05a15d432e3e','4f0edf5d-ed3d-4996-9220-c59ffa54c605','e30ee1a5-b500-4fca-bcef-a5d67399991a','55584e86-edd7-4ba7-adef-83f0063163a9','cb30588f-f6b7-4def-909b-29c655e2c847','79e9320a-de6e-46fa-a93c-4bbb84ed057a','27caf3ec-0efa-45a7-8368-a497de1d8a76','ca626cc9-6299-4fb5-9fe2-e356d30d3dd5','bfeeda00-4d1b-49ef-a150-cd2b080ac641','02370915-fb9d-4920-a84e-55674a1370dc','5f3e9953-e8f6-4895-b0aa-a9d8585c7cb7','13d116a3-003d-4a8c-997a-70de255ff342','49a56b20-ff4c-4ba2-bca7-adb04bc7db60','70bdd2f1-6417-4fad-99e7-e3ef65b9f94b','41af21fb-5826-481f-b880-7b8d6e84a7a6','d11b8e5b-458f-4f44-99c7-2fb140b77bed','e01f3a47-c3cf-43a5-b663-5f8854a6cdfc','3109a070-0876-4718-8cb4-271da26daef5','38415519-2bc1-454d-866d-a726508c8fbc']
BT=[(290,485),(310,475),(330,465),(453,446),(576,428),(314,476)]
VT={'g1':[(113,31),(217,41),(236,64),(236,74),(222,69),(164,61)],'g2':[(124,39),(194,142),(236,100),(236,97),(236,83),(235,89)],'g3':[(158,44),(236,62),(236,87),(236,105),(236,100),(236,59)]}
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def src(uuid): return GEN/f'exec-{uuid}.png'
def contain(p,target,canvas,anchor,tol,out):
 im=Image.open(p).convert('RGBA'); b=im.getchannel('A').getbbox(); fg=im.crop(b); sw,sh=fg.size; tw,th=target
 f=min(tw/sw,th/sh); rw,rh=round(sw*f),round(sh*f); ok=abs(rw-tw)/tw<=tol and abs(rh-th)/th<=tol
 row={'source':str(p),'source_sha':sha(p),'source_bbox':[sw,sh],'target':list(target),'uniform_bbox':[rw,rh],'pass':ok}
 if ok:
  fg=fg.resize((rw,rh),Image.Resampling.LANCZOS); c=Image.new('RGBA',canvas,(0,0,0,0)); x=(canvas[0]-rw)//2; y=(canvas[1]-rh)//2 if anchor=='center' else canvas[1]-12-rh; c.alpha_composite(fg,(x,y)); out.parent.mkdir(parents=True,exist_ok=True); c.save(out); row.update(path=str(out),sha=sha(out),bbox=[x,y,x+rw,y+rh])
 return row
rows=[]
for n,u in enumerate(BODY_NAMES):
 hit=n//6;i=n%6; r=contain(src(u),BT[i],(768,512),'bottom',.05,OUT/'body'/f'hit{hit}'/f'frame-{i}.png'); r.update(registry='body',hit=hit,frame=i); rows.append(r)
for n,u in enumerate(VFX_NAMES):
 grade=f'g{n//12+1}';hit=f'hit{n%12//6+1}';i=n%6; r=contain(src(u),VT[grade][i],(256,256),'center',.08,OUT/'vfx'/grade/hit/f'frame-{i}.png'); r.update(registry='vfx',grade=grade,hit=hit,frame=i); rows.append(r)
OUT.mkdir(parents=True,exist_ok=True); m=OUT/'manifest.json'; m.write_text(json.dumps(rows,indent=2)+'\n')
print('body',sum(r['pass'] for r in rows if r['registry']=='body'),'/18','vfx',sum(r['pass'] for r in rows if r['registry']=='vfx'),'/36'); print(m,sha(m))
