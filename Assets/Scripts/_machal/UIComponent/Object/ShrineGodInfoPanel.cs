using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Shrine;

[RequireComponent(typeof(CanvasGroup))]
[AutoBindPrefix("God")]
public class ShrineGodInfoPanel : UIComponent
{
        [AutoBind]
        [SerializeField] private CanvasGroup rootGroup;

        [AutoBind]
        [SerializeField] private TMP_Text nameText;

        [AutoBind]
        [SerializeField] private TMP_Text descText;

        [AutoBind]
        [SerializeField] private TMP_Text faithLevelText;

        [AutoBind]
        [SerializeField] private TMP_Text stateText;

        [AutoBind]
        [SerializeField] private TMP_Text missionText;

        [AutoBind]
        [SerializeField] private Image iconImage;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.3f;

        private UIAnimationSequence sequence;
        private UIFadeAnimation fadeAnim;
        
        private ShrineGodSO currentGodSo;
        private ShrineRuntimeData currentShrineData;
        private ShrineManager currentManager;

        protected void Awake()
        {
            sequence = gameObject.GetComponent<UIAnimationSequence>();
            if (sequence == null) sequence = gameObject.AddComponent<UIAnimationSequence>();
            
            fadeAnim = gameObject.AddComponent<UIFadeAnimation>();
            
            if (rootGroup == null)
            {
                rootGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Bind(ShrineGodSO godSo, ShrineRuntimeData shrineData, ShrineManager manager)
        {
            currentGodSo = godSo;
            currentShrineData = shrineData;
            currentManager = manager;

            Refresh();
        }

        public void Refresh()
        {
            if (currentGodSo == null || currentShrineData == null || currentManager == null)
            {
                return;
            }

            if (nameText != null)
                nameText.text = currentGodSo.DisplayName;

            if (descText != null)
                descText.text = currentGodSo.Description;

            if (iconImage != null)
            {
                iconImage.sprite = currentGodSo.Icon;
                iconImage.enabled = currentGodSo.Icon != null;
            }

            int faithLevel = currentManager.GetFaithLevel(currentGodSo.GodType);
            if (faithLevelText != null)
                faithLevelText.text = $"Faith Lv.{faithLevel}";

            if (stateText != null)
            {
                FaithStageState state = FaithStageState.Normal;
                ShrinePlayerRuntimeData playerData = currentManager.PlayerRuntimeData;
                bool hasLockedFaith = playerData != null && playerData.HasLockedFaith && playerData.LockedGod == currentGodSo.GodType;

                if (hasLockedFaith) state = FaithStageState.Locked;
                else if (faithLevel >= 7) state = FaithStageState.Successor;
                else if (faithLevel >= 5) state = FaithStageState.Devoted;
                else if (faithLevel >= 1) state = FaithStageState.Influenced;

                stateText.text = state.ToString();
            }

            if (missionText != null)
            {
                StringBuilder sb = new StringBuilder();

                if (currentGodSo.UnlockMissions != null)
                {
                    foreach (var mission in currentGodSo.UnlockMissions)
                    {
                        if (mission == null) continue;
                        if (sb.Length > 0) sb.AppendLine();
                        sb.Append($"Unlock: {mission.DisplayName}");
                    }
                }

                if (currentGodSo.FaithMissions != null)
                {
                    foreach (var mission in currentGodSo.FaithMissions)
                    {
                        if (mission == null) continue;
                        if (sb.Length > 0) sb.AppendLine();
                        sb.Append($"Faith: {mission.DisplayName}");
                    }
                }

                missionText.text = sb.ToString();
            }
        }

        public void HideImmediate()
        {
            if (rootGroup != null)
            {
                rootGroup.alpha = 0f;
                rootGroup.interactable = false;
                rootGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }

        public void PlayShow()
        {
            gameObject.SetActive(true);
            if (sequence != null && rootGroup != null)
            {
                sequence.Clear();
                fadeAnim.targetGroup = rootGroup;
                fadeAnim.fromAlpha = rootGroup.alpha;
                fadeAnim.toAlpha = 1f;
                fadeAnim.duration = fadeDuration;
                fadeAnim.delay = 0f;
                sequence.Append(fadeAnim);
                
                sequence.Play(() => 
                {
                    rootGroup.interactable = true;
                    rootGroup.blocksRaycasts = true;
                });
            }
        }

        public void PlayHide()
        {
            if (sequence != null && rootGroup != null)
            {
                rootGroup.interactable = false;
                rootGroup.blocksRaycasts = false;
                
                sequence.Clear();
                fadeAnim.targetGroup = rootGroup;
                fadeAnim.fromAlpha = rootGroup.alpha;
                fadeAnim.toAlpha = 0f;
                fadeAnim.duration = fadeDuration;
                fadeAnim.delay = 0f;
                sequence.Append(fadeAnim);
                
                sequence.Play(() => 
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                HideImmediate();
            }
        }
}
