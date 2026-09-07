using System;
using System.Collections.Generic;

namespace Battle.Morpg
{
    internal sealed class MorpgPendingReward
    {
        internal string Id, ReservationId;
        internal long DeathSequence;
        internal int Gold, XpQuarterUnits;
        internal bool Ready, Credited;
    }

    internal sealed class MorpgRewardVisualGroup
    {
        internal readonly List<MorpgPendingReward> Rewards = new();
        internal string HostId;
        internal long CreationSequence;
        internal float X, Y, Age;
        internal bool Flying => Age >= MorpgRewardDeliveryQueue.GroundSeconds;
        internal float FlightProgress => Math.Min(1f, Math.Max(0f,
            (Age - MorpgRewardDeliveryQueue.GroundSeconds) / MorpgRewardDeliveryQueue.FlySeconds));
        internal int Gold { get { int sum = 0; foreach (var reward in Rewards) sum += reward.Gold; return sum; } }
        internal int Quarters { get { int sum = 0; foreach (var reward in Rewards) sum += reward.XpQuarterUnits; return sum; } }
    }

    internal interface IMorpgRewardPresentation
    {
        bool IsAvailable { get; }
        bool TryShow(MorpgRewardVisualGroup group);
        void Refresh(MorpgRewardVisualGroup group);
        void Release(MorpgRewardVisualGroup group);
    }

    internal delegate bool MorpgRewardCommit(MorpgPendingReward reward, out string error);

    // Economy remains in BattleRewardLedger. Visuals can only mark receipts ready, never apply accounts.
    internal sealed class MorpgRewardDeliveryQueue
    {
        internal const float GroundSeconds = .45f, FlySeconds = .35f, GroupRadius = .60f;
        internal const int VisualCap = 12;
        private readonly MorpgRewardCommit commit;
        private readonly Dictionary<string, MorpgPendingReward> receipts = new(StringComparer.Ordinal);
        private readonly List<MorpgPendingReward> ordered = new();
        private readonly List<MorpgRewardVisualGroup> groups = new();
        private int cursor;
        private bool draining, closed;
        internal IMorpgRewardPresentation Presentation { get; set; }
        internal int PendingCount => ordered.Count - cursor;
        internal int ActiveVisualCount => groups.Count;
        internal bool Failed { get; private set; }
        internal string Error { get; private set; }
        internal MorpgRewardDeliveryQueue(MorpgRewardCommit commit) { this.commit = commit; }

        internal bool TryReserve(string id, long sequence, string reservation, int gold, int quarters, float x, float y)
        {
            if (string.IsNullOrEmpty(id)) return Fail("missing reward identity");
            if (receipts.ContainsKey(id)) return !Failed;
            if (closed || Failed || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(reservation) ||
                gold < 0 || quarters < 0 || (ordered.Count > 0 && sequence <= ordered[ordered.Count - 1].DeathSequence))
                return Fail("invalid/out-of-order reward reservation");
            var reward = new MorpgPendingReward { Id = id, DeathSequence = sequence, ReservationId = reservation,
                Gold = gold, XpQuarterUnits = quarters };
            receipts.Add(id, reward); ordered.Add(reward);
            if (!Available()) return Flush();
            var group = NearestGround(x, y);
            if (group == null && groups.Count >= VisualCap)
            {
                // A flying host cannot accept more ground receipts. Saturation uses lossless failover.
                foreach (var candidate in groups)
                    if (!candidate.Flying && (group == null || candidate.CreationSequence < group.CreationSequence ||
                        (candidate.CreationSequence == group.CreationSequence && string.CompareOrdinal(candidate.HostId, group.HostId) < 0)))
                        group = candidate;
                if (group == null) return Flush();
            }
            bool created = group == null;
            if (created)
            {
                group = new MorpgRewardVisualGroup { HostId = id, CreationSequence = sequence, X = x, Y = y };
                groups.Add(group);
            }
            group.Rewards.Add(reward);
            try
            {
                if (created && !Presentation.TryShow(group)) return Flush();
                Presentation.Refresh(group);
            }
            catch { return Flush(); }
            return true;
        }

        private MorpgRewardVisualGroup NearestGround(float x, float y)
        {
            MorpgRewardVisualGroup nearest = null;
            float best = GroupRadius * GroupRadius;
            foreach (var group in groups)
            {
                if (group.Flying) continue;
                float dx = group.X - x, dy = group.Y - y, distance = dx * dx + dy * dy;
                if (distance > best) continue;
                if (nearest == null || distance < best || string.CompareOrdinal(group.HostId, nearest.HostId) < 0)
                { nearest = group; best = distance; }
            }
            return nearest;
        }

        internal bool Tick(float deltaTime, bool paused)
        {
            if (Failed) return false;
            if (closed || paused || deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return true;
            if (!Available()) return Flush();
            foreach (var group in groups.ToArray())
            {
                group.Age += deltaTime;
                try { Presentation.Refresh(group); }
                catch { return Flush(); }
                if (group.Age + .000001f < GroundSeconds + FlySeconds) continue;
                foreach (var reward in group.Rewards) reward.Ready = true;
                Release(group);
            }
            return Drain();
        }

        // Explicit visual failure/teardown may call this synchronously, even when normal clocks are paused.
        internal bool Flush()
        {
            foreach (var group in groups.ToArray()) Release(group);
            foreach (var reward in ordered) if (!reward.Credited) reward.Ready = true;
            return Drain();
        }
        internal bool Close()
        {
            closed = true;
            return Flush();
        }
        private bool Available()
        {
            try { return Presentation != null && Presentation.IsAvailable; }
            catch { return false; }
        }
        private void Release(MorpgRewardVisualGroup group)
        {
            groups.Remove(group);
            try { Presentation?.Release(group); } catch { /* Optional visual failure cannot lose a receipt. */ }
        }
        private bool Drain()
        {
            if (Failed) return false;
            if (draining) return true;
            draining = true;
            try
            {
                while (cursor < ordered.Count && ordered[cursor].Ready)
                {
                    var reward = ordered[cursor];
                    if (!commit(reward, out string error)) return Fail(error ?? "reward commit failed");
                    reward.Credited = true; cursor++;
                }
                return true;
            }
            catch (Exception e) { return Fail("reward commit: " + e.Message); }
            finally { draining = false; }
        }
        private bool Fail(string error) { Failed = true; Error = error; return false; }
    }
}
