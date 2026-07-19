using TMPro;
using UnityEngine;
using UIFramework.Data;

namespace UIFramework.Page
{
    [AutoBindPrefix("Top")]
    public class TopBarPage : UIView
    {
        [AutoBind]
        [SerializeField] private TMP_Text _goldText;

        [AutoBind]
        [SerializeField] private TMP_Text _stageText;

        [Header("Relic Lists")]
        [AutoBind]
        [SerializeField] private Transform _godRelicListRoot;
        
        [AutoBind]
        [SerializeField] private Transform _commonRelicListRoot;

        [SerializeField] private TopRelicIcon _relicIconPrefab;

        private void Awake()
        {
            // Init if necessary
        }

        public void Show(TopBarViewData data)
        {
            base.Show();
            Refresh(data);
        }

        public void Refresh(TopBarViewData data)
        {
            if (data == null) return;

            if (_goldText != null)
            {
                _goldText.text = data.currentGold.ToString();
            }

            if (_stageText != null)
            {
                _stageText.text = data.currentStageName;
            }

            RefreshRelicList(data.relicData);
        }

        private void RefreshRelicList(RelicListViewData data)
        {
            //if (_relicIconPrefab == null || data == null) return;

            //if (_godRelicListRoot != null)
            //{
            //    foreach (Transform child in _godRelicListRoot) Destroy(child.gameObject);
            //}
            //if (_commonRelicListRoot != null)
            //{
            //    foreach (Transform child in _commonRelicListRoot) Destroy(child.gameObject);
            //}

            //if (_godRelicListRoot != null)
            //{
            //    foreach (var relic in data.godRelics)
            //    {
            //        TopRelicIcon icon = Instantiate(_relicIconPrefab, _godRelicListRoot);
            //        icon.Bind(relic);
            //    }
            //}

            //if (_commonRelicListRoot != null)
            //{
            //    foreach (var relic in data.commonRelics)
            //    {
            //        TopRelicIcon icon = Instantiate(_relicIconPrefab, _commonRelicListRoot);
            //        icon.Bind(relic);
            //    }
            //}
        }
    }
}
