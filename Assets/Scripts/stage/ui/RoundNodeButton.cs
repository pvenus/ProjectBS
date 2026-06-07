using UnityEngine;
using UnityEngine.UI;

namespace Stage.UI
{
    /// <summary>
    /// 스테이지 맵에서 개별 노드를 표현하는 버튼
    /// </summary>
    public class RoundNodeButton : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject selectedMark;
        [SerializeField] private GameObject clearedMark;

        private RoundNode node;

        public RoundNode Node => node;

        public void Initialize(RoundNode nodeData)
        {
            node = nodeData;

            if (iconImage != null)
            {
                iconImage.sprite = node.icon;
                iconImage.enabled = node.icon != null;
            }

            Refresh();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
        }

        public void Refresh()
        {
            if (node == null)
            {
                return;
            }

            if (button != null)
            {
                button.interactable = node.IsAvailable;
            }

            if (selectedMark != null)
            {
                selectedMark.SetActive(node.isSelected);
            }

            if (clearedMark != null)
            {
                clearedMark.SetActive(node.IsCompleted);
            }

            if (iconImage != null)
            {
                iconImage.sprite = node.icon;
                iconImage.enabled = node.icon != null;
            }

            UpdateBackgroundColor();
        }

        private void UpdateBackgroundColor()
        {
            if (backgroundImage == null || node == null)
            {
                return;
            }

            Color color = Color.white;

            if (node.IsCompleted)
            {
                color = new Color(0.5f, 0.5f, 0.5f); // 회색
            }
            else if (node.IsAvailable)
            {
                color = new Color(1f, 1f, 1f); // 흰색
            }
            else
            {
                color = new Color(0.2f, 0.2f, 0.2f); // 어두운색
            }

            if (node.IsBossNode)
            {
                color = new Color(0.8f, 0.2f, 0.2f); // 보스 강조
            }

            backgroundImage.color = color;
        }

        private void OnClick()
        {
            if (node == null)
            {
                return;
            }

            if (!node.CanExecute())
            {
                return;
            }

            if (StageManager.Instance != null)
            {
                StageManager.Instance.SelectNode(node.nodeId);
            }
        }
    }
}