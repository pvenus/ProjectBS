const sharp=require('sharp'),path=require('path'),fs=require('fs');
const base='Assets/ImagesGenerated/Character/animation/character.child_snatcher_netman.1';
const out='Artifacts/GraphicsRemediation/AnimationCategory/child_snatcher_netman.1/action-selection';
fs.mkdirSync(out,{recursive:true});
const actions={
 run:'character.child_snatcher_netman.1.run.screen_right.four_key_pose.loop',
 attack:'character.child_snatcher_netman.1.attack.weighted_net_cast.four_key_pose.one_shot'
};
async function make(action,prefix){
 const cell=180,g=8,label=28,rows=5,cols=5,W=label+cols*(cell+g)+g,H=label+rows*(cell+g)+g;
 const bg=await sharp({create:{width:W,height:H,channels:4,background:'#D8D5CC'}}).png().toBuffer(), comps=[];
 for(let v=1;v<=5;v++) for(let c=0;c<5;c++){
  const f=c===4?(action==='run'?0:3):c;
  const p=path.join(base,`${prefix}.candidate_v${v}`,`frame-${f}.png`);
  const img=await sharp(p).resize(cell,cell,{fit:'contain'}).png().toBuffer();
  comps.push({input:img,left:label+g+c*(cell+g),top:label+g+(v-1)*(cell+g)});
 }
 const svg=`<svg width="${W}" height="${H}" xmlns="http://www.w3.org/2000/svg"><style>text{font:18px sans-serif;fill:#222}</style>${[0,1,2,3,4].map((x,i)=>`<text x="${label+g+i*(cell+g)}" y="21">${i===4?(action==='run'?'f0 loop':'f3 hold'):'f'+x}</text>`).join('')}${[1,2,3,4,5].map((v,i)=>`<text x="3" y="${label+g+i*(cell+g)+24}">v${v}</text>`).join('')}</svg>`;
 comps.unshift({input:Buffer.from(svg),left:0,top:0});
 await sharp(bg).composite(comps).png({compressionLevel:9}).toFile(path.join(out,`child-snatcher-${action}-candidates5-contact.png`));
}
(async()=>{for(const [a,p] of Object.entries(actions))await make(a,p)})();
