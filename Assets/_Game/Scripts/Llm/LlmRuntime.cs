using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ManagerOfDungeon
{
    public enum LlmStatus
    {
        NotStarted,
        /// <summary>설정에서 꺼짐 → 준비된 대사로 정상 교감.</summary>
        Disabled,
        Starting,
        Ready,
        /// <summary>실행 파일·모델 없음, 시작 실패, 서버 종료 → 준비된 대사로 정상 교감.</summary>
        Failed
    }

    /// <summary>
    /// 로컬 llama-server 프로세스 수명 관리. 게임 시작 시 자동 생성(DontDestroyOnLoad).
    /// - 시작: 빈 포트 선택 → 프로세스 실행 → /health 200 대기 → 예열 요청 1회
    /// - 종료: OnApplicationQuit/OnDestroy에서 Kill. 비정상 종료로 남은 프로세스는 다음 실행 때 pid 파일로 정리
    /// - 서버 로그: persistentDataPath/llm_server.log
    /// </summary>
    public sealed class LlmRuntime : MonoBehaviour
    {
        static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        static readonly RuntimeLlmClient Proxy = new RuntimeLlmClient();

        public static LlmRuntime Instance { get; private set; }

        /// <summary>교감 코드가 쓰는 클라이언트. 호출 시점의 상태에 따라 로컬 LLM 또는 준비된 대사로 보낸다.</summary>
        public static ILlmClient Client => Proxy;

        public static LlmStatus Status => Instance != null ? Instance._status : LlmStatus.NotStarted;
        public static string StatusMessage => Instance != null ? Instance._message : string.Empty;

        public LlmSettings Settings { get; private set; }
        public string BaseUrl { get; private set; } = string.Empty;

        volatile LlmStatus _status = LlmStatus.NotStarted;
        string _message = string.Empty;
        TaskCompletionSource<bool> _ready = NewTcs();
        Process _process;
        LocalLlamaClient _local;
        readonly FakeLlmClient _scripted = new FakeLlmClient();
        StreamWriter _log;
        readonly object _logLock = new object();
        volatile bool _quitting;

        static string PidPath => Path.Combine(Application.persistentDataPath, "llm_server.pid");
        static string LogPath => Path.Combine(Application.persistentDataPath, "llm_server.log");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null)
                return;
            var go = new GameObject("[LlmRuntime]");
            DontDestroyOnLoad(go);
            go.AddComponent<LlmRuntime>();
        }

        /// <summary>지금 요청을 받을 클라이언트.</summary>
        internal ILlmClient ActiveClient
        {
            get
            {
                var s = _status;
                return s == LlmStatus.Disabled || s == LlmStatus.Failed || _local == null ? (ILlmClient)_scripted : _local;
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Settings = LlmSettings.Load();
            _local = new LocalLlamaClient(this, Settings);
            StartRuntime();
        }

        /// <summary>서버가 준비되면 true, 실패/꺼짐이면 false. 토큰이 취소되면 OperationCanceledException.</summary>
        public async Task<bool> WaitUntilReadyAsync(CancellationToken ct)
        {
            var ready = _ready.Task;
            if (ready.IsCompleted)
                return ready.Result;

            var cancel = Task.Delay(Timeout.Infinite, ct);
            var done = await Task.WhenAny(ready, cancel).ConfigureAwait(false);
            if (done != ready)
                ct.ThrowIfCancellationRequested();
            return ready.Result;
        }

        /// <summary>설정 화면용: AI 대화 켜기/끄기(플레이어 설정 파일에 저장 후 재시작).</summary>
        public void SetEnabled(bool enabled)
        {
            if (Settings.enabled == enabled)
                return;
            Settings.enabled = enabled;
            Settings.SaveUserOverride();
            Restart();
        }

        public void Restart()
        {
            StopServer();
            _ready = NewTcs();
            _status = LlmStatus.NotStarted;
            StartRuntime();
        }

        async void StartRuntime()
        {
            try
            {
                if (!Settings.enabled)
                {
                    Finish(false, LlmStatus.Disabled, "AI 대화가 꺼져 있습니다(준비된 대사 사용).");
                    return;
                }

                SetStatus(LlmStatus.Starting, "AI 대화 준비 중…");

                if (!string.IsNullOrEmpty(Settings.externalServerUrl))
                {
                    BaseUrl = Settings.externalServerUrl.TrimEnd('/');
                    bool okExternal = await WaitForHealthAsync(Settings.startupTimeoutSeconds);
                    Finish(okExternal, okExternal ? LlmStatus.Ready : LlmStatus.Failed,
                        okExternal ? "외부 LLM 서버 연결됨: " + BaseUrl : "외부 LLM 서버에 연결하지 못했습니다: " + BaseUrl);
                    if (okExternal)
                        Warmup();
                    return;
                }

                string exe = Settings.ResolvePath(Settings.serverExecutable);
                string model = Settings.ResolvePath(Settings.modelFile);
                if (!File.Exists(exe))
                {
                    Finish(false, LlmStatus.Failed, "llama-server 실행 파일이 없습니다: " + exe);
                    return;
                }
                if (!File.Exists(model))
                {
                    Finish(false, LlmStatus.Failed, "모델 파일이 없습니다: " + model);
                    return;
                }

                KillOrphan();
                int port = Settings.port > 0 ? Settings.port : FindFreePort();
                BaseUrl = "http://127.0.0.1:" + port;
                StartProcess(exe, BuildArgs(model, port));

                bool ok = await WaitForHealthAsync(Settings.startupTimeoutSeconds);
                if (_quitting)
                    return;
                if (!ok)
                {
                    StopServer();
                    Finish(false, LlmStatus.Failed, "LLM 서버 시작 실패(로그: " + LogPath + ")");
                    return;
                }

                Finish(true, LlmStatus.Ready, "AI 대화 준비 완료");
                Warmup();
            }
            catch (Exception e)
            {
                StopServer();
                Finish(false, LlmStatus.Failed, "LLM 서버 시작 오류: " + e.Message);
            }
        }

        string BuildArgs(string model, int port)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("-m \"").Append(model).Append('"');
            sb.Append(" --host 127.0.0.1 --port ").Append(port.ToString(inv));
            sb.Append(" -c ").Append(Settings.contextSize.ToString(inv));
            sb.Append(" -ngl ").Append(Settings.gpuLayers.ToString(inv));
            sb.Append(" -np 1");
            if (Settings.threads > 0)
                sb.Append(" -t ").Append(Settings.threads.ToString(inv));
            if (!string.IsNullOrEmpty(Settings.extraArgs))
                sb.Append(' ').Append(Settings.extraArgs);
            return sb.ToString();
        }

        void StartProcess(string exe, string args)
        {
            OpenLog();
            WriteLog("[LlmRuntime] " + exe + " " + args);

            var psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = Path.GetDirectoryName(exe) ?? string.Empty
            };

            _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _process.OutputDataReceived += (s, e) => WriteLog(e.Data);
            _process.ErrorDataReceived += (s, e) => WriteLog(e.Data);
            _process.Exited += OnProcessExited;
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            try
            {
                File.WriteAllText(PidPath, _process.Id.ToString());
            }
            catch (Exception)
            {
                // pid 파일은 정리용일 뿐이라 실패해도 진행
            }
        }

        void OnProcessExited(object sender, EventArgs e)
        {
            // 스레드풀에서 호출됨: Unity API(Debug 제외) 사용 금지
            if (_quitting)
                return;
            _status = LlmStatus.Failed;
            _message = "LLM 서버가 종료되었습니다. 준비된 대사로 진행합니다.";
            _ready.TrySetResult(false);
            Debug.LogWarning("[LLM] " + _message);
        }

        async Task<bool> WaitForHealthAsync(float timeoutSeconds)
        {
            var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            while (DateTime.UtcNow < deadline && !_quitting)
            {
                if (_process != null && _process.HasExited)
                    return false;

                try
                {
                    using (var resp = await Http.GetAsync(BaseUrl + "/health"))
                    {
                        if (resp.IsSuccessStatusCode)
                            return true; // 모델 로딩 중에는 503
                    }
                }
                catch (Exception)
                {
                    // 아직 포트가 안 열림
                }

                await Task.Delay(500);
            }

            return false;
        }

        async void Warmup()
        {
            // 고정 시스템 프롬프트를 한 번 계산해 두면 첫 교감부터 프롬프트 캐시가 적용된다.
            var prompt = new LlmPrompt(CommunionPromptBuilder.StaticSystemPrompt, "준비", null, maxTokens: 1, temperature: 0f);
            var r = await _local.CompleteAsync(prompt, CancellationToken.None);
            Debug.Log(r.Ok ? $"[LLM] 예열 완료 ({r.ElapsedMs:0}ms)" : "[LLM] 예열 실패: " + r.Error);
        }

        void Finish(bool ok, LlmStatus status, string message)
        {
            SetStatus(status, message);
            _ready.TrySetResult(ok);
            if (ok)
                Debug.Log("[LLM] " + message);
            else if (status == LlmStatus.Disabled)
                Debug.Log("[LLM] " + message);
            else
                Debug.LogWarning("[LLM] " + message);
        }

        void SetStatus(LlmStatus status, string message)
        {
            _status = status;
            _message = message ?? string.Empty;
        }

        void OnApplicationQuit()
        {
            _quitting = true;
            StopServer();
        }

        void OnDestroy()
        {
            if (Instance != this)
                return;
            _quitting = true;
            StopServer();
            Instance = null;
        }

        void StopServer()
        {
            var p = _process;
            _process = null;
            if (p != null)
            {
                try
                {
                    p.Exited -= OnProcessExited;
                    if (!p.HasExited)
                    {
                        p.Kill();
                        p.WaitForExit(2000);
                    }
                }
                catch (Exception)
                {
                    // 이미 종료됨
                }
                finally
                {
                    p.Dispose();
                }

                try
                {
                    if (File.Exists(PidPath))
                        File.Delete(PidPath);
                }
                catch (Exception)
                {
                }
            }

            lock (_logLock)
            {
                _log?.Dispose();
                _log = null;
            }
        }

        static void KillOrphan()
        {
            try
            {
                if (!File.Exists(PidPath))
                    return;
                if (int.TryParse(File.ReadAllText(PidPath).Trim(), out int pid))
                {
                    try
                    {
                        using (var p = Process.GetProcessById(pid))
                        {
                            if (!p.HasExited && p.ProcessName.IndexOf("llama", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                p.Kill();
                                p.WaitForExit(2000);
                                Debug.Log("[LLM] 이전 실행에서 남은 서버를 정리했습니다. pid=" + pid);
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        // 해당 pid 없음
                    }
                }
                File.Delete(PidPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LLM] 남은 서버 정리 실패: " + e.Message);
            }
        }

        static int FindFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        void OpenLog()
        {
            lock (_logLock)
            {
                try
                {
                    _log?.Dispose();
                    _log = new StreamWriter(LogPath, false, new UTF8Encoding(false)) { AutoFlush = true };
                }
                catch (Exception)
                {
                    _log = null;
                }
            }
        }

        void WriteLog(string line)
        {
            if (line == null)
                return;
            lock (_logLock)
            {
                try
                {
                    _log?.WriteLine(line);
                }
                catch (Exception)
                {
                }
            }
        }

        static TaskCompletionSource<bool> NewTcs()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    /// <summary>LlmRuntime 상태에 따라 실제 클라이언트로 전달한다. 런타임이 없으면(에디터 테스트 등) 준비된 대사.</summary>
    public sealed class RuntimeLlmClient : ILlmClient
    {
        static readonly FakeLlmClient Scripted = new FakeLlmClient();

        public Task<LlmResponse> CompleteAsync(LlmPrompt prompt, CancellationToken ct)
        {
            var runtime = LlmRuntime.Instance;
            var inner = runtime != null ? runtime.ActiveClient : Scripted;
            return inner.CompleteAsync(prompt, ct);
        }
    }

    /// <summary>QA용 간단 집계(명세 21장: 교감 정상 지급·실패 폴백). 콘솔에서 LlmTelemetry.Summary 확인.</summary>
    public static class LlmTelemetry
    {
        public static int Requests { get; private set; }
        public static int Succeeded { get; private set; }
        public static int FallbackCount { get; private set; }
        public static float TotalMs { get; private set; }
        public static float MaxMs { get; private set; }
        public static string LastError { get; private set; } = string.Empty;

        public static void Record(LlmResponse response, bool parsedOk, string error)
        {
            if (response == null || response.Scripted)
                return;
            Requests++;
            TotalMs += response.ElapsedMs;
            if (response.ElapsedMs > MaxMs)
                MaxMs = response.ElapsedMs;
            if (response.Ok && parsedOk)
                Succeeded++;
            else
            {
                FallbackCount++;
                LastError = error ?? string.Empty;
            }
        }

        public static string Summary =>
            Requests == 0
                ? "LLM 요청 없음"
                : $"LLM 요청 {Requests} · 성공 {Succeeded} · 폴백 {FallbackCount} · 평균 {TotalMs / Requests:0}ms · 최대 {MaxMs:0}ms · 마지막 오류: {LastError}";
    }
}
