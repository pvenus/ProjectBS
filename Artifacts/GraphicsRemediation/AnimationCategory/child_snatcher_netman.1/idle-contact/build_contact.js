const sharp = require('sharp');
const path = require('path');
const fs = require('fs');

const root = 'Assets/ImagesGenerated/Character/animation/character.child_snatcher_netman.1';
const base = 'character.child_snatcher_netman.1.idle.simple_stable.four_key_pose.loop.candidate_v';
const out = path.dirname(__filename);
const cell = 180, gap = 8, label = 28, cols = 5, rows = 5;

(async()=>{
  const comps=[];
  const bg=await sharp({create:{width:cols*(cell+gap)+gap,height:rows*(cell+label+gap)+gap,channels:4,background:'#2A2D31'}}).png().toBuffer();
  for(let v=1;v<=5;v++){
    const frames=[0,1,2,3,0];
    for(let c=0;c<frames.length;c++){
      const f=path.join(root,base+v,`frame-${frames[c]}.png`);
      const buf=await sharp(f).resize(cell,cell,{fit:'contain',background:{r:216,g:213,b:204,alpha:1}}).png().toBuffer();
      comps.push({input:buf,left:gap+c*(cell+gap),top:gap+(v-1)*(cell+label+gap)+label});
    }
  }
  const svg=`<svg width="${cols*(cell+gap)+gap}" height="${rows*(cell+label+gap)+gap}" xmlns="http://www.w3.org/2000/svg"><style>text{font:20px sans-serif;fill:white}</style>${[1,2,3,4,5].map((v,i)=>`<text x="10" y="${24+i*(cell+label+gap)}">v${v}: f0 f1 f2 f3 f0(loop)</text>`).join('')}</svg>`;
  comps.unshift({input:Buffer.from(svg),left:0,top:0});
  await sharp(bg).composite(comps).png({compressionLevel:9}).toFile(path.join(out,'child-snatcher-idle-candidates5-contact.png'));
})();
