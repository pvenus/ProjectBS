//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using Character;
//using Stat;

//namespace Party.UI
//{
//    [AutoBindPrefix("UI")]
//    public class PartyMemberSummaryUI : AutoBindBehaviour
//    {
//        [Header("Hierarchy Auto Bindings")]
//        [AutoBind] [SerializeField] private Image UI_PortraitImage;
//        [AutoBind] [SerializeField] private TMP_Text UI_NameText;
//        [AutoBind] [SerializeField] private TMP_Text UI_ClassText;
//        [AutoBind] [SerializeField] private TMP_Text UI_LevelText;
//        [AutoBind] [SerializeField] private TMP_Text UI_HpText;
//        [AutoBind] [SerializeField] private TMP_Text UI_AttackText;
//        [AutoBind] [SerializeField] private TMP_Text UI_DefenseText;
//        [AutoBind] [SerializeField] private TMP_Text UI_TraitText;
        
//        [Header("State Overlays")]
//        [AutoBind] [SerializeField] private GameObject UI_StateOverlay;
//        [AutoBind] [SerializeField] private TMP_Text UI_StateText;
//        [AutoBind] [SerializeField] private CanvasGroup UI_CanvasGroup;

//        [Header("Visual Settings")]
//        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
//        [SerializeField] private Color deadColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
//        [SerializeField] private Sprite emptyPortraitPlaceholder;

//        /// <summary>
//        /// 외부에서 런타임 캐릭터 데이터를 직접 받아 바인딩
//        /// </summary>
//        public void SetData(CharacterRuntimeData data, bool isLocked = false)
//        {
//            if (data == null)
//            {
//                SetEmpty();
//                return;
//            }

//            var charSO = data.characterSO;

//            // 1. 기본 인적 정보 매핑
//            if (UI_NameText != null) UI_NameText.text = charSO != null ? charSO.DisplayName : "-";
//            if (UI_ClassText != null) UI_ClassText.text = charSO != null ? charSO.characterType.ToString() : "-";
//            if (UI_LevelText != null) UI_LevelText.text = $"Lv. {data.level}";

//            // 2. 초상화 매핑 (기존 에셋 구조를 유지하기 위해 characterId로 리소스 탐색)
//            if (UI_PortraitImage != null)
//            {
//                Sprite portraitSprite = null;
//                if (charSO != null && !string.IsNullOrEmpty(charSO.characterId))
//                {
//                    portraitSprite = Resources.Load<Sprite>($"portraits/{charSO.characterId}");
//                }
                
//                UI_PortraitImage.sprite = portraitSprite != null ? portraitSprite : emptyPortraitPlaceholder;
//                UI_PortraitImage.enabled = UI_PortraitImage.sprite != null;
//            }

//            // 3. 핵심 스탯 매핑 (MaxHP, Attack, Defense)
//            if (data.finalStats != null && data.finalStats.Count > 0)
//            {
//                float currentHp = data.GetStatValue(StatType.Hp);
//                float maxHp = data.GetStatValue(StatType.MaxHp);
//                float attack = data.GetStatValue(StatType.Attack);
//                float defense = data.GetStatValue(StatType.Defense);

//                if (UI_HpText != null) UI_HpText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
//                if (UI_AttackText != null) UI_AttackText.text = attack.ToString("F0");
//                if (UI_DefenseText != null) UI_DefenseText.text = defense.ToString("F0");
//            }
//            else
//            {
//                // 런타임 스탯이 초기화 전일 경우 기본 정보 기반 출력
//                if (UI_HpText != null) UI_HpText.text = "-";
//                if (UI_AttackText != null) UI_AttackText.text = "-";
//                if (UI_DefenseText != null) UI_DefenseText.text = "-";
//            }

//            // 4. 특성 매핑 (기본값 채우기)
//            if (UI_TraitText != null) UI_TraitText.text = "-";

//            // 5. 오버레이 및 특수 상태 처리 (Locked / Dead)
//            UpdateStateVisuals(isLocked, data.isDead);
//        }

//        public void Clear()
//        {
//            SetEmpty();
//        }

//        /// <summary>
//        /// 데이터가 비어있을 때의 Empty 상태를 연출합니다.
//        /// </summary>
//        public void SetEmpty()
//        {
//            if (UI_NameText != null) UI_NameText.text = "-";
//            if (UI_ClassText != null) UI_ClassText.text = "-";
//            if (UI_LevelText != null) UI_LevelText.text = "-";

//            if (UI_PortraitImage != null)
//            {
//                UI_PortraitImage.sprite = emptyPortraitPlaceholder;
//                UI_PortraitImage.enabled = emptyPortraitPlaceholder != null;
//            }

