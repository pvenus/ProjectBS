using System.Collections.Generic;
using Character;
using Battle.Prop;
using UnityEngine;

namespace Npc.Service
{
    /// <summary>
    /// NPC가 공격/추적할 후보 타겟을 찾는 서비스.
    /// NpcTargeting은 강제 타겟/갱신 주기만 관리하고,
    /// 실제 후보 탐색은 이 서비스에 위임한다.
    /// </summary>
    public class NpcTargetSearchService
    {
        public struct Context
        {
            public Transform self;
            public NpcTargeting.TargetingArchetype archetype;
            public bool includePartyMembersAsTargets;
            public bool includeTowersAsTargets;
            public bool includeBattlePropsAsTargets;
            public bool siegePrioritizeTowers;
            public LayerMask basicAttackTargetLayerMask;
        }

        public Transform FindTarget(Context context)
        {
            if (context.self == null)
            {
                return null;
            }

            if (context.archetype == NpcTargeting.TargetingArchetype.Flying)
            {
                return FindRandomAvailableTarget(context);
            }

            return FindNearestAvailableTarget(context);
        }

        private Transform FindNearestAvailableTarget(Context context)
        {
            bool siegeTowerOnly =
                context.archetype == NpcTargeting.TargetingArchetype.Siege;

            Vector3 selfPos = context.self.position;
            Transform best = null;
            float bestScore = float.MaxValue;

            if (!siegeTowerOnly && context.includePartyMembersAsTargets)
            {
                CharacterManager[] members =
                    Object.FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);

                for (int i = 0; i < members.Length; i++)
                {
                    CharacterManager member = members[i];

                    if (member == null || !member.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (member.RuntimeData == null || member.RuntimeData.characterSO == null || member.RuntimeData.characterSO.characterType != CharacterType.Player)
                    {
                        continue;
                    }

                    if (!IsLayerAllowed(member.gameObject, context.basicAttackTargetLayerMask))
                    {
                        continue;
                    }

                    ConsiderCandidate(
                        member.transform,
                        selfPos,
                        false,
                        context,
                        ref best,
                        ref bestScore);
                }
            }

            if (!siegeTowerOnly && context.includeBattlePropsAsTargets)
            {
                BattlePropController[] props =
                    Object.FindObjectsByType<BattlePropController>(FindObjectsSortMode.None);

                for (int i = 0; i < props.Length; i++)
                {
                    BattlePropController prop = props[i];

                    if (prop == null || !prop.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (!prop.IsTargetable())
                    {
                        continue;
                    }

                    if (!IsLayerAllowed(prop.gameObject, context.basicAttackTargetLayerMask))
                    {
                        continue;
                    }

                    ConsiderCandidate(
                        prop.transform,
                        selfPos,
                        false,
                        context,
                        ref best,
                        ref bestScore);
                }
            }

            if (context.includeTowersAsTargets)
            {
                TowerPropMono[] towers =
                    Object.FindObjectsByType<TowerPropMono>(FindObjectsSortMode.None);

                for (int i = 0; i < towers.Length; i++)
                {
                    TowerPropMono tower = towers[i];

                    if (tower == null || !tower.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (tower.IsDead())
                    {
                        continue;
                    }

                    if (!IsLayerAllowed(tower.gameObject, context.basicAttackTargetLayerMask))
                    {
                        continue;
                    }

                    ConsiderCandidate(
                        tower.transform,
                        selfPos,
                        true,
                        context,
                        ref best,
                        ref bestScore);
                }
            }

            return best;
        }

        private Transform FindRandomAvailableTarget(Context context)
        {
            List<Transform> candidates = new List<Transform>();

            if (context.includePartyMembersAsTargets)
            {
                CharacterManager[] members =
                    Object.FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);

                for (int i = 0; i < members.Length; i++)
                {
                    CharacterManager member = members[i];

                    if (member == null || !member.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (member.transform == context.self)
                    {
                        continue;
                    }

                    if (!IsLayerAllowed(member.gameObject, context.basicAttackTargetLayerMask))
                    {
                        continue;
                    }

                    candidates.Add(member.transform);
                }
            }

            if (context.includeBattlePropsAsTargets)
            {
                BattlePropController[] props =
                    Object.FindObjectsByType<BattlePropController>(FindObjectsSortMode.None);

                for (int i = 0; i < props.Length; i++)
                {
                    BattlePropController prop = props[i];

                    if (prop == null || !prop.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (!prop.IsTargetable())
                    {
                        continue;
                    }

                    if (prop.transform == context.self)
                    {
                        continue;
                    }

                    if (!IsLayerAllowed(prop.gameObject, context.basicAttackTargetLayerMask))
                    {
                        continue;
                    }

                    candidates.Add(prop.transform);
                }
            }

            if (context.includeTowersAsTargets)
            {
                TowerPropMono[] towers =
                    Object.FindObjectsByType<TowerPropMono>(FindObjectsSortMode.None);

                for (int i = 0; i < towers.Length; i++)
                {
                    TowerPropMono tower = towers[i];

                    if (tower == null || !tower.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (tower.IsDead())
                    {
                        continue;
                    }

                    if (!IsLayerAllowed(tower.gameObject, context.basicAttackTargetLayerMask))
                    {
                        continue;
                    }

                    candidates.Add(tower.transform);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            int index = Random.Range(0, candidates.Count);
            return candidates[index];
        }

        private bool IsLayerAllowed(
            GameObject target,
            LayerMask targetLayerMask)
        {
            if (target == null)
            {
                return false;
            }

            if (targetLayerMask.value == 0)
            {
                return true;
            }

            int targetLayerBit = 1 << target.layer;
            return (targetLayerMask.value & targetLayerBit) != 0;
        }

        private void ConsiderCandidate(
            Transform candidate,
            Vector3 selfPos,
            bool isTower,
            Context context,
            ref Transform best,
            ref float bestScore)
        {
            if (candidate == null)
            {
                return;
            }

            if (candidate == context.self)
            {
                return;
            }

            float score =
                (candidate.position - selfPos).sqrMagnitude;

            if (context.archetype == NpcTargeting.TargetingArchetype.Siege
                && context.siegePrioritizeTowers)
            {
                score *= isTower ? 0.35f : 1.65f;
            }

            if (score < bestScore)
            {
                best = candidate;
                bestScore = score;
            }
        }
    }
}
