using System;
using UnityEngine;
using UIFramework.Data;
using UIFramework.Interfaces;
using Item;

namespace UIFramework.Provider
{
    public class RealRelicListProvider : MonoBehaviour, IRelicListProvider
    {
        public event Action<RelicListViewData> OnRelicListChanged;

        private void OnEnable()
        {
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.OnRelicAdded += HandleRelicChanged;
                ItemManager.Instance.OnRelicRemoved += HandleRelicChanged;
            }
        }

        private void OnDisable()
        {
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.OnRelicAdded -= HandleRelicChanged;
                ItemManager.Instance.OnRelicRemoved -= HandleRelicChanged;
            }
        }

        private void HandleRelicChanged(RelicSO relic)
        {
            OnRelicListChanged?.Invoke(GetRelicList());
        }

        public RelicListViewData GetRelicList()
        {
            var data = new RelicListViewData();

            if (ItemManager.Instance != null && ItemManager.Instance.RelicRuntimeData != null)
            {
                foreach (var entry in ItemManager.Instance.RelicRuntimeData.Relics)
                {
                    if (entry == null || entry.relic == null) continue;

                    bool isGodRelic = !string.IsNullOrEmpty(entry.relic.relicId) && entry.relic.relicId.Contains(".god.");
                    RelicType type = isGodRelic ? RelicType.God : RelicType.Common;

                    var viewData = new RelicItemViewData
                    {
                        id = entry.relic.relicId,
                        name = entry.relic.DisplayName,
                        description = entry.relic.Description,
                        icon = entry.relic.icon,
                        type = type,
                        rarity = entry.relic.rarity,
                        isNew = false,
                        isLocked = !entry.isEquipped, // 임시로 장착 안 된 것을 locked 개념으로 매핑
                        count = 1
                    };

                    if (isGodRelic) data.godRelics.Add(viewData);
                    else data.commonRelics.Add(viewData);
                }
            }

            return data;
        }
    }
}
