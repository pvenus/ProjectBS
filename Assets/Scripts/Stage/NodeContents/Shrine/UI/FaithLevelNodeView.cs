using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    [AutoBindPrefix("FaithLevel")]
    public sealed class FaithLevelNodeView : UIComponent
    {
        [AutoBind] [SerializeField] private Button button;
        [AutoBind] [SerializeField] private TMP_Text levelText;
        [AutoBind] [SerializeField] private Image currentMark;
        [AutoBind] [SerializeField] private Image acquiredMark;
        [AutoBind] [SerializeField] private Image lockedMark;
        [AutoBind] [SerializeField] private RectTransform milestoneIconRoot;

        private Action<int> onSelected;
        private int level;

        public int Level => level;
        public RectTransform MilestoneIconRoot => milestoneIconRoot;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        public void Bind(
            int nodeLevel,
            int currentLevel,
            Action<int> selectedCallback = null)
        {
            level = Mathf.Clamp(nodeLevel, 1, 10);
            onSelected = selectedCallback;

            if (levelText != null)
            {
                levelText.text = level.ToString();
            }

            bool current = level == currentLevel;
            bool acquired = level <= currentLevel;
            bool locked = level > currentLevel;

            SetActive(currentMark, current);
            SetActive(acquiredMark, acquired);
            SetActive(lockedMark, locked);

            if (button != null)
            {
                button.interactable = true;
            }
        }

        private void HandleClicked()
        {
            onSelected?.Invoke(level);
        }

        private static void SetActive(Component target, bool active)
        {
            if (target != null)
            {
                target.gameObject.SetActive(active);
            }
        }
    }
}
