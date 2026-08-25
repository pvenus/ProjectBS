//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UIFramework.Data;
//using UIFramework.Interfaces;
//using UIFramework.View;
//using ProjectBS.Core;
//using Shrine;
//using Bless;
//using Item;
//using Effect;

//namespace UIFramework.Test
//{
//    public class TestFaithManager : MonoBehaviour, IFaithManager
//    {
//        [Header("Views")]
//        public FaithSummaryView faithSummaryView;
//        public FaithDetailView faithDetailView;

//        [Header("Mock Configurations")]
//        public Sprite defaultLockedSilhouetteIcon;
//        public Sprite blessingUpgradeIcon;
//        public Sprite classEvolutionIcon;
//        public Sprite questionMarkIcon;

//        [Header("Test Controls")]
//        public int addReputationAmount = 100;

//        public event Action<FaithSummaryUIViewData> OnFaithSummaryChanged;
//        public event Action<FaithDetailUIViewData> OnFaithDetailChanged;

//        private List<ShrineGodSO> _gods = new List<ShrineGodSO>();
//        private Dictionary<string, int> _faithReputations = new Dictionary<string, int>();
//        private List<FaithLevelThresholdData> _levelThresholds = new List<FaithLevelThresholdData>();

//        private void Awake()
//        {
//            AppManagers.Faith = this;
//            InitializeThresholds();
//            LoadGodAssets();
//            InitializeDefaultReputations();
//        }

//        private void Reset()
//        {
//            InitializeThresholds();
//            LoadGodAssets();
//            InitializeDefaultReputations();
//        }

//        private void InitializeThresholds()
//        {
//            // 1~10 레벨 임계치 설정
//            _levelThresholds = new List<FaithLevelThresholdData>
//            {
//                new FaithLevelThresholdData { level = 1, requiredTotalReputation = 100 },
//                new FaithLevelThresholdData { level = 2, requiredTotalReputation = 200 },
//                new FaithLevelThresholdData { level = 3, requiredTotalReputation = 400 },
//                new FaithLevelThresholdData { level = 4, requiredTotalReputation = 700 },
//                new FaithLevelThresholdData { level = 5, requiredTotalReputation = 1100 },
//                new FaithLevelThresholdData { level = 6, requiredTotalReputation = 1600 },
//                new FaithLevelThresholdData { level = 7, requiredTotalReputation = 2200 },
//                new FaithLevelThresholdData { level = 8, requiredTotalReputation = 2900 },
//                new FaithLevelThresholdData { level = 9, requiredTotalReputation = 3700 },
//                new FaithLevelThresholdData { level = 10, requiredTotalReputation = 4600 }
//            };
//        }

//        private void LoadGodAssets()
//        {
//            // Resources.LoadAll을 사용하여 shring 폴더 하위의 모든 ShrineGodSO 로드
//            var loaded = Resources.LoadAll<ShrineGodSO>("shring");
//            if (loaded != null && loaded.Length > 0)
//            {
//                _gods = loaded.ToList();
//                Debug.Log($"[TestFaithManager] Successfully loaded {_gods.Count} ShrineGodSO assets from Resources.");
//                foreach (var god in _gods)
//                {
//                    Debug.Log($"- God: {god.DisplayName} (ID: {god.godId}) Pools: {god.blessingPools.Count} Relics: {god.faithRelicRewards.Count}");
//                }
//            }
//            else
//            {
//                Debug.LogWarning("[TestFaithManager] No ShrineGodSO assets found in Resources/shring. Creating fallback mock data.");
//                CreateFallbackGodAssets();
//            }
//        }

//        private void CreateFallbackGodAssets()
//        {
//            _gods = new List<ShrineGodSO>();

//            // 생명의 신
//            var lifeGod = ScriptableObject.CreateInstance<ShrineGodSO>();
//            lifeGod.godId = "life";
//            lifeGod.godName = "생명의 신";
//            lifeGod.description = "생명력을 증진시키고 회복을 도우는 신";
//            _gods.Add(lifeGod);

