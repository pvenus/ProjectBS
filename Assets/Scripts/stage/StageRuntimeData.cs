

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

        [Header("Event Runtime")]
        public Dictionary<string, float> eventWeightModifiers = new();
        public Dictionary<string, float> tagWeightModifiers = new();
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