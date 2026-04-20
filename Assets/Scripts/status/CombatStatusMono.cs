using System;
using UnityEngine;

/// <summary>
/// 대상의 전투 상태(현재는 Heat/Overheat)를 보관하는 최소 상태 컴포넌트.
/// 계산식이나 폭발 처리 로직은 외부 서비스에서 담당하고,
/// 이 컴포넌트는 상태 저장/갱신/초기화에 집중한다.
/// </summary>
public class CombatStatusMono : MonoBehaviour
{
    [Header("Heat")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float heatDecayPerSecond = 0f;
    [SerializeField] private bool decayOnlyWhenNotOverheated = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    /// <summary>현재 누적 Heat.</summary>
    public float CurrentHeat { get; private set; }

    /// <summary>최대 Heat. CurrentHeat가 이 값 이상이면 Overheated 상태로 본다.</summary>
    public float MaxHeat => maxHeat;

    /// <summary>현재 Overheat 상태 여부.</summary>
    public bool IsOverheated => CurrentHeat >= maxHeat;

    /// <summary>Heat 비율(0~1).</summary>
    public float HeatNormalized => maxHeat <= 0f ? 0f : Mathf.Clamp01(CurrentHeat / maxHeat);

    /// <summary>Heat가 변경될 때 호출된다. (currentHeat, maxHeat)</summary>
    public event Action<float, float> OnHeatChanged;

    /// <summary>Overheat 상태에 처음 진입했을 때 호출된다.</summary>
    public event Action OnOverheatTriggered;

    /// <summary>Heat가 초기화되었을 때 호출된다.</summary>
    public event Action OnHeatReset;

    private void Update()
    {
        TickHeatDecay(Time.deltaTime);
    }

    /// <summary>
    /// Heat를 누적한다.
    /// 이미 Overheat 상태라면 maxHeat까지만 유지한다.
    /// </summary>
    public void AddHeat(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        bool wasOverheated = IsOverheated;
        float previousHeat = CurrentHeat;

        CurrentHeat = Mathf.Clamp(CurrentHeat + amount, 0f, maxHeat);

        if (!Mathf.Approximately(previousHeat, CurrentHeat))
        {
            Log($"AddHeat: +{amount:0.##} => {CurrentHeat:0.##}/{maxHeat:0.##}");
            RaiseHeatChanged();
        }

        if (!wasOverheated && IsOverheated)
        {
            Log("Overheat triggered.");
            OnOverheatTriggered?.Invoke();
        }
    }

    /// <summary>
    /// Heat를 직접 설정한다.
    /// 외부 디버그/테스트 또는 특수 효과에서 사용한다.
    /// </summary>
    public void SetHeat(float value)
    {
        bool wasOverheated = IsOverheated;
        float clamped = Mathf.Clamp(value, 0f, maxHeat);

        if (Mathf.Approximately(CurrentHeat, clamped))
        {
            return;
        }

        CurrentHeat = clamped;
        Log($"SetHeat: {CurrentHeat:0.##}/{maxHeat:0.##}");
        RaiseHeatChanged();

        if (!wasOverheated && IsOverheated)
        {
            Log("Overheat triggered by SetHeat.");
            OnOverheatTriggered?.Invoke();
        }
    }

    /// <summary>
    /// Heat를 모두 초기화한다.
    /// Overheat 폭발 후 기본 상태로 되돌릴 때 사용한다.
    /// </summary>
    public void ResetHeat()
    {
        if (Mathf.Approximately(CurrentHeat, 0f))
        {
            return;
        }

        CurrentHeat = 0f;
        Log("Heat reset.");
        RaiseHeatChanged();
        OnHeatReset?.Invoke();
    }

    /// <summary>
    /// Heat를 일정량 감소시킨다.
    /// 음수 입력은 무시한다.
    /// </summary>
    public void ReduceHeat(float amount)
    {
        if (amount <= 0f || Mathf.Approximately(CurrentHeat, 0f))
        {
            return;
        }

        float previousHeat = CurrentHeat;
        CurrentHeat = Mathf.Clamp(CurrentHeat - amount, 0f, maxHeat);

        if (!Mathf.Approximately(previousHeat, CurrentHeat))
        {
            Log($"ReduceHeat: -{amount:0.##} => {CurrentHeat:0.##}/{maxHeat:0.##}");
            RaiseHeatChanged();
        }
    }

    /// <summary>
    /// 초당 감소량 기준으로 Heat를 자연 감소시킨다.
    /// </summary>
    public void TickHeatDecay(float deltaTime)
    {
        if (deltaTime <= 0f || heatDecayPerSecond <= 0f)
        {
            return;
        }

        if (decayOnlyWhenNotOverheated && IsOverheated)
        {
            return;
        }

        ReduceHeat(heatDecayPerSecond * deltaTime);
    }

    /// <summary>
    /// 현재 상태 기준 Heat 비례 추가 데미지를 계산한다.
    /// coefficient는 스킬마다 다른 계수를 넣는다.
    /// </summary>
    public float GetBonusDamage(float coefficient)
    {
        if (coefficient <= 0f || Mathf.Approximately(CurrentHeat, 0f))
        {
            return 0f;
        }

        return CurrentHeat * coefficient;
    }

    /// <summary>
    /// 외부 서비스에서 폭발 처리 여부를 판단하기 쉽도록 단순 조건 함수를 제공한다.
    /// </summary>
    public bool CanTriggerOverheatExplosion()
    {
        return IsOverheated;
    }

    private void RaiseHeatChanged()
    {
        OnHeatChanged?.Invoke(CurrentHeat, maxHeat);
    }

    private void Log(string message)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.Log($"[CombatStatusMono] {name} - {message}", this);
    }
}
