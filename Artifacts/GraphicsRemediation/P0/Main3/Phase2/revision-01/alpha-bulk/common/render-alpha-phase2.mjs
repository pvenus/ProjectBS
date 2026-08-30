import {createRequire} from 'module';
const require=createRequire(import.meta.url);
const sharp=require('sharp');
import fs from 'fs';
import path from 'path';
import crypto from 'crypto';

sharp.concurrency(1);
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');

function solve(A,b){
  const n=b.length,M=A.map((r,i)=>[...r,b[i]]);
  for(let i=0;i<n;i++){
    let q=i;for(let j=i+1;j<n;j++)if(Math.abs(M[j][i])>Math.abs(M[q][i]))q=j;
    [M[i],M[q]]=[M[q],M[i]];const d=M[i][i];if(Math.abs(d)<1e-12)throw Error('singular');
    for(let k=i;k<=n;k++)M[i][k]/=d;
    for(let j=0;j<n;j++)if(j!==i){const f=M[j][i];for(let k=i;k<=n;k++)M[j][k]-=f*M[i][k];}
  }
  return M.map(r=>r[n]);
}
function fit(samples,ch){
 let active=samples;
 for(let iter=0;iter<3;iter++){
  const A=Array.from({length:6},()=>Array(6).fill(0)),b=Array(6).fill(0);
  for(const s of active){const f=[1,s.x,s.y,s.x*s.x,s.x*s.y,s.y*s.y],v=s.rgb[ch];for(let i=0;i<6;i++){b[i]+=f[i]*v;for(let j=0;j<6;j++)A[i][j]+=f[i]*f[j];}}
  const c=solve(A,b); if(iter===2)return c;
  active=active.filter(s=>{const f=[1,s.x,s.y,s.x*s.x,s.x*s.y,s.y*s.y];const p=f.reduce((z,v,i)=>z+v*c[i],0);return Math.abs(s.rgb[ch]-p)<=12;});
 }
}
const pred=(c,x,y)=>c[0]+c[1]*x+c[2]*y+c[3]*x*x+c[4]*x*y+c[5]*y*y;
const smooth=(v,a,b)=>{let t=Math.max(0,Math.min(1,(v-a)/(b-a)));return t*t*(3-2*t)};

