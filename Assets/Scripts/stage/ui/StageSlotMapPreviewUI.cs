using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Stage;

namespace Stage.UI
{
    /// <summary>
    /// StageDefinitionSO.svgMapSlots를 읽어 그리드 기반 슬롯맵 프리뷰 UI를 표시한다.
    /// StageGraph 생성 없이, RoundNodeSO 배정 없이 SVG 맵 토폴로지를 확인하는 목적의 컴포넌트다.
    ///
    /// 배치 규칙:
    ///  - Y축: depth 기준 (growUpwards=true이면 depth 0이 아래, false이면 위)
    ///  - X축: orderInDepth 기준, 각 depth 행은 중앙 정렬
    ///
    /// 주의: gridPosition / uiPosition / sourcePosition은 StageDefinitionSO에 저장하지 않는다.
    ///       모든 배치 좌표는 이 컴포넌트에서 런타임에 계산한다.
    /// </summary>
    public class StageSlotMapPreviewUI : MonoBehaviour
    {
        // ─── Data Source ──────────────────────────────────────────────────────────

        [Header("Data Source")]
        [Tooltip("미리볼 StageDefinitionSO. svgMapSlots가 채워져 있어야 한다.")]
        [SerializeField] private StageDefinitionSO stageDefinition;

        // ─── Layout Roots ─────────────────────────────────────────────────────────

        [Header("Layout Roots")]
        [Tooltip("노드와 라벨이 배치될 콘텐츠 루트 RectTransform.")]
        [SerializeField] private RectTransform contentRoot;

        [Tooltip("연결선이 배치될 루트. 비워두면 contentRoot를 사용한다. " +
                 "노드 뒤에 선을 그리려면 이 루트가 contentRoot의 형제이며 먼저 렌더링되도록 설정한다.")]
        [SerializeField] private RectTransform pathRoot;

        [Tooltip("활성화 시 자동으로 Rebuild를 실행한다.")]
        [SerializeField] private bool rebuildOnEnable = true;

        [Tooltip("SVG 슬롯맵 프리뷰를 활성화한다. false로 끄면 씬에서 프리뷰가 즉시 제거된다.")]
        [SerializeField] private bool enablePreview = true;

        // ─── Grid Settings ────────────────────────────────────────────────────────

        [Header("Grid Settings")]
        [Tooltip("같은 depth 행 내 슬롯 간 수평 간격 (px)")]
        [SerializeField] private float cellWidth = 150f;

        [Tooltip("depth 간 수직 간격 (px)")]
        [SerializeField] private float depthSpacing = 180f;

        [Tooltip("콘텐츠 영역 최소 여백 (x: 좌우, y: 상하)")]
        [SerializeField] private Vector2 padding = new Vector2(80f, 80f);

        [Tooltip("true: depth 0이 아래, 위로 올라갈수록 depth 증가 (chapter1 기본). " +
                 "false: depth 0이 위, 아래로 내려갈수록 depth 증가.")]
        [SerializeField] private bool growUpwards = true;

        // ─── Story Node Visuals ───────────────────────────────────────────────────

        [Header("Story Node Visuals")]
        [Tooltip("Story 슬롯 원형 크기 (px)")]
        [SerializeField] private float storyNodeSize = 80f;

        [Tooltip("Story 슬롯 배경 색상")]
        [SerializeField] private Color storyNodeColor = new Color(0.18f, 0.44f, 0.90f, 1f);

        [Tooltip("Story 슬롯에 사용할 원형 스프라이트 (선택). 없으면 기본 사각형으로 표시된다.")]
        [SerializeField] private Sprite storyNodeSprite;

        // ─── Random Node Visuals ──────────────────────────────────────────────────

        [Header("Random Node Visuals")]
        [Tooltip("Random 슬롯 원형 크기 (px)")]
        [SerializeField] private float randomNodeSize = 50f;

        [Tooltip("Random 슬롯 배경 색상")]
        [SerializeField] private Color randomNodeColor = new Color(0.58f, 0.58f, 0.58f, 0.75f);

        [Tooltip("Random 슬롯에 사용할 스프라이트 (선택). 없으면 기본 사각형으로 표시된다.")]
        [SerializeField] private Sprite randomNodeSprite;

        [Tooltip("Random 슬롯 외곽 링 색상 (점선 표현 대용)")]
        [SerializeField] private Color randomRingColor = new Color(0.40f, 0.40f, 0.40f, 0.50f);

        // ─── Connection Visuals ───────────────────────────────────────────────────

        [Header("Connection Visuals")]
        [Tooltip("연결선 굵기 (px)")]
        [SerializeField] private float connectionLineWidth = 3f;

        [Tooltip("연결선 색상")]
        [SerializeField] private Color connectionColor = new Color(0.25f, 0.25f, 0.25f, 0.55f);

