import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const sharp = require('sharp');

const ROOT = path.resolve(path.dirname(new URL(import.meta.url).pathname), '..');
const SOURCE = path.join(ROOT, 'source');
const RENDER = path.join(ROOT, 'render');
const svgPath = path.join(SOURCE, 'skill.character.yujin.1.active_1.multi_shot.icon.r3.svg');
const maskPath = path.join(SOURCE, 'ink-brush-mask.r3.png');
const outPath = path.join(RENDER, 'skill.character.yujin.1.active_1.multi_shot.icon.candidate-r3.png');
const sha = b => crypto.createHash('sha256').update(b).digest('hex');
const writeJson = (p, x) => fs.writeFileSync(p, JSON.stringify(x, null, 2) + '\n');

async function makeMask() {
  const w=1254,h=1254,c=Buffer.alloc(w*h);
  for(let y=0;y<h;y++) for(let x=0;x<w;x++) {
    const dx=(x-640)/535, dy=(y-627)/340;
    let v=(dx*dx+dy*dy<1.08)?255:0;
    const n=(x*1103515245+y*12345+((x*y)%7919))>>>0;
    if(v && n%97<7) v=90+(n%120);
    if(v && n%211===0) v=0;
    c[y*w+x]=v;
  }
  await sharp(c,{raw:{width:w,height:h,channels:1}}).png({compressionLevel:9,adaptiveFiltering:false,force:true}).toFile(maskPath);
}

async function renderOne(target) {
  const svg=fs.readFileSync(svgPath), mask=fs.readFileSync(maskPath);
  const base=await sharp(svg,{density:96}).resize(1254,1254,{fit:'fill'}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const m=await sharp(mask).raw().toBuffer();
  for(let i=0;i<1254*1254;i++) {
    const a=Math.round(base.data[i*4+3]*m[i]/255);
    base.data[i*4+3]=a;
    if(a===0){base.data[i*4]=0;base.data[i*4+1]=0;base.data[i*4+2]=0;}
  }
  await sharp(base.data,{raw:base.info}).png({compressionLevel:9,adaptiveFiltering:false,force:true}).toFile(target);
}

async function metrics(p) {
  const {data,info}=await sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let x0=info.width,y0=info.height,x1=-1,y1=-1,x16=info.width,y16=info.height,X16=-1,Y16=-1,residue=0,partial=0,opaque=0,transparent=0;
  const pal={ink:0,blueGray:0,grayWhite:0}, tokens={ink:[24,35,41],blueGray:[99,139,152],grayWhite:[195,199,193]}; let weight=0,out=0;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){const i=(y*info.width+x)*4,a=data[i+3];if(a===0){transparent++;if(data[i]||data[i+1]||data[i+2])residue++;continue;} if(a===255)opaque++;else partial++;x0=Math.min(x0,x);x1=Math.max(x1,x);y0=Math.min(y0,y);y1=Math.max(y1,y);if(a>=16){x16=Math.min(x16,x);X16=Math.max(X16,x);y16=Math.min(y16,y);Y16=Math.max(Y16,y);} let best='',bd=1e9;for(const[k,t]of Object.entries(tokens)){const d=Math.hypot(data[i]-t[0],data[i+1]-t[1],data[i+2]-t[2]);if(d<bd){bd=d;best=k;}}pal[best]+=a;weight+=a;if(bd>72)out++;}
  const pct=Object.fromEntries(Object.entries(pal).map(([k,v])=>[k,+(v/weight*100).toFixed(3)]));
  return {width:info.width,height:info.height,mode:'RGBA8',corners:[[0,0],[1253,0],[0,1253],[1253,1253]].map(([x,y])=>data[(y*1254+x)*4+3]),alpha:{transparent,partial,opaque,zeroRgbResidue:residue},bboxAlphaPositive:[x0,y0,x1,y1],bboxAlpha16:[x16,y16,X16,Y16],bbox16Percent:[+((X16-x16+1)/1254*100).toFixed(2),+((Y16-y16+1)/1254*100).toFixed(2)],margins16Percent:[+(x16/1254*100).toFixed(2),+(y16/1254*100).toFixed(2),+((1253-X16)/1254*100).toFixed(2),+((1253-Y16)/1254*100).toFixed(2)],paletteWeightedPercent:pct,outOfPaletteDistance72Count:out};
}

