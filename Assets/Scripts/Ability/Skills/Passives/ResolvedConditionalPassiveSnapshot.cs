namespace Character.Skill
{
    public sealed class ResolvedConditionalPassiveSnapshot
    {
        public string SourceEquipmentId { get; }
        public int Level { get; }
        public string Revision { get; }
        public float AttackPercent { get; }
        public float DamageReductionPercent { get; }

        public ResolvedConditionalPassiveSnapshot(
            string sourceEquipmentId,
            int level,
            string revision,
            float attackPercent,
            float damageReductionPercent)
        {
            SourceEquipmentId = sourceEquipmentId;
            Level = level;
            Revision = revision;
            AttackPercent = attackPercent;
            DamageReductionPercent = damageReductionPercent;
        }
    }
}