//            // 전쟁의 신
//            var warGod = ScriptableObject.CreateInstance<ShrineGodSO>();
//            warGod.godId = "war";
//            warGod.godName = "전쟁의 신";
//            warGod.description = "용맹과 파괴력을 상징하며 공격을 인도하는 신";
//            _gods.Add(warGod);

//            // 탐욕의 신
//            var greedGod = ScriptableObject.CreateInstance<ShrineGodSO>();
//            greedGod.godId = "greed";
//            greedGod.godName = "탐욕의 신";
//            greedGod.description = "재화의 축적과 획득에 축복을 내리는 신";
//            _gods.Add(greedGod);

//            // 어둠의 신
//            var darkGod = ScriptableObject.CreateInstance<ShrineGodSO>();
//            darkGod.godId = "dark";
//            darkGod.godName = "어둠의 신";
//            darkGod.description = "그림자 속에서 적을 기습하고 치명적 피해를 내리는 신";
//            _gods.Add(darkGod);
//        }

//        private void InitializeDefaultReputations()
//        {
//            _faithReputations.Clear();
            
//            // 테스트를 위해 각 신의 기본 평판을 다르게 설정
//            // 생명의 신: 150 평판 (1레벨 활성화, 2레벨 구간 진행 중)
//            // 전쟁의 신: 450 평판 (3레벨 활성화, 4레벨 구간 진행 중)
//            // 탐욕의 신: 50 평판 (1레벨 미도달, 비활성화)
//            // 어둠의 신: 0 평판 (비활성화)
//            _faithReputations["life"] = 150;
//            _faithReputations["war"] = 450;
//            _faithReputations["greed"] = 50;
//            _faithReputations["dark"] = 0;
//        }

//        private int GetCurrentLevel(int reputation)
//        {
//            int currentLevel = 0;
//            var sorted = _levelThresholds.OrderByDescending(t => t.level).ToList();
//            foreach (var threshold in sorted)
//            {
//                if (reputation >= threshold.requiredTotalReputation)
//                {
//                    return threshold.level;
//                }
//            }
//            return currentLevel;
//        }

//        private int GetLevelStartReputation(int level)
//        {
//            if (level <= 0) return 0;
//            var threshold = _levelThresholds.FirstOrDefault(t => t.level == level);
//            return threshold != null ? threshold.requiredTotalReputation : 0;
//        }

//        private int GetLevelEndReputation(int level)
//        {
//            var threshold = _levelThresholds.FirstOrDefault(t => t.level == level);
//            return threshold != null ? threshold.requiredTotalReputation : 0;
//        }

//        public FaithSummaryUIViewData GetFaithSummary()
//        {
//            var summaryData = new FaithSummaryUIViewData
//            {
//                title = "신앙 요약",
//                items = new List<FaithSummaryItemViewData>()
//            };

//            int activeThreshold = 100; // 1레벨 요구수치
//            var lvl1 = _levelThresholds.FirstOrDefault(t => t.level == 1);
//            if (lvl1 != null)
//            {
//                activeThreshold = lvl1.requiredTotalReputation;
//            }

//            foreach (var god in _gods)
//            {
//                string godKey = god.godId;
//                if (string.IsNullOrEmpty(godKey)) continue;

//                int reputation = _faithReputations.ContainsKey(godKey) ? _faithReputations[godKey] : 0;
//                int currentLevel = GetCurrentLevel(reputation);
//                int currentLevelRequired = GetLevelStartReputation(currentLevel);
                
//                int nextLevelRequired = 0;
//                bool isMaxLevel = false;

//                var nextThreshold = _levelThresholds.OrderBy(t => t.level).FirstOrDefault(t => t.level > currentLevel);
//                if (nextThreshold != null)
//                {
//                    nextLevelRequired = nextThreshold.requiredTotalReputation;
//                }
//                else
//                {
//                    isMaxLevel = true;
//                }

