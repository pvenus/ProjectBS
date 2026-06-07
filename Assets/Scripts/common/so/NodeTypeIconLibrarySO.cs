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
        public class NodeTypeIconEntry
        {
            public RoundNodeType nodeType;
            public Sprite icon;
        }

        [SerializeField]
        private List<NodeTypeIconEntry> icons = new();

        public IReadOnlyList<NodeTypeIconEntry> Icons => icons;

        public Sprite GetIcon(RoundNodeType nodeType)
        {
            NodeTypeIconEntry entry = icons.Find(x =>
                x != null && x.nodeType == nodeType);

            return entry?.icon;
        }
    }
}
