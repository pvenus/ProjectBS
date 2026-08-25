using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIFramework.Widget
{
    public enum TabAlignment
    {
        Horizontal,
        Vertical
    }

    [ExecuteAlways]
    public class UITabWidget : MonoBehaviour
    {
        [Header("Tab Generation Settings")]
        [SerializeField] private TabAlignment alignment = TabAlignment.Horizontal;
        [SerializeField] private GameObject tabButtonPrefab;
        [SerializeField] private float spacing = 10f;

        [Header("Tab Visual Colors")]
        [SerializeField] private Color normalColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        [SerializeField] private Color selectedColor = Color.white;

        [Header("Pages Setup (Assign GameObjects here)")]
        // 인덱스 기반으로 탭과 토글 매핑할 페이지 오브젝트 리스트
        [SerializeField] private List<GameObject> tabPages = new List<GameObject>();

        // 런타임 및 에디터 동적 관리용 버튼 리스트
        [SerializeField] private List<Button> _generatedButtons = new List<Button>();
        public List<Button> GeneratedButtons => _generatedButtons;

        private int _currentActiveIndex = -1;
        public event Action<int> OnTabChanged;

        // 중복 생성 및 MissingReference를 방지하기 위해 매번 씬의 자식 이름을 기준으로 검색/생성하는 프로퍼티
        private Transform TabButtonsRoot
        {
            get
            {
                var root = transform.Find("TabButtonsRoot");
                if (root == null)
                {
                    var rootGo = new GameObject("TabButtonsRoot", typeof(RectTransform));
                    rootGo.transform.SetParent(transform, false);
                    root = rootGo.transform;
                }
                return root;
            }
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                Debug.Log($"[UITabWidget] Start - Initializing. Tab Pages Count: {tabPages.Count}");
                BindButtons(); // 런타임 수집 및 이벤트 바인딩 일괄 수행
                if (tabPages.Count > 0)
                {
                    SelectTab(0);
                }
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // OnValidate 시점의 직렬화 충돌을 방지하기 위해 에디터 delayCall로 레이아웃 정렬 정밀 갱신
            UnityEditor.EditorApplication.delayCall -= DelayUpdateLayout;
            UnityEditor.EditorApplication.delayCall += DelayUpdateLayout;
#endif
        }

        private void DelayUpdateLayout()
        {
#if UNITY_EDITOR
            if (this == null) return;
            UpdateLayoutGroup();
#endif
        }

        private void CollectGeneratedButtons()
        {
            _generatedButtons.Clear();
            var root = transform.Find("TabButtonsRoot");
            if (root != null)
            {
                foreach (Transform child in root)
                {
                    // 자식 또는 본인에게서 Button 컴포넌트 안전 수집
                    var btn = child.GetComponentInChildren<Button>(true);
                    if (btn != null)
                    {
                        _generatedButtons.Add(btn);
                    }
                }
            }
        }

        private void BindButtons()
        {
            _generatedButtons.Clear();
            var root = transform.Find("TabButtonsRoot");
            if (root == null)
            {
                Debug.LogWarning("[UITabWidget] BindButtons - TabButtonsRoot not found! Cannot bind buttons.");
                return;
            }

            int boundCount = 0;
            foreach (Transform child in root)
            {
                var btn = child.GetComponentInChildren<Button>(true);
                if (btn != null)
                {
                    _generatedButtons.Add(btn);
                    
                    int index = boundCount;
                    // 동적 리스너가 중복 등록되지 않도록 정리 후 확실히 추가
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"[UITabWidget] Button Clicked! Index: {index}");
                        SelectTab(index);
                    });
                    boundCount++;
                }
            }

            Debug.Log($"[UITabWidget] BindButtons Completed - Total Bound Buttons: {boundCount}");
        }

        [ContextMenu("Test Select Tab 0")] public void TestSelectTab0() => SelectTab(0);
        [ContextMenu("Test Select Tab 1")] public void TestSelectTab1() => SelectTab(1);
        [ContextMenu("Test Select Tab 2")] public void TestSelectTab2() => SelectTab(2);
        [ContextMenu("Test Select Tab 3")] public void TestSelectTab3() => SelectTab(3);

        public void SelectTab(int index)
        {
            Debug.Log($"[UITabWidget] SelectTab - Index: {index}, Pages Count: {tabPages.Count}");
            if (index < 0 || index >= tabPages.Count)
            {
                Debug.LogWarning($"[UITabWidget] SelectTab - Index {index} is out of bounds! (TabPages Count: {tabPages.Count})");
                return;
            }

            _currentActiveIndex = index;

            // 페이지 토글 활성화/비활성화
            for (int i = 0; i < tabPages.Count; i++)
            {
                if (tabPages[i] != null)
                {
                    tabPages[i].SetActive(i == index);
                }
            }

            // 실시간 상태 반영을 위한 버튼 수집
            CollectGeneratedButtons();

            // 버튼 색상 피드백 변경 (자식 컴포넌트에서 안전하게 Image 획득)
            for (int i = 0; i < _generatedButtons.Count; i++)
            {
                if (_generatedButtons[i] != null)
                {
                    var img = _generatedButtons[i].GetComponentInChildren<Image>(true);
                    if (img != null)
                    {
                        img.color = (i == index) ? selectedColor : normalColor;
                    }
                }
            }

            OnTabChanged?.Invoke(index);

#if UNITY_EDITOR
            // 에디터 타임에도 씬 데이터가 정상 갱신되도록 처리
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(gameObject);
                foreach (var page in tabPages)
                {
                    if (page != null) UnityEditor.EditorUtility.SetDirty(page);
                }
            }
