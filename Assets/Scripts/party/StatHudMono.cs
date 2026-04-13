using UnityEngine;

/// <summary>
/// StatHudMono
///
/// StatMono의 체력 정보를 월드 공간 이미지형 체력 바로 표시하는 전용 컴포넌트.
/// - 체력 정보는 StatMono를 기준으로만 읽는다.
/// - HUD 생성/위치/색상/표시 여부만 담당한다.
/// - 팝업 데미지 로직은 포함하지 않는다.
/// </summary>
[DisallowMultipleComponent]
public class StatHudMono : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private StatMono stat;

    [Header("Display")]
    [SerializeField] private bool showHud = true;
    [SerializeField] private bool hideWhenFullHp = false;
    [SerializeField] private Vector3 hudOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Bar Shape")]
    [SerializeField] private Vector2 barSize = new Vector2(1.8f, 0.22f);
    [SerializeField] private Vector2 barBackgroundSize = new Vector2(1.95f, 0.30f);

    [Header("Bar Color")]
    [SerializeField] private Color hpHighColor = Color.green;
    [SerializeField] private Color hpMidColor = Color.yellow;
    [SerializeField] private Color hpLowColor = Color.red;
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.75f);

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "UI";
    [SerializeField] private int sortingOrder = 500;

    private GameObject _hudRoot;
    private SpriteRenderer _backgroundRenderer;
    private SpriteRenderer _fillRenderer;
    private bool _lastVisible;
    private float _lastRatio = -1f;
    private Color _lastFillColor = new Color(-1f, -1f, -1f, -1f);
    private static Sprite _sharedBarSprite;

    private void Reset()
    {
        stat = GetComponent<StatMono>();
    }

    private void Awake()
    {
        if (stat == null)
            stat = GetComponent<StatMono>();

        EnsureHud();
        RefreshHud(force: true);
    }

    private void LateUpdate()
    {
        if (_hudRoot != null)
            _hudRoot.transform.position = transform.position + hudOffset;

        RefreshHud();
    }

    public void RefreshHud(bool force = false)
    {
        if (!showHud)
        {
            if (_hudRoot != null)
                _hudRoot.SetActive(false);
            return;
        }

        EnsureHud();
        if (_hudRoot == null || _backgroundRenderer == null || _fillRenderer == null)
            return;

        float maxHp = stat != null ? stat.MaxHp : 0f;
        float currentHp = stat != null ? stat.CurrentHp : 0f;
        bool isDead = stat == null || stat.IsDead;

        float ratio = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
        bool visible = showHud && !isDead && (!hideWhenFullHp || ratio < 0.999f);

        if (force || _lastVisible != visible)
        {
            _hudRoot.SetActive(visible);
            _lastVisible = visible;
        }

        if (!visible)
            return;

        Color nextFillColor = EvaluateHpColor(ratio);

        if (force)
        {
            _backgroundRenderer.transform.localScale = new Vector3(barBackgroundSize.x, barBackgroundSize.y, 1f);
            _backgroundRenderer.color = backgroundColor;
        }

        if (force || !Mathf.Approximately(_lastRatio, ratio))
        {
            float fillWidth = Mathf.Max(0.001f, barSize.x * ratio);
            _fillRenderer.transform.localScale = new Vector3(fillWidth, barSize.y, 1f);
            _fillRenderer.transform.localPosition = new Vector3(-(barSize.x - fillWidth) * 0.5f, 0f, 0f);
            _lastRatio = ratio;
        }

        if (force || _lastFillColor != nextFillColor)
        {
            _fillRenderer.color = nextFillColor;
            _lastFillColor = nextFillColor;
        }
    }

    public void SetStat(StatMono targetStat, bool refreshImmediately = true)
    {
        stat = targetStat;

        if (refreshImmediately)
            RefreshHud(force: true);
    }

    private void EnsureHud()
    {
        if (_hudRoot == null)
        {
            Transform existing = transform.Find("StatHud");
            if (existing != null)
                _hudRoot = existing.gameObject;
        }

        if (_hudRoot == null)
        {
            _hudRoot = new GameObject("StatHud");
            _hudRoot.transform.SetParent(transform, false);
        }

        _hudRoot.transform.localPosition = hudOffset;

        Transform backgroundTransform = _hudRoot.transform.Find("Background");
        Transform fillTransform = _hudRoot.transform.Find("Fill");

        if (backgroundTransform == null)
        {
            GameObject background = new GameObject("Background");
            background.transform.SetParent(_hudRoot.transform, false);
            backgroundTransform = background.transform;
        }

        if (fillTransform == null)
        {
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(_hudRoot.transform, false);
            fillTransform = fill.transform;
        }

        if (_backgroundRenderer == null)
            _backgroundRenderer = backgroundTransform.GetComponent<SpriteRenderer>();
        if (_backgroundRenderer == null)
            _backgroundRenderer = backgroundTransform.gameObject.AddComponent<SpriteRenderer>();

        if (_fillRenderer == null)
            _fillRenderer = fillTransform.GetComponent<SpriteRenderer>();
        if (_fillRenderer == null)
            _fillRenderer = fillTransform.gameObject.AddComponent<SpriteRenderer>();

        Sprite sharedSprite = GetSharedBarSprite();
        _backgroundRenderer.sprite = sharedSprite;
        _fillRenderer.sprite = sharedSprite;

        _backgroundRenderer.sortingLayerName = sortingLayerName;
        _backgroundRenderer.sortingOrder = sortingOrder;
        _fillRenderer.sortingLayerName = sortingLayerName;
        _fillRenderer.sortingOrder = sortingOrder + 1;

        _backgroundRenderer.transform.localPosition = Vector3.zero;
        _fillRenderer.transform.localPosition = Vector3.zero;
    }

    private Color EvaluateHpColor(float ratio)
    {
        if (ratio <= 0.3f)
            return hpLowColor;

        if (ratio <= 0.65f)
            return hpMidColor;

        return hpHighColor;
    }

    private static Sprite GetSharedBarSprite()
    {
        if (_sharedBarSprite != null)
            return _sharedBarSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        _sharedBarSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _sharedBarSprite;
    }

    private void OnDisable()
    {
        if (_hudRoot != null)
            _hudRoot.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshHud(force: true);
    }
}
