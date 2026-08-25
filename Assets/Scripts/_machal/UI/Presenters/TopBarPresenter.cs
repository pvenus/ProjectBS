using System;
using System.Collections.Generic;
using UnityEngine;
using Stage;
using Currency;

namespace UIFramework
{
    /// <summary>
    /// TopBar UI의 비즈니스 로직과 데이터 전달을 담당하는 프레젠터 클래스입니다.
    /// 뷰는 데이터를 모르며 프레젠터가 제공하는 인터페이스를 통해서만 화면을 갱신합니다.
    /// </summary>
    public class TopBarPresenter
    {
        private readonly TopBarView _view;

        // 테스트/폴백 데이터 필드
        private int _fallbackGold = 0;
        private string _fallbackStageText = "2-4 / 10";

        private readonly UIMenuItem _menuItemPrefab;
        private readonly ArtifactItem _artifactItemPrefab;

        public event Action<int> OnMenuItemClicked;
        public event Action<int> OnArtifactHovered;
        public event Action<int> OnArtifactExit;
        public event Action<int> OnArtifactSelected;

        public TopBarPresenter(
            TopBarView view,
            UIMenuItem menuItemPrefab,
            ArtifactItem artifactItemPrefab)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _menuItemPrefab = menuItemPrefab;
            _artifactItemPrefab = artifactItemPrefab;

            Initialize();
        }

        private void Initialize()
        {
            // 인게임 실데이터 매니저 이벤트 연결
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
            }

            // 초기 뷰 갱신
            UpdateGold();
            UpdateStage();
        }

        public void Cleanup()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged -= HandleGoldChanged;
            }
        }

        // --- 재화 관련 비즈니스 로직 ---
        public void SetFallbackGold(int amount)
        {
            _fallbackGold = amount;
            UpdateGold();
        }

        private void HandleGoldChanged(int newGold)
        {
            UpdateGold();
        }

        public void UpdateGold()
        {
            int goldAmount = CurrencyManager.Instance != null ? CurrencyManager.Instance.Gold : _fallbackGold;
            _view.SetGoldAmount(goldAmount);
        }

        // --- 스테이지 관련 비즈니스 로직 ---
        public void SetFallbackStageInfo(string text)
        {
            _fallbackStageText = text;
            UpdateStage();
        }

        public void UpdateStage()
        {
            string text = _fallbackStageText;

            if (StageManager.Instance != null)
            {
                var runtime = StageManager.Instance.RuntimeData;
                if (runtime != null && runtime.currentGraph != null)
                {
                    int current = runtime.currentNode != null ? runtime.currentNode.depth + 1 : 1;
                    int total = runtime.currentGraph.GetMaxDepth() + 1;
                    text = $"{runtime.stageId} ({current}/{total})";
                }
            }

            _view.SetStageText(text);
        }

        // --- 시스템 메뉴 관련 비즈니스 로직 ---
        public void PopulateMenu(List<string> options)
        {
            if (_view.Menu == null || _view.Menu.DropdownRoot == null || _menuItemPrefab == null) return;

            // 기존 드롭다운 내 항목들 정리
            foreach (Transform child in _view.Menu.DropdownRoot)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }

            for (int i = 0; i < options.Count; i++)
            {
                int index = i;
                string label = options[i];
                UIMenuItem item = UnityEngine.Object.Instantiate(_menuItemPrefab, _view.Menu.DropdownRoot);
                item.Bind(label, () => OnMenuItemClicked?.Invoke(index));
            }
        }

        // --- 유물(Artifact) 리스트 관련 비즈니스 로직 ---
        public void SetupArtifacts(List<Sprite> icons)
        {
            if (_view.Artifacts == null || _artifactItemPrefab == null) return;

            _view.Artifacts.ListWidget.Clear();

            for (int i = 0; i < icons.Count; i++)
            {
                int index = i;
                Sprite iconSprite = icons[i];

                ArtifactItem item = _view.Artifacts.ListWidget.AddItem(_artifactItemPrefab);
                if (item != null)
                {
                    item.Bind(
                        index,
                        iconSprite,
                        (idx) => OnArtifactHovered?.Invoke(idx),
                        (idx) => OnArtifactExit?.Invoke(idx),
                        (idx) => OnArtifactSelected?.Invoke(idx)
                    );
                }
            }
        }

        public void AddArtifact(Sprite iconSprite)
        {
            if (_view.Artifacts == null || _artifactItemPrefab == null) return;

            int index = _view.Artifacts.ListWidget.Items.Count;
            ArtifactItem item = _view.Artifacts.ListWidget.AddItem(_artifactItemPrefab);
            if (item != null)
            {
                item.Bind(
                    index,
                    iconSprite,
                    (idx) => OnArtifactHovered?.Invoke(idx),
                    (idx) => OnArtifactExit?.Invoke(idx),
                    (idx) => OnArtifactSelected?.Invoke(idx)
                );
            }
        }

        public void RemoveLastArtifact()
        {
            if (_view.Artifacts == null) return;

            var items = _view.Artifacts.ListWidget.Items;
            if (items.Count > 0)
            {
                var lastItem = items[items.Count - 1];
                _view.Artifacts.ListWidget.RemoveItem(lastItem);
            }
        }
    }
}
