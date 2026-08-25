using System;
using UnityEngine;
using TMPro;
using UIFramework.Data;

namespace UIFramework.View
{
    [AutoBindPrefix("UI")]
    public class FaithColumnView : UIComponent
    {
        [AutoBind]
        [SerializeField] private TMP_Text UI_NameText;

        [AutoBind]
        [SerializeField] private Transform Nodes;

        private FaithNodeView[] _nodeViews;

        private void Awake()
        {
            RefreshNodeViews();
        }

        private void RefreshNodeViews()
        {
            if (Nodes != null)
            {
                _nodeViews = Nodes.GetComponentsInChildren<FaithNodeView>(true);
            }
            else
            {
                _nodeViews = GetComponentsInChildren<FaithNodeView>(true);
            }
        }

        public void Bind(FaithColumnViewData data, Action<FaithNodeViewData> onNodeClick)
        {
            if (data == null) return;

            if (UI_NameText == null || !UI_NameText.transform.IsChildOf(transform))
            {
                UI_NameText = GetComponentInChildren<TMP_Text>(true);
            }

            if (UI_NameText != null)
            {
                UI_NameText.text = data.displayName;
            }

            // 에디터 상에서 다른 컬럼의 노드가 잘못 바인딩되었거나 비어있을 경우를 대비한 런타임 재수집
            if (_nodeViews == null || _nodeViews.Length == 0 || (_nodeViews[0] != null && !_nodeViews[0].transform.IsChildOf(transform)))
            {
                RefreshNodeViews();
            }

            if (_nodeViews != null && data.nodes != null)
            {
                for (int i = 0; i < _nodeViews.Length; i++)
                {
                    if (i < data.nodes.Count)
                    {
                        _nodeViews[i].gameObject.SetActive(true);
                        _nodeViews[i].Bind(data.nodes[i], onNodeClick);
                    }
                    else
                    {
                        _nodeViews[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
