using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 보유한 유물들의 목록을 관리하고 표시하는 뷰 컴포넌트입니다.
    /// 구체 위젯 타입에 직접 의존하지 않고 추상 ListWidget을 통해 동작합니다.
    /// </summary>
    public class ArtifactListView : AutoBindBehaviour
    {
        [Tooltip("유물 아이템이 동적 배치될 컨텐츠 루트입니다. 하이어라키 내의 Content 오브젝트를 자동 바인딩합니다.")]
        [AutoBind]
        [SerializeField] private Transform contentRoot;

        private ListWidget _listWidget;

        private void Awake()
        {
            _listWidget = GetComponent<ListWidget>();
            if (_listWidget == null)
            {
                _listWidget = gameObject.AddComponent<LayoutListWidget>();
            }
        }

        public Transform ContentRoot => contentRoot;
        public ListWidget ListWidget => _listWidget;
    }
}