//            if (UI_HpText != null) UI_HpText.text = "-";
//            if (UI_AttackText != null) UI_AttackText.text = "-";
//            if (UI_DefenseText != null) UI_DefenseText.text = "-";

//            if (UI_TraitText != null) UI_TraitText.text = "파티원이 없습니다.";

//            if (UI_StateOverlay != null) UI_StateOverlay.SetActive(false);
//            if (UI_CanvasGroup != null) UI_CanvasGroup.alpha = 1f;
//        }

//        private void UpdateStateVisuals(bool isLocked, bool isDead)
//        {
//            if (UI_StateOverlay == null) return;

//            if (isLocked)
//            {
//                UI_StateOverlay.SetActive(true);
//                if (UI_StateText != null) UI_StateText.text = "잠김";
//                if (UI_CanvasGroup != null) UI_CanvasGroup.alpha = 0.5f;

//                if (UI_PortraitImage != null) UI_PortraitImage.color = lockedColor;
//            }
//            else if (isDead)
//            {
//                BindStateOverlayActive(true);
//                if (UI_StateText != null) UI_StateText.text = "전투 불능";
//                if (UI_CanvasGroup != null) UI_CanvasGroup.alpha = 0.6f;

//                if (UI_PortraitImage != null) UI_PortraitImage.color = deadColor;
//            }
//            else
//            {
//                BindStateOverlayActive(false);
//                if (UI_CanvasGroup != null) UI_CanvasGroup.alpha = 1f;

//                if (UI_PortraitImage != null) UI_PortraitImage.color = Color.white;
//            }
//        }

//        private void BindStateOverlayActive(bool active)
//        {
//            if (UI_StateOverlay != null)
//            {
//                UI_StateOverlay.SetActive(active);
//            }
//        }

//        #region ContextMenu Tests

//        [ContextMenu("Test/Bind Sample Data")]
//        private void BindSampleData()
//        {
//            var testSO = ScriptableObject.CreateInstance<CharacterSO>();
//            testSO.characterId = "war_god_id";
//            testSO.characterType = CharacterType.Player;

//            var testData = new CharacterRuntimeData
//            {
//                characterSO = testSO,
//                level = 5,
//                exp = 40,
//                isDead = false
//            };

//            testData.stats.Add(new StatEntry { statType = StatType.Hp, value = 120 });
//            testData.stats.Add(new StatEntry { statType = StatType.MaxHp, value = 120 });
//            testData.stats.Add(new StatEntry { statType = StatType.Attack, value = 34 });
//            testData.stats.Add(new StatEntry { statType = StatType.Defense, value = 15 });
//            testData.finalStats.AddRange(testData.stats);

//            SetData(testData, false);
//        }

//        [ContextMenu("Test/Bind Sample Data (Locked)")]
//        private void BindSampleDataLocked()
//        {
//            var testSO = ScriptableObject.CreateInstance<CharacterSO>();
//            testSO.characterId = "dark_god_id";
//            testSO.characterType = CharacterType.Npc;

//            var testData = new CharacterRuntimeData
//            {
//                characterSO = testSO,
//                level = 1,
//                exp = 0,
//                isDead = false
//            };

//            testData.stats.Add(new StatEntry { statType = StatType.Hp, value = 50 });
//            testData.stats.Add(new StatEntry { statType = StatType.MaxHp, value = 50 });
//            testData.stats.Add(new StatEntry { statType = StatType.Attack, value = 10 });
//            testData.stats.Add(new StatEntry { statType = StatType.Defense, value = 2 });
//            testData.finalStats.AddRange(testData.stats);

//            SetData(testData, true);
//        }

//        [ContextMenu("Test/Bind Sample Data (Dead)")]
//        private void BindSampleDataDead()
//        {
//            var testSO = ScriptableObject.CreateInstance<CharacterSO>();
//            testSO.characterId = "war_god_id";
//            testSO.characterType = CharacterType.Player;

//            var testData = new CharacterRuntimeData
//            {
//                characterSO = testSO,
//                level = 5,
//                exp = 40,
//                isDead = true
//            };

//            testData.stats.Add(new StatEntry { statType = StatType.Hp, value = 0 });
//            testData.stats.Add(new StatEntry { statType = StatType.MaxHp, value = 120 });
//            testData.stats.Add(new StatEntry { statType = StatType.Attack, value = 34 });
//            testData.stats.Add(new StatEntry { statType = StatType.Defense, value = 15 });
//            testData.finalStats.AddRange(testData.stats);

//            SetData(testData, false);
//        }

//        [ContextMenu("Test/Clear")]
//        private void TestClear()
//        {
//            Clear();
//        }

//        #endregion
//    }
//}
