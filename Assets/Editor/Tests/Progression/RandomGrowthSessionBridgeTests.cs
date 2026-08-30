using System;
using System.Reflection;
using NUnit.Framework;
using Progression;
using Progression.RandomGrowth;
using Session;
using UnityEngine;

namespace ProjectBS.EditorTests.Progression
{
    public sealed class RandomGrowthSessionBridgeTests
    {
        [Test]
        public void GameSessionNewRunCreatesOneRunIdAndClearsProgressionAndRandomGrowth()
        {
            ResetGameSessionSingleton();
            GameObject gameObject = new("GameSession.E1.Test");
            try
            {
                GameSession gameSession = gameObject.AddComponent<GameSession>();
                CountingIdentityFactory factory = new();

                ProgressionRunId first = gameSession.BeginNewProgressionRun(factory);
                gameSession.StageSession.TryCommitChapter1RandomGrowthGraph(
                    first, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var firstManifest);
                gameSession.ProgressionSession.Ledger.TryEarn(new ProgressionEarnRequest(
                    ProgressionSourceRegistry.FixedRescueSegment,
                    ProgressionSourceCategory.Fixed,
                    ProgressionSourceType.BattleVictory,
                    ProgressionSourceRegistry.RescueBattleSource,
                    "result.e1.reset"), out _);

                ProgressionRunId second = gameSession.BeginNewProgressionRun(factory);

                Assert.That(factory.RunIdCalls, Is.EqualTo(2));
                Assert.That(first, Is.Not.EqualTo(second));
                Assert.That(gameSession.ProgressionSession.RunId, Is.EqualTo(second));
                Assert.That(gameSession.ProgressionSession.Ledger.Count, Is.Zero);
                Assert.That(gameSession.StageSession.RandomGrowthSession.RunId, Is.EqualTo(second));
                Assert.That(gameSession.StageSession.RandomGrowthSession.StoredManifest, Is.Null);
                Assert.That(firstManifest, Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                ResetGameSessionSingleton();
            }
        }

        [Test]
        public void StageSessionResetRuntimePreservesIdentityAndStoredManifest()
        {
            StageSession stageSession = new();
            CountingIdentityFactory factory = new();
            ProgressionRunId runId = new("run.stage-session.reentry");
            stageSession.ResetRandomGrowthForNewRun(runId);
            stageSession.TryCommitChapter1RandomGrowthGraph(
                runId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var before);

            stageSession.ResetRuntime();
            stageSession.TryCommitChapter1RandomGrowthGraph(
                runId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var after);

            Assert.That(factory.StageIdCalls, Is.EqualTo(1));
            Assert.That(after, Is.SameAs(before));
            Assert.That(after.RawRoll, Is.EqualTo(before.RawRoll));
        }

        private static void ResetGameSessionSingleton()
        {
            PropertyInfo property = typeof(GameSession).GetProperty(
                nameof(GameSession.Instance),
                BindingFlags.Public | BindingFlags.Static);
            property?.GetSetMethod(true)?.Invoke(null, new object[] { null });
        }

        private sealed class CountingIdentityFactory : IRandomGrowthSessionIdentityFactory
        {
            public int RunIdCalls { get; private set; }
            public int StageIdCalls { get; private set; }

            public ProgressionRunId CreateRunId()
            {
                RunIdCalls++;
                return new ProgressionRunId("run.bridge." + RunIdCalls);
            }

            public string CreateStageGenerationId(ProgressionRunId runId, string chapterId)
            {
                StageIdCalls++;
                return "stage-generation.bridge." + StageIdCalls;
            }
        }
    }
}
