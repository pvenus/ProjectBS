

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    [Serializable]
    public class StageRuntimeData
    {
        [Header("Stage")]
        public string stageId;
        public int currentDepth;
        public int currentRound;
        public int stageSeed;

        [Header("Graph")]
        public StageGraph currentGraph;
        public RoundNode currentNode;

        [Header("History")]
        public List<string> visitedNodeIds = new();
        public List<string> visitedEventIds = new();
        public List<string> recentEventIds = new();
        public List<string> unlockedRouteIds = new();

        [Header("Event Runtime")]
        public Dictionary<string, float> eventWeightModifiers = new();
        public Dictionary<string, float> tagWeightModifiers = new();
        public Dictionary<string, float> poolWeightModifiers = new();
        public Dictionary<string, int> eventAppearCounts = new();

        [Header("State")]
        public bool stageCompleted;
        public bool stageFailed;

        public void VisitNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return;
            }

            if (!visitedNodeIds.Contains(nodeId))
            {
                visitedNodeIds.Add(nodeId);
            }
        }

        public void VisitEvent(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            visitedEventIds.Add(eventId);

            if (!eventAppearCounts.ContainsKey(eventId))
            {
                eventAppearCounts[eventId] = 0;
            }

            eventAppearCounts[eventId]++;

            recentEventIds.Add(eventId);

            while (recentEventIds.Count > 5)
            {
                recentEventIds.RemoveAt(0);
            }
        }

        public void UnlockRoute(string routeId)
        {
            if (string.IsNullOrWhiteSpace(routeId))
            {
                return;
            }

            if (!unlockedRouteIds.Contains(routeId))
            {
                unlockedRouteIds.Add(routeId);
            }

            RoundNode node = currentGraph?.GetNode(routeId);
            if (node == null)
            {
                Debug.LogWarning($"[StageRuntimeData] UnlockRoute node not found. routeId={routeId}");
                return;
            }

            node.Reveal();
        }

        public bool IsRouteUnlocked(string routeId)
        {
            if (string.IsNullOrWhiteSpace(routeId))
            {
                return false;
            }

            return unlockedRouteIds.Contains(routeId);
        }

        public void AddTagModifier(
            string tag,
            float delta)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            if (!tagWeightModifiers.ContainsKey(tag))
            {
                tagWeightModifiers[tag] = 1f;
            }

            tagWeightModifiers[tag] *= delta;
        }

        public float GetTagModifier(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return 1f;
            }

            return tagWeightModifiers.TryGetValue(tag, out float value)
                ? value
                : 1f;
        }

        public void AddPoolModifier(
            string poolId,
            float delta)
        {
            if (string.IsNullOrWhiteSpace(poolId))
            {
                return;
            }

            if (!poolWeightModifiers.ContainsKey(poolId))
            {
                poolWeightModifiers[poolId] = 1f;
            }

            poolWeightModifiers[poolId] *= delta;
        }

        public float GetPoolModifier(string poolId)
        {
            if (string.IsNullOrWhiteSpace(poolId))
            {
                return 1f;
            }

            return poolWeightModifiers.TryGetValue(
                poolId,
                out float value)
                ? value
                : 1f;
        }

        public float GetEventModifier(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return 1f;
            }

            return eventWeightModifiers.TryGetValue(
                eventId,
                out float value)
                ? value
                : 1f;
        }

        public bool HasVisitedEvent(string eventId)
        {
            return visitedEventIds.Contains(eventId);
        }

        public int GetEventAppearCount(string eventId)
        {
            return eventAppearCounts.TryGetValue(
                eventId,
                out int count)
                ? count
                : 0;
        }
    }
}