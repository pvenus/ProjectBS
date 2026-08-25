//using UnityEngine;
//using ProjectBS.Core;
//using UIFramework.Data;

//namespace UIFramework.Test
//{
//    public class TestShopManager : MonoBehaviour, IShopManager
//    {
//        [Header("Shop UI Prefab")]
//        public ShopPage shopPagePrefab;
//        private ShopPage shopPageInstance;

//        [Header("Mock Shop Pools (Optional)")]
//        public System.Collections.Generic.List<Shop.ShopItemPoolSO> mockShopPools = new System.Collections.Generic.List<Shop.ShopItemPoolSO>();

//        [Header("Shop Event Popup Data (Test)")]
//        public EventPopupTestData shopEventPopupData;

//        [Header("Generation Settings")]
//        public int itemsPerPool = 3;

//        private void Awake()
//        {
//            AppManagers.Shop = this;

//            if (shopEventPopupData == null || shopEventPopupData.choices == null || shopEventPopupData.choices.Count == 0)
//            {
//                shopEventPopupData = new EventPopupTestData
//                {
//                    title = "떠돌이 상인 (ShopManager)",
//                    description = "수상한 상인이 보따리를 펼쳐 보입니다. 물건을 보시겠습니까?",
//                    choices = new System.Collections.Generic.List<EventPopupChoiceData>
//                    {
//                        new EventPopupChoiceData { id = "shop_test", label = "물건을 본다", actionType = EventPopupActionType.OpenShopUI },
//                        new EventPopupChoiceData { id = "leave", label = "지나친다", actionType = EventPopupActionType.Close }
//                    }
//                };
//            }
//        }

//        [ContextMenu("Test Open Shop Event Popup")]
//        public void TestOpenShopEventPopup()
//        {
//            if (AppManagers.RandomEvent == null)
//            {
//                Debug.LogWarning("[TestShopManager] AppManagers.RandomEvent (이벤트 매니저)가 씬에 없습니다!");
//                return;
//            }

//            AppManagers.RandomEvent.ShowCustomEventPopup(shopEventPopupData, choice =>
//            {
//                Debug.Log($"[TestShopManager] 상점 조우 이벤트 선택: {choice.label} ({choice.actionType})");

//                if (choice.actionType == EventPopupActionType.OpenShopUI)
//                {
//                    OpenShop(new ShopOpenRequest { ShopId = choice.id });
//                }
//            });
//        }

//        private ShopPage GetOrInstantiateShopPage()
//        {
//            if (shopPageInstance != null) return shopPageInstance;
//            if (shopPagePrefab == null)
//            {
//                Debug.LogWarning("[TestShopManager] ShopPage Prefab is not assigned!");
//                return null;
//            }

//            GameObject popupLayer = GameObject.Find("LobbyUIRoot/PopupLayer");
//            Transform parent = popupLayer != null ? popupLayer.transform : null;
//            if (parent == null)
//            {
//                var canvas = FindObjectOfType<Canvas>();
//                parent = canvas != null ? canvas.transform : transform;
//            }

//            shopPageInstance = Instantiate(shopPagePrefab, parent);
//            return shopPageInstance;
//        }

//        public void OpenShop(ShopOpenRequest request)
//        {
//            Debug.Log($"[TestShopManager] OpenShop requested! ShopId: {request.ShopId}");
//            var page = GetOrInstantiateShopPage();
//            if (page == null) return;

//            var viewData = new ShopUIViewData();
//            viewData.shopTitle = "상점 (테스트 샌드박스)";
//            viewData.shopDescription = "상점 설명 (테스트)";
//            viewData.currentGold = 9999;
//            viewData.items = new System.Collections.Generic.List<ShopItemViewData>();

//            if (mockShopPools != null && mockShopPools.Count > 0)
//            {
//                foreach (var pool in mockShopPools)
//                {
//                    if (pool == null) continue;
//                    var products = pool.GetAvailableProducts();
//                    if (products == null) continue;

//                    for (int i = 0; i < itemsPerPool && i < products.Count; i++)
//                    {
//                        var product = products[i];
//                        if (product != null)
//                        {
//                            string catId = "consumable";
//                            if (product.productType == Shop.ShopProductType.Relic) catId = "relic";
//                            else if (product.productType == Shop.ShopProductType.StrategicSkillItem) catId = "tactic";

//                            viewData.items.Add(new ShopItemViewData
//                            {
//                                itemId = product.productId,
//                                categoryId = catId,
//                                displayName = product.DisplayName,
//                                icon = product.Icon,
//                                description = product.Description,
//                                price = product.price,
//                                soldOut = false,
//                                affordable = true,
//                                disabledReason = ""
//                            });
//                        }
//                    }
//                }
//            }

//            page.Show(viewData, (result) => 
//            {
//                Debug.Log($"[TestShopManager] ShopUIResult Received:\n - Type: {result.type}\n - ItemId: {result.itemId}");
//                if (result.type == ShopUIResultType.PurchaseRequested)
//                {
//                    // Mock Purchase: mark item as sold out in view data and refresh
//                    var item = viewData.items.Find(x => x.itemId == result.itemId);
//                    if (item != null)
//                    {
//                        item.soldOut = true;
//                        viewData.currentGold -= item.price;
//                        page.Show(viewData, null); // Refresh with same data
//                    }
//                }
//            });
//        }
//    }
//}
