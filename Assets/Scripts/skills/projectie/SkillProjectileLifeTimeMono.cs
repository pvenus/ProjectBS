

using UnityEngine;

/// <summary>
/// Projectile 생명주기 전용 Mono.
/// - 시작 시점 기록
/// - 일정 시간 후 자동 종료
/// - 외부에서 강제 종료 가능
/// </summary>
public class SkillProjectileLifeTimeMono : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;
    [Header("Runtime Options")]
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool autoDestroyOnEnd = true;

    private float _elapsedTime;
    private bool _isRunning;

    public float ElapsedTime => _elapsedTime;
    private void Awake()
    {
        if (playOnAwake)
        {
            StartLife(lifetime);
        }
    }

    public bool IsRunning => _isRunning;
    public float Lifetime => lifetime;

    /// <summary>
    /// 외부에서 lifetime 설정 및 시작
    /// </summary>
    public void StartLife(float duration)
    {
        lifetime = Mathf.Max(0.01f, duration);
        _elapsedTime = 0f;
        _isRunning = true;
    }

    /// <summary>
    /// 기본 lifetime으로 시작
    /// </summary>
    public void StartLife()
    {
        _elapsedTime = 0f;
        _isRunning = true;
    }

    /// <summary>
    /// 강제 종료
    /// </summary>
    public void ForceEnd()
    {
        if (!_isRunning)
            return;

        EndLife();
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= lifetime)
        {
            EndLife();
        }
    }

    private void EndLife()
    {
        if (!_isRunning)
            return;

        _isRunning = false;

        if (autoDestroyOnEnd)
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw a simple progress bar above the object
        float t = lifetime > 0f ? Mathf.Clamp01(_elapsedTime / lifetime) : 0f;
        Vector3 pos = transform.position + Vector3.up * 0.5f;
        float width = 0.5f;

        // background
        Gizmos.color = new Color(0f, 0f, 0f, 0.6f);
        Gizmos.DrawCube(pos, new Vector3(width, 0.05f, 0f));

        // progress
        Gizmos.color = Color.green;
        Gizmos.DrawCube(pos + Vector3.left * (width * 0.5f * (1f - t)), new Vector3(width * t, 0.05f, 0f));
    }
#endif
}