using UnityEngine;
using UIFramework.Data;

namespace UIFramework.View
{
    [AutoBindPrefix("UI")]
    public class RelicListView : UIComponent
    {
        [AutoBind]
        [SerializeField] private Transform godContentRoot;

        [AutoBind]
        [SerializeField] private Transform commonContentRoot;

        [SerializeField] private RelicIconView iconPrefab;

        public void Bind(RelicListViewData data)
        {
            if (iconPrefab == null || data == null) return;

            if (godContentRoot != null)
            {
                foreach (Transform child in godContentRoot)
                {
                    Destroy(child.gameObject);
                }
            }

            if (commonContentRoot != null)
            {
                foreach (Transform child in commonContentRoot)
                {
                    Destroy(child.gameObject);
                }
            }

            if (godContentRoot != null && data.godRelics != null)
            {
                foreach (var relic in data.godRelics)
                {
                    CreateIcon(relic, godContentRoot);
                }
            }

            if (commonContentRoot != null && data.commonRelics != null)
            {
                foreach (var relic in data.commonRelics)
                {
                    CreateIcon(relic, commonContentRoot);
                }
            }
        }

        private void CreateIcon(RelicItemViewData itemData, Transform parent)
        {
            RelicIconView iconView = Instantiate(iconPrefab, parent);
            iconView.Bind(itemData);
        }
    }
}
