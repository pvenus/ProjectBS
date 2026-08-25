using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// UI 테스트 씬에서 모의 데이터를 주입하고 동적 이벤트를 발생시켜 UI가 올바르게 작동하는지 확인하는 테스트 컨트롤러 클래스입니다.
    /// </summary>
    public class UITestSceneController : MonoBehaviour
    {
        [Header("UI Views")]
        [SerializeField] private TopBarView topBarView;

        [Header("Prefabs")]
        [SerializeField] private UIMenuItem menuItemPrefab;
        [SerializeField] private ArtifactItem artifactItemPrefab;

        [Header("Test Assets")]
        [SerializeField] private List<Sprite> testArtifactSprites;

        private TopBarPresenter _presenter;
        private int _currentGold = 12345;
        private int _stageNum = 2;
        private int _stageRound = 4;

        private void Start()
        {
            if (topBarView == null)
            {
                Debug.LogError("topBarView is not assigned in UITestSceneController!");
                return;
            }

            // 프레젠터 생성
            _presenter = new TopBarPresenter(topBarView, menuItemPrefab, artifactItemPrefab);

            // 초기 모의 데이터 설정 (아이콘은 에디터 정적 매핑으로 자동 처리되므로 텍스트/보유량만 전달)
            _presenter.SetFallbackGold(_currentGold);
            _presenter.SetFallbackStageInfo($"{_stageNum}-{_stageRound} / 10");

            // 시스템 메뉴 드롭다운 채우기
            var menuOptions = new List<string>
            {
                "사운드 설정",
                "게임 옵션",
                "게임 저장",
                "프로필 보기",
                "로그아웃"
            };
            _presenter.PopulateMenu(menuOptions);

            // 초기 모의 유물 리스트 채우기
            _presenter.SetupArtifacts(testArtifactSprites);

            // 프레젠터 이벤트 구독
            _presenter.OnMenuItemClicked += HandleMenuItemClicked;
            _presenter.OnArtifactHovered += HandleArtifactHovered;
            _presenter.OnArtifactExit += HandleArtifactExit;
            _presenter.OnArtifactSelected += HandleArtifactSelected;

            Debug.Log("[UITestSceneController] UI 테스트가 준비되었습니다. 아래 키를 입력하여 연동 동작을 테스트하십시오:\n" +
                      " - [G 키]: 1,000 골드 추가 및 텍스트 갱신\n" +
                      " - [S 키]: 스테이지 진행 및 진행도 갱신\n" +
                      " - [A 키]: 가로 리스트에 유물 동적 추가 (LayoutListWidget)\n" +
                      " - [R 키]: 리스트의 마지막 유물 동적 삭제 (LayoutListWidget)");
        }

        private void Update()
        {
            if (_presenter == null) return;

            // [G] 골드 추가 테스트
            if (Input.GetKeyDown(KeyCode.G))
            {
                _currentGold += 1000;
                _presenter.SetFallbackGold(_currentGold);
                Debug.Log($"[UITestSceneController] 골드 추가: {_currentGold}");
            }

            // [S] 스테이지 진행 테스트
            if (Input.GetKeyDown(KeyCode.S))
            {
                _stageRound++;
                if (_stageRound > 10)
                {
                    _stageRound = 1;
                    _stageNum++;
                }
                _presenter.SetFallbackStageInfo($"{_stageNum}-{_stageRound} / 10");
                Debug.Log($"[UITestSceneController] 스테이지 변경: {_stageNum}-{_stageRound}");
            }

            // [A] 유물 동적 추가 테스트
            if (Input.GetKeyDown(KeyCode.A) && testArtifactSprites.Count > 0)
            {
                var randSprite = testArtifactSprites[Random.Range(0, testArtifactSprites.Count)];
                _presenter.AddArtifact(randSprite);
                Debug.Log("[UITestSceneController] 새로운 유물 추가됨.");
            }

            // [R] 유물 동적 삭제 테스트
            if (Input.GetKeyDown(KeyCode.R))
            {
                _presenter.RemoveLastArtifact();
                Debug.Log("[UITestSceneController] 마지막 유물 제거됨.");
            }
        }

        private void OnDestroy()
        {
            if (_presenter != null)
            {
                _presenter.Cleanup();
            }
        }

        // --- 프레젠터 이벤트 처리기 ---
        private void HandleMenuItemClicked(int index)
        {
            Debug.Log($"[UITestSceneController] 메뉴 클릭됨 - 인덱스 {index}!");
            // 클릭 시 드롭다운 닫기
            if (topBarView.Menu != null)
            {
                topBarView.Menu.HideDropdown();
            }
        }

        private void HandleArtifactHovered(int index)
        {
            Debug.Log($"[UITestSceneController] 유물 호버 진입: 인덱스 {index}");
        }

        private void HandleArtifactExit(int index)
        {
            Debug.Log($"[UITestSceneController] 유물 호버 이탈: 인덱스 {index}");
        }

        private void HandleArtifactSelected(int index)
        {
            Debug.Log($"[UITestSceneController] 유물 클릭(선택): 인덱스 {index}");
            
            // 리스트 위젯의 선택 상태 변경
            if (topBarView.Artifacts != null)
            {
                var items = topBarView.Artifacts.ListWidget.Items;
                if (index >= 0 && index < items.Count)
                {
                    topBarView.Artifacts.ListWidget.SelectItem(items[index]);
                }
            }
        }
    }
}
