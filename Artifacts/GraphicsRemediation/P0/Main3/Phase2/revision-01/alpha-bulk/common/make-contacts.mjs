import {createRequire} from 'module';
const require=createRequire(import.meta.url);
const sharp=require('sharp');
import fs from 'fs';
import path from 'path';

const root=process.argv[2];
const outRoot=process.argv[3];
const members=[
 ['Y basic G3','yujin-basic-G3/runtime-512.r0.png'],
 ['Y barrage G2','yujin-hwalbin-G2/runtime-512.r0.png'],
 ['Y barrage G3','yujin-hwalbin-G3/runtime-512.r0.png'],
 ['Y outlaw G3','yujin-outlaw-G3/runtime-512.r0.png'],
 ['J basic G1','jihan-basic-G1/runtime-512.r0.png'],
 ['J basic G2','jihan-basic-G2/runtime-512.r0.png'],
 ['J tonic G2','jihan-tonic-G2/runtime-512.r0.png'],
 ['J tonic G3','jihan-tonic-G3/runtime-512.r0.png'],
 ['J acupuncture G3','jihan-acupuncture-G3/runtime-512.r0.png'],
 ['S basic G3','seojin-basic-G3-r1/runtime-512.r0.png'],
 ['S indomitable G2','seojin-indomitable-G2/runtime-512.r0.png'],
 ['S indomitable G3','seojin-indomitable-G3/runtime-512.r0.png'],
 ['S turtle G3','seojin-turtle-G3/runtime-512.r0.png'],
 ['S cannon G3','seojin-cannon-G3-correction-r2-ready/runtime-512.r1.png']
];
fs.mkdirSync(outRoot,{recursive:true});
for(const size of [200,80,32]){
 const cell=size+20,cols=5,rows=Math.ceil(members.length/cols),composite=[];
 for(let i=0;i<members.length;i++){
   const png=await sharp(path.join(root,members[i][1])).resize(size,size,{kernel:'lanczos3',premultiplied:true}).png().toBuffer();
   composite.push({input:png,left:(i%cols)*cell+10,top:Math.floor(i/cols)*cell+10});
 }
 await sharp({create:{width:cols*cell,height:rows*cell,channels:4,background:{r:42,g:45,b:50,alpha:1}}}).composite(composite).png({compressionLevel:9}).toFile(path.join(outRoot,`main3-phase2-selected14-contact-${size}.png`));
}
{
 const size=32,cell=52,cols=5,rows=Math.ceil(members.length/cols),composite=[];
 for(let i=0;i<members.length;i++){
   const png=await sharp(path.join(root,members[i][1])).greyscale().resize(size,size,{kernel:'lanczos3',premultiplied:true}).png().toBuffer();
   composite.push({input:png,left:(i%cols)*cell+10,top:Math.floor(i/cols)*cell+10});
 }
 await sharp({create:{width:cols*cell,height:rows*cell,channels:4,background:{r:238,g:238,b:238,alpha:1}}}).composite(composite).png({compressionLevel:9}).toFile(path.join(outRoot,'main3-phase2-selected14-contact-32-grayscale.png'));
}
