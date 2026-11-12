Manager of Dungeon - 로컬 LLM 폴더
====================================

이 폴더는 빌드에 그대로 복사된다(<게임>_Data/StreamingAssets/LLM).
게임이 시작되면 LlmRuntime이 bin/llama-server.exe를 백그라운드로 실행하고
127.0.0.1의 빈 포트로 교감 대사를 요청한다. 파일이 없으면 준비된 대사로 진행한다.

필요한 파일 (StreamingAssets 기준)
-----------
StreamingAssets/
  LLM/llm_settings.json      ← 기본 설정
  bin/                       ← llama.cpp Windows 릴리스 압축을 그대로 풀기
    llama-server.exe
    *.dll                    (ggml*.dll, llama.dll 등 같이 들어 있는 DLL 전부)
  models/
    Qwen3-4B-Instruct-2507-Q4_K_M.gguf   (약 2.5GB)
(위치를 바꾸면 llm_settings.json의 serverExecutable / modelFile 경로도 같이 바꾼다. 경로는 StreamingAssets 기준)

1) llama.cpp 바이너리
   https://github.com/ggml-org/llama.cpp/releases
   - 권장: Windows x64 "vulkan" 빌드 → NVIDIA/AMD/Intel GPU 모두 동작, GPU가 없으면 CPU로 동작
   - NVIDIA 전용으로 더 빠르게: "cuda" 빌드 + 같은 페이지의 cudart DLL 압축도 bin/에 풀기
   - 라이선스: MIT → 배포 시 LICENSE 파일을 bin/에 같이 넣고 크레딧에 표기

2) 모델
   Qwen3-4B-Instruct-2507 (Apache-2.0) GGUF Q4_K_M
   - Hugging Face에서 "Qwen3-4B-Instruct-2507 GGUF" 검색 (Qwen 공식 또는 신뢰할 수 있는 변환본)
   - 배포 전 모델 카드에서 라이선스(Apache-2.0)를 다시 확인하고 LICENSE를 models/에 같이 넣기
   - 'Instruct-2507'은 생각(thinking) 출력이 없는 버전이라 응답이 빠르다.
     다른 Qwen3 모델을 쓰면 <think> 블록 때문에 느려질 수 있다.
   - 파일명을 바꾸면 llm_settings.json의 modelFile도 같이 바꾼다.

개발 중 빠르게 쓰기
------------------
게임이 직접 띄우지 않고 이미 떠 있는 서버를 쓰려면:
  llama-server.exe -m <모델.gguf> --port 8080 -ngl 99 -c 4096
그리고 llm_settings.json에서  "externalServerUrl": "http://127.0.0.1:8080"
로그에 프롬프트/응답을 보고 싶으면 "logPrompts": true

플레이어별 설정
--------------
%USERPROFILE%\AppData\LocalLow\<회사명>\<제품명>\llm_settings.json 이 있으면 그 파일이 우선한다.
(LlmRuntime.Instance.SetEnabled(false)가 여기에 저장 → 'AI 대화 끄기' 옵션용)

서버 로그: 같은 폴더의 llm_server.log

동작 규칙(명세 6.1)
------------------
- LLM 응답 시간 초과(기본 12초)·JSON 형식 오류 → 준비된 대사 표시, EXP·기억 미지급, 오늘 대화 기회 미소진
- AI 대화 꺼짐 / 실행 파일·모델 없음 / 서버 시작 실패 → 준비된 대사로 '정상 교감'(EXP 지급)

git
---
StreamingAssets/bin/과 models/는 용량이 커서 .gitignore에 넣었다. 팀원은 각자 내려받아 같은 위치에 둔다.
itch.io 업로드는 butler 사용 권장(용량 제한은 업로드 전에 itch.io에서 확인).
