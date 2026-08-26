using System.Collections.Generic;
using Bless;
using Character;
using Item;
using Session;
using Shrine;
using UnityEngine;

namespace Stage.UI
{
    /// <summary>
    /// 팝업 표시 시점에 실제 플레이어 인벤토리를 읽기 전용으로 묶어 반환하는 빌더.
    ///
    /// 규칙:
    ///   - 게임 데이터를 변경하거나 테스트 데이터를 보충하지 않는다.
    ///   - 참조가 없으면 빈 목록을 반환하고 경고를 남긴다.
    /// </summary>
    public static class StagePlayerInventorySnapshotBuilder
    {
        // ── Public API ───────────────────────────────────────────────

        /// <summary>현재 파티의 CharacterRuntimeData 목록을 반환한다.</summary>
        public static IReadOnlyList<CharacterRuntimeData> BuildPartySnapshot()
        {
            GameSession gameSession = GameSession.Instance;
            if (gameSession == null)
            {
                Debug.LogWarning("[StagePlayerInventorySnapshotBuilder] GameSession.Instance is null.");
                return System.Array.Empty<CharacterRuntimeData>();
            }

            List<CharacterRuntimeData> members =
                gameSession.BattleSession?.PartyRuntimeData?.Members;

            if (members == null || members.Count == 0)
            {
                Debug.LogWarning("[StagePlayerInventorySnapshotBuilder] PartyRuntimeData.Members is empty.");
                return System.Array.Empty<CharacterRuntimeData>();
            }

            return members;
        }

        /// <summary>현재 보유 중인 RelicSO 목록을 반환한다.</summary>
        public static IReadOnlyList<RelicSO> BuildOwnedRelicsSnapshot()
        {
            ItemManager itemManager = ItemManager.Instance;
            if (itemManager == null)
            {
                Debug.LogWarning("[StagePlayerInventorySnapshotBuilder] ItemManager.Instance is null.");
                return System.Array.Empty<RelicSO>();
            }

            IReadOnlyList<RelicEntry> entries = itemManager.RelicRuntimeData?.Relics;
            if (entries == null || entries.Count == 0)
            {
                return System.Array.Empty<RelicSO>();
            }

            List<RelicSO> result = new List<RelicSO>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                RelicSO relic = entries[i]?.relic;
                if (relic != null)
                {
                    result.Add(relic);
                }
            }

            return result;
        }

        /// <summary>
        /// 일반 축복(GodType == None) 목록을 반환한다.
        /// </summary>
        public static IReadOnlyList<BlessRuntimeData.BlessEntry> BuildGeneralBlessSnapshot()
        {
            return BuildBlessSnapshot(isGeneral: true);
        }

        /// <summary>
        /// 신앙 축복(GodType != None) 목록을 반환한다.
        /// </summary>
        public static IReadOnlyList<BlessRuntimeData.BlessEntry> BuildFaithBlessSnapshot()
        {
            return BuildBlessSnapshot(isGeneral: false);
        }

        // ── 내부 헬퍼 ────────────────────────────────────────────────

        private static IReadOnlyList<BlessRuntimeData.BlessEntry> BuildBlessSnapshot(
            bool isGeneral)
        {
            BlessManager blessManager = BlessManager.Instance;
            if (blessManager == null)
            {
                Debug.LogWarning("[StagePlayerInventorySnapshotBuilder] BlessManager.Instance is null.");
                return System.Array.Empty<BlessRuntimeData.BlessEntry>();
            }

            IReadOnlyList<BlessRuntimeData.BlessEntry> all = blessManager.Blessings;
            if (all == null || all.Count == 0)
            {
                return System.Array.Empty<BlessRuntimeData.BlessEntry>();
            }

            List<BlessRuntimeData.BlessEntry> result = new List<BlessRuntimeData.BlessEntry>();
            for (int i = 0; i < all.Count; i++)
            {
                BlessRuntimeData.BlessEntry entry = all[i];
                if (entry?.source == null)
                {
                    continue;
                }

                bool isNone = entry.source.GodType == ShrineGodType.None;
                if (isGeneral == isNone)
                {
                    result.Add(entry);
                }
            }

            return result;
        }
    }
}
