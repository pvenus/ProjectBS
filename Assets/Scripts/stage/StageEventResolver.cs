using System;
using System.Collections.Generic;
using UnityEngine;
using Stage;

namespace Stage
{
    public sealed class StageEventResolver
    {
        private readonly bool logDebug;

        public StageEventResolver(
            bool logDebug = false)
        {
            this.logDebug = logDebug;
        }

        public bool Resolve(
            RoundNode node,
            StageRuntimeData runtimeData = null)
        {
            if (node == null)
            {
                return false;
            }

            if (!node.useRandomEventPool)
            {
                return true;
            }

            if (node.resolved)
            {
                return true;
            }

            EventPoolSO pool =
                node.randomPool;

            if (pool == null)
            {
                Debug.LogWarning(
                    $"[StageEventResolver] Pool not found : {node.nodeId}");
                return false;
            }

            RoundNodeSO selected =
                PickRandomNode(
                    pool,
                    runtimeData);

            if (selected == null)
            {
                Debug.LogWarning(
                    $"[StageEventResolver] Failed to resolve node from pool : {pool.poolId}");
                return false;
            }

            ApplyNode(node, selected);

            if (logDebug)
            {
                Debug.Log(
                    $"[StageEventResolver] Resolved {node.nodeId} -> {selected.nodeId}");
            }

            return true;
        }

        private RoundNodeSO PickRandomNode(
            EventPoolSO pool,
            StageRuntimeData runtimeData)
        {
            if (pool == null
                || pool.entries == null
                || pool.entries.Count <= 0)
            {
                return null;
            }

            List<EventPoolEntry> validEntries = new();
            List<float> runtimeWeights = new();
            float totalWeight = 0f;

            for (int i = 0; i < pool.entries.Count; i++)
            {
                EventPoolEntry entry =
                    pool.entries[i];

                if (entry == null
                    || entry.node == null)
                {
                    continue;
                }

                if (entry.weight <= 0)
                {
                    continue;
                }

                float finalWeight =
                    CalculateRuntimeWeight(
                        pool,
                        entry,
                        runtimeData);

                if (finalWeight <= 0f)
                {
                    continue;
                }

                validEntries.Add(entry);
                runtimeWeights.Add(finalWeight);
                totalWeight += finalWeight;
            }

            if (validEntries.Count <= 0
                || totalWeight <= 0)
            {
                return null;
            }

            float randomValue =
                UnityEngine.Random.Range(0f, totalWeight);

            float current = 0f;

            for (int i = 0; i < validEntries.Count; i++)
            {
                EventPoolEntry entry =
                    validEntries[i];

                current += runtimeWeights[i];

                if (randomValue < current)
                {
                    return entry.node;
                }
            }

            return validEntries[^1].node;
        }

        private float CalculateRuntimeWeight(
            EventPoolSO pool,
            EventPoolEntry entry,
            StageRuntimeData runtimeData)
        {
            if (entry == null)
            {
                return 0f;
            }

            float finalWeight = entry.weight;

            if (runtimeData == null)
            {
                return finalWeight;
            }

            if (pool != null)
            {
                finalWeight *=
                    runtimeData.GetPoolModifier(pool.poolId);
            }

            if (entry.node != null
                && entry.node.tags != null)
            {
                for (int i = 0; i < entry.node.tags.Count; i++)
                {
                    string tag = entry.node.tags[i];

                    finalWeight *=
                        runtimeData.GetTagModifier(tag);
                }
            }

            return Mathf.Max(0f, finalWeight);
        }

        private void ApplyNode(
            RoundNode target,
            RoundNodeSO source)
        {
            if (target == null
                || source == null)
            {
                return;
            }

            target.title = source.title;
            target.description = source.description;

            target.nodeType = source.nodeType;
            target.executeMode = source.executeMode;

            target.sceneName = source.sceneName;
            target.eventId = source.eventId;
            target.popupEvent = source.popupEvent;
            target.battleGroupId = source.battleGroupId;

            target.resolved = true;
        }
    }
}