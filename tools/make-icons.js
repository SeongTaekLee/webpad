// 아이콘 생성: 외부 라이브러리 없이 PNG/ICO/SVG를 만든다.  node tools/make-icons.js
const fs = require('fs');
const zlib = require('zlib');
const path = require('path');

const root = path.join(__dirname, '..');

// 0~1 좌표에서 둥근 사각형 안쪽까지의 거리(음수면 안쪽)
function roundRect(x, y, x0, y0, x1, y1, r) {
  const cx = Math.max(x0 + r, Math.min(x, x1 - r));
  const cy = Math.max(y0 + r, Math.min(y, y1 - r));
  return Math.hypot(x - cx, y - cy) - r;
}
function shade(x, y) {
  // 바탕: 파란 둥근 사각형
  if (roundRect(x, y, 0.04, 0.04, 0.96, 0.96, 0.2) > 0) return null;
  let c = [37 + (1 - y) * 30, 99 + (1 - y) * 30, 235, 255];
  // 종이
  if (roundRect(x, y, 0.24, 0.18, 0.76, 0.82, 0.06) <= 0) {
    c = [255, 255, 255, 255];
    // 글줄
    const lines = [[0.32, 0.68, 0.33], [0.32, 0.6, 0.45], [0.32, 0.66, 0.57], [0.32, 0.5, 0.69]];
    for (const [a, b, ly] of lines) {
      if (x >= a && x <= b && Math.abs(y - ly) <= 0.028) c = [150, 175, 230, 255];
    }
    // 커서
    if (x >= 0.53 && x <= 0.565 && y >= 0.64 && y <= 0.74) c = [255, 140, 0, 255];
  }
  return c;
}
function render(size) {
  const ss = 4, buf = Buffer.alloc(size * size * 4);
  for (let py = 0; py < size; py++) for (let px = 0; px < size; px++) {
    let r = 0, g = 0, b = 0, a = 0;
    for (let sy = 0; sy < ss; sy++) for (let sx = 0; sx < ss; sx++) {
      const c = shade((px + (sx + 0.5) / ss) / size, (py + (sy + 0.5) / ss) / size);
      if (c) { r += c[0] * c[3]; g += c[1] * c[3]; b += c[2] * c[3]; a += c[3]; }
    }
    const i = (py * size + px) * 4;
    if (a) { buf[i] = r / a; buf[i + 1] = g / a; buf[i + 2] = b / a; }
    buf[i + 3] = a / (ss * ss);
  }
  return buf;
}
const crcTable = new Int32Array(256).map((_, n) => { let c = n; for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1; return c; });
function crc32(b) { let c = -1; for (const x of b) c = crcTable[(c ^ x) & 255] ^ (c >>> 8); return (c ^ -1) >>> 0; }
function chunk(type, data) {
  const len = Buffer.alloc(4); len.writeUInt32BE(data.length);
  const td = Buffer.concat([Buffer.from(type), data]);
  const crc = Buffer.alloc(4); crc.writeUInt32BE(crc32(td));
  return Buffer.concat([len, td, crc]);
}
function png(size) {
  const raw = render(size), rows = Buffer.alloc(size * (size * 4 + 1));
  for (let y = 0; y < size; y++) { rows[y * (size * 4 + 1)] = 0; raw.copy(rows, y * (size * 4 + 1) + 1, y * size * 4, (y + 1) * size * 4); }
  const ihdr = Buffer.alloc(13); ihdr.writeUInt32BE(size, 0); ihdr.writeUInt32BE(size, 4); ihdr[8] = 8; ihdr[9] = 6;
  return Buffer.concat([Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]), chunk('IHDR', ihdr), chunk('IDAT', zlib.deflateSync(rows, { level: 9 })), chunk('IEND', Buffer.alloc(0))]);
}
function ico(sizes) {
  const imgs = sizes.map(png), head = Buffer.alloc(6 + 16 * sizes.length);
  head.writeUInt16LE(1, 2); head.writeUInt16LE(sizes.length, 4);
  let off = head.length;
  sizes.forEach((s, i) => {
    const e = 6 + i * 16;
    head[e] = s >= 256 ? 0 : s; head[e + 1] = s >= 256 ? 0 : s; head.writeUInt16LE(1, e + 4); head.writeUInt16LE(32, e + 6);
    head.writeUInt32LE(imgs[i].length, e + 8); head.writeUInt32LE(off, e + 12); off += imgs[i].length;
  });
  return Buffer.concat([head, ...imgs]);
}
const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><defs><linearGradient id="g" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#4385ff"/><stop offset="1" stop-color="#2563eb"/></linearGradient></defs><rect x="4" y="4" width="92" height="92" rx="20" fill="url(#g)"/><rect x="24" y="18" width="52" height="64" rx="6" fill="#fff"/><g fill="#96afe6"><rect x="32" y="30.2" width="36" height="5.6" rx="2.8"/><rect x="32" y="42.2" width="28" height="5.6" rx="2.8"/><rect x="32" y="54.2" width="34" height="5.6" rx="2.8"/><rect x="32" y="66.2" width="18" height="5.6" rx="2.8"/></g><rect x="53" y="64" width="3.5" height="10" fill="#ff8c00"/></svg>`;

fs.mkdirSync(path.join(root, 'icons'), { recursive: true });
fs.mkdirSync(path.join(root, 'launcher'), { recursive: true });
fs.writeFileSync(path.join(root, 'icons', 'icon.svg'), svg);
fs.writeFileSync(path.join(root, 'icons', 'icon-192.png'), png(192));
fs.writeFileSync(path.join(root, 'icons', 'icon-512.png'), png(512));
fs.writeFileSync(path.join(root, 'launcher', 'webpad.ico'), ico([16, 24, 32, 48, 64, 256]));
console.log('icons written');
