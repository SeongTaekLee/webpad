# 웹패드 (WebPad)

Notepad++를 쓸 수 없는 보안 환경을 위한 가벼운 코드 편집기예요. 설치가 필요 없어요.

세 가지 형태가 있어요. [Releases](https://github.com/SeongTaekLee/webpad/releases/latest)에서 받으세요.

| 형태 | 쓰는 법 | 이럴 때 |
|---|---|---|
| **`webpad.html`** (파일 하나, 1.5MB) | 더블클릭하면 엣지/크롬에서 열려요. 인터넷이 필요 없어요. | exe 실행이 막힌 PC, 외부 사이트가 막힌 망 |
| **`WebPad.exe`** (포터블, 2.5MB) | 아무 폴더에 두고 실행해요. 데이터는 exe 옆 `WebPad_data`에 저장돼요. | Notepad++와 똑같은 단축키(Ctrl+N/W/Tab), 아무 폴더나 열기, 파일 연결 |
| **웹** https://seongtaeklee.github.io/webpad/ | 접속하거나 "앱으로 설치" | 인터넷이 되는 PC |

### 보안

- 외부로 나가는 통신이 없어요. 모든 형태의 HTML에 CSP(`connect-src`)를 넣어서 브라우저가 외부 요청을 막아요. `webpad.html`은 `default-src 'none'`이라 파일 밖의 어떤 것도 불러오지 않아요.
- 문서 내용과 임시 저장은 그 PC 안에만 있어요. 브라우저 저장소에 두고, exe는 `WebPad_data` 폴더에 둬요.
- exe는 윈도우 기본 WebView2를 써요. 주소 평판 확인(SmartScreen)도 꺼서 외부에 알리지 않아요. 소스(`launcher/WebPad.cs`)를 그대로 다시 빌드할 수 있어요.
- 서명이 없는 exe라 처음 실행할 때 "Windows의 PC 보호" 창이 뜰 수 있어요. 회사 정책으로 exe가 막히면 `webpad.html`을 쓰세요.
- 브라우저(html/웹)에서는 `C:\`나 사용자 폴더 전체처럼 시스템 파일이 있는 폴더를 브라우저가 열지 못하게 막아요. 그 안의 작업 폴더를 고르세요. exe는 윈도우가 직접 열어서 어디든 열 수 있어요.

## 기능

- 탭 여러 개, 폴더 탐색기(파일 이름 검색), 끌어다 놓아 열기
- 문법 색칠: ABAP, SQL, JavaScript/TS, Java/C 계열, Python, JSON, HTML/XML, CSS, Markdown, CSV(열마다 색), INI, YAML, Shell/Batch, 로그
  - Notepad++ 사용자 정의 언어(XML)를 불러올 수 있어요 (설정 → 사용자 언어 불러오기)
- 줄 복제, 삭제, 이동, 추가, 주석 켜기/끄기, 대소문자 변환, 짝 괄호 이동
- 블록(세로) 편집: Alt+드래그, Alt+Shift+방향키
- 시작/끝/양쪽 공백 삭제, 줄 정렬, 중복 줄 제거, 빈 줄 제거, 탭→공백
- 찾기/바꾸기: 대소문자, 단어 단위, 확장 모드(`\n` `\t` `\0` `\x41` ` `), 정규식(`$1` 그룹)
- 두 탭 비교: 나란히 보기, 줄 안의 바뀐 글자 표시, 공백 무시
- JSON 정리, 압축, 키 정렬, 검사(오류 위치로 이동), CSV 표 편집
- 상태 표시줄: 줄·칸, 선택 글자 수, 줄바꿈(CRLF/LF), 인코딩(UTF-8, UTF-8 BOM, EUC-KR, UTF-16), 문법
- 임시 저장: 저장하지 않은 내용도 1초마다 보관해요. 다시 열면 탭이 그대로 돌아와요. 파일 자동 저장도 켤 수 있어요.
- 대용량 파일: 줄이 많으면 보이는 곳 근처만 편집기에 올려요. 20만 줄 로그도 가볍게 편집할 수 있어요.
- Ctrl+휠로 글꼴 크기를 바꿔요. 단축키는 설정 → 단축키에서 바꿀 수 있어요(기본값은 Notepad++와 같아요).

## 빌드

웹 버전은 `index.html` 파일 하나와 `fonts/`로 되어 있어요. exe는 윈도우에 기본으로 있는 C# 컴파일러로 만들어요.

```
node tools\build-single.js  (dist\webpad.html: 글꼴까지 넣은 파일 하나)
launcher\get-webview2.cmd   (WebView2 SDK 받기, 한 번만)
launcher\build.cmd          (dist\WebPad.exe 생성)
```

## 사용한 것

- 글꼴: [나눔고딕코딩](https://github.com/google/fonts/tree/main/ofl/nanumgothiccoding) (SIL Open Font License, `fonts/OFL.txt`)
- ABAP 키워드: [Notepad++ userDefinedLanguages](https://github.com/notepad-plus-plus/userDefinedLanguages)의 ABAP 정의 3종을 합쳤어요
- 실행 파일: [Microsoft WebView2 SDK](https://www.nuget.org/packages/Microsoft.Web.WebView2)
