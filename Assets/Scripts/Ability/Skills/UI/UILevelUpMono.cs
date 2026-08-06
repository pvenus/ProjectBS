using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

/// <summary>
/// Runtime-created Level Up popup UI.
/// - Call UILevelUpMono.Instance.Open() when player levels up.
/// - Pauses the game (Time.timeScale = 0) while open.
/// - Shows 3 options, each with a different image.
/// - Clicking an option closes the popup (effect hook TODO).
///
/// Notes:
/// - Uses Unity UI (uGUI). No prefabs required.
/// - Ensures there is an EventSystem.
/// </summary>
public class UILevelUpMono : MonoBehaviour
{
    public static UILevelUpMono Instance { get; private set; }

    /// <summary>
    /// Fired when a card is selected. Subscribe from gameplay systems.
    /// Example: UILevelUpMono.Instance.OptionSelected += (i) => ...;
    /// </summary>
    public event Action<int> OptionSelected;

    public int LastSelectedIndex { get; private set; } = -1;

    [Header("Popup")]
    [SerializeField] private bool openOnStart = false;

    [Header("Layout")]
    [SerializeField] private Vector2 popupSize = new Vector2(900f, 520f);
    [SerializeField] private float cardWidth = 240f;
    [SerializeField] private float cardHeight = 340f;
    [SerializeField] private float cardSpacing = 28f;

    [Header("Images")]
    [SerializeField] private int imageSize = 96;

    private Canvas _canvas;
    private GameObject _root;
    private GameObject _panel;
    private Button[] _buttons;
    private Image[] _images;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EnsureEventSystem();
        BuildUIIfNeeded();
        CloseImmediate();
    }

    private void Start()
    {
        if (openOnStart)
            Open();
    }

    /// <summary>
    /// Open the popup and pause the world.
    /// </summary>
    public void Open()
    {
        BuildUIIfNeeded();

        if (IsOpen) return;
        IsOpen = true;

        _root.SetActive(true);

        // Pause world
        Time.timeScale = 0f;

        // Focus first button
        if (_buttons != null && _buttons.Length > 0)
            _buttons[0].Select();
    }

    /// <summary>
    /// Close popup and resume the world.
    /// </summary>
    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        _root.SetActive(false);

        // Reset selection highlight so next open starts clean.
        if (_buttons != null && _buttons.Length > 0)
            EventSystem.current?.SetSelectedGameObject(null);

        Time.timeScale = 1f;
    }

    private void CloseImmediate()
    {
        IsOpen = false;
        if (_root != null) _root.SetActive(false);
        LastSelectedIndex = -1;
    }

    private void OnSelect(int index)
    {
        LastSelectedIndex = index;

        // Notify gameplay systems first.
        try
        {
            OptionSelected?.Invoke(index);
        }
        catch (Exception e)
        {
            Debug.LogError($"LevelUp OptionSelected handler threw: {e}");
        }

        Debug.Log($"LevelUp option selected: {index}");

        // Close after selection.
        Close();
    }

    private void BuildUIIfNeeded()
    {
        if (_root != null) return;

        // Canvas
        _root = new GameObject("UI_LevelUpRoot");
        _root.transform.SetParent(transform, false);

        _canvas = _root.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 999;

        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        _root.AddComponent<GraphicRaycaster>();

        // Dim background
        var dim = new GameObject("Dim");
        dim.transform.SetParent(_root.transform, false);
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.65f);
        StretchToFullScreen(dim.GetComponent<RectTransform>());

        // Panel
        _panel = new GameObject("Panel");
        _panel.transform.SetParent(_root.transform, false);
        var panelRt = _panel.AddComponent<RectTransform>();
        panelRt.sizeDelta = popupSize;
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;

        var panelImg = _panel.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

        var panelOutline = _panel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        panelOutline.effectDistance = new Vector2(3f, -3f);

        // Title
        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(_panel.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -28f);
        titleRt.sizeDelta = new Vector2(popupSize.x - 40f, 60f);

        var titleText = titleGo.AddComponent<Text>();
        titleText.text = "LEVEL UP!\nChoose 1";
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 34;
        titleText.color = Color.white;

        // Container for cards
        var row = new GameObject("Row");
        row.transform.SetParent(_panel.transform, false);
        var rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.pivot = new Vector2(0.5f, 0.5f);
        rowRt.anchoredPosition = new Vector2(0f, -10f);
        rowRt.sizeDelta = new Vector2(popupSize.x - 60f, popupSize.y - 130f);

        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = cardSpacing;
        hlg.padding = new RectOffset(10, 10, 10, 10);
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = false;

        _buttons = new Button[3];
        _images = new Image[3];

        // Create 3 cards
        for (int i = 0; i < 3; i++)
        {
            CreateCard(row.transform, i);
        }

        // Different images per option
        _images[0].sprite = CreateSolidSprite(imageSize, imageSize, new Color(1f, 0.35f, 0.35f, 1f));
        _images[1].sprite = CreateSolidSprite(imageSize, imageSize, new Color(0.35f, 1f, 0.55f, 1f));
        _images[2].sprite = CreateSolidSprite(imageSize, imageSize, new Color(0.35f, 0.6f, 1f, 1f));

        _root.SetActive(false);
    }

    private void CreateCard(Transform parent, int index)
    {
        var card = new GameObject($"Card_{index}");
        card.transform.SetParent(parent, false);

        var cardRt = card.AddComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(cardWidth, cardHeight);

        var cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.18f, 0.18f, 0.22f, 1f);

        var cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        cardOutline.effectDistance = new Vector2(2f, -2f);

        // Button
        var btn = card.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.18f, 0.18f, 0.22f, 1f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.30f, 1f);
        colors.pressedColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        colors.selectedColor = colors.highlightedColor;
        btn.colors = colors;

        int captured = index;
        btn.onClick.AddListener(() => OnSelect(captured));
        _buttons[index] = btn;

        // Vertical layout inside card
        var vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 10f;
        vlg.padding = new RectOffset(14, 14, 14, 14);
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        // Image
        var imgGo = new GameObject("Icon");
        imgGo.transform.SetParent(card.transform, false);
        var img = imgGo.AddComponent<Image>();
        img.preserveAspect = true;
        var imgRt = imgGo.GetComponent<RectTransform>();
        imgRt.sizeDelta = new Vector2(imageSize, imageSize);
        _images[index] = img;

        // Label
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(card.transform, false);
        var label = labelGo.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 22;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = (index == 0) ? "Option A" : (index == 1) ? "Option B" : "Option C";

        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.sizeDelta = new Vector2(cardWidth - 30f, 42f);

        // Subtext
        var subGo = new GameObject("SubText");
        subGo.transform.SetParent(card.transform, false);
        var sub = subGo.AddComponent<Text>();
        sub.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        sub.fontSize = 16;
        sub.color = new Color(1f, 1f, 1f, 0.75f);
        sub.alignment = TextAnchor.UpperCenter;
        sub.text = "(Effect later)";

        var subRt = subGo.GetComponent<RectTransform>();
        subRt.sizeDelta = new Vector2(cardWidth - 30f, 80f);

        // Make children sizes respected by layout
        imgGo.AddComponent<LayoutElement>().preferredHeight = imageSize;
        labelGo.AddComponent<LayoutElement>().preferredHeight = 44f;
        subGo.AddComponent<LayoutElement>().preferredHeight = 80f;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
        DontDestroyOnLoad(es);
    }

    private static void StretchToFullScreen(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private static Sprite CreateSolidSprite(int w, int h, Color c)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}
