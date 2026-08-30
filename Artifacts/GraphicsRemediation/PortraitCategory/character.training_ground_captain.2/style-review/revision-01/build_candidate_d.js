const sharp=require('sharp'),fs=require('fs'),path=require('path');
const root=__dirname;
const src=path.join(root,'candidate-C-rgb-wash-simplified.png');
 const out=path.join(root,'candidate-D2-face-ink-masses.png');
(async()=>{
 const a=await sharp(src).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const b=await sharp(src).ensureAlpha().median(3).raw().toBuffer({resolveWithObject:true});
 const d=Buffer.from(a.data),cx=163,cy=73,rx=33,ry=36;
 for(let y=0;y<a.info.height;y++)for(let x=0;x<a.info.width;x++){
  const q=Math.sqrt(((x-cx)/rx)**2+((y-cy)/ry)**2);
  if(q>=1)continue;
  const feather=Math.min(1,(1-q)/0.24),i=(y*a.info.width+x)*4;
  for(let c=0;c<3;c++){
   let v=0.42*a.data[i+c]+0.58*b.data[i+c];
   v=128+(v-128)*0.84;
   v=Math.round(v/(255/15))*(255/15);
   d[i+c]=Math.round(a.data[i+c]*(1-feather)+v*feather);
  }
 }
 await sharp(d,{raw:a.info}).png({compressionLevel:9,adaptiveFiltering:false}).toFile(out);
})();
