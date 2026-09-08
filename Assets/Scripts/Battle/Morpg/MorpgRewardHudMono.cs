using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Morpg
{
    // Optional exact-route view. No account, ledger or reward amount is owned by a glyph or animation.
    internal sealed class MorpgRewardHudMono : MonoBehaviour, IMorpgRewardPresentation
    {
        private const string GoldResource = "battle/morpg/rewards/reward.gold.coin-charm";
        private const string XpResource = "battle/morpg/rewards/reward.xp.faceted-shard";
        private readonly Dictionary<MorpgRewardVisualGroup, GlyphPair> live = new();
        private readonly Stack<GlyphPair> pool = new();
        private RectTransform canvasRect, goldAnchor, xpAnchor, goldPanel, xpPanel;
        private TMP_Text goldText, xpText, goldDelta, xpDelta;
        private Image xpFill;
        private Sprite goldSprite, xpSprite;
        private Action unavailable;
        private bool closing;
        private double shownGold, startGold, targetGold;
        private float shownXp, startXp, targetXp, tweenAge = .3f, shownQuarters, startQuarters;
        private float deltaAge = 1f, coalesceAge = 1f, pulseAge = 1f;
        private int deltaGold, deltaQuarters, earnedQuarters;
        internal bool ReducedMotion { get; set; }
        internal bool ReducedFlash { get; set; }
        public bool IsAvailable => this != null && isActiveAndEnabled && canvasRect != null &&
            canvasRect.gameObject.activeInHierarchy && goldAnchor != null && xpAnchor != null &&
            goldAnchor.gameObject.activeInHierarchy && xpAnchor.gameObject.activeInHierarchy &&
            goldSprite != null && xpSprite != null && Camera.main != null;

        private sealed class GlyphPair
        {
            internal GameObject Root;
            internal RectTransform Gold, Xp;
            internal Vector2 FlightStart;
            internal bool Flying;
        }

        internal static MorpgRewardHudMono TryCreate(Transform owner, Action onUnavailable, int gold, float xp)
        {
            GameObject root = null;
            try
            {
                root = new GameObject("MORPG reward HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                root.transform.SetParent(owner, false);
                var view = root.AddComponent<MorpgRewardHudMono>();
                view.unavailable = onUnavailable;
                view.Build(gold, xp);
                return view;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MORPG] Optional reward HUD unavailable: " + e.Message);
                if (root != null) Destroy(root);
                return null;
            }
        }

        private void Build(int gold, float xp)
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 250;
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasRect = (RectTransform)transform;
            goldSprite = Resources.Load<Sprite>(GoldResource);
            xpSprite = Resources.Load<Sprite>(XpResource);
            Color background = new Color(.025f, .03f, .04f, .82f);
            goldPanel = Box("Gold", canvasRect, new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-24, -24), new Vector2(180, 56), background).rectTransform;
            goldAnchor = Icon("Gold target", goldPanel, goldSprite, new Vector2(-62, 0), 36);
            goldText = Label("Gold value", goldPanel, new Vector2(24, 0), new Vector2(120, 44), new Color32(0xF3, 0xD4, 0x7A, 255));
            goldDelta = Label("Gold arrival", goldPanel, new Vector2(-20, -42), new Vector2(170, 30), new Color32(0xF3, 0xD4, 0x7A, 255));
            xpPanel = Box("Raw XP", canvasRect, new Vector2(.5f, 0), new Vector2(.5f, 0),
                new Vector2(0, 24), new Vector2(600, 28), background).rectTransform;
            xpFill = Box("Encounter XP progress", xpPanel, new Vector2(0, .5f), new Vector2(0, .5f),
                Vector2.zero, new Vector2(600, 28), new Color32(0x5A, 0xA8, 0xB8, 180));
            xpAnchor = Icon("XP target", xpPanel, xpSprite, new Vector2(-274, 0), 30);
            xpText = Label("Raw XP value", xpPanel, new Vector2(0, 0), new Vector2(500, 28), Color.white);
            xpDelta = Label("XP arrival", xpPanel, new Vector2(-225, 34), new Vector2(160, 30), new Color32(0x8F, 0xD1, 0xD5, 255));
            shownGold = startGold = targetGold = gold;
            shownXp = startXp = targetXp = xp;
            RenderNumbers();
            goldDelta.text = xpDelta.text = string.Empty;
        }

        private static Image Box(string name, RectTransform parent, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = pivot;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
            return image;
        }
        private static RectTransform Icon(string name, RectTransform parent, Sprite sprite, Vector2 position, float size)
        {
            var image = Box(name, parent, new Vector2(.5f, .5f), new Vector2(.5f, .5f), position,
                new Vector2(size, size), Color.white);
            image.sprite = sprite; image.preserveAspect = true;
            if (sprite == null) image.color = Color.clear;
            return image.rectTransform;
        }
        private static TMP_Text Label(string name, RectTransform parent, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var text = go.GetComponent<TextMeshProUGUI>();
            foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
                if (font.name == "EBS_SB SDF") { text.font = font; break; }
            text.fontSize = 22; text.color = color; text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false; text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        public bool TryShow(MorpgRewardVisualGroup group)
        {
            if (!IsAvailable || ReducedMotion) return false;
            GlyphPair pair = null;
            while (pool.Count > 0 && (pair == null || pair.Root == null)) pair = pool.Pop();
            if (pair == null || pair.Root == null)
            {
                if (live.Count >= MorpgRewardDeliveryQueue.VisualCap) return false;
                pair = new GlyphPair { Root = new GameObject("Reward bundle glyphs", typeof(RectTransform)) };
                pair.Root.transform.SetParent(canvasRect, false);
                var parent = (RectTransform)pair.Root.transform;
                parent.anchorMin = parent.anchorMax = parent.pivot = new Vector2(.5f, .5f);
                parent.anchoredPosition = Vector2.zero; parent.sizeDelta = Vector2.zero;
                pair.Gold = Icon("coin-charm", parent, goldSprite, Vector2.zero, 38);
                pair.Xp = Icon("faceted-shard", parent, xpSprite, Vector2.zero, 38);
            }
            pair.Flying = false; pair.Root.SetActive(true);
            live.Add(group, pair);
            return true;
        }
        public void Refresh(MorpgRewardVisualGroup group)
        {
            if (!IsAvailable || !live.TryGetValue(group, out var pair) || pair.Root == null ||
                !pair.Root.activeInHierarchy || pair.Gold == null || pair.Xp == null)
                throw new InvalidOperationException("reward glyph/anchor inactive");
            Vector3 screen = Camera.main.WorldToScreenPoint(new Vector3(group.X, group.Y, 0));
            if (screen.z < 0) throw new InvalidOperationException("reward behind camera");
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 ground);
            if (!group.Flying)
            {
                float envelope = group.Age < .12f ? Mathf.Lerp(.82f, 1.06f, group.Age / .12f) :
                    Mathf.Lerp(1.06f, 1f, Mathf.Clamp01((group.Age - .12f) / .20f));
                float groupingScale = group.Rewards.Count > 1 ? 1.12f : 1f;
                pair.Gold.localScale = pair.Xp.localScale = Vector3.one * envelope * groupingScale;
                pair.Gold.anchoredPosition = ground + new Vector2(-17, 2);
                pair.Xp.anchoredPosition = ground + new Vector2(17, 2);
                return;
            }
            if (!pair.Flying) { pair.Flying = true; pair.FlightStart = ground; }
            pair.Gold.localScale = pair.Xp.localScale = Vector3.one;
            Vector2 goldTarget = canvasRect.InverseTransformPoint(goldAnchor.position);
            Vector2 xpTarget = canvasRect.InverseTransformPoint(xpAnchor.position);
            pair.Gold.anchoredPosition = Arc(pair.FlightStart + new Vector2(-17, 2), goldTarget, group.FlightProgress);
            pair.Xp.anchoredPosition = Arc(pair.FlightStart + new Vector2(17, 2), xpTarget, group.FlightProgress);
        }
        private static Vector2 Arc(Vector2 start, Vector2 end, float t)
        {
            Vector2 control = (start + end) * .5f + new Vector2(0, 50f);
            float a = 1f - t;
            return a * a * start + 2f * a * t * control + t * t * end;
        }
        public void Release(MorpgRewardVisualGroup group)
        {
            if (!live.TryGetValue(group, out var pair)) return;
            live.Remove(group);
            if (pair.Root == null) return;
            pair.Root.SetActive(false); pool.Push(pair);
        }

        internal void NotifyCredited(int gold, int quarters, int totalGold, float totalXp)
        {
            if (this == null || closing || goldText == null || xpText == null) return;
            if (coalesceAge > .10f) { deltaGold = 0; deltaQuarters = 0; }
            deltaGold += gold; deltaQuarters += quarters;
            coalesceAge = deltaAge = pulseAge = 0f;
            startQuarters = shownQuarters;
            earnedQuarters += quarters;
            startGold = shownGold; startXp = shownXp; targetGold = totalGold; targetXp = totalXp; tweenAge = 0;
            if (ReducedMotion) { shownGold = targetGold; shownXp = targetXp; shownQuarters = earnedQuarters; tweenAge = .3f; RenderNumbers(); }
        }
        internal void TickHud(float delta, int currentGold, float currentXp)
        {
            if (this == null || closing || !isActiveAndEnabled || goldText == null || xpText == null || delta <= 0) return;
            if (targetGold != currentGold || targetXp != currentXp)
            {
                startGold = shownGold; startXp = shownXp;
                targetGold = currentGold; targetXp = currentXp; tweenAge = 0f;
            }
            tweenAge += delta; deltaAge += delta; coalesceAge += delta; pulseAge += delta;
            float t = Mathf.Clamp01(tweenAge / .30f); t = 1f - (1f - t) * (1f - t);
            shownGold = startGold + (targetGold - startGold) * t;
            shownXp = Mathf.Lerp(startXp, targetXp, t);
            shownQuarters = Mathf.Lerp(startQuarters, earnedQuarters, t);
            RenderNumbers();
            goldDelta.text = deltaAge < .65f ? "+" + deltaGold : string.Empty;
            xpDelta.text = deltaAge < .65f ? "+" + (deltaQuarters / 4m).ToString("0.##") : string.Empty;
            float pulse = ReducedFlash || ReducedMotion ? 1f : 1f + .08f * (1f - Mathf.Clamp01(pulseAge / .12f));
            goldAnchor.localScale = xpAnchor.localScale = Vector3.one * pulse;
        }
        private void RenderNumbers()
        {
            goldText.text = Math.Round(shownGold).ToString("N0");
            if (BattleManager.Instance != null
                && BattleManager.Instance.TryGetBattleExperienceProgress(
                    out float current,
                    out float required))
            {
                xpText.text = $"XP {current:0.##} / {required:0.##}";
                xpFill.rectTransform.sizeDelta = new Vector2(
                    600f * Mathf.Clamp01(current / Mathf.Max(1f, required)),
                    28f);
                return;
            }

            xpText.text = "XP " + shownXp.ToString("0.##");
            xpFill.rectTransform.sizeDelta = Vector2.zero;
        }
        internal void SnapToAuthoritative(int gold, float xp)
        {
            if (this == null || goldText == null || xpText == null || xpFill == null) return;
            shownGold = startGold = targetGold = gold;
            shownXp = startXp = targetXp = xp;
            shownQuarters = startQuarters = earnedQuarters;
            tweenAge = .3f;
            RenderNumbers();
        }

        internal void DisposeView()
        {
            closing = true; unavailable = null;
            if (this != null) Destroy(gameObject);
            live.Clear(); pool.Clear();
        }
        private void OnDisable() { if (!closing) unavailable?.Invoke(); }
        private void OnDestroy() { if (!closing) unavailable?.Invoke(); live.Clear(); pool.Clear(); }
    }
}
