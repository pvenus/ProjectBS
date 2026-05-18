using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 런타임에서 사용하는 스테이지 라운드 노드 데이터.
    /// ScriptableObject 원본이 아니라, 실제 생성된 스테이지 그래프의 개별 노드 인스턴스다.
    /// </summary>
    [Serializable]
    public class RoundNode
    {
        [Header("Identity")]
        public string nodeId;
        public string title;
        public string description;

        [Header("Stage Position")]
        public int depth;
        public int indexInDepth;

        [Header("Type")]
        public RoundNodeType nodeType = RoundNodeType.None;
        public RoundExecuteMode executeMode = RoundExecuteMode.None;
        public RoundNodeState state = RoundNodeState.Locked;

        [Header("Execute Payload")]

        [Header("Resolve")]
        public bool useRandomEventPool;
        public string randomPoolId;
        public bool resolved;

        public string sceneName;
        public string eventId;
        public PopupEventSO popupEvent;
        public string battleGroupId;

        [Header("Visual")]
        public Sprite icon;

        [Header("Graph")]
        public List<string> nextNodeIds = new();
        public List<string> prevNodeIds = new();

        [Header("Flags")]
        public bool isRequired;
        public bool isCleared;
        public bool isSelected;

        [Header("Hidden")]
        public bool isHidden;

        [Tooltip("RoundNodeSO의 기본 숨김 상태")]
        public bool hiddenByDefault;

        public RoundNode()
        {
        }

        public RoundNode(
            string nodeId,
            string title,
            RoundNodeType nodeType,
            RoundExecuteMode executeMode,
            int depth,
            int indexInDepth)
        {
            this.nodeId = nodeId;
            this.title = title;
            this.nodeType = nodeType;
            this.executeMode = executeMode;
            this.depth = depth;
            this.indexInDepth = indexInDepth;
            state = RoundNodeState.Locked;
        }

        public bool IsStartNode => nodeType == RoundNodeType.Start;
        public bool IsBattleNode => nodeType == RoundNodeType.Battle || nodeType == RoundNodeType.EliteBattle || nodeType == RoundNodeType.Boss;
        public bool IsBossNode => nodeType == RoundNodeType.Boss;
        public bool IsEventNode => nodeType == RoundNodeType.Event || nodeType == RoundNodeType.RequiredSubEvent;
        public bool IsAvailable => state == RoundNodeState.Available;
        public bool IsLocked => state == RoundNodeState.Locked;
        public bool IsCompleted => state == RoundNodeState.Cleared || isCleared;

        public void SetAvailable()
        {
            if (IsCompleted)
            {
                return;
            }

            if (isHidden)
            {
                return;
            }

            state = RoundNodeState.Available;
        }

        public void SetLocked()
        {
            if (IsCompleted)
            {
                return;
            }

            state = RoundNodeState.Locked;
        }

        public void SetCleared()
        {
            isCleared = true;
            isSelected = false;
            resolved = true;
            state = RoundNodeState.Cleared;
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }

        public void AddNextNode(string nextNodeId)
        {
            if (string.IsNullOrWhiteSpace(nextNodeId))
            {
                return;
            }

            if (nextNodeIds.Contains(nextNodeId))
            {
                return;
            }

            nextNodeIds.Add(nextNodeId);
        }

        public void AddPrevNode(string prevNodeId)
        {
            if (string.IsNullOrWhiteSpace(prevNodeId))
            {
                return;
            }

            if (prevNodeIds.Contains(prevNodeId))
            {
                return;
            }

            prevNodeIds.Add(prevNodeId);
        }

        public void Reveal()
        {
            isHidden = false;

            if (!IsCompleted)
            {
                state = RoundNodeState.Available;
            }

            if (useRandomEventPool
                && !resolved)
            {
                title = "?";
                description = "Unknown Event";
            }
        }

        public void Hide()
        {
            isHidden = true;
        }


        public void ResolveEvent(
            PopupEventSO popupEvent,
            string eventId)
        {
            this.popupEvent = popupEvent;
            this.eventId = eventId;
            resolved = true;

            if (popupEvent != null)
            {
                title = popupEvent.name;
            }
        }

        public bool NeedsResolve()
        {
            return useRandomEventPool
                && !resolved;
        }

        public bool CanExecute()
        {
            return IsAvailable
                && !IsCompleted
                && !isHidden;
        }
    }
}