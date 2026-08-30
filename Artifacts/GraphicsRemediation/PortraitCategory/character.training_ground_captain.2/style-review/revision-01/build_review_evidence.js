const sharp = require('sharp');
const fs = require('fs');
const path = require('path');

const root = __dirname;
const current = 'Assets/ImagesGenerated/Character/portrait/character.training_ground_captain.2.portrait.png';
const candidate = path.join(root, 'candidate-C-rgb-wash-simplified.png');
const peers = [
  ['Hut guard', 'Assets/ImagesGenerated/Character/portrait/character.hut_blocker_guard.2.portrait.png'],
  ['Mountain spear', 'Assets/ImagesGenerated/Character/portrait/character.mountain_fort_spearman.1.portrait.png'],
  ['Door shield', 'Assets/ImagesGenerated/Character/portrait/character.door_shield_barricader.1.portrait.png'],
  ['Child snatcher', 'Assets/ImagesGenerated/Character/portrait/character.child_snatcher_netman.1.portrait.png'],
];

async function raw(file) {
  return sharp(file).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
}

function metrics(x) {
  let n = 0, sum = 0, sum2 = 0, adjacent = 0, adjacentN = 0, dark = 0;
  const { data, info } = x;
  for (let y = 0; y < info.height; y++) for (let x0 = 0; x0 < info.width; x0++) {
    const i = (y * info.width + x0) * 4;
    if (!data[i + 3]) continue;
    const lum = 0.2126 * data[i] + 0.7152 * data[i + 1] + 0.0722 * data[i + 2];
    n++; sum += lum; sum2 += lum * lum; if (lum < 64) dark++;
    if (x0 + 1 < info.width) {
      const j = i + 4;
      if (data[j + 3]) { adjacent += Math.abs(lum - (0.2126 * data[j] + 0.7152 * data[j + 1] + 0.0722 * data[j + 2])); adjacentN++; }
    }
  }
  return { opaque_or_partial_pixels:n, luminance_mean:sum/n, luminance_sd:Math.sqrt(sum2/n-(sum/n)**2), mean_horizontal_edge_delta:adjacent/adjacentN, very_dark_ratio:dark/n };
}

async function contact() {
  const files = [['Current', current], ['Selected C', candidate], ...peers];
  const cw = 256, ch = 384, gap = 10, label = 30, rowH = ch + label;
  const W = gap + files.length * (cw + gap), H = gap + rowH * 2 + gap;
  const bg = await sharp({create:{width:W,height:H,channels:4,background:'#D8D5CC'}}).png().toBuffer();
  const comps = [];
  for (let i=0;i<files.length;i++) {
    const left = gap + i*(cw+gap);
    const color = await sharp(files[i][1]).resize(cw,ch,{fit:'contain'}).png().toBuffer();
    const gray = await sharp(files[i][1]).greyscale().resize(cw,ch,{fit:'contain'}).png().toBuffer();
    comps.push({input:color,left,top:label},{input:gray,left,top:rowH+label});
  }
  const svg = `<svg width="${W}" height="${H}" xmlns="http://www.w3.org/2000/svg"><style>text{font:17px sans-serif;fill:#222}</style>${files.map((f,i)=>`<text x="${gap+i*(cw+gap)}" y="22">${f[0]}</text>`).join('')}<text x="2" y="${rowH+22}" transform="rotate(-90 2 ${rowH+22})">GRAY</text></svg>`;
  comps.unshift({input:Buffer.from(svg),left:0,top:0});
  await sharp(bg).composite(comps).png({compressionLevel:9}).toFile(path.join(root,'captain2-selectedC-peer4-color-gray-contact.png'));
}

(async()=>{
  const a=await raw(current), b=await raw(candidate);
  if (a.info.width!==b.info.width || a.info.height!==b.info.height) throw new Error('dimension mismatch');
  let alphaMismatch=0, bboxA=[a.info.width,a.info.height,-1,-1], bboxB=[b.info.width,b.info.height,-1,-1];
  for(let i=0,p=0;i<a.data.length;i+=4,p++) {
    if(a.data[i+3]!==b.data[i+3]) alphaMismatch++;
    const x=p%a.info.width,y=Math.floor(p/a.info.width);
    if(a.data[i+3]) bboxA=[Math.min(bboxA[0],x),Math.min(bboxA[1],y),Math.max(bboxA[2],x),Math.max(bboxA[3],y)];
    if(b.data[i+3]) bboxB=[Math.min(bboxB[0],x),Math.min(bboxB[1],y),Math.max(bboxB[2],x),Math.max(bboxB[3],y)];
  }
  const report={dimensions:[a.info.width,a.info.height],alpha_mismatch_pixels:alphaMismatch,foreground_bbox_current:bboxA,foreground_bbox_selected:bboxB,current:metrics(a),selected_C:metrics(b)};
  report.delta={edge_delta_pct:(report.selected_C.mean_horizontal_edge_delta/report.current.mean_horizontal_edge_delta-1)*100,local_contrast_sd_pct:(report.selected_C.luminance_sd/report.current.luminance_sd-1)*100};
  fs.writeFileSync(path.join(root,'captain2-style-metrics.json'),JSON.stringify(report,null,2)+'\n');
  await contact();
})();
