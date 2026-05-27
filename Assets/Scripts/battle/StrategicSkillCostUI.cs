

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    /// <summary>
    /// 전략 스킬 코스트 게이지 UI.
    /// 클래시 로얄 엘릭서처럼 현재 게이지가 차오르는 모습을 보여주고,
    /// 숫자와 칸 단위 표시를 함께 지원한다.
    /// </summary>
    public class StrategicSkillCostUI : MonoBehaviour
    {
        [Header("Gauge Fill")]
        [SerializeField] private Image fillImage;
        [SerializeField] private bool useSmoothFill = true;
        [SerializeField] private float fillSmoothSpeed = 8f;

        [Header("Text")]
        [SerializeField] private TMP_Text gaugeText;
        [SerializeField] private bool showMaxGauge = true;

        [Header("Segments")]
        [SerializeField] private Transform segmentRoot;
        [SerializeField] private Image segmentPrefab;
        [SerializeField] private int segmentCount = 10;
        [SerializeField] private bool rebuildSegmentsOnStart = true;

        [Header("Segment Visual")]
        [SerializeField] private Color activeSegmentColor = Color.white;
        [SerializeField] private Color inactiveSegmentColor = new Color(1f, 1f, 1f, 0.25f);

        private readonly List<Image> segments = new List<Image>();

        [Header("Manager")]
        [SerializeField] private StrategicSkillCostManager managerOverride;
        [SerializeField] private bool findManagerInScene = true;

        private int currentGauge;
        private int maxGauge = 100;
        private float currentFillRate;
        private float targetFillRate;
        private StrategicSkillCostManager subscribedManager;

        private void Start()
        {
            if (rebuildSegmentsOnStart)
            {
                RebuildSegments();
            }

            TrySubscribeManager();
            RefreshFromManager();
        }

        private void OnEnable()
        {
            TrySubscribeManager();
        }

        private void OnDisable()
        {
            UnsubscribeManager();
        }

        private void Update()
        {
            if (subscribedManager == null)
            {
                TrySubscribeManager();
            }

            if (!useSmoothFill)
            {
                return;
            }

            if (Mathf.Approximately(currentFillRate, targetFillRate))
            {
                return;
            }

            currentFillRate = Mathf.Lerp(currentFillRate, targetFillRate, Time.deltaTime * fillSmoothSpeed);
            ApplyFill(currentFillRate);
        }

        private StrategicSkillCostManager ResolveManager()
        {
            if (managerOverride != null)
            {
                return managerOverride;
            }

            if (StrategicSkillCostManager.Instance != null)
            {
                return StrategicSkillCostManager.Instance;
            }

            if (!findManagerInScene)
            {
                return null;
            }

#if UNITY_2023_1_OR_NEWER
            return FindFirstObjectByType<StrategicSkillCostManager>();
#else
            return FindObjectOfType<StrategicSkillCostManager>();
#endif
        }

        private void TrySubscribeManager()
        {
            StrategicSkillCostManager manager = ResolveManager();

            if (manager == null)
            {
                return;
            }

            if (subscribedManager == manager)
            {
                return;
            }

            UnsubscribeManager();

            subscribedManager = manager;
            subscribedManager.OnGaugeChanged += HandleGaugeChanged;
            SetGauge(subscribedManager.CurrentGauge, subscribedManager.MaxGauge, true);
            subscribedManager.ForceNotifyGaugeChanged();
        }

        private void UnsubscribeManager()
        {
            if (subscribedManager == null)
            {
                return;
            }

            subscribedManager.OnGaugeChanged -= HandleGaugeChanged;
            subscribedManager = null;
        }

        private void RefreshFromManager()
        {
            StrategicSkillCostManager manager = ResolveManager();

            if (manager != null)
            {
                SetGauge(manager.CurrentGauge, manager.MaxGauge, true);
            }
            else
            {
                SetGauge(0, maxGauge, true);
            }
        }

        private void HandleGaugeChanged(int current, int max)
        {
            SetGauge(current, max, false);
        }

        private void SetGauge(int current, int max, bool immediate)
        {
            currentGauge = Mathf.Max(0, current);
            maxGauge = Mathf.Max(1, max);
            targetFillRate = Mathf.Clamp01((float)currentGauge / maxGauge);

            if (immediate || !useSmoothFill)
            {
                currentFillRate = targetFillRate;
                ApplyFill(currentFillRate);
            }

            ApplyText();
            ApplySegments();
        }

        private void ApplyFill(float fillRate)
        {
            if (fillImage == null)
            {
                return;
            }

            fillImage.fillAmount = Mathf.Clamp01(fillRate);
        }

        private void ApplyText()
        {
            if (gaugeText == null)
            {
                return;
            }

            if (showMaxGauge)
            {
                gaugeText.text = $"{currentGauge}/{maxGauge}";
            }
            else
            {
                gaugeText.text = currentGauge.ToString();
            }
        }

        private void ApplySegments()
        {
            if (segments.Count <= 0)
            {
                return;
            }

            float gaugePerSegment = (float)maxGauge / segments.Count;

            for (int i = 0; i < segments.Count; i++)
            {
                Image segment = segments[i];

                if (segment == null)
                {
                    continue;
                }

                float requiredGauge = gaugePerSegment * (i + 1);
                bool isActive = currentGauge >= requiredGauge;
                segment.color = isActive ? activeSegmentColor : inactiveSegmentColor;
            }
        }

        [ContextMenu("Rebuild Segments")]
        public void RebuildSegments()
        {
            segments.Clear();

            if (segmentRoot == null)
            {
                return;
            }

            for (int i = segmentRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = segmentRoot.GetChild(i);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(child.gameObject);
                }
                else
#endif
                {
                    Destroy(child.gameObject);
                }
            }

            if (segmentPrefab == null || segmentCount <= 0)
            {
                return;
            }

            for (int i = 0; i < segmentCount; i++)
            {
                Image segment = Instantiate(segmentPrefab, segmentRoot);
                segment.gameObject.SetActive(true);
                segments.Add(segment);
            }

            ApplySegments();
        }
    }
}