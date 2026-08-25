using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework.Data;
using UIFramework.Widget;

namespace UIFramework.View
{
    [AutoBindPrefix("UI")]
    public class FaithDetailView : UIView
    {
        [Header("Tabs")]
        [SerializeField] private UITabWidget tabWidget;
        [SerializeField] private Button[] tabButtons;
        [SerializeField] private TMP_Text[] tabTexts;

        [Header("Track (ProgressBar)")]
        [SerializeField] private Slider trackProgressBar;
        [SerializeField] private FaithNodeView[] trackNodes;
        [SerializeField] private TMP_Text currentLevelText;
        [SerializeField] private TMP_Text faithNameText;

        [Header("Effect Cards")]
        [SerializeField] private Transform currentEffectsRoot;
        [SerializeField] private Transform nextEffectsRoot;
        [SerializeField] private FaithEffectItemView effectItemPrefab;

        [Header("Desc Panel Bindings")]
        [SerializeField] private RectTransform panelDesc;
        [SerializeField] private TMP_Text descTitleText;
        [SerializeField] private TMP_Text descContentText;

        private FaithDetailUIViewData _currentData;
        private int _selectedFaithIndex = 0;

        private void Awake()
        {
            if (panelDesc != null)
            {
                panelDesc.gameObject.SetActive(false);
            }

            // tabWidget이 인스펙터에서 누락된 경우 자식 계층에서 동적 자동 수집 시도
            if (tabWidget == null)
            {
                tabWidget = GetComponentInChildren<UITabWidget>(true);
            }

            // tabWidget이 존재하면 최우선으로 연동
            if (tabWidget != null)
            {
                tabWidget.OnTabChanged += OnTabClicked;
            }
            else if (tabButtons != null)
            {
                for (int i = 0; i < tabButtons.Length; i++)
                {
                    int index = i;
                    if (tabButtons[i] != null)
                    {
                        tabButtons[i].onClick.AddListener(() => OnTabClicked(index));
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (tabWidget != null)
            {
                tabWidget.OnTabChanged -= OnTabClicked;
            }
        }

        public void Show(FaithDetailUIViewData data)
        {
            base.Show();
            Refresh(data);
        }

        public void Refresh(FaithDetailUIViewData data)
        {
            _currentData = data;
            if (_currentData == null || _currentData.faithColumns == null || _currentData.faithColumns.Count == 0) return;

            if (tabWidget != null)
            {
                // UITabWidget의 자식 버튼들을 돌면서 이름 텍스트 설정
                var buttons = tabWidget.GeneratedButtons;
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (i < _currentData.faithColumns.Count)
                    {
                        buttons[i].gameObject.SetActive(true);
                        var tmpText = buttons[i].GetComponentInChildren<TMP_Text>(true);
                        if (tmpText != null)
                        {
                            tmpText.text = _currentData.faithColumns[i].displayName;
                        }
                        else
                        {
                            var normalText = buttons[i].GetComponentInChildren<Text>(true);
                            if (normalText != null)
                            {
                                normalText.text = _currentData.faithColumns[i].displayName;
                            }
                        }
                    }
                    else
                    {
                        buttons[i].gameObject.SetActive(false);
                    }
                }

                // 탭 위젯 상태와 동기화
                tabWidget.SelectTab(_selectedFaithIndex);
            }
            else
            {
                // 레거시 탭 버튼 바인딩 (이름 등 갱신)
                for (int i = 0; i < tabButtons.Length; i++)
                {
                    if (i < _currentData.faithColumns.Count)
                    {
                        tabButtons[i].gameObject.SetActive(true);
                        if (tabTexts != null && i < tabTexts.Length && tabTexts[i] != null)
                        {
                            tabTexts[i].text = _currentData.faithColumns[i].displayName;
                        }
                    }
                    else
                    {
                        tabButtons[i].gameObject.SetActive(false);
                    }
                }
            }

            UpdateSelection();
        }

        private void OnTabClicked(int index)
        {
            _selectedFaithIndex = index;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (_currentData == null || _currentData.faithColumns == null || _selectedFaithIndex >= _currentData.faithColumns.Count) return;

            var colData = _currentData.faithColumns[_selectedFaithIndex];

            // 탭 선택 상태 연출 (색상 변경 - tabWidget이 없을 때만 적용)
            if (tabWidget == null)
            {
                for (int i = 0; i < tabButtons.Length; i++)
                {
                    if (tabButtons[i] != null)
                    {
                        var img = tabButtons[i].GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = (i == _selectedFaithIndex) ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.8f);
                        }
                    }
                }
            }

            // 신앙 이름 및 현재 레벨 갱신
            if (faithNameText != null)
            {
                faithNameText.text = colData.displayName;
            }
            if (currentLevelText != null)
            {
                currentLevelText.text = $"Lv. {colData.currentLevel}";
            }

            // 10칸 Progress Bar 갱신
            if (trackProgressBar != null)
            {
                trackProgressBar.value = colData.levelProgress01;
            }

            // 10개 트랙 노드 갱신
            if (trackNodes != null)
            {
                for (int i = 0; i < trackNodes.Length; i++)
                {
                    if (i < colData.nodes.Count && trackNodes[i] != null)
                    {
                        trackNodes[i].gameObject.SetActive(true);
                        trackNodes[i].Bind(colData.nodes[i], HandleNodeClicked);
                    }
                    else if (trackNodes[i] != null)
                    {
                        trackNodes[i].gameObject.SetActive(false);
                    }
                }
            }

            // 현재 적용 중인 효과 리스트 갱신
            ClearRootChildren(currentEffectsRoot);
            if (currentEffectsRoot != null && effectItemPrefab != null && colData.currentEffects != null)
            {
                foreach (var eff in colData.currentEffects)
                {
                    var item = Instantiate(effectItemPrefab, currentEffectsRoot);
                    item.Bind(eff);
                }
            }

            // 다음 레벨 해금 예정 효과 리스트 갱신
            ClearRootChildren(nextEffectsRoot);
            if (nextEffectsRoot != null && effectItemPrefab != null && colData.nextEffects != null)
            {
                foreach (var eff in colData.nextEffects)
                {
                    var item = Instantiate(effectItemPrefab, nextEffectsRoot);
                    item.Bind(eff);
                }
            }

            // 설명 패널 초기화
            if (panelDesc != null)
            {
                panelDesc.gameObject.SetActive(false);
            }
        }

