using System;
using System.Collections.Generic;
using Stage;
using UnityEngine;

namespace Common.SO
{
    [CreateAssetMenu(
        fileName = "NodeTypeIconLibrary",
        menuName = "BS/Common/Node Type Icon Library")]
    public class NodeTypeIconLibrarySO : ScriptableObject
    {
        [Serializable]
        public class NodeIconEntry
        {
            public NodeIconType iconType;
            public Sprite icon;

            [Tooltip("RoundNodeButton 하위에 생성할 단순 이미지/비주얼 표현용 프리팹. null이면 생성 안 함.")]
            public GameObject prefab;
        }

        [SerializeField]
        private List<NodeIconEntry> entries = new();

        public IReadOnlyList<NodeIconEntry> Entries => entries;

        public NodeIconEntry GetEntry(NodeIconType iconType)
        {
            return entries.Find(e => e != null && e.iconType == iconType);
        }

        public Sprite GetIcon(NodeIconType iconType)
        {
            return GetEntry(iconType)?.icon;
        }

        public GameObject GetPrefab(NodeIconType iconType)
        {
            return GetEntry(iconType)?.prefab;
        }
    }
}
