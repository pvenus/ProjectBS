

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 스테이지 진행 중 플레이어가 선택하고 완료한 경로 기록.
    /// 저장/복구, 디버그 로그, 진행 경로 표시 UI에 사용할 수 있다.
    /// </summary>
    [Serializable]
    public class StageRouteRecord
    {
        [Header("Stage")]
        public string stageId;
        public string stageName;

        [Header("Route")]
        public List<StageRouteNodeRecord> records = new();

        public int Count => records?.Count ?? 0;

        public StageRouteRecord()
        {
        }

        public StageRouteRecord(string stageId, string stageName)
        {
            this.stageId = stageId;
            this.stageName = stageName;
        }

        public void Clear()
        {
            records.Clear();
        }

        public void AddSelectedNode(RoundNode node)
        {
            AddRecord(node, StageRouteActionType.Selected);
        }

        public void AddCompletedNode(RoundNode node)
        {
            AddRecord(node, StageRouteActionType.Completed);
        }

        public void AddSkippedNode(RoundNode node)
        {
            AddRecord(node, StageRouteActionType.Skipped);
        }

        public void AddFailedNode(RoundNode node)
        {
            AddRecord(node, StageRouteActionType.Failed);
        }

        public List<string> GetSelectedNodeIds()
        {
            List<string> result = new();

            foreach (StageRouteNodeRecord record in records)
            {
                if (record.actionType == StageRouteActionType.Selected)
                {
                    result.Add(record.nodeId);
                }
            }

            return result;
        }

        public List<string> GetCompletedNodeIds()
        {
            List<string> result = new();

            foreach (StageRouteNodeRecord record in records)
            {
                if (record.actionType == StageRouteActionType.Completed)
                {
                    result.Add(record.nodeId);
                }
            }

            return result;
        }

        public StageRouteNodeRecord GetLastRecord()
        {
            if (records == null || records.Count == 0)
            {
                return null;
            }

            return records[^1];
        }

        private void AddRecord(RoundNode node, StageRouteActionType actionType)
        {
            if (node == null)
            {
                Debug.LogWarning("[StageRouteRecord] Cannot add null node record.");
                return;
            }

            records.Add(new StageRouteNodeRecord(node, actionType));
        }
    }

    /// <summary>
    /// 경로상에서 특정 노드에 대해 발생한 기록 한 줄.
    /// </summary>
    [Serializable]
    public class StageRouteNodeRecord
    {
        public string nodeId;
        public int depth;
        public int indexInDepth;
        public RoundNodeType nodeType;
        public StageRouteActionType actionType;
        public float time;

        public StageRouteNodeRecord()
        {
        }

        public StageRouteNodeRecord(RoundNode node, StageRouteActionType actionType)
        {
            nodeId = node.nodeId;
            depth = node.depth;
            indexInDepth = node.indexInDepth;
            nodeType = node.nodeType;
            this.actionType = actionType;
            time = Time.time;
        }
    }

    /// <summary>
    /// 스테이지 경로 기록 액션 타입.
    /// </summary>
    public enum StageRouteActionType
    {
        None = 0,
        Selected,
        Completed,
        Skipped,
        Failed
    }
}