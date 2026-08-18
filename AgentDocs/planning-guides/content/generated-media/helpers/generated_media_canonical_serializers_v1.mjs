import { createHash } from "node:crypto";

const PNG_SIGNATURE = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);
const GIF_HEADER = Buffer.from("GIF89a", "ascii");

export function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value && typeof value === "object") return `{${Object.keys(value).sort()
    .map((key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  return JSON.stringify(value);
}

export const sha256Hex = (bytes) => createHash("sha256").update(bytes).digest("hex");
export const settingsSha256 = (settings) => sha256Hex(canonicalJson(settings));

const assertInteger = (value, minimum, maximum, name) => {
  if (!Number.isInteger(value) || value < minimum || value > maximum)
    throw new Error(`invalid_${name}`);
};

const crcTable = Array.from({ length: 256 }, (_, value) => {
  let crc = value;
  for (let bit = 0; bit < 8; bit += 1)
    crc = (crc & 1) ? (0xedb88320 ^ (crc >>> 1)) : (crc >>> 1);
  return crc >>> 0;
});

const crc32 = (bytes) => {
  let crc = 0xffffffff;
  for (const byte of bytes) crc = crcTable[(crc ^ byte) & 0xff] ^ (crc >>> 8);
  return (crc ^ 0xffffffff) >>> 0;
};

const pngChunk = (type, data) => {
  const typeBytes = Buffer.from(type, "ascii");
  const length = Buffer.alloc(4);
  length.writeUInt32BE(data.length);
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(Buffer.concat([typeBytes, data])));
  return Buffer.concat([length, typeBytes, data, crc]);
};

const adler32 = (bytes) => {
  let a = 1;
  let b = 0;
  for (const byte of bytes) {
    a = (a + byte) % 65521;
    b = (b + a) % 65521;
  }
  return ((b << 16) | a) >>> 0;
};

const deflateStoredZlib = (bytes) => {
  const parts = [Buffer.from([0x78, 0x01])];
  for (let offset = 0; offset < bytes.length || offset === 0;) {
    const length = Math.min(65535, bytes.length - offset);
    const final = offset + length >= bytes.length;
    const header = Buffer.alloc(5);
    header[0] = final ? 1 : 0;
    header.writeUInt16LE(length, 1);
    header.writeUInt16LE((~length) & 0xffff, 3);
    parts.push(header, bytes.subarray(offset, offset + length));
    offset += length;
    if (final) break;
  }
  const checksum = Buffer.alloc(4);
  checksum.writeUInt32BE(adler32(bytes));
  parts.push(checksum);
  return Buffer.concat(parts);
};

const inflateStoredZlib = (bytes) => {
  if (bytes.length < 11 || bytes[0] !== 0x78 || bytes[1] !== 0x01)
    throw new Error("png_zlib_header_mismatch");
  let offset = 2;
  const parts = [];
  for (;;) {
    const header = bytes[offset];
    if (header !== 0 && header !== 1) throw new Error("png_deflate_mode_mismatch");
    offset += 1;
    const length = bytes.readUInt16LE(offset);
    const inverse = bytes.readUInt16LE(offset + 2);
    offset += 4;
    if (((~length) & 0xffff) !== inverse || offset + length > bytes.length - 4)
      throw new Error("png_deflate_block_invalid");
    parts.push(bytes.subarray(offset, offset + length));
    offset += length;
    if (header === 1) break;
  }
  if (offset !== bytes.length - 4) throw new Error("png_deflate_trailing_bytes");
  const result = Buffer.concat(parts);
  if (bytes.readUInt32BE(offset) !== adler32(result))
    throw new Error("png_adler32_mismatch");
  return result;
};

export const pngRgba8StoreSettings = (width, height) => ({
  schemaVersion: "generated_media_png_rgba8_store_settings_v1",
  serializerKey: "generated_media_png_rgba8_store_v1",
  serializerVersion: "1.0.0",
  width,
  height,
  pngSignature: "89504e470d0a1a0a",
  chunkOrder: ["IHDR", "IDAT", "IEND"],
  ancillaryChunks: [],
  bitDepth: 8,
  colorType: 6,
  compressionMethod: 0,
  filterMethod: 0,
  filterTypePerScanline: 0,
  interlaceMethod: 0,
  zlibHeader: "7801",
  deflatePolicy: "rfc1951_stored_blocks_max_65535_v1",
  idatChunkCount: 1,
  metadataPolicy: "none",
});

export function serializePngRgba8({ width, height, rgba }) {
  assertInteger(width, 1, 0x7fffffff, "png_width");
  assertInteger(height, 1, 0x7fffffff, "png_height");
  const pixels = Buffer.from(rgba);
  if (pixels.length !== width * height * 4) throw new Error("png_rgba_length_mismatch");
  const scanlines = Buffer.alloc(height * (1 + width * 4));
  for (let y = 0; y < height; y += 1)
    pixels.copy(scanlines, y * (1 + width * 4) + 1, y * width * 4, (y + 1) * width * 4);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0);
  ihdr.writeUInt32BE(height, 4);
  ihdr.set([8, 6, 0, 0, 0], 8);
  return Buffer.concat([PNG_SIGNATURE, pngChunk("IHDR", ihdr),
    pngChunk("IDAT", deflateStoredZlib(scanlines)), pngChunk("IEND", Buffer.alloc(0))]);
}

export function inspectCanonicalPngRgba8(bytes) {
  const png = Buffer.from(bytes);
  if (!png.subarray(0, 8).equals(PNG_SIGNATURE)) throw new Error("png_signature_mismatch");
  let offset = 8;
  const chunks = [];
  while (offset < png.length) {
    if (offset + 12 > png.length) throw new Error("png_chunk_truncated");
    const length = png.readUInt32BE(offset);
    const type = png.subarray(offset + 4, offset + 8).toString("ascii");
    const data = png.subarray(offset + 8, offset + 8 + length);
    const expected = png.readUInt32BE(offset + 8 + length);
    if (expected !== crc32(png.subarray(offset + 4, offset + 8 + length)))
      throw new Error("png_crc_mismatch");
    chunks.push({ type, data });
    offset += 12 + length;
  }
  if (canonicalJson(chunks.map(({ type }) => type)) !== canonicalJson(["IHDR", "IDAT", "IEND"]))
    throw new Error("png_chunk_order_mismatch");
  const ihdr = chunks[0].data;
  if (ihdr.length !== 13 || canonicalJson([...ihdr.subarray(8)])
    !== canonicalJson([8, 6, 0, 0, 0])) throw new Error("png_ihdr_mismatch");
  const width = ihdr.readUInt32BE(0);
  const height = ihdr.readUInt32BE(4);
  const scanlines = inflateStoredZlib(chunks[1].data);
  if (scanlines.length !== height * (1 + width * 4))
    throw new Error("png_scanline_length_mismatch");
  const rgba = Buffer.alloc(width * height * 4);
  for (let y = 0; y < height; y += 1) {
    const rowOffset = y * (1 + width * 4);
    if (scanlines[rowOffset] !== 0) throw new Error("png_filter_mismatch");
    scanlines.copy(rgba, y * width * 4, rowOffset + 1, rowOffset + 1 + width * 4);
  }
  return { width, height, rgba, settings: pngRgba8StoreSettings(width, height) };
}

const rgbKey = (rgb) => rgb.join(",");
const compareRgb = (left, right) => left[0] - right[0]
  || left[1] - right[1] || left[2] - right[2];
const uint16le = (value) => {
  const bytes = Buffer.alloc(2);
  bytes.writeUInt16LE(value);
  return bytes;
};

const packNineBitCodes = (codes) => {
  const output = [];
  let accumulator = 0;
  let bitCount = 0;
  for (const code of codes) {
    accumulator |= code << bitCount;
    bitCount += 9;
    while (bitCount >= 8) {
      output.push(accumulator & 0xff);
      accumulator >>>= 8;
      bitCount -= 8;
    }
  }
  if (bitCount) output.push(accumulator & 0xff);
  return Buffer.from(output);
};

const literalGifLzw = (indices) => {
  const codes = [];
  for (let offset = 0; offset < indices.length; offset += 250) {
    codes.push(256);
    for (const index of indices.subarray(offset, offset + 250)) codes.push(index);
  }
  codes.push(257);
  return packNineBitCodes(codes);
};

const gifSubBlocks = (bytes) => {
  const parts = [];
  for (let offset = 0; offset < bytes.length; offset += 255) {
    const part = bytes.subarray(offset, offset + 255);
    parts.push(Buffer.from([part.length]), part);
  }
  parts.push(Buffer.from([0]));
  return Buffer.concat(parts);
};

export const gif89aIndexedSettings = (width, height, delaysCentiseconds,
  transparentRgb) => ({
  schemaVersion: "generated_media_gif89a_indexed_settings_v1",
  serializerKey: "generated_media_gif89a_indexed_v1",
  serializerVersion: "1.0.0",
  width,
  height,
  frameCount: delaysCentiseconds.length,
  delaysCentiseconds: [...delaysCentiseconds],
  header: "GIF89a",
  globalColorTablePolicy:
    "index0_transparent_then_opaque_rgb_lexicographic_unique_then_zero_pad_256",
  globalColorTableEntries: 256,
  transparentRgb: [...transparentRgb],
  transparentColorIndex: 0,
  opaqueAlpha: 255,
  transparentAlpha: 0,
  colorResolutionBits: 8,
  sortFlag: false,
  backgroundColorIndex: 0,
  pixelAspectRatio: 0,
  disposalMethod: 1,
  userInputFlag: false,
  transparencyFlag: true,
  frameRectanglePolicy: "full_canvas_each_frame",
  localColorTables: false,
  interlaced: false,
  lzwMinimumCodeSize: 8,
  lzwPolicy: "literal_indices_clear_every_250_v1",
  imageDataSubBlockMaximum: 255,
  applicationExtensions: [],
  commentExtensions: [],
  plainTextExtensions: [],
  loopPolicy: "one_shot_no_application_extension",
  metadataPolicy: "none",
});

export function serializeGif89aIndexed({ width, height, framesRgba,
  delaysCentiseconds, transparentRgb }) {
  assertInteger(width, 1, 65535, "gif_width");
  assertInteger(height, 1, 65535, "gif_height");
  if (!Array.isArray(framesRgba) || framesRgba.length < 1
    || framesRgba.length !== delaysCentiseconds.length)
    throw new Error("gif_frame_count_mismatch");
  if (!Array.isArray(transparentRgb) || transparentRgb.length !== 3)
    throw new Error("gif_transparent_rgb_invalid");
  transparentRgb.forEach((value) => assertInteger(value, 0, 255, "gif_transparent_rgb"));
  delaysCentiseconds.forEach((value) => assertInteger(value, 1, 65535, "gif_delay"));
  const frames = framesRgba.map((frame) => Buffer.from(frame));
  for (const frame of frames)
    if (frame.length !== width * height * 4) throw new Error("gif_rgba_length_mismatch");
  const opaque = new Map();
  for (const frame of frames) {
    for (let offset = 0; offset < frame.length; offset += 4) {
      const rgb = [frame[offset], frame[offset + 1], frame[offset + 2]];
      const alpha = frame[offset + 3];
      if (alpha === 0) {
        if (compareRgb(rgb, transparentRgb) !== 0)
          throw new Error("gif_transparent_rgb_mismatch");
      } else if (alpha === 255) opaque.set(rgbKey(rgb), rgb);
      else throw new Error("gif_alpha_not_binary");
    }
  }
  const opaquePalette = [...opaque.values()].sort(compareRgb);
  if (opaquePalette.length > 255) throw new Error("gif_opaque_palette_overflow");
  const palette = [transparentRgb, ...opaquePalette];
  while (palette.length < 256) palette.push([0, 0, 0]);
  const paletteBytes = Buffer.from(palette.flat());
  const opaqueIndexes = new Map(opaquePalette.map((rgb, index) => [rgbKey(rgb), index + 1]));
  const logicalScreen = Buffer.concat([uint16le(width), uint16le(height),
    Buffer.from([0xf7, 0, 0]), paletteBytes]);
  const parts = [GIF_HEADER, logicalScreen];
  frames.forEach((frame, frameIndex) => {
    const indices = Buffer.alloc(width * height);
    for (let pixel = 0, offset = 0; offset < frame.length; pixel += 1, offset += 4)
      indices[pixel] = frame[offset + 3] === 0 ? 0
        : opaqueIndexes.get(rgbKey([frame[offset], frame[offset + 1], frame[offset + 2]]));
    const delay = delaysCentiseconds[frameIndex];
    parts.push(Buffer.from([0x21, 0xf9, 0x04, 0x05,
      delay & 0xff, delay >>> 8, 0x00, 0x00]));
    parts.push(Buffer.from([0x2c]), uint16le(0), uint16le(0),
      uint16le(width), uint16le(height), Buffer.from([0x00, 0x08]),
      gifSubBlocks(literalGifLzw(indices)));
  });
  parts.push(Buffer.from([0x3b]));
  return Buffer.concat(parts);
}

const readSubBlocks = (bytes, start) => {
  const parts = [];
  let offset = start;
  for (;;) {
    const length = bytes[offset];
    offset += 1;
    if (length === 0) break;
    if (offset + length > bytes.length) throw new Error("gif_subblock_truncated");
    parts.push(bytes.subarray(offset, offset + length));
    offset += length;
  }
  return { data: Buffer.concat(parts), offset };
};

const unpackCanonicalGifIndices = (bytes, expectedLength) => {
  const output = [];
  let accumulator = 0;
  let bitCount = 0;
  let offset = 0;
  let sinceClear = 251;
  let ended = false;
  while (offset < bytes.length || bitCount >= 9) {
    while (bitCount < 9 && offset < bytes.length) {
      accumulator |= bytes[offset] << bitCount;
      bitCount += 8;
      offset += 1;
    }
    if (bitCount < 9) break;
    const code = accumulator & 0x1ff;
    accumulator >>>= 9;
    bitCount -= 9;
    if (code === 256) {
      if (sinceClear > 250 && output.length !== 0) throw new Error("gif_lzw_clear_interval_mismatch");
      sinceClear = 0;
    } else if (code === 257) {
      ended = true;
      break;
    } else if (code <= 255 && sinceClear < 250) {
      output.push(code);
      sinceClear += 1;
    } else throw new Error("gif_lzw_policy_mismatch");
  }
  if (!ended || output.length !== expectedLength) throw new Error("gif_lzw_length_mismatch");
  return Buffer.from(output);
};

export function inspectCanonicalGif89a(bytes) {
  const gif = Buffer.from(bytes);
  if (!gif.subarray(0, 6).equals(GIF_HEADER)) throw new Error("gif_header_mismatch");
  const width = gif.readUInt16LE(6);
  const height = gif.readUInt16LE(8);
  if (gif[10] !== 0xf7 || gif[11] !== 0 || gif[12] !== 0)
    throw new Error("gif_logical_screen_mismatch");
  const paletteBytes = gif.subarray(13, 13 + 768);
  let offset = 13 + 768;
  const framesRgba = [];
  const delaysCentiseconds = [];
  while (gif[offset] !== 0x3b) {
    if (gif[offset] !== 0x21 || gif[offset + 1] !== 0xf9
      || gif[offset + 2] !== 4 || gif[offset + 3] !== 5
      || gif[offset + 6] !== 0 || gif[offset + 7] !== 0)
      throw new Error("gif_graphics_control_mismatch");
    delaysCentiseconds.push(gif.readUInt16LE(offset + 4));
    offset += 8;
    if (gif[offset] !== 0x2c || gif.readUInt16LE(offset + 1) !== 0
      || gif.readUInt16LE(offset + 3) !== 0
      || gif.readUInt16LE(offset + 5) !== width
      || gif.readUInt16LE(offset + 7) !== height || gif[offset + 9] !== 0
      || gif[offset + 10] !== 8) throw new Error("gif_image_descriptor_mismatch");
    const blocks = readSubBlocks(gif, offset + 11);
    const indices = unpackCanonicalGifIndices(blocks.data, width * height);
    const rgba = Buffer.alloc(width * height * 4);
    indices.forEach((index, pixel) => {
      rgba[pixel * 4] = paletteBytes[index * 3];
      rgba[pixel * 4 + 1] = paletteBytes[index * 3 + 1];
      rgba[pixel * 4 + 2] = paletteBytes[index * 3 + 2];
      rgba[pixel * 4 + 3] = index === 0 ? 0 : 255;
    });
    framesRgba.push(rgba);
    offset = blocks.offset;
  }
  if (offset !== gif.length - 1) throw new Error("gif_trailing_bytes");
  const transparentRgb = [...paletteBytes.subarray(0, 3)];
  return { width, height, framesRgba, delaysCentiseconds, transparentRgb,
    loopExtensionPresent: false,
    settings: gif89aIndexedSettings(width, height, delaysCentiseconds, transparentRgb) };
}
