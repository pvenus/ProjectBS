using System;
using System.Collections.Generic;
using System.Linq;
using Battle.Morpg;

internal static class RewardDeliveryTests
{
    private sealed class Account : IRevisionedRewardAccount
    {
        public decimal Value { get; private set; }
        public long Revision { get; private set; }
        internal bool Reject;
        public bool TryApply(decimal delta, long revision) { if (Reject || revision != Revision) return false; Value += delta; Revision++; return true; }
        public bool TryRestore(decimal value, long revision) { if (revision != Revision) return false; Value = value; Revision++; return true; }
    }
    private sealed class View : IMorpgRewardPresentation
    {
        internal readonly List<MorpgRewardVisualGroup> Live = new();
        internal bool Available = true, RejectShow, ThrowRefresh, ThrowRelease;
        internal int MaxLive;
        public bool IsAvailable => Available;
        public bool TryShow(MorpgRewardVisualGroup group) { if (RejectShow) return false; Live.Add(group); MaxLive = Math.Max(MaxLive, Live.Count); return true; }
        public void Refresh(MorpgRewardVisualGroup group) { if (ThrowRefresh) throw new Exception("inactive/missing glyph"); }
        public void Release(MorpgRewardVisualGroup group) { Live.Remove(group); if (ThrowRelease) throw new Exception("destroyed glyph"); }
    }
    private sealed class Fixture
    {
        internal readonly Account Gold = new(), Xp = new();
        internal readonly View View = new();
        internal readonly List<string> Commits = new();
        internal readonly MorpgRewardDeliveryQueue Queue;
        internal readonly BattleRewardLedger Ledger;
        internal Fixture()
        {
            Ledger = new BattleRewardLedger(Gold, Xp);
            Queue = new MorpgRewardDeliveryQueue(Commit) { Presentation = View };
        }
        private bool Commit(MorpgPendingReward r, out string error)
        {
            bool result = Ledger.TryCredit(r.Id, r.DeathSequence, r.ReservationId, r.Gold, r.XpQuarterUnits, out error);
            if (result) Commits.Add(r.Id);
            return result;
        }
        internal bool Add(long sequence, float x = 0, float y = 0, string id = null, int amount = 1) =>
            Queue.TryReserve(id ?? "r" + sequence, sequence, "reservation-" + sequence, amount, amount, x, y);
    }
    private static int passed;
    private static void Check(bool yes, string why) { if (!yes) throw new Exception(why); }
    private static void Test(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }
    internal static void Main()
    {
        Test("arrival-commit-after-450ms-ground-350ms-flight", () => {
            var f = new Fixture(); Check(f.Add(1, 7, -3), "reserve");
            Check(f.Gold.Value == 0 && f.Ledger.Bundles.Count == 0, "old immediate account path");
            f.Queue.Tick(.449f, false); Check(!f.View.Live[0].Flying, "early flight");
            f.Queue.Tick(.001f, false); Check(f.View.Live[0].Flying && f.Gold.Value == 0, "dwell boundary");
            f.Queue.Tick(.349f, false); Check(f.Gold.Value == 0, "early arrival credit");
            f.Queue.Tick(.001f, false); Check(f.Gold.Value == 1 && f.Xp.Value == .25m && f.View.Live.Count == 0, "arrival not atomic");
        });
        Test("duplicate-before-and-after-arrival-mutation-zero", () => {
            var f = new Fixture(); f.Add(1); f.Add(1); Check(f.Queue.PendingCount == 1, "pending duplicate");
            f.Queue.Tick(.8f, false); f.Add(1); f.Queue.Close(); f.Queue.Close();
            Check(f.Commits.Count == 1 && f.Gold.Value == 1, "duplicate committed");
        });
        Test("nearest-ground-radius-and-lexical-tie-first-position", () => {
            var f = new Fixture(); f.Add(1, -.5f, 0, "z"); f.Add(2, .5f, 0, "a"); f.Add(3, 0, 0, "m");
            Check(f.View.Live.Count == 2, "group count");
            var host = f.View.Live.Single(g => g.HostId == "a");
            Check(host.Rewards.Count == 2 && host.X == .5f, "nearest tie or host position");
        });
        Test("cap12-oldest-ground-grouping-and-28-death-conservation", () => {
            var f = new Fixture();
            for (int i = 1; i <= 28; i++) f.Add(i, i * 2, 0, null, i <= 22 ? 1 : 3);
            Check(f.View.Live.Count == 12 && f.View.MaxLive == 12 && f.View.Live[0].Rewards.Count == 17, "cap/grouping");
            f.Queue.Tick(.8f, false);
            Check(f.Gold.Value == 40 && f.Xp.Value == 10 && f.Commits.Count == 28 && f.Queue.PendingCount == 0, "conservation");
        });
        Test("group-arrival-reordering-keeps-death-order", () => {
            var f = new Fixture(); f.Add(1); f.Queue.Tick(.3f, false); f.Add(2, 3); f.Add(3, .1f);
            f.Queue.Tick(.5f, false); Check(f.Commits.SequenceEqual(new[] { "r1" }), "later grouped credit jumped queue");
            f.Queue.Tick(.3f, false); Check(f.Commits.SequenceEqual(new[] { "r1", "r2", "r3" }), "death order changed");
        });
        Test("pause-freezes-dwell-flight-and-commit", () => {
            var f = new Fixture(); f.Add(1); f.Queue.Tick(100, true);
            Check(f.View.Live[0].Age == 0 && f.Gold.Value == 0, "paused progress");
            f.Queue.Tick(.5f, false); f.Queue.Tick(100, true); Check(f.Gold.Value == 0, "paused flight committed");
            f.Queue.Tick(.3f, false); Check(f.Gold.Value == 1, "resume lost reward");
        });
        Test("missing-assets-or-HUD-immediate-failover", () => {
            var f = new Fixture(); f.View.Available = false; f.Add(1);
            Check(f.Gold.Value == 1 && f.View.Live.Count == 0, "missing presentation lost reward");
            f.Queue.Presentation = null; f.Add(2); Check(f.Gold.Value == 2, "null HUD lost reward");
        });
        Test("inactive-glyph-or-show-failure-immediate-failover", () => {
            var f = new Fixture(); f.View.RejectShow = true; f.Add(1); Check(f.Gold.Value == 1, "show fail lost");
            f.View.RejectShow = false; f.Add(2); f.View.ThrowRefresh = true;
            f.Queue.Tick(.1f, false); Check(f.Gold.Value == 2 && f.Queue.ActiveVisualCount == 0, "inactive glyph lost");
        });
        Test("midflight-HUD-loss-drains-in-order", () => {
            var f = new Fixture(); f.Add(1); f.Add(2, 3); f.Queue.Tick(.5f, false); f.View.Available = false;
            f.Queue.Tick(.01f, false); Check(f.Commits.SequenceEqual(new[] { "r1", "r2" }) && f.View.Live.Count == 0, "HUD loss");
        });
        Test("teardown-defeat-close-pays-confirmed-only-and-is-idempotent", () => {
            var f = new Fixture(); f.Add(1); f.Add(2, 3, 0, null, 3); f.Queue.Tick(.1f, true);
            f.View.ThrowRelease = true; Check(f.Queue.Close(), "close failed"); f.Queue.Close();
            Check(f.Gold.Value == 4 && f.Xp.Value == 1 && f.Commits.Count == 2 && f.Queue.ActiveVisualCount == 0, "teardown duplicate/loss");
        });
        Test("all-flying-cap-saturation-fails-over-without-13th-visual", () => {
            var f = new Fixture(); for (int i = 1; i <= 12; i++) f.Add(i, i * 2);
            f.Queue.Tick(.5f, false); f.Add(13, 50);
            Check(f.View.MaxLive == 12 && f.Gold.Value == 13 && f.Queue.ActiveVisualCount == 0, "flying cap overflow");
        });
        Test("arrival-transaction-failure-restores-gold-and-is-terminal", () => {
            var f = new Fixture(); f.Add(1); f.Xp.Reject = true;
            Check(!f.Queue.Tick(.8f, false) && f.Queue.Failed, "failed transaction accepted");
            Check(f.Gold.Value == 0 && f.Xp.Value == 0, "partial account commit");
            f.Queue.Flush(); Check(f.Commits.Count == 0, "failed terminal retried");
        });
        Console.WriteLine("PASS " + passed + "/" + passed + " production delivery queue + ledger tests");
    }
}
