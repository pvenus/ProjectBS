using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shop
{
    /// <summary>
    /// 현재 열린 상점의 런타임 데이터.
    /// 어떤 아이템이 생성되었는지, 구매 상태가 어떤지, 상점이 열려있는지 등을 관리한다.
    /// </summary>
    [Serializable]
    public class ShopRuntimeData
    {
        [Header("Identity")]
        public string shopId;
        public ShopType shopType = ShopType.Normal;
        public string shopName = "상점";

        [Header("Runtime")]
        public List<ShopRuntimeGroup> groups = new();
        public bool isOpened;
        public bool isClosed;

        [Header("Debug")]
        public string generatedFromPoolId;
        public int seed;

        public bool HasGroups => groups != null && groups.Count > 0;

        public ShopRuntimeData()
        {
        }

        public ShopRuntimeData(string shopId, ShopType shopType = ShopType.Normal)
        {
            this.shopId = shopId;
            this.shopType = shopType;
        }

        public void Open()
        {
            isOpened = true;
            isClosed = false;
        }

        public void Close()
        {
            isOpened = false;
            isClosed = true;
        }

        public void Clear()
        {
            groups.Clear();
            isOpened = false;
            isClosed = false;
        }

        public ShopRuntimeGroup AddGroup(
            string groupId,
            string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                groupId = $"group_{groups.Count}";
            }

            ShopRuntimeGroup group =
                new ShopRuntimeGroup(
                    groupId,
                    groupName);

            groups.Add(group);
            return group;
        }

        public void AddItemToGroup(
            ShopRuntimeGroup group,
            ShopRuntimeItem item)
        {
            if (group == null)
            {
                return;
            }

            group.AddItem(item);
        }

        public ShopRuntimeGroup GetGroup(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId) || groups == null)
            {
                return null;
            }

            return groups.FirstOrDefault(x => x != null && x.groupId == groupId);
        }
    }
    [Serializable]
    public class ShopRuntimeGroup
    {
        public string groupId;
        public string groupName;
        public List<ShopRuntimeItem> items = new();

        public ShopRuntimeGroup()
        {
        }

        public ShopRuntimeGroup(
            string groupId,
            string groupName)
        {
            this.groupId = groupId;
            this.groupName = groupName;
        }

        public void AddItem(ShopRuntimeItem item)
        {
            if (item == null)
            {
                return;
            }

            if (!items.Contains(item))
            {
                items.Add(item);
            }
        }
    }
}