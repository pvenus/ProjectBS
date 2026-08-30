using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Character;
using Party;
using Skill;

namespace Stage
{
    public enum SafeGrowthEligibilityStatus
    {
        Eligible = 0,
        NoCandidate = 10,
        InvalidRoster = 20,
        InvalidData = 30,
        Stale = 40
    }

    public sealed class SafeGrowthEligibleTarget
    {
        public SafeGrowthEligibleTarget(string ownerCharacterId, string equipmentId,
            string canonicalSkillId, int currentLevel, int maxLevel)
        {
            OwnerCharacterId = ownerCharacterId ?? string.Empty;
            EquipmentId = equipmentId ?? string.Empty;
            CanonicalSkillId = canonicalSkillId ?? string.Empty;
            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;
        }
        public string OwnerCharacterId { get; }
        public string EquipmentId { get; }
        public string CanonicalSkillId { get; }
        public int CurrentLevel { get; }
        public int MaxLevel { get; }
    }

    public sealed class SafeGrowthEligibilitySnapshot
    {
        internal SafeGrowthEligibilitySnapshot(SafeGrowthEligibilityStatus status,
            IReadOnlyList<SafeGrowthEligibleTarget> targets, string fingerprint)
        {
            Status = status;
            Targets = Array.AsReadOnly((targets ?? Array.Empty<SafeGrowthEligibleTarget>()).ToArray());
            Fingerprint = fingerprint ?? string.Empty;
            Revision = Fingerprint.Length >= 16 ? Fingerprint.Substring(0, 16) : Fingerprint;
        }
        public SafeGrowthEligibilityStatus Status { get; }
        public IReadOnlyList<SafeGrowthEligibleTarget> Targets { get; }
        public int EligibleCount => Targets.Count;
        public int TargetCount => Math.Min(2, Targets.Count);
        public string Fingerprint { get; }
        public string Revision { get; }
    }

    public sealed class PartyWideSafeGrowthEligibilityQuery
    {
        public SafeGrowthEligibilitySnapshot Query(PartyRuntimeData party,
            IEnumerable<EquipmentSkillSO> canonicalCatalog)
        {
            if (party?.Members == null || party.Members.Count == 0)
                return Snapshot(SafeGrowthEligibilityStatus.NoCandidate, Array.Empty<SafeGrowthEligibleTarget>());

            Dictionary<string, EquipmentSkillSO> catalog = new(StringComparer.Ordinal);
            if (canonicalCatalog == null) return Snapshot(SafeGrowthEligibilityStatus.InvalidData, null);
            foreach (EquipmentSkillSO skill in canonicalCatalog)
            {
                if (skill == null || string.IsNullOrWhiteSpace(skill.EquipmentId)
                    || !catalog.TryAdd(skill.EquipmentId, skill))
                    return Snapshot(SafeGrowthEligibilityStatus.InvalidData, null);
            }

            HashSet<string> owners = new(StringComparer.Ordinal);
            HashSet<string> targets = new(StringComparer.Ordinal);
            List<SafeGrowthEligibleTarget> eligible = new();
            foreach (CharacterRuntimeData member in party.Members)
            {
                string owner = member?.characterSO?.CharacterId;
                if (string.IsNullOrWhiteSpace(owner) || !owners.Add(owner)
                    || member.skillInstances == null)
                    return Snapshot(SafeGrowthEligibilityStatus.InvalidRoster, null);

                foreach (EquipmentSkillInstanceData instance in member.skillInstances)
                {
                    if (instance == null || string.IsNullOrWhiteSpace(instance.equipmentId)
                    || !targets.Add(instance.equipmentId))
                        return Snapshot(SafeGrowthEligibilityStatus.InvalidRoster, null);
                    if (!catalog.TryGetValue(instance.equipmentId, out EquipmentSkillSO canonical)
                        || !string.Equals(canonical.EquipmentId, instance.equipmentId, StringComparison.Ordinal)
                        || canonical.UpgradeTableSo?.Entries == null
                        || canonical.UpgradeTableSo.Entries.Count == 0)
                        return Snapshot(SafeGrowthEligibilityStatus.InvalidData, null);

                    int max = canonical.UpgradeTableSo.Entries.Max(x => x?.Level ?? 0);
                    int current = instance.currentLevel;
                    bool nextExists = canonical.UpgradeTableSo.Entries.Any(x => x != null && x.Level == current + 1);
                    if (current < 1 || max < 1)
                        return Snapshot(SafeGrowthEligibilityStatus.InvalidData, null);
                    if (!member.isDead && current < max)
                    {
                        if (!nextExists) return Snapshot(SafeGrowthEligibilityStatus.InvalidData, null);
                        eligible.Add(new SafeGrowthEligibleTarget(owner, instance.equipmentId,
                            canonical.EquipmentId, current, max));
                    }
                }
            }

            eligible.Sort((a, b) =>
            {
                int owner = StringComparer.Ordinal.Compare(a.OwnerCharacterId, b.OwnerCharacterId);
                return owner != 0 ? owner : StringComparer.Ordinal.Compare(a.EquipmentId, b.EquipmentId);
            });
            return Snapshot(eligible.Count == 0 ? SafeGrowthEligibilityStatus.NoCandidate
                : SafeGrowthEligibilityStatus.Eligible, eligible);
        }

        public bool IsCurrent(SafeGrowthEligibilitySnapshot expected, PartyRuntimeData party,
            IEnumerable<EquipmentSkillSO> catalog)
        {
            if (expected == null) return false;
            SafeGrowthEligibilitySnapshot current = Query(party, catalog);
            return current.Status == expected.Status
                && string.Equals(current.Fingerprint, expected.Fingerprint, StringComparison.Ordinal);
        }

        private static SafeGrowthEligibilitySnapshot Snapshot(
            SafeGrowthEligibilityStatus status, IReadOnlyList<SafeGrowthEligibleTarget> targets)
        {
            SafeGrowthEligibleTarget[] values = (targets ?? Array.Empty<SafeGrowthEligibleTarget>()).ToArray();
            List<string> fields = new() { "chapter1.safe-growth.eligibility.v1", ((int)status).ToString(CultureInfo.InvariantCulture) };
            foreach (SafeGrowthEligibleTarget value in values)
            {
                fields.Add(value.OwnerCharacterId); fields.Add(value.EquipmentId);
                fields.Add(value.CanonicalSkillId);
                fields.Add(value.CurrentLevel.ToString(CultureInfo.InvariantCulture));
                fields.Add(value.MaxLevel.ToString(CultureInfo.InvariantCulture));
            }
            return new SafeGrowthEligibilitySnapshot(status, values, Hash(fields));
        }

        private static string Hash(IEnumerable<string> fields)
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream, new UTF8Encoding(false), true))
            {
                foreach (string field in fields)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(field ?? string.Empty);
                    writer.Write(bytes.Length); writer.Write(bytes);
                }
            }
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(stream.ToArray()).Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