//                int currentLevelReputation = isMaxLevel ? 0 : reputation - currentLevelRequired;
//                int nextLevelRequiredReputation = isMaxLevel ? 1 : nextLevelRequired - currentLevelRequired;
//                float progress = isMaxLevel ? 1f : Mathf.Clamp01((float)currentLevelReputation / nextLevelRequiredReputation);

//                bool isActive = reputation >= activeThreshold;

//                var itemData = new FaithSummaryItemViewData
//                {
//                    faithId = godKey,
//                    displayName = god.DisplayName,
//                    icon = god.icon,
//                    totalReputation = reputation,
//                    currentLevel = currentLevel,
//                    nextLevel = isMaxLevel ? currentLevel : currentLevel + 1,
//                    currentLevelReputation = currentLevelReputation,
//                    nextLevelRequiredReputation = nextLevelRequiredReputation,
//                    progress01 = progress,
//                    isMaxLevel = isMaxLevel
//                };

//                if (isActive)
//                {
//                    summaryData.items.Add(itemData);
//                }
//            }

//            return summaryData;
//        }

//        private string GetStatName(Stat.StatType statType)
//        {
//            switch (statType)
//            {
//                case Stat.StatType.MaxHp:
//                case Stat.StatType.MaxHpPercent:
//                    return "체력";
//                case Stat.StatType.Attack:
//                case Stat.StatType.AttackPercent:
//                    return "공격력";
//                case Stat.StatType.HpRegen:
//                    return "회복량";
//                case Stat.StatType.AttackSpeed:
//                case Stat.StatType.AttackSpeedPercent:
//                    return "공격 속도";
//                case Stat.StatType.MoveSpeed:
//                case Stat.StatType.MoveSpeedPercent:
//                    return "이동 속도";
//                case Stat.StatType.CritChance:
//                    return "치명타 확률";
//                case Stat.StatType.CritDamage:
//                    return "치명타 피해";
//                case Stat.StatType.Defense:
//                    return "방어력";
//                case Stat.StatType.CooldownReduction:
//                    return "재사용 대기시간 감소";
//                case Stat.StatType.GoldGain:
//                    return "골드 획득량";
//                case Stat.StatType.ExpGain:
//                    return "경험치 획득량";
//                default:
//                    return statType.ToString();
//            }
//        }

//        private string GetBlessingComparisonText(BlessSO currentBless, BlessSO nextBless)
//        {
//            if (nextBless == null) return "";
//            if (currentBless == null)
//            {
//                List<string> lines = new List<string>();
//                foreach (var effect in nextBless.effects)
//                {
//                    if (effect is StatModifierEffectSO statEffect)
//                    {
//                        string statName = GetStatName(statEffect.targetStat);
//                        bool isPct = statEffect.modifierType == StatModifierType.Percent;
//                        string valStr = isPct ? $"+{statEffect.value * 100f}%" : $"+{statEffect.value}";
//                        lines.Add($"{statName} {valStr} 활성화");
//                    }
//                    else if (effect != null && !string.IsNullOrWhiteSpace(effect.description))
//                    {
//                        lines.Add(effect.description);
//                    }
//                }
//                return lines.Count > 0 ? string.Join("\n", lines) : nextBless.description;
//            }

//            List<string> comparisonLines = new List<string>();
//            foreach (var nextEffect in nextBless.effects)
//            {
//                if (nextEffect is StatModifierEffectSO nextStatEffect)
//                {
//                    var currStatEffect = currentBless.effects
//                        .OfType<StatModifierEffectSO>()
//                        .FirstOrDefault(e => e.targetStat == nextStatEffect.targetStat && e.modifierType == nextStatEffect.modifierType);

//                    string statName = GetStatName(nextStatEffect.targetStat);
//                    bool isPct = nextStatEffect.modifierType == StatModifierType.Percent;
                    
