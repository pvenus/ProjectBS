using Skill;
using UnityEngine;

namespace Character.UI
{
    /// <summary>
    /// 실제 스킬 사용 성공 이벤트를 머리 위 최근 스킬 View에 전달한다.
    /// 레거시 프리팹/코드 연결을 보존하기 위해 기존 클래스명을 유지한다.
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
                "CharacterRecentSkillPresenter");

            Transform hudParent = parent != null
                ? parent
                : target.transform;

            hudObject.transform.SetParent(hudParent, false);

            CharacterSkillCooldownUI ui =
                hudObject.AddComponent<CharacterSkillCooldownUI>();

            ui.SkillManager =
                target.GetComponent<CharacterSkillManager>();

            return ui;
        }
        [SerializeField] private CharacterSkillManager skillManager;
        public CharacterSkillManager SkillManager
        {
            get => skillManager;
            set
            {
                if (skillManager == value)
                {
                    return;
                }

                Unsubscribe();
                skillManager = value;

                if (isActiveAndEnabled)
                {
                    Subscribe();
                }
            }
        }
        [SerializeField] private CharacterSkillCooldownSlot slotPrefab;
        [SerializeField] private CharacterSkillCooldownSlot recentSkillView;
        private const string SlotPrefabResourcePath =
            "ui/character/character_skill_cooldown_slot";

        private bool isSubscribed;

        private void Awake()
        {
            ResolveReferences();
            EnsureView();
        }

        private void OnEnable()
        {
            ResolveReferences();
            EnsureView();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            recentSkillView?.Hide();
        }

        private void OnDestroy()
        {
            Unsubscribe();
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

        private void EnsureView()
        {
            if (recentSkillView != null || slotPrefab == null)
            {
                return;
            }

            recentSkillView = Instantiate(slotPrefab, transform);
            recentSkillView.name = "RecentSkillView";
            recentSkillView.Hide();
        }

        private void Subscribe()
        {
            if (isSubscribed || skillManager == null)
            {
                return;
            }

            skillManager.SkillUseSucceeded += HandleSkillUseSucceeded;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (skillManager != null)
            {
                skillManager.SkillUseSucceeded -= HandleSkillUseSucceeded;
            }

            isSubscribed = false;
        }

        private void HandleSkillUseSucceeded(
            EquipmentSkillRuntimeData runtime)
        {
            EnsureView();

            Sprite icon = runtime?.sourceEquipment?.Icon;
            recentSkillView?.ShowRecentSkill(icon);
        }
    }
}
