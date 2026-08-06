using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 팝업의 생명주기(열기·닫기·풀링)를 전담하는 싱글톤 뷰 컨트롤러.
/// - PopupViewRegistrySO 에서 팝업 타입과 프리팹 매칭 정보를 읽어 configMap을 구성한다.
/// - 팝업 인스턴스를 lazy instantiate 하고, 닫힌 팝업은 타입별 pool에 보관한다.
/// - 같은 PopupType 은 동시에 하나만 열린다.
/// - DontDestroyOnLoad 전용 Canvas를 자체 생성하므로 씬 전환에 독립적이다.
/// - 도메인 ViewData 생성, SetData 호출, 도메인 Manager 직접 참조는 담당하지 않는다.
/// </summary>
public class UIPopupViewController : MonoBehaviour
{
    // ── 싱글톤 ────────────────────────────────────────────────────

    private static UIPopupViewController instance;

    public static UIPopupViewController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<UIPopupViewController>();
            }

            return instance;
        }
    }

    // ── 직렬화 필드 ───────────────────────────────────────────────

    [SerializeField] private PopupViewRegistrySO registry;

    [Header("Canvas Settings")]
    [Tooltip("팝업 Canvas 정렬 순서. 다른 Canvas보다 높게 설정할 것.")]
    [SerializeField] private int canvasSortOrder = 100;
    [Tooltip("팝업 Canvas 해상도. 기본 QHD 기준")]
	[SerializeField] private Vector2 canvasResolution = new Vector2(2560, 1440);

    // ── 내부 상태 ─────────────────────────────────────────────────

    private Canvas popupCanvas;
    private Transform popupRoot;

    private readonly Dictionary<PopupType, PopupViewConfig> configMap = new();
    private readonly Dictionary<PopupType, Queue<UIView>> pooledPopups = new();
    private readonly Dictionary<PopupType, UIView> openedPopups = new();
    private readonly Stack<UIView> popupStack = new();
    private readonly Dictionary<UIView, PopupType> popupTypeMap = new();

    // ── 생명주기 ─────────────────────────────────────────────────

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        InitializePopupCanvas();
        InitializeConfigMap();
    }

    // ── Canvas 초기화 ─────────────────────────────────────────────

    /// <summary>
    /// DontDestroyOnLoad 전용 팝업 Canvas를 자체 생성한다.
    /// 씬 전환과 무관하게 항상 유효하다.
    /// </summary>
    private void InitializePopupCanvas()
    {
        GameObject canvasGo = new GameObject("PopupCanvas");
        canvasGo.transform.SetParent(transform, false);

        popupCanvas = canvasGo.AddComponent<Canvas>();
        popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        popupCanvas.sortingOrder = canvasSortOrder;
        //popupCanvas.renderingDisplaySize = canvasResolution;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2560, 1440);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        popupRoot = canvasGo.transform;
    }

    // ── Config 초기화 ─────────────────────────────────────────────

    private void InitializeConfigMap()
    {
        if (registry == null)
        {
            Debug.LogError($"[{nameof(UIPopupViewController)}] registry가 null입니다. Inspector에서 PopupViewRegistrySO를 할당하세요.", this);
            return;
        }

        IReadOnlyList<PopupViewConfig> configs = registry.Configs;

        for (int i = 0; i < configs.Count; i++)
        {
            PopupViewConfig cfg = configs[i];

            if (cfg == null)
            {
                Debug.LogWarning($"[{nameof(UIPopupViewController)}] registry.configs[{i}]가 null입니다. 건너뜁니다.");
                continue;
            }

            if (cfg.type == PopupType.None)
            {
                Debug.LogWarning($"[{nameof(UIPopupViewController)}] registry.configs[{i}]의 type이 PopupType.None입니다. 건너뜁니다.");
                continue;
            }

            if (cfg.prefab == null)
            {
                Debug.LogWarning($"[{nameof(UIPopupViewController)}] registry.configs[{i}] ({cfg.type})의 prefab이 null입니다. 건너뜁니다.");
                continue;
            }

            if (configMap.ContainsKey(cfg.type))
            {
                Debug.LogWarning($"[{nameof(UIPopupViewController)}] 중복 PopupType 발견: {cfg.type}. 첫 번째 항목만 사용됩니다.");
                continue;
            }

            configMap[cfg.type] = cfg;
        }
    }

    // ── Public API ────────────────────────────────────────────────

    /// <summary>
    /// 팝업을 열고 UIView 인스턴스를 반환한다.
    /// 이미 열린 타입이면 기존 인스턴스를 반환한다.
    /// </summary>
    public UIView Open(PopupType type)
    {
        if (type == PopupType.None)
        {
            Debug.LogError($"[{nameof(UIPopupViewController)}] PopupType.None으로 Open을 호출했습니다.");
            return null;
        }

        // 이미 열린 팝업이 있으면 반환
        if (openedPopups.TryGetValue(type, out UIView existing) && existing != null)
        {
            return existing;
        }

        // config 검색
        if (!configMap.TryGetValue(type, out PopupViewConfig config))
        {
            Debug.LogError($"[{nameof(UIPopupViewController)}] PopupType.{type}에 해당하는 config가 없습니다. PopupViewRegistrySO를 확인하세요.");
            return null;
        }

        // pool 에서 재사용
        UIView popup = DequeueFromPool(type);

        if (popup == null)
        {
            // lazy instantiate — 팝업 Canvas 하위에 생성
            popup = Instantiate(config.prefab, popupRoot);
        }
        else
        {
            popup.transform.SetParent(popupRoot, false);
        }

        RegisterPopup(type, popup);
        popup.OnOpenFromManager();

        return popup;
    }

    /// <summary>
    /// 팝업을 열고 구체적인 타입 T 로 캐스팅해서 반환한다.
    /// </summary>
    public T Open<T>(PopupType type) where T : UIView
    {
        UIView popup = Open(type);

        if (popup == null)
            return null;

        if (popup is T typedPopup)
            return typedPopup;

        Debug.LogError($"[{nameof(UIPopupViewController)}]: 타입 캐스팅 실패. 요청 타입: {typeof(T).Name}, 실제 타입: {popup.GetType().Name}.", this);
        return null;
    }

    /// <summary>
    /// 특정 팝업 인스턴스를 닫는다.
    /// </summary>
    public void Close(UIView popup)
    {
        if (popup == null)
            return;

        if (!popupTypeMap.TryGetValue(popup, out PopupType type))
        {
            Debug.LogWarning($"[{nameof(UIPopupViewController)}] 닫으려는 팝업의 PopupType 매핑을 찾지 못했습니다: {popup.name}");
            return;
        }

        UnregisterPopup(type, popup);
        popup.OnCloseFromManager();
        EnqueueToPool(type, popup);
    }

    /// <summary>
    /// 특정 타입의 팝업을 닫는다.
    /// 열려 있지 않으면 아무것도 하지 않는다.
    /// </summary>
    public void Close(PopupType type)
    {
        if (!openedPopups.TryGetValue(type, out UIView popup) || popup == null)
            return;

        Close(popup);
    }

    /// <summary>
    /// 스택 최상단 팝업을 닫는다.
    /// null 또는 이미 닫힌 팝업은 제거하며, 유효한 최상단을 찾아 닫는다.
    /// </summary>
    public void CloseTopPopup()
    {
        while (popupStack.Count > 0)
        {
            UIView top = popupStack.Peek();

            if (top == null || !openedPopups.ContainsValue(top))
            {
                popupStack.Pop();
                continue;
            }

            Close(top);
            return;
        }
    }

    /// <summary>
    /// 현재 열린 모든 팝업을 닫는다.
    /// </summary>
    public void CloseAllPopups()
    {
        // 순회 중 수정 방지를 위해 목록 복사
        List<UIView> toClose = new List<UIView>(openedPopups.Values);

        for (int i = 0; i < toClose.Count; i++)
        {
            if (toClose[i] != null)
            {
                Close(toClose[i]);
            }
        }

        popupStack.Clear();
    }

    /// <summary>
    /// 열려 있는 팝업 인스턴스를 타입으로 조회한다.
    /// </summary>
    public bool TryGetOpenedPopup(PopupType type, out UIView popup)
    {
        return openedPopups.TryGetValue(type, out popup) && popup != null;
    }

    // ── 내부 헬퍼 ────────────────────────────────────────────────

    private void RegisterPopup(PopupType type, UIView popup)
    {
        openedPopups[type] = popup;
        popupTypeMap[popup] = type;
        popupStack.Push(popup);
    }

    private void UnregisterPopup(PopupType type, UIView popup)
    {
        openedPopups.Remove(type);
        popupTypeMap.Remove(popup);
        RemoveFromStack(popup);
    }

    private void EnqueueToPool(PopupType type, UIView popup)
    {
        if (!pooledPopups.TryGetValue(type, out Queue<UIView> queue))
        {
            queue = new Queue<UIView>();
            pooledPopups[type] = queue;
        }

        // 중복 반환 방지
        if (!queue.Contains(popup))
        {
            queue.Enqueue(popup);
        }
    }

    private UIView DequeueFromPool(PopupType type)
    {
        if (pooledPopups.TryGetValue(type, out Queue<UIView> queue) && queue.Count > 0)
        {
            return queue.Dequeue();
        }

        return null;
    }

    private void RemoveFromStack(UIView popup)
    {
        // Stack 은 중간 요소를 제거하는 API 가 없으므로 재구성
        UIView[] items = popupStack.ToArray();
        popupStack.Clear();

        // ToArray() 는 top → bottom 순서이므로 역순으로 Push 해야 순서 유지
        for (int i = items.Length - 1; i >= 0; i--)
        {
            if (items[i] != popup)
            {
                popupStack.Push(items[i]);
            }
        }
    }
}
