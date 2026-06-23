using Character.Runtime.Skill;
using Skill;
using UnityEngine;

namespace Character.UI
{
    /// <summary>
    /// 캐릭터 머리 위 HP HUD와 같은 방식으로 활성 스킬 쿨타임을 표시한다.
    /// 표시 항목은 스킬 아이콘과 남은 쿨타임 초 단위 텍스트만 사용한다.
    /// </summary>
    public class CharacterSkillCooldownUI : MonoBehaviour
    {
        public static CharacterSkillCooldownUI EnsureFor(
            CharacterManager target,
            Transform parent = null)
        {
            if (target == null)
            {
                return null;
            }

            CharacterSkillCooldownUI existing =
                target.GetComponentInChildren<CharacterSkillCooldownUI>(true);

            if (existing != null)
            {
                return existing;
            }

            GameObject hudObject = new GameObject(
                "CharacterSkillCooldownUI",
                typeof(CharacterSkillCooldownUI));

            Transform hudParent = parent != null
                ? parent
                : target.transform;

            hudObject.transform.SetParent(hudParent, false);

            CharacterSkillCooldownUI ui =
                hudObject.GetComponent<CharacterSkillCooldownUI>();

            ui.skillManager =
                target.GetComponent<CharacterSkillManager>();

            hudObject.transform.localPosition =
                new Vector3(0f, 2.2f, 0f);

            return ui;
        }
        [SerializeField] private CharacterSkillManager skillManager;
        public CharacterSkillManager SkillManager
        {
            get => skillManager;
            set => skillManager = value;
        }
        [SerializeField] private CharacterSkillCooldownSlot slotPrefab;
        private const string SlotPrefabResourcePath =
            "ui/character/character_skill_cooldown_slot";

        [Header("Auto Create Slot")]
        [SerializeField] private int maxVisibleSlotCount = 4;
        [SerializeField] private float slotSpacing = 1.0f;

        private CharacterSkillCooldownSlot[] slots;
        private readonly System.Collections.Generic.List<string> activeCooldownOrder =
            new();

        private void Awake()
        {
            ResolveReferences();
            EnsureSlots();
        }

        private void LateUpdate()
        {
            ResolveReferences();
            EnsureSlots();
            Refresh();
        }

        private void ResolveReferences()
        {
            if (skillManager == null)
            {
                skillManager = GetComponentInParent<CharacterSkillManager>();
            }

            if (slotPrefab == null)
            {
                slotPrefab =
                    Resources.Load<CharacterSkillCooldownSlot>(
                        SlotPrefabResourcePath);
            }
        }

        private void EnsureSlots()
        {
            if (transform == null)
            {
                return;
            }

            if (slotPrefab == null)
            {
                return;
            }

            int slotCount = Mathf.Max(1, maxVisibleSlotCount);

            if (slots != null && slots.Length == slotCount)
            {
                return;
            }

            ClearSlots();
            slots = new CharacterSkillCooldownSlot[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                slots[i] = CreateSlot(i);
            }
        }

        private void ClearSlots()
        {
            if (slots == null)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    continue;
                }

                Destroy(slots[i].gameObject);
            }

            slots = null;
        }

        private CharacterSkillCooldownSlot CreateSlot(int index)
        {
            CharacterSkillCooldownSlot slot;

            if (slotPrefab != null)
            {
                slot = Instantiate(slotPrefab, transform);
            }
            else
            {
                return null;
            }

            slot.name = $"SkillCooldownSlot_{index}";

            slot.transform.localPosition =
                new Vector3(index * slotSpacing, 0f, 0f);
            slot.transform.localScale = Vector3.one;

            slot.Hide();
            return slot;
        }

        private void Refresh()
        {
            if (slots == null || slots.Length == 0)
            {
                return;
            }

            HideAllSlots();

            if (skillManager == null || skillManager.SkillRuntimeData == null)
            {
                return;
            }

            CharacterSkillRuntimeData runtime = skillManager.SkillRuntimeData;

            // Remove expired cooldowns from the FIFO order
            for (int i = activeCooldownOrder.Count - 1; i >= 0; i--)
            {
                string orderedSkillId = activeCooldownOrder[i];

                if (!runtime.cooldownEndTimes.TryGetValue(
                        orderedSkillId,
                        out float orderedCooldownEndTime) ||
                    orderedCooldownEndTime <= Time.time)
                {
                    activeCooldownOrder.RemoveAt(i);
                }
            }

            int visibleIndex = 0;

            foreach (SkillPoolSlotData skillPoolSlotData in runtime.skillPool.Slots)
            {
                if (skillPoolSlotData?.SkillSo == null)
                {
                    continue;
                }

                string skillId = skillPoolSlotData.SkillSo.EquipmentId;
                if (string.IsNullOrEmpty(skillId))
                {
                    continue;
                }

                if (!runtime.cooldownEndTimes.TryGetValue(
                        skillId,
                        out float cooldownEndTime))
                {
                    continue;
                }

                float remainingSeconds = cooldownEndTime - Time.time;

                if (remainingSeconds <= 0f)
                {
                    continue;
                }

                if (!activeCooldownOrder.Contains(skillId))
                {
                    activeCooldownOrder.Add(skillId);
                }

                // FIFO ordering handled below.
            }

            foreach (string skillId in activeCooldownOrder)
            {
                if (visibleIndex >= slots.Length)
                {
                    break;
                }

                if (!runtime.cooldownEndTimes.TryGetValue(
                        skillId,
                        out float cooldownEndTime))
                {
                    continue;
                }

                float remainingSeconds = cooldownEndTime - Time.time;

                if (remainingSeconds <= 0f)
                {
                    continue;
                }

                SkillPoolSlotData matchedSlot = null;

                foreach (SkillPoolSlotData slotData in runtime.skillPool.Slots)
                {
                    if (slotData?.SkillSo?.EquipmentId == skillId)
                    {
                        matchedSlot = slotData;
                        break;
                    }
                }

                if (matchedSlot?.SkillSo == null)
                {
                    continue;
                }

                CharacterSkillCooldownSlot uiSlot = slots[visibleIndex];

                if (uiSlot != null)
                {
                    uiSlot.Show(
                        matchedSlot.SkillSo.Icon,
                        remainingSeconds);
                }

                visibleIndex++;
            }
        }

        private void HideAllSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i]?.Hide();
            }
        }
    }
}