//                    float currVal = currStatEffect != null ? currStatEffect.value : 0f;
//                    float nextVal = nextStatEffect.value;
//                    float diff = nextVal - currVal;

//                    string currStr = isPct ? $"+{currVal * 100f}%" : $"+{currVal}";
//                    string nextStr = isPct ? $"+{nextVal * 100f}%" : $"+{nextVal}";
//                    string diffStr = isPct ? $"+{diff * 100f}%" : $"+{diff}";

//                    comparisonLines.Add($"{statName} {currStr} → <color=#00FF00>{nextStr}</color> (<color=#00FF00>{diffStr}</color>)");
//                }
//                else if (nextEffect != null && !string.IsNullOrWhiteSpace(nextEffect.description))
//                {
//                    comparisonLines.Add(nextEffect.description);
//                }
//            }

//            return comparisonLines.Count > 0 ? string.Join("\n", comparisonLines) : nextBless.description;
//        }

//        private BlessSO GetBlessingFromPool(ShrineGodSO god, string poolType, int progressionStep)
//        {
//            var pool = god.blessingPools?.FirstOrDefault(p => p != null && p.poolId.Contains(poolType));
//            if (pool == null && poolType == "Common")
//            {
//                pool = god.blessingPools?.FirstOrDefault();
//            }

//            if (pool != null && pool.blessings != null)
//            {
//                var entry = pool.blessings.FirstOrDefault(e => e != null && e.progressionStep == progressionStep);
//                return entry?.blessing;
//            }
//            return null;
//        }

//        public FaithDetailUIViewData GetFaithDetail()
//        {
//            var detailData = new FaithDetailUIViewData();

//            // 1차 순회: 확정 신앙(5레벨 이상) 체크
//            string dominantFaithId = null;
//            Dictionary<string, int> faithLevelMap = new Dictionary<string, int>();

//            foreach (var god in _gods)
//            {
//                string godKey = god.godId;
//                int rep = _faithReputations.ContainsKey(godKey) ? _faithReputations[godKey] : 0;
//                int level = GetCurrentLevel(rep);
//                faithLevelMap[godKey] = level;

//                if (level >= 5 && dominantFaithId == null)
//                {
//                    dominantFaithId = godKey;
//                }
//            }

//            // 2차 순회: 각 신앙의 1~10레벨 트랙 정보 및 효과 빌드
//            foreach (var god in _gods)
//            {
//                string godKey = god.godId;
//                int currentLevel = faithLevelMap[godKey];
//                int currentLevelRequired = GetLevelStartReputation(currentLevel);
//                int nextLevelRequired = 0;
//                bool isMaxLevel = false;

//                var nextThreshold = _levelThresholds.OrderBy(t => t.level).FirstOrDefault(t => t.level > currentLevel);
//                if (nextThreshold != null)
//                {
//                    nextLevelRequired = nextThreshold.requiredTotalReputation;
//                }
//                else
//                {
//                    isMaxLevel = true;
//                }

//                int currentLevelReputation = isMaxLevel ? 0 : _faithReputations[godKey] - currentLevelRequired;
//                int nextLevelRequiredReputation = isMaxLevel ? 1 : nextLevelRequired - currentLevelRequired;
//                float progress = isMaxLevel ? 1f : Mathf.Clamp01((float)currentLevelReputation / nextLevelRequiredReputation);

//                var colViewData = new FaithColumnViewData
//                {
//                    faithId = godKey,
//                    displayName = god.DisplayName,
//                    icon = god.icon,
//                    totalReputation = _faithReputations[godKey],
//                    currentLevel = currentLevel,
//                    currentLevelReputation = currentLevelReputation,
//                    nextLevelRequiredReputation = nextLevelRequiredReputation,
//                    levelProgress01 = (float)currentLevel / 10f // 10칸 게이지바 용도
//                };

//                // 확정신앙이 본인이 아닌 경우 락아웃 처리
//                bool isLockedOut = (dominantFaithId != null && dominantFaithId != godKey);

