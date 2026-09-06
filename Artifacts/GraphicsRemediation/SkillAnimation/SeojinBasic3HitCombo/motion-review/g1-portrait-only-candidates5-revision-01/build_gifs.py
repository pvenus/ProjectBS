from __future__ import annotations
import hashlib,json,shutil
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).parent
BOARDS={
'A':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-194fee63-dab9-4b16-baed-290cf0f581bf.png',
'B':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-9a38a7ca-cddb-435c-a9aa-9fde519922b3.png',
'C':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-e9765dc0-c7ca-48a3-b61a-408c2a7eaeeb.png',
'D':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-e54f1e46-42b0-490f-80cf-e5e03391ef4f.png',
'E':'/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-a0a8e7b5-108a-4106-9ea1-6a8065fdafb1.png'}
DELAYS=[80,80,80,80,120,80,80,80,80,80,120,80,80,80,80,80,120,320]
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
boards=ROOT/'boards';gifs=ROOT/'gifs';contacts=ROOT/'contacts';boards.mkdir(parents=True,exist_ok=True);gifs.mkdir(parents=True,exist_ok=True);contacts.mkdir(parents=True,exist_ok=True)
records=[];index=Image.new('RGB',(768,512*5),(238,235,228))
for row,(name,src) in enumerate(BOARDS.items()):
 bp=boards/f'candidate-{name}-motion-board.png';shutil.copyfile(src,bp);im=Image.open(bp).convert('RGB');assert im.size==(1536,1024)
 bg=im.getpixel((1535,1023));frames=[]
 for i in range(18):
  col=i%6;r=i//6;x0=round(col*im.width/6);x1=round((col+1)*im.width/6);y0=round(r*im.height/3);y1=round((r+1)*im.height/3)
  cell=im.crop((x0,y0,x1,y1));s=min(720/cell.width,480/cell.height);q=cell.resize((round(cell.width*s),round(cell.height*s)),Image.Resampling.LANCZOS);canvas=Image.new('RGB',(768,512),bg);canvas.paste(q,((768-q.width)//2,(512-q.height)//2));frames.append(canvas)
 gp=gifs/f'candidate-{name}-continuous18-768x512.gif';frames[0].save(gp,save_all=True,append_images=frames[1:],duration=DELAYS,loop=0,disposal=2)
 cp=contacts/f'candidate-{name}-contact18.png';thumb=Image.new('RGB',(768,512),bg)
 for i,f in enumerate(frames): thumb.paste(f.resize((128,171),Image.Resampling.LANCZOS),((i%6)*128,(i//6)*171))
 thumb.save(cp);index.paste(thumb,(0,row*512))
 records.append({'candidate':name,'board':str(bp),'board_sha256':sha(bp),'gif':str(gp),'gif_sha256':sha(gp),'contact':str(cp),'contact_sha256':sha(cp),'physical_frames':18,'dimensions':[768,512],'duration_ms':1800})
ip=ROOT/'candidate5-index.png';index.save(ip)
m={'status':'REVIEW_ONLY_NO_INSTALL_AUTHORITY','portrait_reference':'/Users/pvenus/ProjectBS/Assets/ImagesGenerated/Character/portrait/character.seojin.1.portrait.png','portrait_sha256':'ba2f769ba7d45909d618f7fd672a9bdad61015b9553d3c0d360bc49a13bb97cf','method':'five_joint_18_pose_motion_boards_then_review_only_cell_playback','segments':{'hit1':[0,6],'hit2':[6,12],'hit3':[12,18]},'contacts':[4,10,16],'delays_ms':DELAYS,'candidates':records,'index':{'path':str(ip),'sha256':sha(ip)},'production_alpha':0,'install':0}
(ROOT/'manifest.json').write_text(json.dumps(m,indent=2)+'\n')
print(json.dumps({'root':str(ROOT),'index':str(ip),'manifest':str(ROOT/'manifest.json')},indent=2))