async function render(member,src,outRoot,scale){
 fs.mkdirSync(outRoot,{recursive:true});
 const {data,info}=await sharp(src).removeAlpha().raw().toBuffer({resolveWithObject:true});
 const W=info.width,H=info.height,samples=[];
 for(let y=0;y<H;y+=2)for(let x=0;x<W;x+=2)if(x<48||x>=W-48||y<48||y>=H-48){const i=(y*W+x)*3;samples.push({x:x/(W-1),y:y/(H-1),rgb:[data[i],data[i+1],data[i+2]]});}
 const cf=[fit(samples,0),fit(samples,1),fit(samples,2)], rgba=Buffer.alloc(W*H*4),mask=Buffer.alloc(W*H),trimap=Buffer.alloc(W*H),identity=Buffer.alloc(W*H*4);
 let partial=0,opaque=0,residue=0,minX=W,minY=H,maxX=-1,maxY=-1,opaqueMismatch=0;
 for(let y=0;y<H;y++)for(let x=0;x<W;x++){
  const n=y*W+x,i=n*3,o=n*4,nx=x/(W-1),ny=y/(H-1),bg=cf.map(c=>pred(c,nx,ny));
  const rgb=[data[i],data[i+1],data[i+2]],dr=rgb[0]-bg[0],dg=rgb[1]-bg[1],db=rgb[2]-bg[2];
  const dist=Math.sqrt(dr*dr+dg*dg+db*db),lum=Math.abs(.2126*dr+.7152*dg+.0722*db),a=Math.round(smooth(Math.max(dist,1.35*lum),30,72)*255);
  mask[n]=a;trimap[n]=a<=8?0:a>=245?255:128;
  if(a===0){rgba[o]=rgba[o+1]=rgba[o+2]=rgba[o+3]=0;}
  else {const af=a/255;for(let c=0;c<3;c++)rgba[o+c]=a===255?rgb[c]:Math.max(0,Math.min(255,Math.round((rgb[c]-bg[c]*(1-af))/af)));rgba[o+3]=a;if(a===255){opaque++;if(rgba[o]!==rgb[0]||rgba[o+1]!==rgb[1]||rgba[o+2]!==rgb[2])opaqueMismatch++;}else partial++;if(a>=16){minX=Math.min(minX,x);maxX=Math.max(maxX,x);minY=Math.min(minY,y);maxY=Math.max(maxY,y);}}
  const m=a===255&&rgba[o]===rgb[0]&&rgba[o+1]===rgb[1]&&rgba[o+2]===rgb[2]?0:255;identity[o]=identity[o+1]=identity[o+2]=m;identity[o+3]=255;
 }
 const rawMaster=await sharp(rgba,{raw:{width:W,height:H,channels:4}}).png({compressionLevel:9}).toBuffer();
 const rawMask=await sharp(mask,{raw:{width:W,height:H,channels:1}}).png({compressionLevel:9}).toBuffer();
 const rawTrimap=await sharp(trimap,{raw:{width:W,height:H,channels:1}}).png({compressionLevel:9}).toBuffer();
 const target=Math.round(W*scale),off=Math.floor((W-target)/2);
 const transformed=scale===1?rawMaster:await sharp(rawMaster).resize(target,target,{kernel:'lanczos3',premultiplied:true}).extend({top:off,bottom:W-target-off,left:off,right:W-target-off,background:{r:0,g:0,b:0,alpha:0}}).png({compressionLevel:9}).toBuffer();
 const runtime=await sharp(transformed).resize(512,512,{kernel:'lanczos3',premultiplied:true}).png({compressionLevel:9}).toBuffer();
 const p={mask:path.join(outRoot,'alpha-mask.r0.png'),trimap:path.join(outRoot,'trimap.r0.png'),master:path.join(outRoot,'master-1254.r0.png'),runtime:path.join(outRoot,'runtime-512.r0.png'),identity:path.join(outRoot,'foreground-rgb-identity.r0.png')};
 fs.writeFileSync(p.mask,rawMask);fs.writeFileSync(p.trimap,rawTrimap);fs.writeFileSync(p.master,transformed);fs.writeFileSync(p.runtime,runtime);await sharp(identity,{raw:{width:W,height:H,channels:4}}).png().toFile(p.identity);
 const bbox={minX,minY,maxX,maxY,widthPct:(maxX-minX+1)/W*100,heightPct:(maxY-minY+1)/H*100,marginsPct:{left:minX/W*100,right:(W-1-maxX)/W*100,top:minY/H*100,bottom:(H-1-maxY)/H*100}};
 const metrics={member,source:{path:src,sha256:sha(src),width:W,height:H},backgroundModel:'robust quadratic RGB surface from outer48px samples',alpha:'smoothstep(max(RGBdistance,1.35*lumaDistance),30,72); paper-grain calibrated, not visual-threshold relaxation',preTransform:{partialPixels:partial,opaquePixels:opaque,opaqueInternalRgbMismatch:opaqueMismatch,bboxAlphaGe16:bbox},normalization:{scale,translation:[0,0],rotation:0,nonuniform:false},final:{alphaZeroRgbResidue:residue},outputs:{mask:sha(p.mask),trimap:sha(p.trimap),master:sha(p.master),runtime512:sha(p.runtime),identity:sha(p.identity)}};
 fs.writeFileSync(path.join(outRoot,'metrics.r0.json'),JSON.stringify(metrics,null,2)+'\n');
 const rep={renderCount:1,deterministicReproductionPendingTechnicalAudit:true,masterSha256:sha(p.master),runtime512Sha256:sha(p.runtime)};fs.writeFileSync(path.join(outRoot,'reproduction.r0.json'),JSON.stringify(rep,null,2)+'\n');
 const auth={member,inputPath:src,inputSha256:sha(src),styleStatus:'STYLE_SELECTED',alphaStatus:'PRODUCTION_R0',transform:metrics.normalization};fs.writeFileSync(path.join(outRoot,'source-authority.json'),JSON.stringify(auth,null,2)+'\n');
 return metrics;
}

const root=process.argv[2];
const member=process.argv[3];
const source=process.argv[4];
if(!root||!member||!source) throw Error('usage: render-alpha-phase2.mjs OUT_ROOT MEMBER SOURCE');
const result=await render(member,source,path.join(root,member),1.0);
console.log(JSON.stringify(result,null,2));