        // ─── Label Settings ───────────────────────────────────────────────────────

        [Header("Label Settings")]
        [Tooltip("슬롯 label 텍스트 표시 여부")]
        [SerializeField] private bool showLabels = true;

        [Tooltip("슬롯 subLabel 텍스트 표시 여부 (Story 슬롯 전용)")]
        [SerializeField] private bool showSubLabels = true;

        [Tooltip("label 폰트 크기")]
        [SerializeField] private float labelFontSize = 12f;

        [Tooltip("subLabel 폰트 크기")]
        [SerializeField] private float subLabelFontSize = 10f;

        [Tooltip("label 텍스트 색상")]
        [SerializeField] private Color labelColor = Color.black;

        [Tooltip("subLabel 텍스트 색상")]
        [SerializeField] private Color subLabelColor = new Color(0.28f, 0.28f, 0.28f, 1f);

        // ─── Runtime State ────────────────────────────────────────────────────────

        /// <summary>slotId → contentRoot 기준 anchoredPosition 캐시</summary>
        private readonly Dictionary<string, Vector2> _slotPositions = new();

        /// <summary>런타임에 생성된 모든 GameObject. Clear 시 일괄 파괴한다.</summary>
        private readonly List<GameObject> _spawnedObjects = new();

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>데이터 소스를 교체하고 프리뷰를 다시 빌드한다.</summary>
        public void SetDefinition(StageDefinitionSO definition)
        {
            stageDefinition = definition;
            Rebuild();
        }

        /// <summary>현재 stageDefinition.svgMapSlots를 기반으로 프리뷰를 재구성한다.</summary>
        [ContextMenu("Rebuild Preview")]
        public void Rebuild()
        {
            if (!enablePreview)
            {
                Clear();
                return;
            }

            Clear();

            if (!ValidateBeforeBuild())
            {
                return;
            }

            if (pathRoot == null)
            {
                pathRoot = contentRoot;
            }

            // 1. 슬롯 그리드 좌표 계산 (런타임 전용, SO에 저장하지 않음)
            CalculateSlotPositions();

            // 2. 연결선 생성 (노드 뒤에 표시되도록 pathRoot에 먼저 생성)
            DrawConnections();

            // 3. 슬롯 노드 및 라벨 생성
            DrawNodes();

            // 4. 전체 오프셋 보정 및 콘텐츠 크기 조정
            ApplyPaddingAndResize();

            Debug.Log(
                $"[StageSlotMapPreviewUI] Preview built. " +
                $"stageId={stageDefinition.stageId}, " +
                $"slots={stageDefinition.svgMapSlots.Count}, " +
                $"positionsMapped={_slotPositions.Count}, " +
                $"objectsSpawned={_spawnedObjects.Count}");
        }

        /// <summary>생성된 모든 프리뷰 오브젝트를 제거한다.</summary>
        [ContextMenu("Clear Preview")]
        public void Clear()
        {
            foreach (GameObject go in _spawnedObjects)
            {
                if (go == null)
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(go);
                }
                else
                {
                    Destroy(go);
                }
#else
                Destroy(go);
#endif
            }

            _spawnedObjects.Clear();
            _slotPositions.Clear();
        }

        // ─── Validation ───────────────────────────────────────────────────────────

        private bool ValidateBeforeBuild()
        {
            if (stageDefinition == null)
            {
                Debug.LogWarning("[StageSlotMapPreviewUI] stageDefinition is null. Assign a StageDefinitionSO.");
                return false;
            }

            if (stageDefinition.svgMapSlots == null || stageDefinition.svgMapSlots.Count == 0)
            {
                Debug.LogWarning(
                    $"[StageSlotMapPreviewUI] stageDefinition '{stageDefinition.name}' has no svgMapSlots. " +
                    "Run the SVG extraction tool first.");
                return false;
            }

            if (contentRoot == null)
            {
                Debug.LogError("[StageSlotMapPreviewUI] contentRoot is not assigned.");
                return false;
            }

            return true;
        }

        // ─── Grid Position Calculation ────────────────────────────────────────────

