using UnityEngine;

/// <summary>
/// Manages the 7-base-emotion state for a party member.
/// Other systems (events, combat, relationships) modify these values.
/// StateMono and SkillBrainMono read these values to make decisions.
///
/// 7-base emotions:
/// - joy     : 희 (기쁨)
/// - anger   : 노 (분노)
/// - sadness : 애 (슬픔)
/// - pleasure: 락 (즐거움)
/// - love    : 애 (사랑)
/// - hate    : 오 (미움)
/// - desire  : 욕 (욕망)
/// </summary>
public class EmotionMono : MonoBehaviour
{
    [Header("7 Base Emotions")]
    [Range(0f,1f)] public float joy = 0.5f;
    [Range(0f,1f)] public float anger = 0.2f;
    [Range(0f,1f)] public float sadness = 0.2f;
    [Range(0f,1f)] public float pleasure = 0.5f;
    [Range(0f,1f)] public float love = 0.5f;
    [Range(0f,1f)] public float hate = 0.2f;
    [Range(0f,1f)] public float desire = 0.4f;

    [Header("Decay")]
    [Tooltip("How quickly emotions return toward neutral over time.")]
    [SerializeField] private float decaySpeed = 0.02f;

    const float Neutral = 0.5f;

    void Update()
    {
        ApplyDecay();
    }

    void ApplyDecay()
    {
        joy = MoveToNeutral(joy);
        anger = MoveToNeutral(anger);
        sadness = MoveToNeutral(sadness);
        pleasure = MoveToNeutral(pleasure);
        love = MoveToNeutral(love);
        hate = MoveToNeutral(hate);
        desire = MoveToNeutral(desire);
    }

    float MoveToNeutral(float value)
    {
        return Mathf.MoveTowards(value, Neutral, decaySpeed * Time.deltaTime);
    }

    public void AddJoy(float value)
    {
        joy = Clamp01(joy + value);
    }

    public void AddAnger(float value)
    {
        anger = Clamp01(anger + value);
    }

    public void AddSadness(float value)
    {
        sadness = Clamp01(sadness + value);
    }

    public void AddPleasure(float value)
    {
        pleasure = Clamp01(pleasure + value);
    }

    public void AddLove(float value)
    {
        love = Clamp01(love + value);
    }

    public void AddHate(float value)
    {
        hate = Clamp01(hate + value);
    }

    public void AddDesire(float value)
    {
        desire = Clamp01(desire + value);
    }

    float Clamp01(float v)
    {
        return Mathf.Clamp01(v);
    }

    public float GetEmotionIntensity()
    {
        // Useful metric for evolution systems
        return Mathf.Max(joy, anger, sadness, pleasure, love, hate, desire);
    }
}
