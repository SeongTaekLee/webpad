// 포맷터 회귀 테스트: index.html 안의 SQL·XML·ABAP 포맷 함수를 그대로 꺼내 tests/format 의 입력을 돌리고 기대 출력과 비교한다.
//   node tools/test-format.js            비교 (다르면 실패)
//   node tools/test-format.js --update   기대 출력(*.expected.*)을 지금 결과로 다시 쓴다
//   node tools/test-format.js --show 이름 한 사례의 결과를 출력한다
// 사례 하나 = tests/format/<이름>.<확장자> (입력) + <이름>.expected.<확장자> (기대 출력).
// 확장자가 sql → sqlFormat, xml → xmlFormat (2칸), min.xml → xmlMinifyText, abap → abapFormat.
// 입력 첫 줄이 "#error" 로 시작하면 오류가 나야 하는 사례이고, 기대 출력은 오류 메시지다.
'use strict';
const fs = require('fs');
const path = require('path');

const root = path.join(__dirname, '..');
const html = fs.readFileSync(path.join(root, 'index.html'), 'utf8');

function between(startMark, endMark) {
  const s = html.indexOf(startMark);
  if (s < 0) throw new Error('index.html 에서 찾지 못함: ' + startMark);
  const e = html.indexOf(endMark, s);
  if (e < 0) throw new Error('index.html 에서 찾지 못함: ' + endMark);
  return html.slice(s, e);
}
function line(mark) {
  const s = html.indexOf(mark);
  if (s < 0) throw new Error('index.html 에서 찾지 못함: ' + mark);
  return html.slice(s, html.indexOf('\n', s));
}

const src = [
  line('function wordSet('),
  between('var OPS=', 'function loadUdlSpecs('),
  'Object.keys(SPECS).forEach(function(id){ LANGS[id]=compileLang(id, SPECS[id]); });',
  line('function isWideCode('),
  between('// ---------- SQL: 강(river) 정렬', '// 포맷 적용:'),
  'return { sqlFormat: sqlFormat, xmlFormat: xmlFormat, xmlMinifyText: xmlMinifyText, abapFormat: abapFormat };',
].join('\n');
const F = new Function(src)();

function run(name, text) {
  if (/\.min\.xml$/.test(name)) return F.xmlMinifyText(text);
  if (/\.xml$/.test(name)) return F.xmlFormat(text, '  ');
  if (/\.sql$/.test(name)) return F.sqlFormat(text);
  if (/\.abap$/.test(name)) return F.abapFormat(text);
  throw new Error('모르는 확장자: ' + name);
}
function expectedName(name) {
  const m = /^(.*?)\.((?:min\.)?[a-z]+)$/.exec(name);
  return m[1] + '.expected.' + m[2];
}

const dir = path.join(root, 'tests', 'format');
const args = process.argv.slice(2);
const update = args.includes('--update');
const show = args.indexOf('--show') >= 0 ? (args[args.indexOf('--show') + 1] || '') : null;
const inputs = fs.readdirSync(dir).filter(f => !/\.expected\./.test(f)).sort();

let fail = 0, pass = 0;
for (const name of inputs) {
  if (show !== null && !name.startsWith(show)) continue;
  let text = fs.readFileSync(path.join(dir, name), 'utf8').replace(/\r\n?/g, '\n');
  let wantError = false;
  if (text.startsWith('#error')) { wantError = true; text = text.slice(text.indexOf('\n') + 1); }
  let out;
  try {
    out = run(name, text);
    if (wantError) { console.log('FAIL ' + name + ': 오류가 나야 하는데 정리됐어요'); fail++; continue; }
  } catch (e) {
    if (!wantError) { console.log('FAIL ' + name + ': ' + (e && e.message || e)); if (e && e.stack && !e.pos) console.log(e.stack); fail++; continue; }
    out = (e && e.message || String(e)) + (e && e.pos != null ? ' @' + e.pos : '');
  }
  if (show) { console.log('---- ' + name + '\n' + out); continue; }
  const expFile = path.join(dir, expectedName(name));
  if (update || !fs.existsSync(expFile)) { fs.writeFileSync(expFile, out); console.log('WROTE ' + expectedName(name)); pass++; continue; }
  const exp = fs.readFileSync(expFile, 'utf8').replace(/\r\n?/g, '\n');
  if (exp === out) { pass++; continue; }
  fail++;
  const a = exp.split('\n'), b = out.split('\n');
  let k = 0; while (k < a.length && k < b.length && a[k] === b[k]) k++;
  console.log('FAIL ' + name + ' (' + (k + 1) + '번째 줄부터 다름)');
  console.log('  기대: ' + JSON.stringify(a.slice(k, k + 3).join('\n')));
  console.log('  결과: ' + JSON.stringify(b.slice(k, k + 3).join('\n')));
}
if (show === null) console.log(pass + ' 통과, ' + fail + ' 실패');
process.exit(fail ? 1 : 0);
