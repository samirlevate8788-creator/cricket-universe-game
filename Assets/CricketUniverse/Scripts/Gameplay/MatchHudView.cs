using CricketUniverse.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CricketUniverse.Gameplay
{
    internal sealed class MatchHudView : MonoBehaviour
    {
        private Text scoreText;
        private Text situationText;
        private Text statusText;
        private Text aimText;
        private Button actionButton;
        private Text actionLabel;
        private Button loftButton;
        private Text loftLabel;
        private RectTransform timingMarker;
        private readonly Color panelColor = new Color(0.025f, 0.08f, 0.10f, 0.86f);
        private readonly Color accentColor = new Color(0.15f, 0.86f, 0.64f, 1f);

        public static MatchHudView Create(CricketMatchController controller)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();

            GameObject canvasObject = new GameObject("Match HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            MatchHudView view = canvasObject.AddComponent<MatchHudView>();
            view.Build(controller);
            return view;
        }

        private void Build(CricketMatchController controller)
        {
            MakePanel("Score Panel", transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -18), new Vector2(560, 182));
            scoreText = MakeText("Score", transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -24),
                new Vector2(520, 150), 36, TextAnchor.UpperLeft, Color.white);
            situationText = MakeText("Situation", transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(32, -130),
                new Vector2(550, 55), 22, TextAnchor.UpperLeft, new Color(0.72f, 0.84f, 0.85f));
            statusText = MakeText("Ball Call", transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -35),
                new Vector2(720, 72), 30, TextAnchor.MiddleCenter, Color.white);
            BuildTimingMeter();
            aimText = MakeText("Aim", transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 190),
                new Vector2(380, 48), 21, TextAnchor.MiddleCenter, new Color(0.89f, 0.93f, 0.88f));

            Button left = MakeButton("Aim Left", "AIM LEFT", transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(150, 86), new Vector2(205, 92));
            left.onClick.AddListener(() => controller.AdjustAim(-0.22f));
            Button right = MakeButton("Aim Right", "AIM RIGHT", transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(395, 86), new Vector2(205, 92));
            right.onClick.AddListener(() => controller.AdjustAim(0.22f));

            Button run = MakeButton("Run", "RUN", transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(150, 202), new Vector2(205, 82));
            run.onClick.AddListener(controller.AttemptRun);

            loftButton = MakeButton("Loft", "GROUND", transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-155, 202), new Vector2(240, 82));
            loftLabel = loftButton.GetComponentInChildren<Text>();
            loftButton.onClick.AddListener(controller.ToggleLoft);

            Button shot = MakeButton("Shot", "SHOT", transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-155, 92), new Vector2(240, 108));
            shot.onClick.AddListener(controller.TryShot);

            actionButton = MakeButton("Match Action", "BOWL", transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-155, 326), new Vector2(270, 86));
            actionLabel = actionButton.GetComponentInChildren<Text>();
            actionButton.onClick.AddListener(controller.ActionButton);
        }

        public void SetLoft(bool enabled)
        {
            if (loftLabel == null) return;
            loftLabel.text = enabled ? "LOFTED" : "GROUND";
            ColorBlock colors = loftButton.colors;
            colors.normalColor = enabled ? new Color(0.67f, 0.30f, 0.11f, 0.96f) : new Color(0.08f, 0.26f, 0.25f, 0.96f);
            colors.highlightedColor = Color.Lerp(colors.normalColor, Color.white, 0.22f);
            loftButton.colors = colors;
        }

        public void SetBallProgress(float progress, bool visible)
        {
            if (timingMarker == null) return;
            timingMarker.gameObject.SetActive(visible);
            float normalized = Mathf.Clamp01(progress);
            timingMarker.anchorMin = new Vector2(normalized, 0f);
            timingMarker.anchorMax = new Vector2(normalized, 1f);
        }

        public void Refresh(CricketMatchModel match, string status, ShotTiming timing, float aim)
        {
            InningsScore score = match.Current;
            scoreText.text = $"{match.battingTeam}\n{score.runs} / {score.wickets}";
            string target = match.currentInnings == 0 ? $"INNINGS 1  -  {score.OversText}/{match.oversPerInnings}.0 OVERS" : $"TARGET {match.target}  -  {score.OversText}/{match.oversPerInnings}.0 OVERS";
            float runRate = score.legalBalls == 0 ? 0f : (score.runs * 6f / score.legalBalls);
            string chase = match.currentInnings == 0 ? $"RR {runRate:0.00}   -   Bowling: {match.bowlingTeam}" : $"RR {runRate:0.00}   -   NEED {match.RunsRequired} OFF {match.BallsRemaining}";
            situationText.text = $"{target}\n{chase}";
            statusText.text = status;
            Color timingColor = timing == ShotTiming.Perfect ? accentColor : timing == ShotTiming.Good ? new Color(1f, 0.78f, 0.19f) : timing == ShotTiming.Poor ? new Color(1f, 0.38f, 0.27f) : Color.white;
            statusText.color = timing == ShotTiming.None || status.Contains("WICKET") ? Color.white : timingColor;
            aimText.text = $"SHOT AIM   {(aim < -0.22f ? "LEFT" : aim > 0.22f ? "RIGHT" : "STRAIGHT")}";

            if (match.phase == MatchPhase.ReadyToBowl) actionLabel.text = "BOWL  -  ENTER";
            else if (match.phase == MatchPhase.InningsBreak) actionLabel.text = "NEXT INNINGS";
            else if (match.phase == MatchPhase.MatchComplete) actionLabel.text = "PLAY AGAIN";
            else actionLabel.text = "BALL IN PLAY";
            actionButton.interactable = match.phase == MatchPhase.ReadyToBowl || match.phase == MatchPhase.InningsBreak || match.phase == MatchPhase.MatchComplete;
        }

        private Image MakePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = anchorMin;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            panel.GetComponent<Image>().color = panelColor;
            return panel.GetComponent<Image>();
        }

        private void BuildTimingMeter()
        {
            Image track = MakePanel("Timing Meter", transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -116), new Vector2(520, 22));
            track.color = new Color(0.04f, 0.09f, 0.09f, 0.88f);
            GameObject zone = new GameObject("Perfect Zone", typeof(RectTransform), typeof(Image));
            zone.transform.SetParent(track.transform, false);
            RectTransform zoneRect = zone.GetComponent<RectTransform>();
            zoneRect.anchorMin = new Vector2(0.77f, 0f); zoneRect.anchorMax = new Vector2(0.87f, 1f);
            zoneRect.offsetMin = Vector2.zero; zoneRect.offsetMax = Vector2.zero;
            zone.GetComponent<Image>().color = new Color(0.15f, 0.86f, 0.64f, 0.46f);
            GameObject marker = new GameObject("Ball Timing", typeof(RectTransform), typeof(Image));
            marker.transform.SetParent(track.transform, false);
            timingMarker = marker.GetComponent<RectTransform>();
            timingMarker.anchorMin = Vector2.zero; timingMarker.anchorMax = new Vector2(0f, 1f);
            timingMarker.pivot = new Vector2(0.5f, 0.5f); timingMarker.sizeDelta = new Vector2(6f, 0f);
            marker.GetComponent<Image>().color = Color.white;
            marker.SetActive(false);
        }

        private static Text MakeText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size,
            int fontSize, TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = anchorMin;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            return text;
        }

        private static Button MakeButton(string name, string label, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = anchorMin;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.05f, 0.20f, 0.20f, 0.95f);
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = Color.Lerp(image.color, Color.white, 0.22f);
            colors.pressedColor = new Color(0.68f, 0.95f, 0.84f, 1f);
            button.colors = colors;
            Text text = MakeText("Label", buttonObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 25, TextAnchor.MiddleCenter, Color.white);
            RectTransform labelRect = text.GetComponent<RectTransform>();
            labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
            text.text = label;
            text.fontStyle = FontStyle.Bold;
            return button;
        }
    }
}
