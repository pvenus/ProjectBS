using System.Collections.Generic;
using Bless;
using Character;
using Character.UI;
using Item;
using Shop;
using Stage.UI;
using UI;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// Stage 씬에서 네 개의 플레이어 정보 팝업 열기/닫기를 전담하는 Coordinator.
    ///
    /// 구조:
    ///   테스트 버튼 / Stage 노드 이벤트
    ///        ↓
    ///   StagePlayerPopupCoordinator  (이 클래스)
    ///        ↓
    ///   UIPopupViewController → 프리팹 UIView 인스턴스화
    ///        ↓
    ///   실제 런타임 데이터 조회 → 기존 Presenter/View 호출
    ///
    /// 동시에 하나의 팝업만 열리도록 제어한다.
    /// 데이터나 필수 참조가 없으면 임시 데이터를 만들지 않고 경고 후 false 반환한다.
    /// </summary>
    public class StagePlayerPopupCoordinator : MonoBehaviour
    {
        // ── 현재 열린 팝업 추적 ──────────────────────────────────────
        private PopupType currentOpenType = PopupType.None;

        // ── 각 팝업 뷰 캐시 (Open 시 취득) ──────────────────────────
        private StageShopPanelView         shopView;
        private StageCharacterInfoPanelView charInfoView;
        private StageRelicInfoPanelView     relicInfoView;
        private StageOwnedEffectsPanelView  ownedEffectsView;

        // ── Public API ───────────────────────────────────────────────

        /// <summary>
        /// 상점 팝업을 연다.
        /// UIPopupViewController 를 통해 Shop_Fixed 프리팹을 취득하고,
        /// StageShopManager.OpenShop(data) 를 호출한다.
        /// </summary>
        public bool OpenShop(ShopExecutionData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[StagePlayerPopupCoordinator] OpenShop failed: data is null.");
                return false;
            }

            if (data.pools == null || data.pools.Count == 0)
            {
                Debug.LogWarning("[StagePlayerPopupCoordinator] OpenShop failed: pools is empty.");
                return false;
            }

            if (!OpenPanel(PopupType.StageShop, out UIView view))
            {
                return false;
            }

            if (StageShopManager.Instance != null)
            {
                StageShopManager.Instance.OpenShop(data.pools, data.itemCount, data.shopType);
            }
            else
            {
                Debug.LogWarning("[StagePlayerPopupCoordinator] StageShopManager.Instance is null.");
            }

            return true;
        }

        /// <summary>실제 파티 런타임 데이터를 조회해 캐릭터 정보 팝업을 연다.</summary>
        public bool OpenCharacterInfo()
        {
            IReadOnlyList<CharacterRuntimeData> party = StagePlayerInventorySnapshotBuilder.BuildPartySnapshot();
            if (party == null || party.Count == 0)
            {
                Debug.LogWarning("[StagePlayerPopupCoordinator] OpenCharacterInfo: Party snapshot is empty.");
                return false;
            }

            if (!OpenPanel(PopupType.StageCharacterInfo, out UIView view))
            {
                return false;
            }

            if (view != null)
            {
                CharacterSkillContentInfoPresenter presenter =
                    view.GetComponentInChildren<CharacterSkillContentInfoPresenter>(true);
                if (presenter != null)
                {
                    presenter.SetCharacters(party);
                }
                else
                {
                    Debug.LogWarning("[StagePlayerPopupCoordinator] CharacterSkillContentInfoPresenter not found in charInfoView.");
                }
            }

            return true;
        }

        /// <summary>실제 보유 유물을 조회해 유물 정보 팝업을 연다.</summary>
        public bool OpenRelicInfo()
        {
            IReadOnlyList<RelicSO> relics = StagePlayerInventorySnapshotBuilder.BuildOwnedRelicsSnapshot();

            if (!OpenPanel(PopupType.StageRelicInfo, out UIView view))
            {
                return false;
            }

            if (view != null)
            {
                RelicCollectionView collectionView =
                    view.GetComponentInChildren<RelicCollectionView>(true);
                if (collectionView != null)
                {
                    collectionView.ShowRelics(relics);
                }
                else
                {
                    Debug.LogWarning("[StagePlayerPopupCoordinator] RelicCollectionView not found in relicInfoView.");
                }
            }

            return true;
        }

        /// <summary>실제 유물/축복 인벤토리를 조회해 보유효과 팝업을 연다.</summary>
        public bool OpenOwnedEffects()
        {
            IReadOnlyList<RelicSO> relics =
                StagePlayerInventorySnapshotBuilder.BuildOwnedRelicsSnapshot();
            IReadOnlyList<BlessRuntimeData.BlessEntry> generalBlesses =
                StagePlayerInventorySnapshotBuilder.BuildGeneralBlessSnapshot();
            IReadOnlyList<BlessRuntimeData.BlessEntry> faithBlesses =
                StagePlayerInventorySnapshotBuilder.BuildFaithBlessSnapshot();

            if (!OpenPanel(PopupType.StageOwnedEffects, out UIView view))
            {
                return false;
            }

            if (view != null)
            {
                OwnedEffectInventoryPresenter presenter =
                    view.GetComponentInChildren<OwnedEffectInventoryPresenter>(true);
                if (presenter != null)
                {
                    presenter.ShowOwnedEffects(relics, generalBlesses, faithBlesses);
                }
                else
                {
                    Debug.LogWarning("[StagePlayerPopupCoordinator] OwnedEffectInventoryPresenter not found in opened view.");
                }
            }

            return true;
        }

        /// <summary>현재 열려 있는 팝업을 닫는다.</summary>
        public void CloseCurrentPanel()
        {
            if (currentOpenType == PopupType.None)
            {
                return;
            }

            if (currentOpenType == PopupType.StageShop
                && StageShopManager.Instance != null
                && StageShopManager.Instance.IsOpened)
            {
                StageShopManager.Instance.CloseShop();
            }

            if (UIPopupViewController.Instance == null)
            {
                Debug.LogWarning("[StagePlayerPopupCoordinator] CloseCurrentPanel: UIPopupViewController.Instance is null.");
                currentOpenType = PopupType.None;
                return;
            }

            UIPopupViewController.Instance.Close(currentOpenType);
            currentOpenType = PopupType.None;
        }

        // ── 내부 헬퍼 ────────────────────────────────────────────────

        private bool OpenPanel(PopupType type, out UIView view)
        {
            view = null;

            if (UIPopupViewController.Instance == null)
            {
                Debug.LogWarning($"[StagePlayerPopupCoordinator] Cannot open {type}: UIPopupViewController.Instance is null.");
                return false;
            }

            // 동시에 하나만 열리도록: 다른 팝업이 열려 있으면 먼저 닫는다
            if (currentOpenType != PopupType.None && currentOpenType != type)
            {
                CloseCurrentPanel();
            }

            // 이미 같은 타입이 열려 있으면 재사용
            if (currentOpenType == type)
            {
                view = GetCachedView(type);
                return true;
            }

            view = UIPopupViewController.Instance.Open(type);
            if (view == null)
            {
                Debug.LogWarning($"[StagePlayerPopupCoordinator] UIPopupViewController returned null for {type}. " +
                                 "PopupViewRegistrySO에 해당 타입이 등록되어 있는지 확인하세요.");
                return false;
            }

            currentOpenType = type;
            CacheAndSubscribeCloseButton(type, view);
            return true;
        }

        private UIView GetCachedView(PopupType type)
        {
            return type switch
            {
                PopupType.StageShop => shopView,
                PopupType.StageCharacterInfo => charInfoView,
                PopupType.StageRelicInfo => relicInfoView,
                PopupType.StageOwnedEffects => ownedEffectsView,
                _ => null
            };
        }

        private void CacheAndSubscribeCloseButton(PopupType type, UIView view)
        {
            if (view == null) return;

            switch (type)
            {
                case PopupType.StageShop:
                    shopView = view as StageShopPanelView ?? view.GetComponentInChildren<StageShopPanelView>(true);
                    if (shopView != null)
                        shopView.OnCloseRequested += CloseCurrentPanel;
                    break;

                case PopupType.StageCharacterInfo:
                    charInfoView = view as StageCharacterInfoPanelView ?? view.GetComponentInChildren<StageCharacterInfoPanelView>(true);
                    if (charInfoView != null)
                        charInfoView.OnCloseRequested += CloseCurrentPanel;
                    break;

                case PopupType.StageRelicInfo:
                    relicInfoView = view as StageRelicInfoPanelView ?? view.GetComponentInChildren<StageRelicInfoPanelView>(true);
                    if (relicInfoView != null)
                        relicInfoView.OnCloseRequested += CloseCurrentPanel;
                    break;

                case PopupType.StageOwnedEffects:
                    ownedEffectsView = view as StageOwnedEffectsPanelView ?? view.GetComponentInChildren<StageOwnedEffectsPanelView>(true);
                    if (ownedEffectsView != null)
                        ownedEffectsView.OnCloseRequested += CloseCurrentPanel;
                    break;
            }
        }
    }
}
