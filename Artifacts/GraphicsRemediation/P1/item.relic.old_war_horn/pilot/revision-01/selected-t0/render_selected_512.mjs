import sharp from '/Users/pvenus/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp/dist/index.mjs';
import fs from 'node:fs/promises';
import crypto from 'node:crypto';
import path from 'node:path';

const project='/Users/pvenus/ProjectBS';
const src=path.join(project,'Artifacts/GraphicsRemediation/P1/item.relic.old_war_horn/pilot/revision-01/selected/item.relic.old_war_horn.icon.selected-reframe-B.png');
const outDir=path.join(project,'Artifacts/GraphicsRemediation/P1/item.relic.old_war_horn/pilot/revision-01/selected-t0/pilot-512');
const out=path.join(outDir,'item.relic.old_war_horn.icon.selected-reframe-B-512.png');
const tempA='/private/tmp/projectbs-art-p1-old-war-horn-512-render-a.png';
const tempB='/private/tmp/projectbs-art-p1-old-war-horn-512-render-b.png';
const sha=async p=>crypto.createHash('sha256').update(await fs.readFile(p)).digest('hex');
const sinc=x=>x===0?1:Math.sin(Math.PI*x)/(Math.PI*x);
const lanczos=(x,a=3)=>Math.abs(x)<a?sinc(x)*sinc(x/a):0;
const srgbToLinear=v=>{v/=255;return v<=0.04045?v/12.92:Math.pow((v+0.055)/1.055,2.4)};
const linearToSrgb=v=>{v=Math.max(0,Math.min(1,v));const s=v<=0.0031308?12.92*v:1.055*Math.pow(v,1/2.4)-0.055;return Math.max(0,Math.min(255,Math.round(s*255)))};
function weights(srcN,dstN){
  const scale=srcN/dstN, radius=3*scale, all=[];
  for(let d=0;d<dstN;d++){
    const center=(d+0.5)*scale-0.5, left=Math.ceil(center-radius), right=Math.floor(center+radius), row=[];let sum=0;
    for(let s=left;s<=right;s++){const clamped=Math.max(0,Math.min(srcN-1,s));const w=lanczos((center-s)/scale)/scale;row.push([clamped,w]);sum+=w;}
    all.push(row.map(([s,w])=>[s,w/sum]));
  } return all;
}
async function render(dest){
  const {data,info}=await sharp(src).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const sw=info.width,sh=info.height,dw=512,dh=512;
  if(sw!==1254||sh!==1254||info.channels!==4)throw new Error(`source ${sw}x${sh} c${info.channels}`);
  const wx=weights(sw,dw),wy=weights(sh,dh),tmp=new Float32Array(dw*sh*4),dst=new Float32Array(dw*dh*4);
  for(let y=0;y<sh;y++)for(let x=0;x<dw;x++){
    let r=0,g=0,b=0,a=0;
    for(const [sx,w] of wx[x]){const i=(y*sw+sx)*4, af=data[i+3]/255;a+=af*w;r+=srgbToLinear(data[i])*af*w;g+=srgbToLinear(data[i+1])*af*w;b+=srgbToLinear(data[i+2])*af*w;}
    const o=(y*dw+x)*4;tmp[o]=r;tmp[o+1]=g;tmp[o+2]=b;tmp[o+3]=a;
  }
  for(let y=0;y<dh;y++)for(let x=0;x<dw;x++){
    let r=0,g=0,b=0,a=0;
    for(const [sy,w] of wy[y]){const i=(sy*dw+x)*4;r+=tmp[i]*w;g+=tmp[i+1]*w;b+=tmp[i+2]*w;a+=tmp[i+3]*w;}
    const o=(y*dw+x)*4;dst[o]=r;dst[o+1]=g;dst[o+2]=b;dst[o+3]=a;
  }
  const rgba=Buffer.alloc(dw*dh*4);
  for(let p=0;p<dw*dh;p++){
    const i=p*4, a=Math.max(0,Math.min(1,dst[i+3])), aq=Math.round(a*255);
    if(aq===0){rgba[i]=rgba[i+1]=rgba[i+2]=rgba[i+3]=0;continue;}
    rgba[i]=linearToSrgb(dst[i]/a);rgba[i+1]=linearToSrgb(dst[i+1]/a);rgba[i+2]=linearToSrgb(dst[i+2]/a);rgba[i+3]=aq;
  }
  await sharp(rgba,{raw:{width:dw,height:dh,channels:4}}).png({compressionLevel:9}).toFile(dest);
}
await render(tempA); await render(tempB);
const shaA=await sha(tempA),shaB=await sha(tempB);
if(shaA!==shaB)throw new Error(`render SHA mismatch ${shaA} ${shaB}`);
const aRaw=await sharp(tempA).ensureAlpha().raw().toBuffer(),bRaw=await sharp(tempB).ensureAlpha().raw().toBuffer();
let mismatch=0;for(let i=0;i<aRaw.length;i++)if(aRaw[i]!==bRaw[i])mismatch++;
if(mismatch!==0)throw new Error(`decoded mismatch ${mismatch}`);
await fs.mkdir(outDir,{recursive:true}); await fs.copyFile(tempA,out);

