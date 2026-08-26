using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI.PartyHud
{
    [DisallowMultipleComponent]
    public sealed class PartyHudMemberView : MonoBehaviour
    {
        private const int ActiveSkillSlotCount = 4;

        [Header("Identity")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image portraitForegroundImage;
        [SerializeField] private TMP_Text nameText;

        [Header("Health")]
        [SerializeField] private Image hpBackgroundImage;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private TMP_Text hpText;

        [Header("Status")]
        [SerializeField] private GameObject statusRoot;
        [SerializeField] private Image statusIndicatorImage;
        [SerializeField] private TMP_Text statusText;

        [Header("Skills")]
        [SerializeField] private RectTransform activeSkillRoot;
        [SerializeField] private RectTransform passiveSkillRoot;
        [SerializeField] private PartyHudSkillSlotView skillSlotPrefab;

        [Header("Reuse")]
        [Tooltip("Skips HUD-only skill construction when this View is nested as a portrait-only selector.")]
        [SerializeField] private bool portraitOnlyMode;

        private readonly List<PartyHudSkillSlotView> activeSkillSlots =
            new List<PartyHudSkillSlotView>(ActiveSkillSlotCount);

        private PartyHudSkillSlotView passiveSkillSlot;
        private PartyHudMemberData data;
        private bool showBasicAttack = true;
        private bool missingPrefabReported;

        private void Awake()
        {
            if (portraitOnlyMode)
            {
                ApplyPortraitOnlyMode();
                return;
            }

            EnsureSkillSlots();
        }

        public void Render(PartyHudMemberData memberData, bool isBasicAttackVisible)
        {
            data = memberData;
            showBasicAttack = isBasicAttackVisible;
            gameObject.SetActive(memberData != null);

            if (memberData == null)
            {
                return;
            }

            EnsureSkillSlots();
            SetIdentity(memberData.DisplayName, memberData.Portrait);
            SetHealth(memberData.CurrentHp, memberData.MaxHp);
            SetStatus(memberData.StatusText, memberData.StatusColor);
            RenderSkills(memberData.ActiveSkills, memberData.PassiveSkill);
        }

        public void SetBasicAttackVisible(bool visible)
        {
            showBasicAttack = visible;

            if (data != null)
            {
                RenderSkills(data.ActiveSkills, data.PassiveSkill);
            }
        }

        public void SetIdentity(string displayName, Sprite portrait)
        {
            if (nameText != null)
            {
                nameText.text = displayName ?? string.Empty;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.enabled = portrait != null;
            }
        }

        public void RenderPortraitOnly(Sprite portrait)
        {
            portraitOnlyMode = true;
            ApplyPortraitOnlyMode();
            SetIdentity(string.Empty, portrait);
        }

        public void SetPortraitForeground(Sprite foreground)
        {
            if (portraitForegroundImage == null)
            {
                return;
            }

            portraitForegroundImage.sprite = foreground;
            portraitForegroundImage.enabled = foreground != null;
        }

        public void SetHealth(float currentHp, float maxHp)
        {
            float safeCurrent = Mathf.Max(0f, currentHp);
            float safeMax = Mathf.Max(0f, maxHp);

            if (hpFillImage != null)
            {
                hpFillImage.type = Image.Type.Filled;
                hpFillImage.fillMethod = Image.FillMethod.Horizontal;
                hpFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                hpFillImage.fillAmount =
                    safeMax <= 0f
                        ? 0f
                        : Mathf.Clamp01(safeCurrent / safeMax);
            }

            if (hpText != null)
            {
                hpText.text =
                    $"{Mathf.CeilToInt(safeCurrent):N0} / {Mathf.CeilToInt(safeMax):N0}";
            }
        }

        public void SetStatus(string label, Color color)
        {
            bool visible = !string.IsNullOrWhiteSpace(label);

            if (statusRoot != null)
            {
                statusRoot.SetActive(visible);
            }

            if (statusIndicatorImage != null)
            {
                statusIndicatorImage.color = color;
            }

            if (statusText != null)
            {
                statusText.text = visible ? label : string.Empty;
                statusText.color = color;
            }
        }

        private void RenderSkills(
            IReadOnlyList<PartyHudSkillSlotData> activeSkills,
            PartyHudSkillSlotData passiveSkill)
        {
            EnsureSkillSlots();

            PartyHudSkillSlotData basicAttack = null;
            List<PartyHudSkillSlotData> otherActiveSkills =
                new List<PartyHudSkillSlotData>(ActiveSkillSlotCount - 1);

            if (activeSkills != null)
            {
                for (int index = 0; index < activeSkills.Count; index++)
                {
                    PartyHudSkillSlotData skill = activeSkills[index];
                    if (skill == null)
                    {
                        continue;
                    }

                    if (basicAttack == null && skill.IsBasicAttack)
                    {
                        basicAttack = skill;
                    }
                    else
                    {
                        otherActiveSkills.Add(skill);
                    }
                }
            }

            if (basicAttack == null && otherActiveSkills.Count > 0)
            {
                basicAttack = otherActiveSkills[0];
                otherActiveSkills.RemoveAt(0);
            }

            if (activeSkillSlots.Count > 0)
            {
                PartyHudSkillSlotView basicSlot = activeSkillSlots[0];
                basicSlot.Render(basicAttack);
                basicSlot.gameObject.SetActive(showBasicAttack && basicAttack != null);
            }

            for (int slotIndex = 1; slotIndex < activeSkillSlots.Count; slotIndex++)
            {
                int dataIndex = slotIndex - 1;
                PartyHudSkillSlotData skill =
                    dataIndex < otherActiveSkills.Count
                        ? otherActiveSkills[dataIndex]
                        : null;

                activeSkillSlots[slotIndex].Render(skill);
            }

            if (passiveSkillSlot != null)
            {
                passiveSkillSlot.Render(passiveSkill);
                if (passiveSkill != null)
                {
                    passiveSkillSlot.SetState(PartyHudSkillState.Passive);
                }
            }
        }

        private void EnsureSkillSlots()
        {
            if (portraitOnlyMode)
            {
                return;
            }

            if (skillSlotPrefab == null)
            {
                if (!missingPrefabReported)
                {
                    Debug.LogError(
                        "[PartyHudMemberView] Skill slot prefab is not assigned.",
                        this);
                    missingPrefabReported = true;
                }

                return;
            }

            if (activeSkillRoot != null)
            {
                while (activeSkillSlots.Count < ActiveSkillSlotCount)
                {
                    PartyHudSkillSlotView slot =
                        Instantiate(skillSlotPrefab, activeSkillRoot);
                    slot.name =
                        activeSkillSlots.Count == 0
                            ? "BasicAttackSlot"
                            : $"ActiveSkillSlot_{activeSkillSlots.Count}";
                    activeSkillSlots.Add(slot);
                }
            }

            if (passiveSkillSlot == null && passiveSkillRoot != null)
            {
                passiveSkillSlot = Instantiate(skillSlotPrefab, passiveSkillRoot);
                passiveSkillSlot.name = "PassiveSkillSlot";
            }
        }

        private void ApplyPortraitOnlyMode()
        {
            SetActive(nameText, false);
            SetActive(hpBackgroundImage, false);
            SetActive(hpFillImage, false);
            SetActive(hpText, false);

            if (statusRoot != null)
            {
                statusRoot.SetActive(false);
            }

            if (activeSkillRoot != null)
            {
                activeSkillRoot.gameObject.SetActive(false);
            }

            if (passiveSkillRoot != null)
            {
                passiveSkillRoot.gameObject.SetActive(false);
            }
        }

        private static void SetActive(Component component, bool active)
        {
            if (component != null)
            {
                component.gameObject.SetActive(active);
            }
        }
    }
}