        /// <summary>
        /// depth/orderInDepth를 기반으로 각 슬롯의 contentRoot 기준 anchoredPosition을 계산한다.
        /// 좌표는 이 컴포넌트에만 존재하며 StageDefinitionSO에는 저장되지 않는다.
        /// </summary>
        private void CalculateSlotPositions()
        {
            _slotPositions.Clear();

            // depth별로 묶고, 각 그룹 내에서 orderInDepth 오름차순 정렬
            var byDepth = stageDefinition.svgMapSlots
                .GroupBy(s => s.depth)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.orderInDepth).ToList());

            float ySign = growUpwards ? 1f : -1f;

            foreach (KeyValuePair<int, List<StageMapSlot>> pair in byDepth)
            {
                int depth = pair.Key;
                List<StageMapSlot> slotsInRow = pair.Value;
                int count = slotsInRow.Count;

                // 행 전체를 x=0 기준으로 중앙 정렬
                float totalRowWidth = (count - 1) * cellWidth;
                float rowStartX = -totalRowWidth * 0.5f;

                float y = depth * depthSpacing * ySign;

                for (int i = 0; i < count; i++)
                {
                    float x = rowStartX + i * cellWidth;
                    _slotPositions[slotsInRow[i].slotId] = new Vector2(x, y);
                }
            }
        }

        // ─── Connection Lines ─────────────────────────────────────────────────────

        private void DrawConnections()
        {
            foreach (StageMapSlot slot in stageDefinition.svgMapSlots)
            {
                if (!_slotPositions.TryGetValue(slot.slotId, out Vector2 fromPos))
                {
                    continue;
                }

                if (slot.connections == null || slot.connections.Count == 0)
                {
                    continue;
                }

                foreach (StageSlotConnection conn in slot.connections)
                {
                    if (string.IsNullOrEmpty(conn.toSlotId))
                    {
                        continue;
                    }

                    if (!_slotPositions.TryGetValue(conn.toSlotId, out Vector2 toPos))
                    {
                        Debug.LogWarning(
                            $"[StageSlotMapPreviewUI] Connection target not found. " +
                            $"from={slot.slotId}, to={conn.toSlotId}");
                        continue;
                    }

                    GameObject lineGo = CreateConnectionLine(fromPos, toPos);
                    _spawnedObjects.Add(lineGo);
                }
            }
        }

        /// <summary>
        /// 두 점 사이에 선 역할을 하는 Image 오브젝트를 생성한다.
        /// 중점에 위치하고, 두 점을 잇는 방향으로 회전시킨 뒤, 거리만큼 너비를 늘린다.
        /// </summary>
        private GameObject CreateConnectionLine(Vector2 from, Vector2 to)
        {
            GameObject go = new GameObject("Connection");
            go.transform.SetParent(pathRoot, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Vector2 dir = to - from;
            float distance = dir.magnitude;
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rect.anchoredPosition = (from + to) * 0.5f;
            rect.sizeDelta = new Vector2(distance, connectionLineWidth);
            rect.localRotation = Quaternion.Euler(0f, 0f, angleDeg);

            Image img = go.AddComponent<Image>();
            img.color = connectionColor;
            img.raycastTarget = false;

            return go;
        }

        // ─── Slot Nodes ───────────────────────────────────────────────────────────

        private void DrawNodes()
        {
            foreach (StageMapSlot slot in stageDefinition.svgMapSlots)
            {
                if (!_slotPositions.TryGetValue(slot.slotId, out Vector2 pos))
                {
                    continue;
                }

                bool isStory = slot.role == StageMapSlotRole.Story;
                float size = isStory ? storyNodeSize : randomNodeSize;
                Color color = isStory ? storyNodeColor : randomNodeColor;
                Sprite sprite = isStory ? storyNodeSprite : randomNodeSprite;

                // 슬롯 원형 노드
                GameObject nodeGo = CreateNodeCircle(slot.slotId, pos, size, color, sprite, isStory);
                _spawnedObjects.Add(nodeGo);

                // label: 노드 위에 배치
                if (showLabels && !string.IsNullOrEmpty(slot.label))
                {
                    float labelOffsetY = size * 0.5f + 6f;
                    GameObject labelGo = CreateTextLabel(
                        name: $"Label_{slot.slotId}",
                        pos: pos + new Vector2(0f, labelOffsetY),
                        size: new Vector2(180f, 22f),
                        text: slot.label,
                        fontSize: labelFontSize,
                        color: labelColor,
                        fontStyle: FontStyles.Bold);
                    _spawnedObjects.Add(labelGo);
                }

                // subLabel: 노드 아래에 배치 (Story 슬롯만)
                if (showSubLabels && !string.IsNullOrEmpty(slot.subLabel))
                {
                    float subLabelOffsetY = -(size * 0.5f + 16f);
                    GameObject subLabelGo = CreateTextLabel(
                        name: $"SubLabel_{slot.slotId}",
                        pos: pos + new Vector2(0f, subLabelOffsetY),
                        size: new Vector2(180f, 18f),
                        text: slot.subLabel,
                        fontSize: subLabelFontSize,
                        color: subLabelColor,
                        fontStyle: FontStyles.Normal);
                    _spawnedObjects.Add(subLabelGo);
                }
            }
        }

        /// <summary>슬롯 노드를 나타내는 Image 오브젝트를 contentRoot에 생성한다.</summary>
        private GameObject CreateNodeCircle(
            string slotId, Vector2 pos, float size,
            Color color, Sprite sprite, bool isStory)
        {
            GameObject go = new GameObject($"Node_{slotId}");
            go.transform.SetParent(contentRoot, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(size, size);

            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
            }

            // Random 슬롯: 외곽 링을 추가해 점선 테두리 느낌을 낸다
            if (!isStory)
            {
                AddRandomNodeRing(go);
            }

            return go;
        }

        /// <summary>
        /// Random 슬롯 구분을 위해 노드 바깥쪽에 반투명 링 이미지를 추가한다.
        /// 링은 노드의 자식으로 생성되며 _spawnedObjects에 별도 등록하지 않는다.
        /// (부모 노드가 삭제될 때 함께 삭제된다.)
        /// </summary>
        private void AddRandomNodeRing(GameObject nodeGo)
        {
            GameObject ring = new GameObject("RandomRing");
            ring.transform.SetParent(nodeGo.transform, false);
            ring.transform.SetAsFirstSibling(); // 노드 이미지 뒤에 렌더링

            RectTransform ringRect = ring.AddComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = new Vector2(-5f, -5f);
            ringRect.offsetMax = new Vector2(5f, 5f);

            Image ringImg = ring.AddComponent<Image>();
            ringImg.color = randomRingColor;
            ringImg.raycastTarget = false;
        }

        // ─── Text Labels ──────────────────────────────────────────────────────────

        /// <summary>TextMeshProUGUI를 이용해 라벨 오브젝트를 contentRoot에 생성한다.</summary>
        private GameObject CreateTextLabel(
            string name, Vector2 pos, Vector2 size,
            string text, float fontSize, Color color, FontStyles fontStyle)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(contentRoot, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = fontStyle;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

            return go;
        }

        // ─── Content Fit ──────────────────────────────────────────────────────────

        /// <summary>
        /// 생성된 오브젝트들의 최소/최대 좌표를 기반으로 padding 오프셋을 적용하고
        /// 콘텐츠 루트의 상위 ScrollRect Content 크기를 업데이트한다.
        /// </summary>
        private void ApplyPaddingAndResize()
        {
            if (_slotPositions.Count == 0)
            {
                return;
            }

            float minX = _slotPositions.Values.Min(v => v.x);
            float maxX = _slotPositions.Values.Max(v => v.x);
            float minY = _slotPositions.Values.Min(v => v.y);
            float maxY = _slotPositions.Values.Max(v => v.y);

            // 슬롯 위치는 이미 x=0 중심으로 생성되므로 offsetX ≈ 0.
            // y는 최솟값이 padding.y 위치에 오도록 올린다.
            float offsetX = -(minX + maxX) * 0.5f;
            float offsetY = -minY + padding.y;

            // _spawnedObjects에 등록된 모든 RectTransform 일괄 이동
            foreach (GameObject go in _spawnedObjects)
            {
                if (go == null)
                {
                    continue;
                }

                RectTransform rect = go.GetComponent<RectTransform>();
                if (rect == null)
                {
                    continue;
                }

                rect.anchoredPosition += new Vector2(offsetX, offsetY);
            }

            // ScrollRect Content 크기 조정
            float contentWidth = (maxX - minX) + padding.x * 2f;
            float contentHeight = (maxY - minY) + padding.y * 2f;

            if (contentRoot != null)
            {
                RectTransform scrollContent = contentRoot.parent as RectTransform;
                if (scrollContent != null)
                {
                    // ContentSizeFitter가 있으면 비활성화 (충돌 방지)
                    ContentSizeFitter fitter = scrollContent.GetComponent<ContentSizeFitter>();
                    if (fitter != null)
                    {
                        fitter.enabled = false;
                    }

                    scrollContent.sizeDelta = new Vector2(contentWidth, contentHeight);
                }
                else
                {
                    // 상위 ScrollContent가 없으면 contentRoot 자체를 조정
                    contentRoot.sizeDelta = new Vector2(contentWidth, contentHeight);
                }
            }

            Debug.Log(
                $"[StageSlotMapPreviewUI] Content size set: {contentWidth:F0} x {contentHeight:F0}. " +
                $"Slot bounds: x[{minX:F0}~{maxX:F0}] y[{minY:F0}~{maxY:F0}]");
        }

        // ─── Unity Lifecycle ──────────────────────────────────────────────────────

        private void OnEnable()
        {
            if (!enablePreview || !rebuildOnEnable)
            {
                return;
            }

            if (stageDefinition != null
                && stageDefinition.svgMapSlots != null
                && stageDefinition.svgMapSlots.Count > 0)
            {
                Rebuild();
            }
        }

        private void OnDisable()
        {
            Clear();
        }
    }
}
