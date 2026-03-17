
using UnityEngine;

/// <summary>
/// EvolutionMono
///
/// - Watches EmotionMono values.
/// - When an emotion crosses a threshold, evolves (replaces or learns) a skill.
///
/// This is intentionally simple and data-driven via Inspector so you can
/// author different party members without changing code.
/// </summary>
[DisallowMultipleComponent]
public class EvolutionMono : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private EmotionMono emotion;
    [SerializeField] private SkillBrainMono brain;

    [Header("Evolution Rules")]
    [Tooltip("If aggression reaches this threshold, evolve Slam (space-making) skill.")]
    [Range(0f, 1f)]
    [SerializeField] private float aggressionEvolveThreshold = 0.9f;

    [SerializeField] private SlamSkill evolvedSlamSkill;

    [Tooltip("If fear reaches this threshold, evolve Guard (defensive) skill.")]
    [Range(0f, 1f)]
    [SerializeField] private float fearEvolveThreshold = 0.9f;

    [SerializeField] private GuardSkill evolvedGuardSkill;

    [Tooltip("If trust reaches this threshold, evolve Taunt (control) skill.")]
    [Range(0f, 1f)]
    [SerializeField] private float trustEvolveThreshold = 0.9f;

    [SerializeField] private TauntSkill evolvedTauntSkill;

    [Header("Behavior")]
    [Tooltip("If true, evolution happens only once (recommended).")]
    [SerializeField] private bool evolveOnlyOnce = true;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private bool _evolvedSlam;
    private bool _evolvedGuard;
    private bool _evolvedTaunt;

    private void Awake()
    {
        if (emotion == null) emotion = GetComponent<EmotionMono>();
        if (brain == null) brain = GetComponent<SkillBrainMono>();

        if (emotion == null)
            Debug.LogWarning("[EvolutionMono] EmotionMono missing on the same GameObject.");

        if (brain == null)
            Debug.LogWarning("[EvolutionMono] SkillBrainMono missing on the same GameObject.");
    }

    private void Update()
    {
        if (emotion == null || brain == null)
            return;

        // You can choose to run evolution checks less frequently later.
        TickEvolution();
    }

    private void TickEvolution()
    {
    }

    private bool AllEvolved()
    {
        // If some evolved skill slots are not configured, treat them as already-done.
        bool slamDone = (evolvedSlamSkill == null) || _evolvedSlam;
        bool guardDone = (evolvedGuardSkill == null) || _evolvedGuard;
        bool tauntDone = (evolvedTauntSkill == null) || _evolvedTaunt;
        return slamDone && guardDone && tauntDone;
    }
}
