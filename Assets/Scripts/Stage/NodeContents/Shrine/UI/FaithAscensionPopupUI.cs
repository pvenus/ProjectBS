using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine
{
    public class FaithAscensionPopupUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text levelText;

        [Header("Buttons")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button rejectButton;

        [Header("Optional")]
        [SerializeField] private Image godIcon;

        private ShrineManager shrineManager;
        private ShrineGodType currentGodType;

        private void Awake()
        {
            shrineManager = ShrineManager.Instance;

            if (acceptButton != null)
            {
                acceptButton.onClick.AddListener(OnAccept);
            }

            if (rejectButton != null)
            {
                rejectButton.onClick.AddListener(OnReject);
            }

            SetVisible(false);
        }

        private void OnEnable()
        {
            if (shrineManager != null)
            {
                shrineManager.OnFaithAscensionRequested += Open;
            }
        }

        private void OnDisable()
        {
            if (shrineManager != null)
            {
                shrineManager.OnFaithAscensionRequested -= Open;
            }
        }

        public void Open(ShrineGodType godType)
        {
            currentGodType = godType;

            Refresh();
            SetVisible(true);
        }

        public void Close()
        {
            SetVisible(false);
        }

        private void Refresh()
        {
            if (titleText != null)
            {
                titleText.text =
                    $"{currentGodType} Ascension";
            }

            if (descriptionText != null)
            {
                descriptionText.text =
                    "Do you wish to devote yourself fully to this god?\n"
                    + "Other divine paths will be closed.";
            }

            if (levelText != null
                && shrineManager != null)
            {
                int level =
                    shrineManager.GetFaithLevel(currentGodType);

                int affinity =
                    shrineManager.GetFaithAffinity(currentGodType);

                levelText.text =
                    $"Faith Lv.{level}\nAffinity : {affinity}";
            }
        }

        private void OnAccept()
        {
            if (shrineManager == null)
            {
                return;
            }

            shrineManager.AcceptFaithAscension();

            Close();
        }

        private void OnReject()
        {
            if (shrineManager == null)
            {
                return;
            }

            shrineManager.RejectFaithAscension();

            Close();
        }

        private void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}