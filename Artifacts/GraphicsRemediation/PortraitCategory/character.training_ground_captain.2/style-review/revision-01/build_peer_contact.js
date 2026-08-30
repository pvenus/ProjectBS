const sharp=require('sharp'),path=require('path');
const files=[
 ['Captain2','Assets/ImagesGenerated/Character/portrait/character.training_ground_captain.2.portrait.png'],
 ['Hut guard','Assets/ImagesGenerated/Character/portrait/character.hut_blocker_guard.2.portrait.png'],
 ['Mountain spear','Assets/ImagesGenerated/Character/portrait/character.mountain_fort_spearman.1.portrait.png'],
 ['Door shield','Assets/ImagesGenerated/Character/portrait/character.door_shield_barricader.1.portrait.png'],
 ['Child snatcher','Assets/ImagesGenerated/Character/portrait/character.child_snatcher_netman.1.portrait.png']
];
(async()=>{const cellW=300,cellH=450,gap=12,top=36,W=files.length*(cellW+gap)+gap,H=cellH+top+gap;
 const bg=await sharp({create:{width:W,height:H,channels:4,background:'#D8D5CC'}}).png().toBuffer(); const comps=[];
 for(let i=0;i<files.length;i++){const b=await sharp(files[i][1]).resize(cellW,cellH,{fit:'contain',background:{r:216,g:213,b:204,alpha:1}}).png().toBuffer();comps.push({input:b,left:gap+i*(cellW+gap),top:top});}
 const svg=`<svg width="${W}" height="${H}" xmlns="http://www.w3.org/2000/svg"><style>text{font:20px sans-serif;fill:#222}</style>${files.map((f,i)=>`<text x="${gap+i*(cellW+gap)}" y="26">${f[0]}</text>`).join('')}</svg>`; comps.unshift({input:Buffer.from(svg),left:0,top:0});
 await sharp(bg).composite(comps).png({compressionLevel:9}).toFile(path.join(__dirname,'captain2-peer5-contact.png'));})();
