using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 본 게임에서만 필요한 팝업(일시정지 · 전투 결과 · 엔딩 · 게임오버 · 기록)과 알림 배너.
    /// 기본 화면은 기존 DungeonRoomScreen을 그대로 쓰고, 이 컴포넌트는 그 위에 겹치는 창만 담당한다.
    /// 색·외곽선은 기존 화면(밝은 회색 패널 + 어두운 글자)에 맞췄다.
    /// </summary>
    public sealed class GameHud : MonoBehaviour
    {
        const string CanvasName = "GameModalCanvas";
        const int LogLinesShown = 14;

        static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);
        static readonly Color WindowColor = new Color(0.97f, 0.97f, 0.94f, 0.98f);
        static readonly Color ButtonColor = new Color(0.93f, 0.93f, 0.93f, 1f);
        static readonly Color LineColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        static readonly Color TextDark = new Color(0.15f, 0.15f, 0.15f, 1f);
        static readonly Color BannerColor = new Color(0.16f, 0.16f, 0.18f, 0.94f);
        static readonly Color BannerText = new Color(1f, 0.878f, 0.588f, 1f);

        GameDirector _director;
        Font _font;

        GameObject _bannerRoot;
        Text _banner;
        float _bannerUntil;

        GameObject _unlock;
        Text _unlockText;
        System.Action _unlockClosed;

        GameObject _modal;
        Text _modalTitle;
        Text _modalBody;
        Transform _modalButtons;

        public void Bind(GameDirector director)
        {
            _director = director;
            _font = PackAssets.KoreanFont();
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Build();
        }

        void Update()
        {
            if (_bannerRoot != null && _bannerRoot.activeSelf && Time.unscaledTime > _bannerUntil)
                _bannerRoot.SetActive(false);
        }

        // ───────────── 배너 ─────────────

        public void Banner(string message)
        {
            if (_bannerRoot == null)
                return;
            _banner.text = message;
            _bannerRoot.SetActive(true);
            _bannerUntil = Time.unscaledTime + 2.5f;
        }

        // ───────────── 팝업 ─────────────

        public void ShowPause()
        {
            ShowModal(
                "일시정지",
                "ESC를 다시 누르면 게임으로 돌아갑니다.",
                new[] { "게임으로 돌아가기", "저장하기", "불러오기", "메인 메뉴로 나가기" },
                new UnityAction[]
                {
                    () => _director.TogglePause(),
                    () => { _director.SaveManual(); _modalBody.text = _director.Status; },
                    () => _director.LoadManual(),
                    () => _director.ReturnToMenu()
                });
        }

        public void ShowResult(SettlementReport report)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < report.Lines.Count; i++)
                sb.AppendLine(report.Lines[i]);
            ShowModal(
                report.Won ? "방어 성공" : "방어 실패",
                sb.ToString(),
                new[] { "확인" },
                new UnityAction[] { () => _director.ConfirmResult() });
        }

        public void ShowEnding()
        {
            int days = BalanceConfig.TargetDays;
            ShowModal(
                "돌아갈 시간이다.",
                "마계로 돌아갈 수 있는 문이 열렸다.\n\n" +
                $"{days}일 전에는 아무것도 없던 던전이었지만, 지금 이곳에는 당신이 만든 공간과 함께 싸워온 몬스터들이 있다.",
                new[] { "마계로 돌아간다", "이 던전의 마왕으로 남는다" },
                new UnityAction[]
                {
                    () => _director.ChooseReturnToDemonRealm(),
                    () => _director.ChooseStay()
                });
        }

        public void ShowGameOver()
        {
            var s = _director.State;
            ShowModal(
                "던전이 함락되었다",
                $"Day {s.day}에 남은 코인을 모두 잃었습니다.\n같은 날 06:00 자동저장에서 이어하기로 다시 도전할 수 있습니다.",
                new[] { "메인 메뉴로" },
                new UnityAction[] { () => _director.ReturnToMenu() });
        }

        /// <summary>상단 편지 아이콘: 최근 운영 기록.</summary>
        public void ShowLog(List<string> log)
        {
            var sb = new StringBuilder();
            int start = Mathf.Max(0, log.Count - LogLinesShown);
            for (int i = start; i < log.Count; i++)
                sb.AppendLine(log[i]);
            ShowModal(
                "운영 기록",
                sb.Length > 0 ? sb.ToString() : "아직 기록이 없습니다.",
                new[] { "닫기" },
                new UnityAction[] { HideModal });
        }

        public bool UnlockOpen => _unlock != null && _unlock.activeSelf;

        /// <summary>전략 해금 팝업: 전략명·설명만 표시, 화면 아무 곳이나 클릭하면 닫힌다(11-10 개정).</summary>
        public void ShowStrategyUnlock(StrategyInfo info, string monsterName, System.Action onClosed)
        {
            if (_unlock == null || info == null)
            {
                onClosed?.Invoke();
                return;
            }

            _unlockText.text =
                $"<size=22>{monsterName} · 새 전략 해금</size>\n\n" +
                $"<size=40><b>{info.Name}</b></size>\n\n" +
                $"{info.Description}\n\n" +
                "<size=18>(아무 곳이나 클릭)</size>";
            _unlockClosed = onClosed;
            _unlock.SetActive(true);
            _unlock.transform.SetAsLastSibling();
        }

        void CloseStrategyUnlock()
        {
            if (_unlock != null)
                _unlock.SetActive(false);
            var callback = _unlockClosed;
            _unlockClosed = null;
            callback?.Invoke();
        }

        public void HideModal()
        {
            if (_modal != null)
                _modal.SetActive(false);
        }

        void ShowModal(string title, string body, string[] labels, UnityAction[] actions)
        {
            _modalTitle.text = title;
            _modalBody.text = body;
            for (int i = _modalButtons.childCount - 1; i >= 0; i--)
                Destroy(_modalButtons.GetChild(i).gameObject);
            for (int i = 0; i < labels.Length && i < actions.Length; i++)
                MakeButton(_modalButtons, labels[i], actions[i]);
            _modal.SetActive(true);
        }

        // ───────────── 구성 ─────────────

        void Build()
        {
            var old = transform.Find(CanvasName);
            if (old != null)
                Destroy(old.gameObject);

            var canvasGo = new GameObject(CanvasName, typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50; // 기존 DungeonCanvas 위
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = canvasGo.transform;

            // 알림 배너 (상단 바 바로 아래)
            var banner = MakeRect("Banner", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(760f, 60f));
            var bannerImage = banner.gameObject.AddComponent<Image>();
            bannerImage.color = BannerColor;
            bannerImage.raycastTarget = false;
            _banner = FillText(banner, 26, TextAnchor.MiddleCenter, BannerText);
            _bannerRoot = banner.gameObject;
            _bannerRoot.SetActive(false);

            // 팝업
            var modal = MakeRect("Modal", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            modal.gameObject.AddComponent<Image>().color = DimColor;
            var window = MakeRect("Window", modal, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 560f));
            window.gameObject.AddComponent<Image>().color = WindowColor;
            AddOutline(window.gameObject, 2f);

            var title = MakeRect("Title", window, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(-60f, 56f));
            _modalTitle = title.gameObject.AddComponent<Text>();
            ConfigureText(_modalTitle, 34, TextAnchor.MiddleCenter, TextDark);

            var body = MakeRect("Body", window, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, 24f), new Vector2(-80f, -240f));
            _modalBody = body.gameObject.AddComponent<Text>();
            ConfigureText(_modalBody, 22, TextAnchor.UpperLeft, TextDark);

            var buttons = MakeRect("Buttons", window, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(-60f, 120f));
            var grid = buttons.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(370f, 48f);
            grid.spacing = new Vector2(16f, 14f);
            grid.childAlignment = TextAnchor.LowerCenter;
            _modalButtons = buttons;
            _modal = modal.gameObject;
            _modal.SetActive(false);

            // 전략 해금 팝업 (화면 전체가 닫기 버튼)
            var unlock = MakeRect("StrategyUnlock", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var unlockDim = unlock.gameObject.AddComponent<Image>();
            unlockDim.color = DimColor;
            var unlockButton = unlock.gameObject.AddComponent<Button>();
            unlockButton.targetGraphic = unlockDim;
            unlockButton.transition = Selectable.Transition.None;
            unlockButton.onClick.AddListener(CloseStrategyUnlock);
            var card = MakeRect("Card", unlock, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 360f));
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = WindowColor;
            cardImage.raycastTarget = false;
            AddOutline(card.gameObject, 2f);
            _unlockText = FillText(card, 24, TextAnchor.MiddleCenter, TextDark);
            _unlock = unlock.gameObject;
            _unlock.SetActive(false);
        }

        void MakeButton(Transform parent, string label, UnityAction action)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = ButtonColor;
            AddOutline(go, 1.5f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            if (action != null)
                button.onClick.AddListener(action);

            var text = FillText((RectTransform)go.transform, 20, TextAnchor.MiddleCenter, TextDark);
            text.text = label;
        }

        static RectTransform MakeRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        Text FillText(RectTransform parent, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12f, 6f);
            rt.offsetMax = new Vector2(-12f, -6f);
            var text = go.AddComponent<Text>();
            ConfigureText(text, size, anchor, color);
            return text;
        }

        void ConfigureText(Text text, int size, TextAnchor anchor, Color color)
        {
            text.font = _font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
        }

        static void AddOutline(GameObject go, float distance)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = LineColor;
            outline.effectDistance = new Vector2(distance, -distance);
        }
    }
}
