const sharp=require('sharp'),fs=require('fs'),path=require('path');
const src='Assets/ImagesGenerated/Character/portrait/character.training_ground_captain.2.portrait.png';
const root=__dirname;
async function variant(name,sigma,mix,contrast,levels){
 const orig=await sharp(src).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const blur=await sharp(src).ensureAlpha().blur(sigma).raw().toBuffer({resolveWithObject:true});
 const out=Buffer.alloc(orig.data.length);
 for(let i=0;i<out.length;i+=4){for(let c=0;c<3;c++){let v=orig.data[i+c]*mix+blur.data[i+c]*(1-mix);v=128+(v-128)*contrast;v=Math.round(v/(255/(levels-1)))*(255/(levels-1));out[i+c]=Math.max(0,Math.min(255,Math.round(v)));}out[i+3]=orig.data[i+3];}
 await sharp(out,{raw:orig.info}).png({compressionLevel:9,adaptiveFiltering:false}).toFile(path.join(root,name));
}
(async()=>{await variant('candidate-B-rgb-grouped.png',0.65,0.62,0.90,40);await variant('candidate-C-rgb-wash-simplified.png',0.9,0.48,0.84,28);
 const files=[['Current',src],['B grouped',path.join(root,'candidate-B-rgb-grouped.png')],['C wash',path.join(root,'candidate-C-rgb-wash-simplified.png')]];
 const cw=341,ch=512,g=12,top=34,W=files.length*(cw+g)+g,H=ch+top+g;
 const bg=await sharp({create:{width:W,height:H,channels:4,background:'#D8D5CC'}}).png().toBuffer(),comps=[];
 for(let i=0;i<files.length;i++){let b=await sharp(files[i][1]).resize(cw,ch,{fit:'contain',background:{r:216,g:213,b:204,alpha:1}}).png().toBuffer();comps.push({input:b,left:g+i*(cw+g),top:top});}
 const svg=`<svg width="${W}" height="${H}" xmlns="http://www.w3.org/2000/svg"><style>text{font:20px sans-serif;fill:#222}</style>${files.map((f,i)=>`<text x="${g+i*(cw+g)}" y="25">${f[0]}</text>`).join('')}</svg>`;comps.unshift({input:Buffer.from(svg),left:0,top:0});
 await sharp(bg).composite(comps).png({compressionLevel:9}).toFile(path.join(root,'captain2-style-candidates-contact.png'));
})();
