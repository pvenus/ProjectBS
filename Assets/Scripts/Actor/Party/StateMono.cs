using UnityEngine;

/// <summary>
/// Determines the behavioral state of a party member based on EmotionMono.
/// Other systems (SkillBrain, visuals, etc.) read this state to change behavior.
/// </summary>
public class StateMono : MonoBehaviour
{
    public enum PartyState
    {
        Normal,
        Aggressive,
        Defensive,
        Panic,
        Support
    }

    [Header("Current State")]
    [SerializeField] private PartyState currentState = PartyState.Normal;

    [Header("Thresholds")]
    [SerializeField] private float aggressionThreshold = 0.7f;
    [SerializeField] private float fearThreshold = 0.7f;
    [SerializeField] private float trustThreshold = 0.7f;

    private EmotionMono emotion;

    public PartyState CurrentState => currentState;

    void Awake()
    {
        emotion = GetComponent<EmotionMono>();

        if (emotion == null)
        {
            Debug.LogWarning("StateMono requires EmotionMono on the same GameObject.");
        }
    }

    void Update()
    {
        if (emotion == null)
            return;

        EvaluateState();
    }

    void EvaluateState()
    {
        // Default fallback
        SetState(PartyState.Normal);
    }

    void SetState(PartyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        Debug.Log($"[StateMono] State changed -> {currentState}");
    }
}
