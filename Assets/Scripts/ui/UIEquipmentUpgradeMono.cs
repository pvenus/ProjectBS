

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 장비 획득 / 합성 테스트용 UI Mono.
/// - Acquire 버튼으로 테스트 장비를 인벤토리에 추가한다.
/// - 장비 리스트 버튼을 클릭해 합성 대상 장비를 선택한다.
/// - Upgrade 버튼으로 같은 장비/같은 등급 3개 합성을 시도한다.
/// - 성공/실패 후 UI를 갱신한다.
/// </summary>
public class UIEquipmentUpgradeMono : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EquipmentInventoryMono equipmentInventory;
    [SerializeField] private EquipmentSkillSO acquireEquipmentSo;

    [Header("Buttons")]
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button refreshButton;

    [Header("List UI")]
    [SerializeField] private Transform listRoot;
    [SerializeField] private Button itemButtonPrefab;

    [Header("Text UI")]
    [SerializeField] private TMP_Text selectedText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text countText;

    private readonly List<Button> spawnedButtons = new();
    private OwnedEquipmentData selectedEquipment;

    public OwnedEquipmentData SelectedEquipment => selectedEquipment;

    private void Awake()
    {
        if (equipmentInventory == null)
        {
            equipmentInventory = FindObjectOfType<EquipmentInventoryMono>();
        }

        // Auto-find UI components if not assigned
        if (acquireButton == null)
            acquireButton = transform.Find("AcquireButton")?.GetComponent<Button>();

        if (upgradeButton == null)
            upgradeButton = transform.Find("UpgradeButton")?.GetComponent<Button>();

        if (refreshButton == null)
            refreshButton = transform.Find("RefreshButton")?.GetComponent<Button>();

        if (listRoot == null)
            listRoot = transform.Find("ListRoot");

        if (itemButtonPrefab == null)
            itemButtonPrefab = Resources.Load<Button>("UI/ItemButtonPrefab");

        if (selectedText == null)
            selectedText = transform.Find("SelectedText")?.GetComponent<TMPro.TMP_Text>();

        if (statusText == null)
            statusText = transform.Find("StatusText")?.GetComponent<TMPro.TMP_Text>();

        if (countText == null)
            countText = transform.Find("CountText")?.GetComponent<TMPro.TMP_Text>();

        EnsureGeneratedUI();
    }

    private void EnsureGeneratedUI()
    {
        if (acquireButton != null && upgradeButton != null && listRoot != null && selectedText != null && statusText != null && countText != null && itemButtonPrefab != null)
        {
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("EquipmentUpgradeTestCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            transform.SetParent(canvas.transform, false);
        }

        RectTransform root = GetComponent<RectTransform>();
        if (root == null)
        {
            root = gameObject.AddComponent<RectTransform>();
        }

        root.anchorMin = new Vector2(0f, 1f);
        root.anchorMax = new Vector2(0f, 1f);
        root.pivot = new Vector2(0f, 1f);
        root.anchoredPosition = new Vector2(40f, -40f);
        root.sizeDelta = new Vector2(520f, 700f);

        Image background = GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
        }
        background.color = new Color(0f, 0f, 0f, 0.65f);

        VerticalLayoutGroup rootLayout = GetComponent<VerticalLayoutGroup>();
        if (rootLayout == null)
        {
            rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
        }
        rootLayout.padding = new RectOffset(16, 16, 16, 16);
        rootLayout.spacing = 8f;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = false;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        if (countText == null)
        {
            countText = CreateText("CountText", transform, "Owned: 0", 24, 36f);
        }

        if (selectedText == null)
        {
            selectedText = CreateText("SelectedText", transform, "Selected: None", 20, 58f);
        }

        if (statusText == null)
        {
            statusText = CreateText("StatusText", transform, "Ready", 18, 42f);
        }

        if (acquireButton == null || upgradeButton == null || refreshButton == null)
        {
            Transform buttonRow = CreatePanel("ButtonRow", transform, 48f);
            HorizontalLayoutGroup rowLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            if (acquireButton == null)
            {
                acquireButton = CreateButton("AcquireButton", buttonRow, "Acquire");
            }

            if (upgradeButton == null)
            {
                upgradeButton = CreateButton("UpgradeButton", buttonRow, "Upgrade");
            }

            if (refreshButton == null)
            {
                refreshButton = CreateButton("RefreshButton", buttonRow, "Refresh");
            }
        }

        if (listRoot == null)
        {
            Transform listPanel = CreatePanel("ListPanel", transform, 480f);
            ScrollRect scrollRect = listPanel.gameObject.AddComponent<ScrollRect>();
            Image listBackground = listPanel.gameObject.AddComponent<Image>();
            listBackground.color = new Color(1f, 1f, 1f, 0.08f);

            GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(listPanel, false);
            RectTransform viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentGo = new GameObject("ListRoot", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            RectTransform contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup contentLayout = contentGo.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            contentLayout.spacing = 6f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            listRoot = contentGo.transform;
        }

        if (itemButtonPrefab == null)
        {
            itemButtonPrefab = CreateButton("ItemButtonPrefab", transform, "Item");
            itemButtonPrefab.gameObject.SetActive(false);
        }
    }

    private Transform CreatePanel(string objectName, Transform parent, float height)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        LayoutElement layoutElement = go.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = height;
        layoutElement.minHeight = height;
        layoutElement.flexibleWidth = 1f;

        return go.transform;
    }

    private TMP_Text CreateText(string objectName, Transform parent, string text, int fontSize, float height)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        TMP_Text tmp = go.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = true;

        LayoutElement layoutElement = go.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = height;
        layoutElement.minHeight = height;
        layoutElement.flexibleWidth = 1f;

        return tmp;
    }

    private Button CreateButton(string objectName, Transform parent, string label)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        Button button = go.GetComponent<Button>();

        LayoutElement layoutElement = go.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 44f;
        layoutElement.minHeight = 44f;
        layoutElement.flexibleWidth = 1f;

        TMP_Text text = CreateText("Text", go.transform, label, 18, 44f);
        text.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private void OnEnable()
    {
        Debug.Log($"[UIEquipmentUpgradeMono] Auto wiring status - AcquireBtn:{acquireButton!=null}, UpgradeBtn:{upgradeButton!=null}, ListRoot:{listRoot!=null}", this);

        if (acquireButton != null)
        {
            acquireButton.onClick.AddListener(OnClickAcquire);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnClickUpgrade);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshUI);
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (acquireButton != null)
        {
            acquireButton.onClick.RemoveListener(OnClickAcquire);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnClickUpgrade);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveListener(RefreshUI);
        }
    }

    private void OnClickAcquire()
    {
        if (equipmentInventory == null)
        {
            SetStatus("Inventory is null.");
            return;
        }

        if (acquireEquipmentSo == null)
        {
            SetStatus("Acquire Equipment SO is null.");
            return;
        }

        OwnedEquipmentData owned = equipmentInventory.Acquire(acquireEquipmentSo);
        selectedEquipment = owned;

        SetStatus(owned != null
            ? $"Acquired: {owned.DisplayName} / {owned.CurrentGrade}"
            : "Acquire failed.");

        RefreshUI();
    }

    private void OnClickUpgrade()
    {
        if (equipmentInventory == null)
        {
            SetStatus("Inventory is null.");
            return;
        }

        if (selectedEquipment == null)
        {
            SetStatus("Select equipment first.");
            return;
        }

        EquipmentGrade beforeGrade = selectedEquipment.CurrentGrade;
        bool success = equipmentInventory.TryUpgrade(selectedEquipment);

        SetStatus(success
            ? $"Upgrade success: {beforeGrade} → {selectedEquipment.CurrentGrade}"
            : "Upgrade failed. Need same equipment / same grade x3.");

        RefreshUI();
    }

    public void RefreshUI()
    {
        ClearList();

        if (equipmentInventory == null)
        {
            SetSelectedText(null);
            SetCountText(0);
            return;
        }

        IReadOnlyList<OwnedEquipmentData> equipments = equipmentInventory.Equipments;
        SetCountText(equipments != null ? equipments.Count : 0);

        if (equipments != null)
        {
            for (int i = 0; i < equipments.Count; i++)
            {
                CreateItemButton(equipments[i], i);
            }
        }

        if (selectedEquipment != null && !ContainsEquipment(selectedEquipment))
        {
            selectedEquipment = null;
        }

        SetSelectedText(selectedEquipment);
    }

    private void CreateItemButton(OwnedEquipmentData equipment, int index)
    {
        if (equipment == null || listRoot == null || itemButtonPrefab == null)
        {
            return;
        }

        Button button = Instantiate(itemButtonPrefab, listRoot);
        button.gameObject.SetActive(true);
        spawnedButtons.Add(button);

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = BuildItemLabel(equipment, index);
        }

        button.onClick.AddListener(() =>
        {
            selectedEquipment = equipment;
            SetSelectedText(selectedEquipment);
            SetStatus($"Selected: {equipment.DisplayName} / {equipment.CurrentGrade}");
            RefreshUI();
        });
    }

    private string BuildItemLabel(OwnedEquipmentData equipment, int index)
    {
        string selectedMark = equipment == selectedEquipment ? "▶ " : string.Empty;
        string equipMark = equipment.IsEquipped ? " [Equipped]" : string.Empty;
        string lockMark = equipment.IsLocked ? " [Locked]" : string.Empty;

        return $"{selectedMark}{index + 1}. {equipment.DisplayName} / {equipment.CurrentGrade}{equipMark}{lockMark}";
    }

    private void ClearList()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();
    }

    private bool ContainsEquipment(OwnedEquipmentData equipment)
    {
        if (equipmentInventory == null || equipmentInventory.Equipments == null || equipment == null)
        {
            return false;
        }

        IReadOnlyList<OwnedEquipmentData> equipments = equipmentInventory.Equipments;
        for (int i = 0; i < equipments.Count; i++)
        {
            if (equipments[i] == equipment)
            {
                return true;
            }
        }

        return false;
    }

    private void SetSelectedText(OwnedEquipmentData equipment)
    {
        if (selectedText == null)
        {
            return;
        }

        selectedText.text = equipment != null
            ? $"Selected: {equipment.DisplayName} / {equipment.CurrentGrade}\nID: {equipment.InstanceId}"
            : "Selected: None";
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log($"[EquipmentUpgradeUI] {message}", this);
    }

    private void SetCountText(int count)
    {
        if (countText != null)
        {
            countText.text = $"Owned: {count}";
        }
    }
}