#endif
        }

        [ContextMenu("Rebuild Tab Widget")]
        public void RebuildTabWidget()
        {
            // 1. 기존 생성된 자식들 및 버튼 청소
            ClearGeneratedButtons();

            if (tabButtonPrefab == null)
            {
                Debug.LogWarning("[UITabWidget] Tab Button Prefab is not assigned! Cannot build tabs.");
                return;
            }

            // 2. 레이아웃 셋업
            UpdateLayoutGroup();

            // 3. 페이지 설정 수만큼 버튼 동적 생성 및 이벤트 바인딩
            var root = TabButtonsRoot;
            for (int i = 0; i < tabPages.Count; i++)
            {
                var page = tabPages[i];
                string tabName = page != null ? page.name : $"Tab {i + 1}";

                var btnGo = Instantiate(tabButtonPrefab, root);
                btnGo.name = $"TabButton_{i}_{tabName}";

                // 텍스트 바인딩
                var tmpText = btnGo.GetComponentInChildren<TMP_Text>();
                if (tmpText != null)
                {
                    tmpText.text = tabName;
                }
                else
                {
                    var normalText = btnGo.GetComponentInChildren<Text>();
                    if (normalText != null)
                    {
                        normalText.text = tabName;
                    }
                }

                var btn = btnGo.GetComponentInChildren<Button>(true);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(btnGo);
                if (btn != null) UnityEditor.EditorUtility.SetDirty(btn);
#endif
            }

            CollectGeneratedButtons();

            // 런타임에 Rebuild 되는 경우 새로 생성된 버튼들에 클릭 리스너를 즉시 할당하도록 바인딩 호출
            if (Application.isPlaying)
            {
                BindButtons();
            }

            // 초기 탭 선택 적용
            if (tabPages.Count > 0)
            {
                SelectTab(0);
            }

            // 에디터/런타임 레이아웃 강제 즉시 갱신 (겹침 방지)
            var rect = root.GetComponent<RectTransform>();
            if (rect != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(root.gameObject);
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif

            Debug.Log($"[UITabWidget] Successfully rebuilt {tabPages.Count} tabs under TabButtonsRoot.");
        }

        private void UpdateLayoutGroup()
        {
            var root = transform.Find("TabButtonsRoot");
            if (root == null) return; // 에디터 셋업 전이면 리턴

            HorizontalLayoutGroup horGroup = root.GetComponent<HorizontalLayoutGroup>();
            VerticalLayoutGroup verGroup = root.GetComponent<VerticalLayoutGroup>();

            if (alignment == TabAlignment.Horizontal)
            {
                if (verGroup != null)
                {
                    DestroyImmediate(verGroup);
                }
                if (horGroup == null)
                {
                    horGroup = root.gameObject.AddComponent<HorizontalLayoutGroup>();
                }
                horGroup.spacing = spacing;
                horGroup.childAlignment = TextAnchor.MiddleCenter;
                
                // 프리팹 원래 디자인 크기 유지
                horGroup.childControlWidth = false;
                horGroup.childControlHeight = false;
                horGroup.childForceExpandWidth = false;
                horGroup.childForceExpandHeight = false;
            }
            else
            {
                if (horGroup != null)
                {
                    DestroyImmediate(horGroup);
                }
                if (verGroup == null)
                {
                    verGroup = root.gameObject.AddComponent<VerticalLayoutGroup>();
                }
                verGroup.spacing = spacing;
                verGroup.childAlignment = TextAnchor.MiddleCenter;
                
                // 프리팹 원래 디자인 크기 유지
                verGroup.childControlWidth = false;
                verGroup.childControlHeight = false;
                verGroup.childForceExpandWidth = false;
                verGroup.childForceExpandHeight = false;
            }

            // Layout Fitter를 통해 전체 부모 영역 자동 크기 동기화
            var contentSizeFitter = root.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = root.gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 정렬 컴포넌트 셋업 후 즉시 강제 갱신
            var rect = root.GetComponent<RectTransform>();
            if (rect != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        private void ClearGeneratedButtons()
        {
            _generatedButtons.Clear();

            var root = transform.Find("TabButtonsRoot");
            if (root != null)
            {
                List<GameObject> children = new List<GameObject>();
                foreach (Transform child in root)
                {
                    if (child != null)
                    {
                        children.Add(child.gameObject);
                    }
                }
                foreach (var child in children)
                {
                    if (child != null)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(child);
                        }
                        else
                        {
                            DestroyImmediate(child);
                        }
                    }
                }
            }
        }
    }
}
