using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 아이템 리스트를 동적 생성하고 관리하는 추상 베이스 위젯입니다.
    /// 뷰는 이 위젯의 구체 레이아웃 구현(Horizontal, Vertical, Grid)에 의존하지 않습니다.
    /// </summary>
    public abstract class ListWidget : MonoBehaviour
    {
        public abstract Transform ContentRoot { get; }
        public abstract IReadOnlyList<MonoBehaviour> Items { get; }

        public abstract event Action<MonoBehaviour> OnItemSelected;

        public abstract void Clear();
        public abstract T AddItem<T>(T prefab) where T : MonoBehaviour;
        public abstract void RemoveItem(MonoBehaviour item);
        public abstract void SelectItem(MonoBehaviour item);
        public abstract void Sort(Comparison<MonoBehaviour> comparison);
    }
}
