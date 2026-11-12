using System.IO;
using UnityEngine;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 로컬 LLM 설정. 읽는 순서:
    /// 1) persistentDataPath/llm_settings.json  (플레이어별 덮어쓰기: AI 대화 끄기 등)
    /// 2) StreamingAssets/LLM/llm_settings.json (배포 기본값)
    /// 3) 코드 기본값
    /// 상대 경로는 StreamingAssets 기준.
    /// </summary>
    [System.Serializable]
    public sealed class LlmSettings
    {
        public const string DefaultFileName = "LLM/llm_settings.json";
        public const string UserFileName = "llm_settings.json";

        /// <summary>false면 서버를 띄우지 않고 준비된 대사(정상 교감, EXP 지급)로만 진행.</summary>
        public bool enabled = true;

        /// <summary>개발용: 이미 떠 있는 서버(예: http://127.0.0.1:8080)를 쓴다. 비우면 게임이 직접 실행.</summary>
        public string externalServerUrl = "";

        public string serverExecutable = "bin/llama-server.exe";
        public string modelFile = "models/Qwen3-4B-Instruct-2507-Q4_K_M.gguf";

        /// <summary>0이면 빈 포트를 자동으로 고른다.</summary>
        public int port = 0;
        public int contextSize = 4096;
        /// <summary>GPU에 올릴 레이어 수. 99 = 전부. GPU가 없으면 llama.cpp가 CPU로 돈다.</summary>
        public int gpuLayers = 99;
        /// <summary>0이면 llama.cpp 기본값.</summary>
        public int threads = 0;
        public string extraArgs = "";

        public float startupTimeoutSeconds = 120f;
        /// <summary>요청 1건 제한 시간(서버 준비 대기 포함). 넘으면 대체 대사 + EXP 미지급.</summary>
        public float requestTimeoutSeconds = 12f;

        public int maxTokens = 320;
        public float temperature = 0.8f;
        public float topP = 0.9f;

        /// <summary>true면 프롬프트·응답 원문을 콘솔에 남긴다(개발용).</summary>
        public bool logPrompts = false;

        public static string UserPath => Path.Combine(Application.persistentDataPath, UserFileName);
        public static string DefaultPath => Path.Combine(Application.streamingAssetsPath, DefaultFileName);

        public static LlmSettings Load()
        {
            var settings = TryRead(UserPath) ?? TryRead(DefaultPath) ?? new LlmSettings();
            settings.Clamp();
            return settings;
        }

        public void SaveUserOverride()
        {
            try
            {
                File.WriteAllText(UserPath, JsonUtility.ToJson(this, true));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[LLM] 설정 저장 실패: " + e.Message);
            }
        }

        public string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            return Path.IsPathRooted(path) ? path : Path.Combine(Application.streamingAssetsPath, path);
        }

        void Clamp()
        {
            if (contextSize < 1024) contextSize = 1024;
            if (maxTokens < 64) maxTokens = 64;
            if (requestTimeoutSeconds < 2f) requestTimeoutSeconds = 2f;
            if (startupTimeoutSeconds < 5f) startupTimeoutSeconds = 5f;
        }

        static LlmSettings TryRead(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;
                var s = new LlmSettings();
                JsonUtility.FromJsonOverwrite(File.ReadAllText(path), s);
                return s;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LLM] 설정 읽기 실패 ({path}): {e.Message}");
                return null;
            }
        }
    }
}
