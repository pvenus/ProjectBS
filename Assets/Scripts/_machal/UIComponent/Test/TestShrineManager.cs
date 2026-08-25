//using UnityEngine;
//using ProjectBS.Core;
//using UIFramework.Data;

//namespace UIFramework.Test
//{
//    public class TestShrineManager : MonoBehaviour, IShrineManager
//    {
//        [Header("UI Prefab")]
//        public ShrineMainPage shrinePagePrefab;
//        private ShrineMainPage shrinePageInstance;

//        [Header("Shrine Event Popup Data (Test)")]
//        public EventPopupTestData shrineEventPopupData;

//        [Header("Mock Shrine Data (Test)")]
//        public string mockShrineName = "테스트 신전";
        
//        [Tooltip("인스펙터에서 신(ShrineGodSO) 데이터를 직접 할당하여 테스트합니다.")]
//        public System.Collections.Generic.List<Shrine.ShrineGodSO> mockGods;
        
//        [Tooltip("신전 축복 선택 단계에서 띄워줄 축복(BlessSO) 후보들입니다.")]
//        public System.Collections.Generic.List<Bless.BlessSO> mockBlessingCandidates;

//        private void Awake()
//        {
//            AppManagers.Shrine = this;

//            if (shrineEventPopupData == null || shrineEventPopupData.choices == null || shrineEventPopupData.choices.Count == 0)
//            {
//                shrineEventPopupData = new EventPopupTestData
//                {
//                    title = "낡은 신전 (ShrineManager)",
//                    description = "희미한 향냄새가 감도는 신전입니다. 제단 앞에서 무엇을 하시겠습니까?",
//                    choices = new System.Collections.Generic.List<EventPopupChoiceData>
//                    {
//                        new EventPopupChoiceData { id = "shrine_test", label = "기도한다", actionType = EventPopupActionType.OpenShrineUI },
//                        new EventPopupChoiceData { id = "leave", label = "떠난다", actionType = EventPopupActionType.Close }
//                    }
//                };
//            }
//        }

//        [ContextMenu("Test Open Shrine Event Popup")]
//        public void TestOpenShrineEventPopup()
//        {
//            if (AppManagers.RandomEvent == null)
//            {
//                Debug.LogWarning("[TestShrineManager] AppManagers.RandomEvent (이벤트 매니저)가 씬에 없습니다!");
//                return;
//            }

//            AppManagers.RandomEvent.ShowCustomEventPopup(shrineEventPopupData, choice =>
//            {
//                Debug.Log($"[TestShrineManager] 신전 조우 이벤트 선택: {choice.label} ({choice.actionType})");

//                if (choice.actionType == EventPopupActionType.OpenShrineUI)
//                {
//                    OpenShrine(new ShrineOpenRequest { ShrineId = choice.id });
//                }
//            });
//        }

//        public void OpenShrine(ShrineOpenRequest request)
//        {
//            Debug.Log($"[TestShrineManager] OpenShrine requested! ShrineId: {request.ShrineId}");
//            var page = GetOrInstantiate(shrinePagePrefab, ref shrinePageInstance);
            
//            if (page != null)
//            {
//                var viewData = new ShrineUIViewData
//                {
//                    state = ShrineUIState.MainSelection,
//                    title = mockShrineName,
//                    description = "제단 앞에서 무엇을 하시겠습니까?",
//                    currentFaith = 0
//                };
                
//                page.Show(viewData, (result) => 
//                {
//                    Debug.Log($"[TestShrineManager] ShrineUIResult Received:\n - Type: {result.type}\n - GodId: {result.selectedGodId}\n - BlessingId: {result.selectedBlessingId}");
                    
//                    if (result.type == ShrineUIResultType.SelectEnterFaith)
//                    {
//                        viewData.state = ShrineUIState.GodSelection;
//                        viewData.description = "신앙 활동을 진행할 신을 선택하세요.";
                        
//                        viewData.selectableGods.Clear();
//                        if (mockGods != null && mockGods.Count > 0)
//                        {
//                            foreach (var godSo in mockGods)
//                            {
//                                if (godSo != null)
//                                {
//                                    viewData.selectableGods.Add(new ShrineGodViewData
//                                    {
//                                        godId = godSo.godType.ToString(),
//                                        displayName = godSo.DisplayName,
//                                        description = godSo.description,
//                                        portrait = godSo.icon,
//                                        faithLevel = 1,
//                                        reputation = 0,
//                                        selectable = true
//                                    });
//                                }
//                            }
//                        }
//                        page.Refresh(viewData); // Refresh with new state
//                    }
//                    else if (result.type == ShrineUIResultType.SelectHealAndBless)
//                    {
//                        viewData.state = ShrineUIState.BlessingSelection;
//                        viewData.description = "어떤 축복을 받으시겠습니까?";

//                        viewData.selectableBlessings.Clear();
//                        if (mockBlessingCandidates != null && mockBlessingCandidates.Count > 0)
//                        {
//                            for (int i = 0; i < mockBlessingCandidates.Count; i++)
//                            {
//                                var blessSo = mockBlessingCandidates[i];
//                                if (blessSo != null)
//                                {
//                                    viewData.selectableBlessings.Add(new ShrineBlessingViewData
//                                    {
//                                        blessingId = blessSo.blessingId,
//                                        displayName = blessSo.DisplayName,
//                                        description = blessSo.description,
//                                        icon = blessSo.icon,
//                                        cost = 100,
//                                        selectable = true
//                                    });
//                                }
//                            }
//                        }
//                        page.Refresh(viewData);
//                    }
//                    else if (result.type == ShrineUIResultType.Pray || result.type == ShrineUIResultType.Donate)
//                    {
//                        if (result.type == ShrineUIResultType.Donate && ProjectBS.Core.AppManagers.Currency is TestCurrencyManager currencyMgr)
//                        {
//                            currencyMgr.Gold -= 100; // 가짜 기부 비용
//                        }
                        
//                        viewData.state = ShrineUIState.Result;
//                        viewData.selectableGods.Clear();
//                        viewData.description = "기도/기부를 마쳤습니다. (테스트)";
//                        page.Refresh(viewData);
//                    }
//                    else if (result.type == ShrineUIResultType.SelectBlessing)
//                    {
//                        if (ProjectBS.Core.AppManagers.Currency is TestCurrencyManager currencyMgr)
//                        {
//                            currencyMgr.Gold -= 100; // 가짜 축복 비용
//                        }

//                        viewData.state = ShrineUIState.Result;
//                        viewData.selectableBlessings.Clear();
//                        viewData.description = "축복을 받았습니다. 체력이 회복되었습니다. (테스트)";
//                        page.Refresh(viewData);
//                    }
//                    else if (result.type == ShrineUIResultType.Close)
//                    {
//                        page.Hide();
//                    }
//                });
//            }
//        }

//        private T GetOrInstantiate<T>(T prefab, ref T instance) where T : MonoBehaviour
//        {
//            if (instance != null) return instance;
//            if (prefab == null) return null;
//            Transform parent = GameObject.Find("LobbyUIRoot/PopupLayer")?.transform ?? FindObjectOfType<Canvas>()?.transform ?? transform;
//            instance = Instantiate(prefab, parent);
//            return instance;
//        }
//    }
//}
