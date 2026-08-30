using System.Collections.Generic;
using NUnit.Framework;
using Progression.Portfolio;
using Stage;

public sealed class Chapter1Event45RouteSelectionContractTests
{
    [Test]
    public void BattlePurposeFilterWinsBeforeShortestAndOrdinalTieBreak()
    {
        var graph = new StageGraph("stage", "stage");
        var source = Node("source", "Event");
        var safe = Node("safe", "Event");
        var battleFar = Node("battle.z", "Battle");
        var battleNear = Node("battle.a", "Battle");
        graph.AddNode(source);
        graph.AddNode(safe);
        graph.AddNode(battleFar);
        graph.AddNode(battleNear);
        source.nextNodeIds.AddRange(new[] { safe.nodeId, battleFar.nodeId, battleNear.nodeId });
        var candidates = new StageRouteCandidateSnapshot
        {
            snapshotId = "event45.snapshot",
            sourceNodeId = source.nodeId,
            graphRevision = 0,
            candidates = new List<StageRouteCandidate>
            {
                Candidate(safe, 0), Candidate(battleFar, 3), Candidate(battleNear, 1)
            }
        };

        Assert.That(graph.TryCommitImmediateSuccessorRoute(candidates,
            (ImmediateSuccessorRouteSelectionMode)Chapter1Event45SelectionContract.SelectionModeValue,
            out _, out string error), Is.True,
            error);
        Assert.That(graph.committedRouteTargetNodeId, Is.EqualTo("battle.a"));
    }

    [Test]
    public void SelectorFailsClosedWithoutMixedBattleAndNonBattleCandidates()
    {
        var graph = new StageGraph("stage", "stage");
        var source = Node("source", "Event");
        var battleA = Node("battle.a", "Battle");
        var battleB = Node("battle.b", "Battle");
        graph.AddNode(source);
        graph.AddNode(battleA);
        graph.AddNode(battleB);
        source.nextNodeIds.AddRange(new[] { battleA.nodeId, battleB.nodeId });
        var candidates = new StageRouteCandidateSnapshot
        {
            snapshotId = "event45.invalid",
            sourceNodeId = source.nodeId,
            graphRevision = 0,
            candidates = new List<StageRouteCandidate>
                { Candidate(battleA, 1), Candidate(battleB, 2) }
        };

        Assert.That(graph.TryCommitImmediateSuccessorRoute(candidates,
            (ImmediateSuccessorRouteSelectionMode)Chapter1Event45SelectionContract.SelectionModeValue,
            out _, out string error), Is.False);
        Assert.That(error, Is.EqualTo("STAGE_ROUTE_BATTLE_PURPOSE_CANDIDATES_INVALID"));
    }

    private static RoundNode Node(string id, string purpose) => new(id,
        RoundNodeType.Event, 0, 0) { templateNodeId = purpose };

    private static StageRouteCandidate Candidate(RoundNode node, int remaining) => new()
    {
        nodeId = node.nodeId,
        purposeId = node.LocalizationMainKey,
        remainingNodeCount = remaining
    };
}