        private void ClearRootChildren(Transform root)
        {
            if (root == null) return;
            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }
        }

        private void HandleNodeClicked(FaithNodeViewData nodeData)
        {
            if (panelDesc != null)
            {
                panelDesc.gameObject.SetActive(true);

                if (descTitleText != null)
                {
                    descTitleText.text = nodeData.title;
                }

                if (descContentText != null)
                {
                    if (nodeData.isUnlocked)
                    {
                        descContentText.text = nodeData.description;
                    }
                    else
                    {
                        // 잠금 노드 클릭 시 안내 툴팁 형태로 출력
                        descContentText.text = $"<color=#AAAAAA>[잠금 상태] 신앙 레벨 {nodeData.requiredLevel} 달성 시 해금됩니다.</color>\n\n<color=#888888>{nodeData.description}</color>";
                    }
                }
            }
        }
    }

    [AutoBindPrefix("UI")]
    public class FaithEffectItemView : UIComponent
    {
        [AutoBind] [SerializeField] private Image UI_Icon;
        [AutoBind] [SerializeField] private TMP_Text UI_TitleText;
        [AutoBind] [SerializeField] private TMP_Text UI_DescText;

        public void Bind(FaithEffectItemData data)
        {
            if (data == null) return;

            if (UI_Icon != null)
            {
                if (data.icon != null)
                {
                    UI_Icon.sprite = data.icon;
                    UI_Icon.gameObject.SetActive(true);
                }
                else
                {
                    UI_Icon.gameObject.SetActive(false);
                }
            }

            if (UI_TitleText != null)
            {
                UI_TitleText.text = data.title;
            }

            if (UI_DescText != null)
            {
                UI_DescText.text = data.description;
            }
        }
    }
}
