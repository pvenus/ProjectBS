

using TMPro;
using Bless;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>
    /// 신전 축복 후보 1개를 표시하는 버튼 UI.
    /// ShrineBlessingRuntime을 바인딩하고 클릭 시 ShrineManager.SelectBlessingBySlot으로 전달한다.
    /// </summary>
    public class ShrineBlessingButtonUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShrineManager shrineManager;

        [Header("UI")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private TMP_Text effectText;

        [Header("Options")]
        [SerializeField] private bool hideWhenEmpty = true;

        private BlessRuntimeData.BlessEntry blessingRuntime;

        public BlessRuntimeData.BlessEntry BlessingRuntime => blessingRuntime;

        private void Awake()
        {
            if (shrineManager == null)
            {
                shrineManager = ShrineManager.Instance;
            }
        }

        private void OnEnable()
        {
            if (shrineManager == null)
            {
                shrineManager = ShrineManager.Instance;
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        public void Bind(BlessRuntimeData.BlessEntry runtime, ShrineManager manager = null)
        {
            blessingRuntime = runtime;

            if (manager != null)
            {
                shrineManager = manager;
            }
            else if (shrineManager == null)
            {
                shrineManager = ShrineManager.Instance;
            }

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
            }

            Refresh();
        }

        public void Refresh()
        {
            if (blessingRuntime == null
                || blessingRuntime.source == null)
            {
                SetEmpty();
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (iconImage != null)
            {
                iconImage.sprite = blessingRuntime.Icon;
                iconImage.enabled = blessingRuntime.Icon != null;
            }

            if (nameText != null)
            {
                nameText.text = blessingRuntime.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = blessingRuntime.Description;
            }

            if (categoryText != null)
            {
                categoryText.text = blessingRuntime.Category.ToString();
            }

            if (effectText != null)
            {
                effectText.text = blessingRuntime.source != null
                    ? blessingRuntime.source.description
                    : string.Empty;
            }

            RefreshState();
        }

        public void Clear()
        {
            blessingRuntime = null;
            SetEmpty();
        }

        private void RefreshState()
        {
            if (blessingRuntime == null)
            {
                return;
            }

            bool canSelect = blessingRuntime.CanSelect;

            if (button != null)
            {
                button.interactable = canSelect;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = blessingRuntime.isSelected
                    ? new Color(0.8f, 0.8f, 0.35f, 1f)
                    : blessingRuntime.isLocked
                        ? new Color(0.25f, 0.25f, 0.25f, 1f)
                        : Color.white;
            }
        }

        private void SetEmpty()
        {
            if (hideWhenEmpty)
            {
                gameObject.SetActive(false);
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (nameText != null)
            {
                nameText.text = "Empty";
            }

            if (descriptionText != null)
            {
                descriptionText.text = string.Empty;
            }

            if (categoryText != null)
            {
                categoryText.text = string.Empty;
            }

            if (effectText != null)
            {
                effectText.text = string.Empty;
            }

            if (button != null)
            {
                button.interactable = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = Color.white;
            }
        }

        private void HandleClick()
        {
            if (blessingRuntime == null)
            {
                return;
            }

            if (!blessingRuntime.CanSelect)
            {
                return;
            }

            if (shrineManager == null)
            {
                Debug.LogWarning("[ShrineBlessingButtonUI] Click failed. ShrineManager is null.");
                return;
            }

            shrineManager.SelectBlessingBySlot(blessingRuntime.slotIndex);
        }
    }
}