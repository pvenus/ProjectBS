using System.Linq;
using NUnit.Framework;
using Stage;

public sealed class StageRouteCommitSnapshotTests
{
    [Test]
    public void Snapshot_OrdersCandidatesByStableNodeId()
    {
        StageGraph graph = CreateGraph();
        Assert.That(graph.TryCreateImmediateSuccessorRouteSnapshot(graph.GetNode("source"),
            "snapshot.one", out StageRouteCandidateSnapshot snapshot, out string error), Is.True, error);
        Assert.That(snapshot.candidates.Select(item => item.nodeId),
            Is.EqualTo(new[] { "long", "short" }));
    }

    [Test]
    public void ShortestCommit_SelectsShortestCandidateOnly()
    {
        StageGraph graph = CreateGraph();
        graph.TryCreateImmediateSuccessorRouteSnapshot(graph.GetNode("source"), "snapshot.one",
            out StageRouteCandidateSnapshot snapshot, out _);
        Assert.That(graph.TryCommitImmediateSuccessorRoute(snapshot,
            ImmediateSuccessorRouteSelectionMode.ShortestRemainingToSectionExit,
            out _, out string error), Is.True, error);
        Assert.That(graph.GetNextNodes(graph.GetNode("source")).Single().nodeId, Is.EqualTo("short"));
    }

    [Test]
    public void LongestCommit_SelectsLongestCandidateOnly()
    {
        StageGraph graph = CreateGraph();
        graph.TryCreateImmediateSuccessorRouteSnapshot(graph.GetNode("source"), "snapshot.one",
            out StageRouteCandidateSnapshot snapshot, out _);
        Assert.That(graph.TryCommitImmediateSuccessorRoute(snapshot,
            ImmediateSuccessorRouteSelectionMode.LongestRemainingToSectionExit,
            out _, out string error), Is.True, error);
        Assert.That(graph.GetNextNodes(graph.GetNode("source")).Single().nodeId, Is.EqualTo("long"));
    }

    [Test]
    public void Commit_DoesNotMutateAuthoredEdges()
    {
        StageGraph graph = CreateGraph();
        string[] before = graph.GetNode("source").nextNodeIds.ToArray();
        graph.TryCreateImmediateSuccessorRouteSnapshot(graph.GetNode("source"), "snapshot.one",
            out StageRouteCandidateSnapshot snapshot, out _);
        graph.TryCommitImmediateSuccessorRoute(snapshot,
            ImmediateSuccessorRouteSelectionMode.ShortestRemainingToSectionExit, out _, out _);
        Assert.That(graph.GetNode("source").nextNodeIds, Is.EqualTo(before));
    }

    [Test]
    public void Rollback_RestoresUncommittedRoute()
    {
        StageGraph graph = CreateGraph();
        graph.TryCreateImmediateSuccessorRouteSnapshot(graph.GetNode("source"), "snapshot.one",
            out StageRouteCandidateSnapshot snapshot, out _);
        graph.TryCommitImmediateSuccessorRoute(snapshot,
            ImmediateSuccessorRouteSelectionMode.ShortestRemainingToSectionExit,
            out StageRouteCommitSnapshot rollback, out _);
        Assert.That(graph.TryRollbackImmediateSuccessorRoute(rollback), Is.True);
        Assert.That(graph.GetNextNodes(graph.GetNode("source")).Count, Is.EqualTo(2));
    }

    [Test]
    public void StaleSnapshot_IsRejectedWithoutMutation()
    {
        StageGraph graph = CreateGraph();
        graph.TryCreateImmediateSuccessorRouteSnapshot(graph.GetNode("source"), "snapshot.one",
            out StageRouteCandidateSnapshot snapshot, out _);
        graph.routeRevision++;
        Assert.That(graph.TryCommitImmediateSuccessorRoute(snapshot,
            ImmediateSuccessorRouteSelectionMode.ShortestRemainingToSectionExit,
            out _, out string error), Is.False);
        Assert.That(error, Is.EqualTo("STAGE_ROUTE_SNAPSHOT_STALE"));
        Assert.That(graph.committedRouteTargetNodeId, Is.Null.Or.Empty);
    }

    private static StageGraph CreateGraph()
    {
        var graph = new StageGraph("stage.test", "test");
        graph.AddNode(new RoundNode("source", RoundNodeType.Event, 0, 0));
        graph.AddNode(new RoundNode("short", RoundNodeType.Event, 1, 0));
        graph.AddNode(new RoundNode("long", RoundNodeType.Event, 1, 1));
        graph.AddNode(new RoundNode("middle", RoundNodeType.Event, 2, 0));
        graph.AddNode(new RoundNode("exit", RoundNodeType.Boss, 3, 0));
        graph.ConnectNodes("source", "short");
        graph.ConnectNodes("source", "long");
        graph.ConnectNodes("short", "exit");
        graph.ConnectNodes("long", "middle");
        graph.ConnectNodes("middle", "exit");
        graph.currentNodeId = "source";
        return graph;
    }
}
