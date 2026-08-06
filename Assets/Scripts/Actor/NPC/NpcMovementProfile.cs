using UnityEngine;

/// <summary>
/// Centralized NPC movement/archetype profile.
/// - Defines melee / ranged / siege / flying behavior presets
/// - Exposes stop distance and movement style decisions to other NPC modules
/// - Can optionally push the selected archetype into NpcTargeting and NpcPathing
/// </summary>
public class NpcMovementProfile : MonoBehaviour
{
    public enum MovementArchetype
    {
        Normal,
        Melee,
        Ranged,
        Siege,
        Flying
    }

    [Header("References")]
    [SerializeField] private NpcTargeting targeting;
    [SerializeField] private NpcPathing pathing;

    [Header("Archetype")]
    [SerializeField] private MovementArchetype archetype = MovementArchetype.Normal;
    [SerializeField] private bool applyProfileOnAwake = true;
    [SerializeField] private bool autoSyncTargetingArchetype = true;
    [SerializeField] private bool autoSyncPathingArchetype = true;

    [Header("Movement Stats")]
    [SerializeField] private float normalMoveSpeed = 2.2f;
    [SerializeField] private float meleeMoveSpeed = 2.7f;
    [SerializeField] private float rangedMoveSpeed = 2.1f;
    [SerializeField] private float siegeMoveSpeed = 1.55f;
    [SerializeField] private float flyingMoveSpeed = 3.1f;

    [Header("Stop Distance")]
    [SerializeField] private float normalStopDistance = 0.75f;
    [SerializeField] private float meleeStopDistance = 0.6f;
    [SerializeField] private float rangedPreferredDistance = 3.5f;
    [SerializeField] private float siegePreferredDistance = 4.5f;
    [SerializeField] private float flyingStopDistance = 0.4f;

    [Header("Targeting Bias")]
    [SerializeField] private bool siegePrioritizeTowers = true;
    [SerializeField] private bool rangedPreferSaferDistance = true;
    [SerializeField] private bool meleeCommitToNearestTarget = true;
    [SerializeField] private bool flyingIgnoreGroundPathing = true;

    [Header("Debug")]
    [SerializeField] private bool debugProfileLog = false;

    private void Awake()
    {
        CacheReferences();

        if (applyProfileOnAwake)
            ApplyProfile();
    }

    private void OnValidate()
    {
        CacheReferences();

        normalMoveSpeed = Mathf.Max(0.01f, normalMoveSpeed);
        meleeMoveSpeed = Mathf.Max(0.01f, meleeMoveSpeed);
        rangedMoveSpeed = Mathf.Max(0.01f, rangedMoveSpeed);
        siegeMoveSpeed = Mathf.Max(0.01f, siegeMoveSpeed);
        flyingMoveSpeed = Mathf.Max(0.01f, flyingMoveSpeed);

        normalStopDistance = Mathf.Max(0f, normalStopDistance);
        meleeStopDistance = Mathf.Max(0f, meleeStopDistance);
        rangedPreferredDistance = Mathf.Max(0f, rangedPreferredDistance);
        siegePreferredDistance = Mathf.Max(0f, siegePreferredDistance);
        flyingStopDistance = Mathf.Max(0f, flyingStopDistance);
    }

    private void CacheReferences()
    {
        if (targeting == null)
            targeting = GetComponent<NpcTargeting>();

        if (pathing == null)
            pathing = GetComponent<NpcPathing>();
    }

    public void ApplyProfile()
    {
        CacheReferences();

        if (autoSyncTargetingArchetype && targeting != null)
            targeting.SetArchetype(ConvertToTargetingArchetype(archetype));

        if (autoSyncPathingArchetype && pathing != null)
            pathing.SetArchetype(ConvertToPathingArchetype(archetype));

        if (pathing != null)
        {
            pathing.SetMoveSpeed(GetMoveSpeed());
            pathing.SetStopDistances(
                meleeStopDistance,
                rangedPreferredDistance,
                siegePreferredDistance,
                flyingStopDistance
            );
        }

        if (debugProfileLog)
        {
            Debug.Log(
                $"[NpcMovementProfile] npc={name}, archetype={archetype}, moveSpeed={GetMoveSpeed()}, stopDistance={GetStopDistance()}, siegePrioritizeTowers={siegePrioritizeTowers}, flyingIgnoreGroundPathing={flyingIgnoreGroundPathing}"
            );
        }
    }

    public MovementArchetype GetArchetype()
    {
        return archetype;
    }

    public void SetArchetype(MovementArchetype newArchetype, bool applyNow = true)
    {
        archetype = newArchetype;

        if (applyNow)
            ApplyProfile();
    }

    public float GetMoveSpeed()
    {
        switch (archetype)
        {
            case MovementArchetype.Melee:
                return meleeMoveSpeed;
            case MovementArchetype.Ranged:
                return rangedMoveSpeed;
            case MovementArchetype.Siege:
                return siegeMoveSpeed;
            case MovementArchetype.Flying:
                return flyingMoveSpeed;
            default:
                return normalMoveSpeed;
        }
    }

    public float GetStopDistance()
    {
        switch (archetype)
        {
            case MovementArchetype.Melee:
                return meleeStopDistance;
            case MovementArchetype.Ranged:
                return rangedPreferredDistance;
            case MovementArchetype.Siege:
                return siegePreferredDistance;
            case MovementArchetype.Flying:
                return flyingStopDistance;
            default:
                return normalStopDistance;
        }
    }

    public bool IsFlying()
    {
        return archetype == MovementArchetype.Flying;
    }

    public bool IsSiege()
    {
        return archetype == MovementArchetype.Siege;
    }

    public bool IsRanged()
    {
        return archetype == MovementArchetype.Ranged;
    }

    public bool IsMelee()
    {
        return archetype == MovementArchetype.Melee;
    }

    public bool ShouldPrioritizeTowers()
    {
        return archetype == MovementArchetype.Siege && siegePrioritizeTowers;
    }

    public bool ShouldPreferSaferDistance()
    {
        return archetype == MovementArchetype.Ranged && rangedPreferSaferDistance;
    }

    public bool ShouldCommitToNearestTarget()
    {
        return archetype == MovementArchetype.Melee && meleeCommitToNearestTarget;
    }

    public bool ShouldIgnoreGroundPathing()
    {
        return archetype == MovementArchetype.Flying && flyingIgnoreGroundPathing;
    }

    private NpcTargeting.TargetingArchetype ConvertToTargetingArchetype(MovementArchetype value)
    {
        switch (value)
        {
            case MovementArchetype.Melee:
                return NpcTargeting.TargetingArchetype.Melee;
            case MovementArchetype.Ranged:
                return NpcTargeting.TargetingArchetype.Ranged;
            case MovementArchetype.Siege:
                return NpcTargeting.TargetingArchetype.Siege;
            case MovementArchetype.Flying:
                return NpcTargeting.TargetingArchetype.Flying;
            default:
                return NpcTargeting.TargetingArchetype.Normal;
        }
    }

    private NpcPathing.PathingArchetype ConvertToPathingArchetype(MovementArchetype value)
    {
        switch (value)
        {
            case MovementArchetype.Melee:
                return NpcPathing.PathingArchetype.Melee;
            case MovementArchetype.Ranged:
                return NpcPathing.PathingArchetype.Ranged;
            case MovementArchetype.Siege:
                return NpcPathing.PathingArchetype.Siege;
            case MovementArchetype.Flying:
                return NpcPathing.PathingArchetype.Flying;
            default:
                return NpcPathing.PathingArchetype.Normal;
        }
    }
}
