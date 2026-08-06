
using UnityEngine;

/// <summary>
/// PartyAgentMono
///
/// Central "hub" component for a party member.
///
/// Goals:
/// - Keep code management easy by having one place that wires modules together.
/// - Each module remains small (EmotionMono, StatMono, PerceptionMono, StateMono, SkillBrainMono, SkillExecutorMono, EvolutionMono).
/// - Provide safe accessors for other scripts.
/// - Provide simple debug toggles.
///
/// Usage:
/// - Add this to a party member GameObject.
/// - Add the modules you want on the same GameObject.
/// - This will auto-find them in Awake and can auto-add missing core modules if configured.
/// </summary>
[DisallowMultipleComponent]
public class PartyAgentMono : MonoBehaviour
{
    [Header("Auto wiring")]
    [Tooltip("If true, missing core modules will be added automatically.")]
    [SerializeField] private bool autoAddMissingModules = true;

    [Tooltip("If true, required references are validated and warnings are printed.")]
    [SerializeField] private bool validateOnAwake = true;

    [Header("Modules")]
    [SerializeField] private StatMono stat;
    [SerializeField] private EmotionMono emotion;
    [SerializeField] private PerceptionMono perception;
    [SerializeField] private StateMono state;
    [SerializeField] private SkillBrainMono brain;
    [SerializeField] private EvolutionMono evolution;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    // Public read-only accessors
    public StatMono Stat => stat;
    public EmotionMono Emotion => emotion;
    public PerceptionMono Perception => perception;
    public StateMono State => state;
    public SkillBrainMono Brain => brain;
    public EvolutionMono Evolution => evolution;

    private void Reset()
    {
        // For convenience in Editor.
        AutoWire();
    }

    private void Awake()
    {
        AutoWire();

        if (validateOnAwake)
            ValidateSetup();

        if (debugLog)
            Debug.Log($"[PartyAgent] Awake name={name} modules: stat={(stat!=null)} emotion={(emotion!=null)} perception={(perception!=null)} state={(state!=null)} brain={(brain!=null)} evolution={(evolution!=null)}");
    }

    private void AutoWire()
    {
        // Find existing modules
        stat = (stat != null) ? stat : GetComponent<StatMono>();
        emotion = (emotion != null) ? emotion : GetComponent<EmotionMono>();
        perception = (perception != null) ? perception : GetComponent<PerceptionMono>();
        state = (state != null) ? state : GetComponent<StateMono>();
        brain = (brain != null) ? brain : GetComponent<SkillBrainMono>();
        evolution = (evolution != null) ? evolution : GetComponent<EvolutionMono>();

        if (!autoAddMissingModules)
            return;

        // Auto-add core modules if missing
        if (stat == null) stat = gameObject.AddComponent<StatMono>();
        if (emotion == null) emotion = gameObject.AddComponent<EmotionMono>();
        if (perception == null) perception = gameObject.AddComponent<PerceptionMono>();
        if (state == null) state = gameObject.AddComponent<StateMono>();
        if (brain == null) brain = gameObject.AddComponent<SkillBrainMono>();
        if (evolution == null) evolution = gameObject.AddComponent<EvolutionMono>();
    }

    private void ValidateSetup()
    {
        // These are the core modules for your AI party agent.
        if (stat == null) Debug.LogWarning($"[PartyAgent] StatMono missing on {name}");
        if (emotion == null) Debug.LogWarning($"[PartyAgent] EmotionMono missing on {name}");
        if (perception == null) Debug.LogWarning($"[PartyAgent] PerceptionMono missing on {name}");
        if (state == null) Debug.LogWarning($"[PartyAgent] StateMono missing on {name}");
        if (brain == null) Debug.LogWarning($"[PartyAgent] SkillBrainMono missing on {name}");
        if (evolution == null) Debug.LogWarning($"[PartyAgent] EvolutionMono missing on {name}");

        // Optional cross-wiring (light-touch):
        // - Brain should prefer PerceptionMono for enemy counts.
        // We keep this as a warning only, because you might still be iterating.
        if (brain != null && perception == null)
            Debug.LogWarning($"[PartyAgent] Brain exists but Perception is missing. Brain will have limited context. ({name})");
    }

    // -----------------
    // Convenience helpers
    // -----------------

    public bool IsDead()
    {
        return stat != null && stat.IsDead;
    }

    public float Hp01()
    {
        return (stat != null) ? stat.Hp01 : 0f;
    }
}
