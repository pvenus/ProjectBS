import { createRequire } from 'module';
import fs from 'fs';
import path from 'path';
import crypto from 'crypto';

const require = createRequire(import.meta.url);
const sharp = require('sharp');
sharp.concurrency(1);

const sha256 = p => crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const root = process.argv[2];
const source = process.argv[3];
if (!root || !source) throw new Error('usage: repair-cannon-authority.mjs ROOT SOURCE');

const master = path.join(root, 'master-1254.r1.png');
const priorRuntime = path.join(root, 'runtime-512.r1.png');
const output = path.join(root, 'runtime-512.reproduction-r2.png');
const tempA = path.join(root, 'runtime-512.reproduction-r2.A.tmp.png');
const tempB = path.join(root, 'runtime-512.reproduction-r2.B.tmp.png');

const resize = async target => {
  await sharp(master)
    .pipelineColourspace('scrgb')
    .resize(512, 512, { kernel: 'lanczos3', premultiplied: true })
    .toColourspace('srgb')
    .ensureAlpha()
    .png({ compressionLevel: 9, adaptiveFiltering: false, interlace: false })
    .toFile(target);
};

await resize(tempA);
await resize(tempB);
if (sha256(tempA) !== sha256(tempB)) throw new Error('A/B encoded identity mismatch');
fs.copyFileSync(tempA, output);

const decodedA = await sharp(tempA).ensureAlpha().raw().toBuffer();
const decodedB = await sharp(tempB).ensureAlpha().raw().toBuffer();
if (!decodedA.equals(decodedB)) throw new Error('A/B decoded RGBA mismatch');

const { data, info } = await sharp(master).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
let partial = 0, opaque = 0, residue = 0, minX = info.width, minY = info.height, maxX = -1, maxY = -1;
for (let y = 0; y < info.height; y++) for (let x = 0; x < info.width; x++) {
  const i = (y * info.width + x) * 4, a = data[i + 3];
  if (a === 0 && (data[i] || data[i + 1] || data[i + 2])) residue++;
  if (a > 0 && a < 255) partial++;
  if (a === 255) opaque++;
  if (a >= 16) { minX = Math.min(minX, x); minY = Math.min(minY, y); maxX = Math.max(maxX, x); maxY = Math.max(maxY, y); }
}
const corners = [[0,0],[info.width-1,0],[0,info.height-1],[info.width-1,info.height-1]].map(([x,y]) => data[(y*info.width+x)*4+3]);
const bbox = { minX, minY, maxX, maxY, widthPct: (maxX-minX+1)/info.width*100, heightPct: (maxY-minY+1)/info.height*100,
  marginsPct: { left:minX/info.width*100, right:(info.width-1-maxX)/info.width*100, top:minY/info.height*100, bottom:(info.height-1-maxY)/info.height*100 } };

const versions = { node: process.version, sharp: sharp.versions.sharp, libvips: sharp.versions.vips };
const authority = {
  member: 'seojin-cannon-G3-correction-r2-ready', status: 'VISUAL_SELECTED_ALPHA_READY_NOT_PROMOTED',
  visualSource: { path: source, sha256: sha256(source) },
  acceptedMaster: { path: master, sha256: sha256(master), width: info.width, height: info.height, channels: info.channels },
  extractionAndNormalization: {
    authority: 'existing accepted r1 output; no rerender or visual modification in this repair',
    foregroundDesignIdentity: 'selected-B muzzle, diagonal fall axis, off-center impact, asymmetric rupture scars and irregular residual patches are unchanged; no repaint, crop, rotation, nonuniform transform, or geometry edit',
    alphaExtraction: 'asset-specific matte separation recorded by render-alpha-cannon-r2.mjs',
    transform: { uniformScalePercent: 96.012759, translationPercent: [0,0], rotationDegrees: 0, nonuniformScale: false, crop: false },
    placement: 'centered on unchanged 1254x1254 canvas'
  },
  masterMetrics: { partialPixels: partial, opaquePixels: opaque, alphaZeroRgbResidue: residue, cornerAlpha: corners, bboxAlphaGe16: bbox },
  runtime512: {
    priorStored: { path: priorRuntime, sha256: sha256(priorRuntime), disposition: 'provenance-only; deterministic authority superseded' },
    acceptedReproduction: { path: output, sha256: sha256(output), width: 512, height: 512, format: 'PNG RGBA8 sRGB' },
    algorithm: { decoder: 'sharp/libvips', source: 'acceptedMaster', resize: [512,512], kernel: 'lanczos3', premultipliedAlpha: true, sourceColorspace: 'sRGB', workingColorspace: 'scRGB linear-light', targetColorspace: 'sRGB', alphaZeroRgbPolicy: 'zero', edgeMode: 'libvips extend copy/default; no canvas extension during 512 resize', output: { format:'PNG', channels:'RGBA8', compressionLevel:9, adaptiveFiltering:false, filter:'none', interlace:false, profile:'none embedded', ancillaryChunkPolicy:'sharp/libvips deterministic defaults; no custom text/time chunks' } },
    commandDigestInput: "sharp(master).pipelineColourspace('scrgb').resize(512,512,{kernel:'lanczos3',premultiplied:true}).toColourspace('srgb').ensureAlpha().png({compressionLevel:9,adaptiveFiltering:false,interlace:false})",
    renderA: sha256(tempA), renderB: sha256(tempB), encodedIdentity: sha256(tempA) === sha256(tempB), decodedRgbaMismatch: 0
  },
  versions,
  forbiddenWrites: { canonical: 0, meta: 0, guid: 0, staging: 0, unity: 0 }
};
fs.writeFileSync(path.join(root, 'source-authority.r2.json'), JSON.stringify(authority, null, 2) + '\n');
fs.writeFileSync(path.join(root, 'metrics.r2.json'), JSON.stringify(authority.masterMetrics, null, 2) + '\n');
fs.writeFileSync(path.join(root, 'reproduction.r2.json'), JSON.stringify(authority.runtime512, null, 2) + '\n');
console.log(JSON.stringify({ output, outputSha256: sha256(output), manifest: path.join(root, 'source-authority.r2.json'), versions }, null, 2));
