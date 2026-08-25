using System;
using System.Collections.Generic;
using UnityEngine;
using UIFramework.Data;

namespace UIFramework.View
{
    [AutoBindPrefix("UI")]
    public class FaithSummaryView : UIView
    {
        [Header("Hierarchy Bindings")]
        [AutoBind] [SerializeField] private Transform VerticalGroup;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject faithSummaryItemPrefab;

        private readonly List<GameObject> _spawnedItems = new List<GameObject>();
        private Action<FaithSummaryItemViewData> _onItemSelected;

        private void Awake()
        {
            ClearItems();
        }

        public void Show(FaithSummaryUIViewData data, Action<FaithSummaryItemViewData> onItemSelected)
        {
            _onItemSelected = onItemSelected;
            base.Show();
            Refresh(data);
        }

        public void Refresh(FaithSummaryUIViewData data)
        {
            ClearItems();

            if (data != null && data.items != null && VerticalGroup != null && faithSummaryItemPrefab != null)
            {
                foreach (var itemData in data.items)
                {
                    GameObject itemGo = Instantiate(faithSummaryItemPrefab, VerticalGroup);
                    _spawnedItems.Add(itemGo);

                    var itemView = itemGo.GetComponent<FaithSummaryItemView>();
                    if (itemView != null)
                    {
                        itemView.Bind(itemData, HandleItemClicked);
                    }
                }
            }
        }

        private void ClearItems()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            _spawnedItems.Clear();

            // 에디터 디자인 시점에 배치된 디자인용 자리 표시(Placeholder) 자식들을 런타임 시작 시 정리
            if (VerticalGroup != null)
            {
                var childrenToDestroy = new List<GameObject>();
                foreach (Transform child in VerticalGroup)
                {
                    childrenToDestroy.Add(child.gameObject);
                }
                
                foreach (var childGo in childrenToDestroy)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(childGo);
                    }
                }
            }
        }

        private void HandleItemClicked(FaithSummaryItemViewData itemData)
        {
            _onItemSelected?.Invoke(itemData);
        }
    }
}
