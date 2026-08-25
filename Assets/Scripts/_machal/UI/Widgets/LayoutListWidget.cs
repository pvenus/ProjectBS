using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    public enum LayoutDirection
    {
        Horizontal,
        Vertical,
        Grid
    }

    /// <summary>
    /// ListWidget의 구체 구현체로, 방향(Direction) 옵션에 따라 레이아웃 그룹을 자동으로 관리합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class LayoutListWidget : ListWidget
    {
        [SerializeField] private LayoutDirection direction = LayoutDirection.Horizontal;
        [SerializeField] private Transform contentRoot;

        public LayoutDirection Direction
        {
            get => direction;
            set
            {
                direction = value;
                EnsureLayoutComponent();
            }
        }

        public override Transform ContentRoot => contentRoot != null ? contentRoot : transform;

        private readonly List<MonoBehaviour> _items = new List<MonoBehaviour>();
        public override IReadOnlyList<MonoBehaviour> Items => _items;

        public override event Action<MonoBehaviour> OnItemSelected;

        private void Awake()
        {
            EnsureLayoutComponent();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                EnsureLayoutComponent();
            }
        }

        private void EnsureLayoutComponent()
        {
            var root = ContentRoot;
            if (root == null) return;

            switch (direction)
            {
                case LayoutDirection.Horizontal:
                    if (root.GetComponent<HorizontalLayoutGroup>() == null)
                    {
                        var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
                        layout.childControlWidth = false;
                        layout.childControlHeight = false;
                        layout.childForceExpandWidth = false;
                        layout.childForceExpandHeight = false;
                    }
                    break;
                case LayoutDirection.Vertical:
                    if (root.GetComponent<VerticalLayoutGroup>() == null)
                    {
                        var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
                        layout.childControlWidth = false;
                        layout.childControlHeight = false;
                        layout.childForceExpandWidth = false;
                        layout.childForceExpandHeight = false;
                    }
                    break;
                case LayoutDirection.Grid:
                    if (root.GetComponent<GridLayoutGroup>() == null)
                    {
                        root.gameObject.AddComponent<GridLayoutGroup>();
                    }
                    break;
            }
        }

        public override void Clear()
        {
            foreach (var item in _items)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _items.Clear();

            var root = ContentRoot;
            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }
        }

        public override T AddItem<T>(T prefab)
        {
            if (prefab == null) return null;

            T instance = Instantiate(prefab, ContentRoot);
            _items.Add(instance);

            var selectable = instance.GetComponentInChildren<SelectableWidget>();
            if (selectable != null)
            {
                selectable.OnSelected += () => SelectItem(instance);
            }

            return instance;
        }

        public override void RemoveItem(MonoBehaviour item)
        {
            if (item == null) return;

            if (_items.Remove(item))
            {
                Destroy(item.gameObject);
            }
        }

        public override void SelectItem(MonoBehaviour item)
        {
            foreach (var existing in _items)
            {
                if (existing == null) continue;
                var selectable = existing.GetComponentInChildren<SelectableWidget>();
                if (selectable != null)
                {
                    selectable.IsSelected = (existing == item);
                }
            }
            OnItemSelected?.Invoke(item);
        }

        public override void Sort(Comparison<MonoBehaviour> comparison)
        {
            _items.Sort(comparison);
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null)
                {
                    _items[i].transform.SetSiblingIndex(i);
                }
            }
        }
    }
}
