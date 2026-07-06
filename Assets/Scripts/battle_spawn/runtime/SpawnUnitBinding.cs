using System;
using System.Collections.Generic;
using UnityEngine;
using Character;

public enum SpawnUnitRole
{
    Any,
    Melee,
    Ranged,
    Tank,
    Support,
    Elite,
    Boss
}

public readonly struct SpawnUnitRequest
{
    public string UnitKey { get; }
    public SpawnUnitRole Role { get; }

    public SpawnUnitRequest(string unitKey, SpawnUnitRole role)
    {
        UnitKey = unitKey;
        Role = role;
    }
}

public interface ISpawnUnitResolver
{
    CharacterSO Resolve(SpawnUnitRequest request);
}

[Serializable]
public sealed class SpawnUnitBinding
{
    [SerializeField] private string unitKey;
    [SerializeField] private SpawnUnitRole role = SpawnUnitRole.Any;
    [SerializeField] private CharacterSO character;

    public string UnitKey => unitKey;
    public SpawnUnitRole Role => role;
    public CharacterSO Character => character;
}

public sealed class SpawnUnitBindingResolver : ISpawnUnitResolver
{
    private readonly Dictionary<string, CharacterSO> byKey = new Dictionary<string, CharacterSO>();
    private readonly Dictionary<SpawnUnitRole, CharacterSO> byRole = new Dictionary<SpawnUnitRole, CharacterSO>();

    public SpawnUnitBindingResolver(IEnumerable<SpawnUnitBinding> bindings)
    {
        if (bindings == null) return;

        foreach (var binding in bindings)
        {
            if (binding == null || binding.Character == null) continue;

            if (!string.IsNullOrEmpty(binding.UnitKey) && !byKey.ContainsKey(binding.UnitKey))
            {
                byKey.Add(binding.UnitKey, binding.Character);
            }

            if (binding.Role != SpawnUnitRole.Any && !byRole.ContainsKey(binding.Role))
            {
                byRole.Add(binding.Role, binding.Character);
            }
        }
    }

    public CharacterSO Resolve(SpawnUnitRequest request)
    {
        if (!string.IsNullOrEmpty(request.UnitKey) && byKey.TryGetValue(request.UnitKey, out CharacterSO keyed))
        {
            return keyed;
        }

        if (request.Role != SpawnUnitRole.Any && byRole.TryGetValue(request.Role, out CharacterSO roleBased))
        {
            return roleBased;
        }

        return null;
    }
}