//                // 1) 1~10레벨 노드 빌드
//                for (int reqLvl = 1; reqLvl <= 10; reqLvl++)
//                {
//                    bool isUnlocked = !isLockedOut && (currentLevel >= reqLvl);
//                    Sprite nodeIcon = null;
//                    string title = "";
//                    string description = "";

//                    // 노드 상태 결정
//                    string nodeStatus = "Future";
//                    if (isLockedOut && reqLvl >= 5)
//                    {
//                        nodeStatus = "LockedOut";
//                    }
//                    else
//                    {
//                        if (currentLevel > reqLvl) nodeStatus = "Cleared";
//                        else if (currentLevel == reqLvl) nodeStatus = "Current";
//                        else nodeStatus = "Future";
//                    }

//                    // 레벨별 보상 바인딩
//                    switch (reqLvl)
//                    {
//                        case 1:
//                            title = "기본 축복 활성화";
//                            nodeIcon = god.icon;
//                            var bless1 = GetBlessingFromPool(god, "Common", 1);
//                            description = $"[기본 축복 활성화]\n신앙의 기본 축복이 활성화됩니다.\n\n효과:\n{(bless1 != null ? bless1.description : "기본 축복 1단계 활성화")}";
//                            break;
//                        case 2:
//                        case 4:
//                        case 6:
//                        case 7:
//                        case 9:
//                            title = "기본 축복 강화";
//                            nodeIcon = blessingUpgradeIcon != null ? blessingUpgradeIcon : god.icon;
//                            var prevBless = GetBlessingFromPool(god, "Common", reqLvl - 1);
//                            var currBless = GetBlessingFromPool(god, "Common", reqLvl);
//                            description = $"[기본 축복 강화]\n기본 축복의 성능이 한 단계 강화됩니다.\n\n강화 수치:\n{GetBlessingComparisonText(prevBless, currBless)}";
//                            break;
//                        case 3:
//                            title = "전용 유물 획득";
//                            var relic = god.GetFaithRelicReward(3);
//                            if (relic != null)
//                            {
//                                title = relic.DisplayName;
//                                nodeIcon = relic.icon;
//                                description = $"[전용 유물 획득]\n{relic.DisplayName} 유물을 획득합니다.\n\n유물 효과:\n{relic.Description}";
//                            }
//                            else
//                            {
//                                nodeIcon = defaultLockedSilhouetteIcon;
//                                description = "[유물 획득]\n3레벨 달성 시 고유 유물을 획득합니다.";
//                            }
//                            break;
//                        case 5:
//                            title = "확정 신앙 & 1차 진화";
//                            nodeIcon = questionMarkIcon != null ? questionMarkIcon : god.icon;
//                            var bless5 = GetBlessingFromPool(god, "Enhanced", 5);
//                            description = $"[확정 신앙]\n이 신앙을 선택하면 나머지 다른 신앙들의 효과는 전부 비활성화됩니다.\n\n[1차 직업 진화]\n{god.DisplayName} 계열 직업의 캐릭터가 진화(성전사)합니다.\n\n[추가 축복 획득]\n{(bless5 != null ? bless5.DisplayName : "5레벨 추가 축복")}을 획득합니다.\n\n축복 효과:\n{(bless5 != null ? bless5.description : "5레벨 추가 축복 획득")}";
//                            break;
//                        case 8:
//                            title = "추가 축복 강화";
//                            nodeIcon = blessingUpgradeIcon != null ? blessingUpgradeIcon : god.icon;
//                            var b5 = GetBlessingFromPool(god, "Enhanced", 5);
//                            var b8 = GetBlessingFromPool(god, "Enhanced", 8);
//                            description = $"[추가 축복 강화]\n5레벨에 획득한 추가 축복 효과가 강화됩니다.\n\n강화 수치:\n{GetBlessingComparisonText(b5, b8)}";
//                            break;
//                        case 10:
//                            title = "2차 직업 최종 진화";
//                            nodeIcon = classEvolutionIcon != null ? classEvolutionIcon : god.icon;
//                            description = $"[2차 직업 최종 진화]\n{god.DisplayName} 계열 직업의 캐릭터가 최종 형태의 직업(대성천사)으로 진화합니다.\n\n해당 계열 클래스의 잠재력이 한계까지 발휘됩니다.";
//                            break;
//                    }

