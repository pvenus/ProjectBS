namespace Effect
{
    public static class EffectResolveHelper
    {
        private static readonly EffectResolver resolver = new();

        public static EffectEntryRuntime CreateRuntimeEntry(
            EffectEntrySO effectEntrySo,
            Character.CharacterManager targetCharacter,
            Character.CharacterManager sourceCharacter = null,
            UnityEngine.Transform sourceTransform = null,
            UnityEngine.Vector2 projectileDirection = default)
        {
            return resolver.Resolve(
                effectEntrySo,
                targetCharacter,
                sourceCharacter,
                sourceTransform,
                projectileDirection);
        }

        public static EffectRuntimeData CreateRuntimeData(
            EffectEntrySO effectEntrySo,
            Character.CharacterManager targetCharacter,
            Character.CharacterManager sourceCharacter = null,
            UnityEngine.Transform sourceTransform = null,
            UnityEngine.Vector2 projectileDirection = default)
        {
            EffectEntryRuntime runtimeEntry = CreateRuntimeEntry(
                effectEntrySo,
                targetCharacter,
                sourceCharacter,
                sourceTransform,
                projectileDirection);

            return runtimeEntry?.RuntimeData;
        }
    }
}