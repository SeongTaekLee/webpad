// 파일 하나짜리 웹패드 만들기: 글꼴과 아이콘을 HTML 안에 넣어 dist/webpad.html 로 저장한다.
// 인터넷 없이 더블클릭으로 열 수 있다.  node tools/build-single.js
const fs = require('fs');
const path = require('path');

const root = path.join(__dirname, '..');
let html = fs.readFileSync(path.join(root, 'index.html'), 'utf8');
const dataUri = (file, type) => `data:${type};base64,${fs.readFileSync(path.join(root, file)).toString('base64')}`;

function rep(from, to) {
  if (!html.includes(from)) throw new Error('missing: ' + from);
  html = html.split(from).join(to);
}
rep("url('fonts/NanumGothicCoding-R.woff2')", `url('${dataUri('fonts/NanumGothicCoding-R.woff2', 'font/woff2')}')`);
rep("url('fonts/NanumGothicCoding-B.woff2')", `url('${dataUri('fonts/NanumGothicCoding-B.woff2', 'font/woff2')}')`);
rep('href="icons/icon.svg"', `href="${dataUri('icons/icon.svg', 'image/svg+xml')}"`);
// 보안 환경용: 외부로 나가는 통신을 브라우저가 막도록 CSP를 박는다 (파일 안의 스크립트·스타일·글꼴만 허용)
html = html.replace(/<meta http-equiv="Content-Security-Policy"[^>]*>/, `<meta http-equiv="Content-Security-Policy" content="default-src 'none'; script-src 'unsafe-inline'; style-src 'unsafe-inline'; font-src data:; img-src data: blob:; connect-src 'none'; form-action 'none'; base-uri 'none'">`);
if (!html.includes("connect-src 'none'")) throw new Error('CSP not replaced');
// 파일 버전에서는 exe 받기 링크(외부 사이트)를 숨긴다
rep('<button data-cmd="getExe">실행 파일(.exe) 받기</button>', '');

fs.mkdirSync(path.join(root, 'dist'), { recursive: true });
const out = path.join(root, 'dist', 'webpad.html');
fs.writeFileSync(out, html);
console.log(out, (fs.statSync(out).size / 1024 / 1024).toFixed(2) + 'MB');