//                    if (isLockedOut && reqLvl >= 5)
//                    {
//                        isUnlocked = false;
//                        description = "<color=red>[잠금됨] 다른 신앙이 이미 확정 도달하여 해금할 수 없습니다.</color>\n\n" + description;
//                    }

//                    colViewData.nodes.Add(new FaithNodeViewData
//                    {
//                        nodeId = $"{godKey}_{reqLvl}",
//                        requiredLevel = reqLvl,
//                        isUnlocked = isUnlocked,
//                        icon = (isUnlocked && nodeIcon != null) ? nodeIcon : (defaultLockedSilhouetteIcon != null ? defaultLockedSilhouetteIcon : nodeIcon),
//                        title = title,
//                        description = description,
//                        status = nodeStatus
//                    });
//                }

//                // 2) 현재 효과 리스트 빌드 (현재레벨 기준 적용 중인 효과들 분할)
//                if (currentLevel >= 1 && !isLockedOut)
//                {
//                    // 기본 축복
//                    var activeCommon = GetBlessingFromPool(god, "Common", currentLevel);
//                    if (activeCommon != null)
//                    {
//                        colViewData.currentEffects.Add(new FaithEffectItemData
//                        {
//                            icon = activeCommon.icon != null ? activeCommon.icon : god.icon,
//                            title = $"기본 축복: {activeCommon.DisplayName}",
//                            description = activeCommon.description,
//                            type = "Blessing"
//                        });
//                    }

//                    // 유물
//                    if (currentLevel >= 3)
//                    {
//                        var relic = god.GetFaithRelicReward(3);
//                        if (relic != null)
//                        {
//                            colViewData.currentEffects.Add(new FaithEffectItemData
//                            {
//                                icon = relic.icon,
//                                title = $"유물: {relic.DisplayName}",
//                                description = relic.Description,
//                                type = "Relic"
//                            });
//                        }
//                    }

//                    // 1차 진화 & 추가 축복
//                    if (currentLevel >= 5)
//                    {
//                        colViewData.currentEffects.Add(new FaithEffectItemData
//                        {
//                            icon = classEvolutionIcon != null ? classEvolutionIcon : god.icon,
//                            title = "1차 직업 진화",
//                            description = $"{god.DisplayName} 계열 캐릭터가 성전사로 진화했습니다.",
//                            type = "Evolution"
//                        });

//                        int enhanceStep = currentLevel >= 8 ? 8 : 5;
//                        var activeEnhanced = GetBlessingFromPool(god, "Enhanced", enhanceStep);
//                        if (activeEnhanced != null)
//                        {
//                            colViewData.currentEffects.Add(new FaithEffectItemData
//                            {
//                                icon = activeEnhanced.icon != null ? activeEnhanced.icon : god.icon,
//                                title = (enhanceStep == 8 ? "강화 축복: " : "추가 축복: ") + activeEnhanced.DisplayName,
//                                description = activeEnhanced.description,
//                                type = "Blessing"
//                            });
//                        }
//                    }

//                    // 2차 진화
//                    if (currentLevel >= 10)
//                    {
//                        colViewData.currentEffects.Add(new FaithEffectItemData
//                        {
//                            icon = classEvolutionIcon != null ? classEvolutionIcon : god.icon,
//                            title = "2차 직업 최종 진화",
//                            description = $"{god.DisplayName} 계열 캐릭터가 대성천사로 진화했습니다.",
//                            type = "Evolution"
//                        });
//                    }
//                }
//                else
//                {
//                    colViewData.currentEffects.Add(new FaithEffectItemData
//                    {
//                        icon = defaultLockedSilhouetteIcon,
//                        title = "비활성화 상태",
//                        description = "평판이 부족하여 축복이 아직 활성화되지 않았습니다.",
//                        type = "Blessing"
//                    });
//                }