async function contact(size,name,background='#171a1c') {
  const refs=['Assets/ImagesGenerated/Skill/icon/skill.character.yujin.1.basic_attack.basic_attack.icon.png',outPath,'Assets/ImagesGenerated/Skill/icon/skill.character.yujin.1.passive_1.passive_1.icon.png'];
  const tiles=[];for(const p of refs){const b=await sharp(p).resize(size,size,{fit:'contain'}).png().toBuffer();tiles.push(b);} const gap=Math.max(8,Math.round(size*.1));const canvas=sharp({create:{width:size*3+gap*4,height:size+gap*2,channels:4,background}});await canvas.composite(tiles.map((input,i)=>({input,left:gap+i*(size+gap),top:gap}))).png().toFile(path.join(ROOT,name));
}

async function main(){
  fs.mkdirSync(RENDER,{recursive:true}); if(!fs.existsSync(maskPath)) await makeMask();
  const a=path.join(RENDER,'.render-a.png'),b=path.join(RENDER,'.render-b.png'); await renderOne(a);await renderOne(b);const A=fs.readFileSync(a),B=fs.readFileSync(b);if(sha(A)!==sha(B))throw new Error('NON_DETERMINISTIC_PNG');
  const ar=await sharp(A).raw().toBuffer(),br=await sharp(B).raw().toBuffer();let mismatch=0,maxDelta=0;for(let i=0;i<ar.length;i++){const d=Math.abs(ar[i]-br[i]);if(d)mismatch++;if(d>maxDelta)maxDelta=d;}fs.copyFileSync(a,outPath);fs.unlinkSync(a);fs.unlinkSync(b);
  const m=await metrics(outPath);writeJson(path.join(ROOT,'style-metrics.json'),m);
  await contact(200,'family-contact-200.png');await contact(80,'family-contact-80.png');await contact(32,'family-contact-32.png');
  const cand=await sharp(outPath).resize(420,420,{fit:'contain'}).png().toBuffer();const alpha=await sharp(outPath).ensureAlpha().extractChannel(3).joinChannel([await sharp(outPath).extractChannel(3).toBuffer(),await sharp(outPath).extractChannel(3).toBuffer()]).png().toBuffer();
  await sharp({create:{width:900,height:450,channels:4,background:'#ffffff'}}).composite([{input:cand,left:15,top:15},{input:cand,left:465,top:15},{input:alpha,left:240,top:15}]).png().toFile(path.join(ROOT,'alpha-contact.png'));
  const versions={node:process.version,sharp:sharp.versions.sharp,vips:sharp.versions.vips,rsvg:'2.62.91',png:sharp.versions.png};const normalizedCommand=['node','source/render-r3.mjs'];
  writeJson(path.join(ROOT,'render-manifest.json'),{schema:1,bundle:'codex-primary-runtime/dependencies',versions,locale:'C',timezone:'UTC',concurrency:1,network:false,fontDiscovery:false,random:false,canvas:[1254,1254],colorSpace:'sRGB',alpha:'straight',png:{compressionLevel:9,adaptiveFiltering:false,interlace:false},sourceSha:sha(fs.readFileSync(svgPath)),maskSha:sha(fs.readFileSync(maskPath)),rendererSha:sha(fs.readFileSync(new URL(import.meta.url))),normalizedCommand,commandDigest:sha(Buffer.from(JSON.stringify(normalizedCommand)))});
  writeJson(path.join(ROOT,'reproduction.json'),{renderASha:sha(A),renderBSha:sha(B),byteIdentical:true,decodedRgbaMismatch:mismatch,maxChannelDelta:maxDelta,outputSha:sha(fs.readFileSync(outPath))});
}
main().catch(e=>{console.error(e);process.exit(1)});
