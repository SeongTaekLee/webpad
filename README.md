# 웹패드 (WebPad)

Notepad++를 쓸 수 없는 환경을 위한 가벼운 코드 편집기예요. 설치가 필요 없어요.

- **웹 버전**: https://seongtaeklee.github.io/webpad/ (엣지·크롬. 오른쪽 위 "앱으로 설치"를 누르면 앱처럼 쓸 수 있어요)
- **실행 파일 버전**: [Releases](https://github.com/SeongTaekLee/webpad/releases/latest)에서 `WebPad.exe`를 받아 아무 폴더에 두고 실행하면 돼요.
  - 포터블이에요. 설정과 임시 저장은 exe 옆 `WebPad_data` 폴더에 저장돼요.
  - 윈도우 내장 WebView2로 자체 창을 띄워요. 그래서 Ctrl+N / Ctrl+W / Ctrl+Tab을 Notepad++처럼 쓸 수 있어요.
  - 파일을 exe로 열 수 있어요(연결 프로그램, exe 위로 끌어다 놓기). 이미 켜져 있으면 같은 창에 탭으로 열려요.

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
launcher\get-webview2.cmd   (WebView2 SDK 받기, 한 번만)
launcher\build.cmd          (dist\WebPad.exe 생성)
```

## 사용한 것

- 글꼴: [나눔고딕코딩](https://github.com/google/fonts/tree/main/ofl/nanumgothiccoding) (SIL Open Font License, `fonts/OFL.txt`)
- ABAP 키워드: [Notepad++ userDefinedLanguages](https://github.com/notepad-plus-plus/userDefinedLanguages)의 ABAP 정의 3종을 합쳤어요
- 실행 파일: [Microsoft WebView2 SDK](https://www.nuget.org/packages/Microsoft.Web.WebView2)