//                // 3) 다음 레벨 효과 리스트 빌드 (다음레벨에서 상승/해금되는 효과 비교)
//                if (isMaxLevel)
//                {
//                    colViewData.nextEffects.Add(new FaithEffectItemData
//                    {
//                        icon = god.icon,
//                        title = "최대 레벨 도달",
//                        description = "신앙이 최고 레벨에 도달하여 모든 효과가 극대화되었습니다.",
//                        type = "Blessing"
//                    });
//                }
//                else if (isLockedOut)
//                {
//                    colViewData.nextEffects.Add(new FaithEffectItemData
//                    {
//                        icon = defaultLockedSilhouetteIcon,
//                        title = "해금 불가 (잠금)",
//                        description = "다른 신앙이 이미 확정 도달하여 해금할 수 없습니다.",
//                        type = "Blessing"
//                    });
//                }
//                else
//                {
//                    int nextLvl = currentLevel + 1;

//                    // 기본 축복 강화 예정 비교
//                    var currCommon = GetBlessingFromPool(god, "Common", currentLevel);
//                    var nextCommon = GetBlessingFromPool(god, "Common", nextLvl);
//                    if (nextCommon != null)
//                    {
//                        string compText = GetBlessingComparisonText(currCommon, nextCommon);
//                        colViewData.nextEffects.Add(new FaithEffectItemData
//                        {
//                            icon = blessingUpgradeIcon != null ? blessingUpgradeIcon : nextCommon.icon,
//                            title = "기본 축복 강화 예정",
//                            description = compText,
//                            type = "Blessing"
//                        });
//                    }

//                    // 유물 해금 예정
//                    if (nextLvl == 3)
//                    {
//                        var relic = god.GetFaithRelicReward(3);
//                        if (relic != null)
//                        {
//                            colViewData.nextEffects.Add(new FaithEffectItemData
//                            {
//                                icon = relic.icon,
//                                title = $"[Lv3 해금] 유물: {relic.DisplayName}",
//                                description = relic.Description,
//                                type = "Relic"
//                            });
//                        }
//                    }

//                    // 확정 신앙 & 1차 진화 예정
//                    if (nextLvl == 5)
//                    {
//                        var bless5 = GetBlessingFromPool(god, "Enhanced", 5);
//                        colViewData.nextEffects.Add(new FaithEffectItemData
//                        {
//                            icon = questionMarkIcon != null ? questionMarkIcon : god.icon,
//                            title = "[Lv5 해금] 확정 신앙 & 1차 진화",
//                            description = $"[1차 진화] 성전사 캐릭터 진화\n[추가 축복] {(bless5 != null ? bless5.DisplayName : "추가 축복 획득")}\n* 타 신앙 잠금 처리",
//                            type = "Evolution"
//                        });
//                    }

//                    // 추가 축복 강화 예정
//                    if (nextLvl == 8)
//                    {
//                        var b5 = GetBlessingFromPool(god, "Enhanced", 5);
//                        var b8 = GetBlessingFromPool(god, "Enhanced", 8);
//                        if (b8 != null)
//                        {
//                            string compText = GetBlessingComparisonText(b5, b8);
//                            colViewData.nextEffects.Add(new FaithEffectItemData
//                            {
//                                icon = blessingUpgradeIcon != null ? blessingUpgradeIcon : b8.icon,
//                                title = "[Lv8 해금] 추가 축복 강화 예정",
//                                description = compText,
//                                type = "Blessing"
//                            });
//                        }
//                    }

