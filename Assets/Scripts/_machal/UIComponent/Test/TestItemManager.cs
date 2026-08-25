using UnityEngine;
using ProjectBS.Core;
using UIFramework.Interfaces;
using UIFramework.Data;
using Item;
using System.Collections.Generic;

namespace UIFramework.Test
{
    public class TestItemManager : MonoBehaviour, IRelicListProvider
    {
        [Header("Relic Collection UI Reference")]
        [SerializeField] private RelicCollectionView relicCollectionView;
        [SerializeField] private Sprite defaultLockedSilhouetteIcon;

        [Header("Relic Collection Database")]
        [SerializeField] private List<RelicSO> relicCollectionDatabase = new List<RelicSO>();
        [SerializeField] private List<string> ownedRelicIds = new List<string>();

        public event System.Action<RelicListViewData> OnRelicListChanged;

        private RelicListViewData currentData;

        private void Awake()
        {
            AppManagers.Relic = this;
        }

        private void Start()
        {
            GenerateMockData();
        }

        public RelicListViewData GetRelicList()
        {
            if (currentData == null)
            {
                GenerateMockData();
            }
            return currentData;
        }

        [ContextMenu("Refresh List")]
        public void GenerateMockData()
        {
            currentData = new RelicListViewData();

            if (relicCollectionDatabase != null)
            {
                foreach (var r in relicCollectionDatabase)
                {
                    if (r == null) continue;

                    // 보유하고 있는 유물만 상단바 리스트에 표시
                    if (ownedRelicIds.Contains(r.relicId))
                    {
                        bool isGod = r.relicId.Contains(".god.");
                        if (isGod)
                        {
                            currentData.godRelics.Add(ConvertToViewData(r, RelicType.God));
                        }
                        else
                        {
                            currentData.commonRelics.Add(ConvertToViewData(r, RelicType.Common));
                        }
                    }
                }
            }

            OnRelicListChanged?.Invoke(currentData);
        }

        private RelicItemViewData ConvertToViewData(RelicSO relic, RelicType type)
        {
            return new RelicItemViewData
            {
                id = relic.relicId,
                name = relic.DisplayName,
                description = relic.Description,
                icon = relic.icon,
                type = type,
                rarity = relic.rarity,
                isNew = false,
                isLocked = false,
                count = 1
            };
        }

        #region Relic Collection Test Methods

        [ContextMenu("Open Relic Collection")]
        public void OpenRelicCollection()
        {
            if (relicCollectionView == null)
            {
                Debug.LogWarning("[TestItemManager] RelicCollectionView 참조가 없습니다.");
                return;
            }

            relicCollectionView.ShowRelics(
                relicCollectionDatabase,
                ownedRelicIds,
                defaultLockedSilhouetteIcon,
                HandleRelicCollectionResult);
            Debug.Log("[TestItemManager] 유물 도감 UI를 열었습니다.");
        }

        [ContextMenu("Refresh Relic Collection")]
        public void RefreshRelicCollection()
        {
            if (relicCollectionView == null || !relicCollectionView.gameObject.activeInHierarchy) return;

            // 데이터 동기화
            GenerateMockData();

            relicCollectionView.RefreshRelics(
                relicCollectionDatabase,
                ownedRelicIds,
                defaultLockedSilhouetteIcon);
            Debug.Log("[TestItemManager] 유물 도감 UI를 갱신했습니다.");
        }

        [ContextMenu("Clear Relic Selection")]
        public void ClearSelection()
        {
            if (relicCollectionView != null)
            {
                relicCollectionView.ClearSelection();
                Debug.Log("[TestItemManager] 유물 선택을 해제했습니다.");
            }
        }

        [ContextMenu("Toggle First Relic Owned")]
        public void ToggleFirstRelicOwned()
        {
            if (relicCollectionDatabase.Count == 0) return;

            string firstId = relicCollectionDatabase[0].relicId;
            ToggleOwnedId(firstId);
        }

        private void ToggleOwnedId(string id)
        {
            if (ownedRelicIds.Contains(id))
            {
                ownedRelicIds.Remove(id);
                Debug.Log($"[TestItemManager] 보유 목록에서 제거됨: {id}");
            }
            else
            {
                ownedRelicIds.Add(id);
                Debug.Log($"[TestItemManager] 보유 목록에 추가됨: {id}");
            }

            RefreshRelicCollection();
        }

        [ContextMenu("Add Mock Relic")]
        public void AddMockRelic()
        {
            string mockId = $"mock_relic_{Random.Range(1000, 9999)}";
            ownedRelicIds.Add(mockId);
            
            Debug.Log($"[TestItemManager] Mock 유물 ID 보유 목록에 추가: {mockId}");
            RefreshRelicCollection();
        }

        private void HandleRelicCollectionResult(RelicCollectionResult result)
        {
            if (result.type == RelicCollectionResultType.Close)
            {
                Debug.Log("[TestItemManager] 도감 닫기 요청. UI를 숨깁니다.");
                relicCollectionView.Hide();
            }
            else if (result.type == RelicCollectionResultType.SelectRelic)
            {
                Debug.Log($"[TestItemManager] 유물 선택됨: {result.relicId}");
            }
        }

        #endregion
    }
}