const sourceMetadata=await sharp(src).metadata();
const {data:sourceData,info:sourceInfo}=await sharp(src).ensureAlpha().raw().toBuffer({resolveWithObject:true});
let sourceResidue=0,sourceTransparent=0,sourcePartial=0,sourceOpaque=0,sourceMinX=1254,sourceMinY=1254,sourceMaxX=-1,sourceMaxY=-1,alphaMin=255,alphaMax=0;
for(let y=0;y<1254;y++)for(let x=0;x<1254;x++){const i=(y*1254+x)*4,a=sourceData[i+3];alphaMin=Math.min(alphaMin,a);alphaMax=Math.max(alphaMax,a);if(a===0){sourceTransparent++;if(sourceData[i]||sourceData[i+1]||sourceData[i+2])sourceResidue++;}if(a>0&&a<255)sourcePartial++;if(a===255)sourceOpaque++;if(a>=16){sourceMinX=Math.min(sourceMinX,x);sourceMinY=Math.min(sourceMinY,y);sourceMaxX=Math.max(sourceMaxX,x);sourceMaxY=Math.max(sourceMaxY,y)}}
const sourceBBoxW=sourceMaxX-sourceMinX+1,sourceBBoxH=sourceMaxY-sourceMinY+1;
const sourceMetrics={width:sourceInfo.width,height:sourceInfo.height,channels:sourceInfo.channels,format:sourceMetadata.format,space:sourceMetadata.space,hasAlpha:sourceMetadata.hasAlpha,depth:sourceMetadata.depth,alphaMin,alphaMax,transparent:sourceTransparent,partial:sourcePartial,opaque:sourceOpaque,residue:sourceResidue,corners:[sourceData[3],sourceData[(1253)*4+3],sourceData[(1253*1254)*4+3],sourceData[(1254*1254-1)*4+3]],bbox:{minX:sourceMinX,minY:sourceMinY,maxX:sourceMaxX,maxY:sourceMaxY,width:sourceBBoxW,height:sourceBBoxH,widthPct:sourceBBoxW/1254*100,heightPct:sourceBBoxH/1254*100,marginsPx:{left:sourceMinX,right:1253-sourceMaxX,top:sourceMinY,bottom:1253-sourceMaxY},marginsPct:{left:sourceMinX/1254*100,right:(1253-sourceMaxX)/1254*100,top:sourceMinY/1254*100,bottom:(1253-sourceMaxY)/1254*100}},blank:sourceOpaque+sourcePartial===0};

const {data,info}=await sharp(out).ensureAlpha().raw().toBuffer({resolveWithObject:true});
let residue=0,transparent=0,partial=0,opaque=0,minAlpha=255,maxAlpha=0,minX=512,minY=512,maxX=-1,maxY=-1;
for(let y=0;y<512;y++)for(let x=0;x<512;x++){const i=(y*512+x)*4,a=data[i+3];minAlpha=Math.min(minAlpha,a);maxAlpha=Math.max(maxAlpha,a);if(a===0){transparent++;if(data[i]||data[i+1]||data[i+2])residue++;}if(a>0&&a<255)partial++;if(a===255)opaque++;if(a>=16){minX=Math.min(minX,x);minY=Math.min(minY,y);maxX=Math.max(maxX,x);maxY=Math.max(maxY,y)}}
const bboxW=maxX-minX+1,bboxH=maxY-minY+1;
const outputMetadata=await sharp(out).metadata();
const metrics={sourcePath:path.relative(project,src),sourceSha256:await sha(src),source:sourceMetrics,outputPath:path.relative(project,out),outputSha256:await sha(out),tool:`node ${process.version}; sharp ${sharp.versions.sharp}; libvips ${sharp.versions.vips}; custom separable Lanczos3`,commandDigest:crypto.createHash('sha256').update('sRGB->linear; premultiply; separable downsample Lanczos3 radius3*scale; unpremultiply; sRGB RGBA8; alpha0 RGB zero').digest('hex'),renderA:{path:tempA,sha256:shaA},renderB:{path:tempB,sha256:shaB},byteShaIdentity:shaA===shaB,decodedRgbaMismatch:mismatch,output:{width:info.width,height:info.height,channels:info.channels,format:outputMetadata.format,space:outputMetadata.space,hasAlpha:outputMetadata.hasAlpha,depth:outputMetadata.depth,alphaMin:minAlpha,alphaMax:maxAlpha,transparent,residue,partial,opaque,corners:[data[3],data[(511)*4+3],data[(511*512)*4+3],data[(512*512-1)*4+3]],bbox:{minX,minY,maxX,maxY,width:bboxW,height:bboxH,widthPct:bboxW/512*100,heightPct:bboxH/512*100,marginsPx:{left:minX,right:511-maxX,top:minY,bottom:511-maxY},marginsPct:{left:minX/512*100,right:(511-maxX)/512*100,top:minY/512*100,bottom:(511-maxY)/512*100}}}};
await fs.writeFile(path.join(path.dirname(outDir),'metrics.json'),JSON.stringify(metrics,null,2)+'\n');