//                    // 최종 진화 예정
//                    if (nextLvl == 10)
//                    {
//                        colViewData.nextEffects.Add(new FaithEffectItemData
//                        {
//                            icon = classEvolutionIcon != null ? classEvolutionIcon : god.icon,
//                            title = "[Lv10 해금] 2차 최종 진화",
//                            description = $"{god.DisplayName} 계열 캐릭터가 대성천사로 최종 진화합니다.",
//                            type = "Evolution"
//                        });
//                    }
//                }

//                detailData.faithColumns.Add(colViewData);
//            }

//            return detailData;
//        }

//        public void AddReputation(string faithId, int amount)
//        {
//            if (_faithReputations.ContainsKey(faithId))
//            {
//                _faithReputations[faithId] += amount;
//                Debug.Log($"[TestFaithManager] Added {amount} reputation to {faithId}. Total: {_faithReputations[faithId]}");

//                OnFaithSummaryChanged?.Invoke(GetFaithSummary());
//                OnFaithDetailChanged?.Invoke(GetFaithDetail());

//                RefreshViews();
//            }
//        }

//        [ContextMenu("Open Faith Summary")]
//        public void OpenFaithSummary()
//        {
//            if (faithSummaryView == null)
//            {
//                faithSummaryView = FindObjectOfType<FaithSummaryView>(true);
//            }

//            if (faithSummaryView == null)
//            {
//                Debug.LogError("[TestFaithManager] FaithSummaryView is missing in the scene!");
//                return;
//            }

//            var viewData = GetFaithSummary();
//            faithSummaryView.Show(viewData, OnFaithItemSelected);
//        }

//        [ContextMenu("Open Faith Detail")]
//        public void OpenFaithDetail()
//        {
//            if (faithDetailView == null)
//            {
//                faithDetailView = FindObjectOfType<FaithDetailView>(true);
//            }

//            if (faithDetailView == null)
//            {
//                Debug.LogError("[TestFaithManager] FaithDetailView is missing in the scene!");
//                return;
//            }

//            var viewData = GetFaithDetail();
//            faithDetailView.Show(viewData);
//        }

//        [ContextMenu("Refresh Faith Summary")]
//        public void RefreshFaithSummary()
//        {
//            if (faithSummaryView != null && faithSummaryView.gameObject.activeInHierarchy)
//            {
//                faithSummaryView.Refresh(GetFaithSummary());
//            }
//            else
//            {
//                OpenFaithSummary();
//            }
//        }

//        [ContextMenu("Refresh Faith Detail")]
//        public void RefreshFaithDetail()
//        {
//            if (faithDetailView != null && faithDetailView.gameObject.activeInHierarchy)
//            {
//                faithDetailView.Refresh(GetFaithDetail());
//            }
//            else
//            {
//                OpenFaithDetail();
//            }
//        }

//        [ContextMenu("Add 100 Rep To War Faith")]
//        public void Add100RepToWar()
//        {
//            AddReputation("war", 100);
//        }

//        [ContextMenu("Add 1000 Rep To Life Faith (Force 5Lv)")]
//        public void Add1000RepToLife()
//        {
//            AddReputation("life", 1000);
//        }

//        [ContextMenu("Reset All Reputation")]
//        public void ResetAllReputation()
//        {
//            InitializeDefaultReputations();
//            Debug.Log("[TestFaithManager] Reset all faith reputation to default values.");
//            RefreshViews();
//        }

//        private void RefreshViews()
//        {
//            if (faithSummaryView != null && faithSummaryView.gameObject.activeInHierarchy)
//            {
//                faithSummaryView.Refresh(GetFaithSummary());
//            }
//            if (faithDetailView != null && faithDetailView.gameObject.activeInHierarchy)
//            {
//                faithDetailView.Refresh(GetFaithDetail());
//            }
//        }

//        private void OnFaithItemSelected(FaithSummaryItemViewData item)
//        {
//            Debug.Log($"[TestFaithManager] Selected Faith Item: {item.displayName} (ID: {item.faithId})");
//            OpenFaithDetail();
//        }
//    }
//}
