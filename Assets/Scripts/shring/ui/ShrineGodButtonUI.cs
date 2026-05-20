using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mission;

namespace Shrine.UI
{
    /// <summary>
    /// 신 선택 버튼 UI.
    /// ShrineGodSO와 현재 신앙 상태를 표시하고 클릭 시 ShrineManager.SelectGod 호출.
    /// </summary>
    public class ShrineGodButtonUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShrineManager shrineManager;

        [Header("UI")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text faithLevelText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text missionText;

        [Header("Options")]
        [SerializeField] private bool hideWhenEmpty = true;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new(0.85f, 0.8f, 0.35f, 1f);
        [SerializeField] private Color lockedColor = new(0.25f, 0.25f, 0.25f, 1f);

        private ShrineGodSO god;
        private bool isSelected;
        private bool isLocked;

        public ShrineGodSO God => god;

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

        public void Bind(ShrineGodSO godSo, ShrineManager manager = null)
        {
            god = godSo;

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
            if (god == null)
            {
                SetEmpty();
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            ShrinePlayerRuntimeData faithData =
                shrineManager != null
                    ? shrineManager.PlayerRuntimeData
                    : null;

            int faithLevel =
                shrineManager != null
                    ? shrineManager.GetFaithLevel(god.godType)
                    : 0;

            int faithAffinity =
                shrineManager != null
                    ? shrineManager.GetFaithAffinity(god.godType)
                    : 0;

            FaithStageState state = FaithStageState.None;

            if (faithData != null)
            {
                int currentFaith = faithLevel;

                bool hasLockedFaith =
                    faithData.HasLockedFaith
                    && faithData.LockedGod == god.godType;

                if (hasLockedFaith)
                {
                    state = FaithStageState.Locked;
                }
                else if (currentFaith >= 7)
                {
                    state = FaithStageState.Successor;
                }
                else if (currentFaith >= 5)
                {
                    state = FaithStageState.Devoted;
                }
                else if (currentFaith >= 1)
                {
                    state = FaithStageState.Influenced;
                }
                else
                {
                    state = FaithStageState.Normal;
                }
            }

            isSelected = shrineManager != null
                         && shrineManager.CurrentShrine != null
                         && shrineManager.CurrentShrine.selectedGod == god.godType;

            bool isUnlocked =
                shrineManager != null
                && shrineManager.CurrentShrine != null
                && shrineManager.CurrentShrine
                    .availableGods
                    .Contains(god.godType);

            isLocked = !isUnlocked;

            if (iconImage != null)
            {
                iconImage.sprite = god.icon;
                iconImage.enabled = god.icon != null;
            }

            if (nameText != null)
            {
                nameText.text = god.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = god.description;
            }

            if (faithLevelText != null)
            {
                faithLevelText.text =
                    $"Faith Lv.{faithLevel}\nAffinity : {faithAffinity}";
            }

            if (stateText != null)
            {
                stateText.text = state.ToString();
            }

            if (missionText != null)
            {
                string missionLabel = string.Empty;

                if (god.unlockMissions != null)
                {
                    foreach (Mission.MissionSO mission in god.unlockMissions)
                    {
                        if (mission == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(missionLabel))
                        {
                            missionLabel += "\n";
                        }

                        missionLabel +=
                            $"Unlock : {mission.displayName}";
                    }
                }

                if (god.faithMissions != null)
                {
                    foreach (Mission.MissionSO mission in god.faithMissions)
                    {
                        if (mission == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(missionLabel))
                        {
                            missionLabel += "\n";
                        }

                        missionLabel +=
                            $"Faith : {mission.displayName}";
                    }
                }

                missionText.text = missionLabel;
            }

            RefreshState();
        }

        public void Clear()
        {
            god = null;
            SetEmpty();
        }

        private void RefreshState()
        {
            bool interactable = !isLocked;

            if (button != null)
            {
                button.interactable = interactable;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = isSelected
                    ? selectedColor
                    : isLocked
                        ? lockedColor
                        : normalColor;
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

            if (faithLevelText != null)
            {
                faithLevelText.text = string.Empty;
            }

            if (stateText != null)
            {
                stateText.text = string.Empty;
            }

            if (missionText != null)
            {
                missionText.text = string.Empty;
            }

            if (button != null)
            {
                button.interactable = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
        }

        private void HandleClick()
        {
            if (god == null)
            {
                return;
            }

            if (isLocked)
            {
                return;
            }

            if (shrineManager == null)
            {
                Debug.LogWarning("[ShrineGodButtonUI] Click failed. ShrineManager is null.");
                return;
            }

            shrineManager.SelectGod(god.godType);
            Refresh();
        }
    }
}