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
        public string templateNodeId;

        public string LocalizationMainKey =>
            roundNodeSO != null
                ? roundNodeSO.LocalizationMainKey
                : (!string.IsNullOrEmpty(templateNodeId) ? templateNodeId : nodeId);

        public string Title =>
            roundNodeSO != null
                ? roundNodeSO.Title
                : string.Empty;

        [Header("Stage Position")]
        public int depth;
        public int indexInDepth;

        [Header("Route")]
        /// <summary>
        /// 논리 경로 키. 연결 규칙의 핵심 데이터.
        /// "0", "1", "1.0", "1.1" 등 점(.)으로 경로 계층을 구분한다.
        /// GlobalHubNode는 빈 값을 허용한다.
        /// </summary>
        public string routeKey;

        /// <summary>
        /// 노드 연결 종류.
        /// RouteNode     = 특정 routeKey 경로 전용 노드 (routeKey 정확히 일치 시 연결)
        /// RouteHubNode  = routeKey 계열 하위 경로를 모으는 허브
        /// GlobalHubNode = 모든 routeKey와 연결 가능한 전역 허브
        /// </summary>
        public StageNodeKind kind = StageNodeKind.RouteNode;

        [Header("Type")]
        public RoundNodeType nodeType = RoundNodeType.None;
        public RoundNodeState state = RoundNodeState.Locked;

        [Header("Execute Payload")]
        public RoundNodeSO roundNodeSO;

        [Header("Resolve")]
        public bool useRandomEventPool;
        public EventPoolSO randomPool;
        public bool resolved;

        public PopupEventSO popupEvent;

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
            templateNodeId = string.Empty;
        }

        public RoundNode(
            string nodeId,
            RoundNodeType nodeType,
            int depth,
            int indexInDepth,
            string routeKey = "",
            StageNodeKind kind = StageNodeKind.RouteNode)
        {
            this.nodeId = nodeId;
            this.nodeType = nodeType;
            this.depth = depth;
            this.indexInDepth = indexInDepth;
            this.routeKey = routeKey;
            this.kind = kind;
            state = RoundNodeState.Locked;
            templateNodeId = string.Empty;
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
        }

        public void Hide()
        {
            isHidden = true;
        }


        public void ResolveEvent(PopupEventSO popupEvent)
        {
            this.popupEvent = popupEvent;
            resolved = true;
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