using System;
using UnityEngine;
using UnityEngine.UI;

public enum AutoScrollViewDirection
{
	Vertical,
	Horizontal
}

public enum AutoScrollViewLayoutMode
{
	Direction,
	Vertical,
	Horizontal,
	None
}

public enum AutoScrollViewContentSizeMode
{
	ManualByItems,
	ContentSizeFitter,
	None
}

public enum AutoScrollViewScrollbarMode
{
	None,
	DirectionOnly,
	Both
}

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class AutoScrollView : MonoBehaviour
{
	[Header("Generated References")]
	[SerializeField] private RectTransform scrollView;
	[SerializeField] private ScrollRect scrollRect;
	[SerializeField] private RectTransform viewport;
	[SerializeField] private RectTransform content;
	[SerializeField] private Scrollbar verticalScrollbar;
	[SerializeField] private Scrollbar horizontalScrollbar;

	[Header("Direction")]
	[SerializeField] private AutoScrollViewDirection direction = AutoScrollViewDirection.Vertical;

	[Header("View Size")]
	[SerializeField] private bool applyViewSize = true;
	[SerializeField] private Vector2 viewSize = new Vector2(300f, 300f);

	[Header("Item Size")]
	[SerializeField] private Vector2 itemSize = new Vector2(120f, 30f);
	[SerializeField] private bool stretchItemCrossAxis = true;

	[Header("Layout")]
	[SerializeField] private AutoScrollViewLayoutMode layoutMode = AutoScrollViewLayoutMode.Direction;
	[SerializeField] private AutoScrollViewContentSizeMode contentSizeMode = AutoScrollViewContentSizeMode.ManualByItems;
	[SerializeField] private float spacing = 8f;
	[SerializeField] private RectOffset padding;
	[SerializeField] private TextAnchor childAlignment = TextAnchor.UpperLeft;

	[Header("Scrollbar")]
	[SerializeField] private AutoScrollViewScrollbarMode scrollbarMode = AutoScrollViewScrollbarMode.None;
	[SerializeField] private float scrollbarSize = 20f;
	[SerializeField] private bool showScrollbarOnlyWhenOverflow = true;
	[SerializeField] private bool scrollbarTakesViewportSpace = false;

	[Header("Visual")]
	[SerializeField] private Color scrollViewBackgroundColor = new Color(1f, 1f, 1f, 0.15f);
	[SerializeField] private Color viewportMaskColor = Color.white;
	[SerializeField] private Color scrollbarBackgroundColor = new Color(1f, 1f, 1f, 0.25f);
	[SerializeField] private Color scrollbarHandleColor = new Color(1f, 1f, 1f, 0.75f);

	[Header("Editor")]
	[SerializeField] private bool configureInEditor = true;

	private const string ScrollViewName = "ScrollView";
	private const string ViewportName = "Viewport";
	private const string ContentName = "Content";
	private const string VerticalScrollbarName = "Scrollbar Vertical";
	private const string HorizontalScrollbarName = "Scrollbar Horizontal";
	private const string SlidingAreaName = "Sliding Area";
	private const string HandleName = "Handle";

	public RectTransform ScrollView => scrollView;
	public ScrollRect ScrollRect => scrollRect;
	public RectTransform Viewport => viewport;
	public RectTransform Content => content;
	public Vector2 ItemSize => itemSize;

	private void Reset()
	{
		EnsureDefaultValues();
		Configure();
	}

	private void Awake()
	{
		EnsureDefaultValues();
		Configure();
	}

	private void OnValidate()
	{
		EnsureDefaultValues();

		if (!configureInEditor)
		{
			return;
		}

		Configure();
	}

	[ContextMenu("Configure Auto Scroll View")]
	public void Configure()
	{
		EnsureDefaultValues();

		EnsureHierarchy();
		ApplyRootOptions();
		ApplyScrollViewOptions();
		ApplyViewportOptions(false, false);
		ApplyContentOptions();
		ApplyLayoutOptions();
		ApplyContentSizeOptions();
		ApplyScrollbarOptions();
		ApplyExistingItemLayouts();

		RebuildLayout();
		RefreshScrollbarVisibilityByOverflow();
	}

	public GameObject AddItem(GameObject prefab)
	{
		return AddItem(prefab, null);
	}

	public GameObject AddItem(GameObject prefab, Action<GameObject> onCreated)
	{
		Configure();

		if (prefab == null)
		{
			Debug.LogError("[AutoScrollView] Item prefab is null.", this);
			return null;
		}

		if (content == null)
		{
			Debug.LogError("[AutoScrollView] Content is null.", this);
			return null;
		}

		GameObject itemObject = Instantiate(prefab, content, false);
		itemObject.name = prefab.name;

		ResetItemRect(itemObject);
		ApplyItemLayout(itemObject);

		onCreated?.Invoke(itemObject);

		RebuildLayout();
		RefreshScrollbarVisibilityByOverflow();

		ResetScrollPositionToStart();

		return itemObject;
	}

	public void ClearItems()
	{
		Configure();

		if (content == null)
		{
			return;
		}

		for (int i = content.childCount - 1; i >= 0; i--)
		{
			DestroySafe(content.GetChild(i).gameObject);
		}

		RebuildLayout();
		RefreshScrollbarVisibilityByOverflow();
	}

	public void SetDirection(AutoScrollViewDirection value)
	{
		direction = value;
		Configure();
	}

	public void SetViewSize(Vector2 value)
	{
		viewSize = value;
		Configure();
	}

	public void SetItemSize(Vector2 value)
	{
		itemSize = value;
		Configure();
	}

	public void SetPadding(int left, int right, int top, int bottom)
	{
		padding = new RectOffset(left, right, top, bottom);
		Configure();
	}

	public void SetSpacing(float value)
	{
		spacing = value;
		Configure();
	}

	private void EnsureDefaultValues()
	{
		if (padding == null)
		{
			padding = new RectOffset(8, 8, 8, 8);
		}

		scrollbarSize = Mathf.Max(1f, scrollbarSize);
		itemSize.x = Mathf.Max(1f, itemSize.x);
		itemSize.y = Mathf.Max(1f, itemSize.y);
	}

	private void EnsureHierarchy()
	{
		EnsureScrollViewObject();
		EnsureViewportObject();
		EnsureContentObject();
		EnsureScrollbarObjectsIfNeeded();
	}

	private void EnsureScrollViewObject()
	{
		Transform found = transform.Find(ScrollViewName);

		if (found == null)
		{
			GameObject obj = new GameObject(ScrollViewName, typeof(RectTransform));
			obj.transform.SetParent(transform, false);
			found = obj.transform;
		}

		scrollView = found.GetComponent<RectTransform>();

		scrollRect = found.GetComponent<ScrollRect>();

		if (scrollRect == null)
		{
			scrollRect = found.gameObject.AddComponent<ScrollRect>();
		}

		Image image = found.GetComponent<Image>();

		if (image == null)
		{
			image = found.gameObject.AddComponent<Image>();
		}

		image.color = scrollViewBackgroundColor;
		image.raycastTarget = true;
	}

	private void EnsureViewportObject()
	{
		if (scrollView == null)
		{
			return;
		}

		Transform found = scrollView.Find(ViewportName);

		if (found == null)
		{
			GameObject obj = new GameObject(ViewportName, typeof(RectTransform));
			obj.transform.SetParent(scrollView, false);
			found = obj.transform;
		}

		viewport = found.GetComponent<RectTransform>();

		Image image = found.GetComponent<Image>();

		if (image == null)
		{
			image = found.gameObject.AddComponent<Image>();
		}

		image.color = viewportMaskColor;
		image.raycastTarget = true;

		Mask mask = found.GetComponent<Mask>();

		if (mask == null)
		{
			mask = found.gameObject.AddComponent<Mask>();
		}

		mask.showMaskGraphic = false;
	}

	private void EnsureContentObject()
	{
		if (viewport == null)
		{
			return;
		}

		Transform found = viewport.Find(ContentName);

		if (found == null)
		{
			GameObject obj = new GameObject(ContentName, typeof(RectTransform));
			obj.transform.SetParent(viewport, false);
			found = obj.transform;
		}

		content = found.GetComponent<RectTransform>();
	}

	private void EnsureScrollbarObjectsIfNeeded()
	{
		bool needVertical =
			scrollbarMode == AutoScrollViewScrollbarMode.Both ||
			scrollbarMode == AutoScrollViewScrollbarMode.DirectionOnly && direction == AutoScrollViewDirection.Vertical;

		bool needHorizontal =
			scrollbarMode == AutoScrollViewScrollbarMode.Both ||
			scrollbarMode == AutoScrollViewScrollbarMode.DirectionOnly && direction == AutoScrollViewDirection.Horizontal;

		if (needVertical)
		{
			verticalScrollbar = EnsureVerticalScrollbar();
		}
		else
		{
			verticalScrollbar = FindScrollbar(VerticalScrollbarName);
		}

		if (needHorizontal)
		{
			horizontalScrollbar = EnsureHorizontalScrollbar();
		}
		else
		{
			horizontalScrollbar = FindScrollbar(HorizontalScrollbarName);
		}
	}

	private Scrollbar FindScrollbar(string objectName)
	{
		if (scrollView == null)
		{
			return null;
		}

		Transform found = scrollView.Find(objectName);

		if (found == null)
		{
			return null;
		}

		return found.GetComponent<Scrollbar>();
	}

	private Scrollbar EnsureVerticalScrollbar()
	{
		Transform found = scrollView.Find(VerticalScrollbarName);

		if (found == null)
		{
			found = CreateScrollbarObject(VerticalScrollbarName, true).transform;
		}

		return ConfigureScrollbar(found.gameObject, true);
	}

	private Scrollbar EnsureHorizontalScrollbar()
	{
		Transform found = scrollView.Find(HorizontalScrollbarName);

		if (found == null)
		{
			found = CreateScrollbarObject(HorizontalScrollbarName, false).transform;
		}

		return ConfigureScrollbar(found.gameObject, false);
	}

	private GameObject CreateScrollbarObject(string objectName, bool vertical)
	{
		GameObject scrollbarObject = new GameObject(objectName, typeof(RectTransform));
		scrollbarObject.transform.SetParent(scrollView, false);

		GameObject slidingArea = new GameObject(SlidingAreaName, typeof(RectTransform));
		slidingArea.transform.SetParent(scrollbarObject.transform, false);

		GameObject handle = new GameObject(HandleName, typeof(RectTransform));
		handle.transform.SetParent(slidingArea.transform, false);

		Image backgroundImage = scrollbarObject.AddComponent<Image>();
		backgroundImage.color = scrollbarBackgroundColor;
		backgroundImage.raycastTarget = true;

		Image handleImage = handle.AddComponent<Image>();
		handleImage.color = scrollbarHandleColor;
		handleImage.raycastTarget = true;

		Scrollbar scrollbar = scrollbarObject.AddComponent<Scrollbar>();
		scrollbar.handleRect = handle.GetComponent<RectTransform>();
		scrollbar.direction = vertical
			? Scrollbar.Direction.BottomToTop
			: Scrollbar.Direction.LeftToRight;

		return scrollbarObject;
	}

	private Scrollbar ConfigureScrollbar(GameObject scrollbarObject, bool vertical)
	{
		Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();

		if (scrollbar == null)
		{
			scrollbar = scrollbarObject.AddComponent<Scrollbar>();
		}

		Image backgroundImage = scrollbarObject.GetComponent<Image>();

		if (backgroundImage == null)
		{
			backgroundImage = scrollbarObject.AddComponent<Image>();
		}

		backgroundImage.color = scrollbarBackgroundColor;
		backgroundImage.raycastTarget = true;

		RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();

		if (vertical)
		{
			scrollbarRect.anchorMin = new Vector2(1f, 0f);
			scrollbarRect.anchorMax = new Vector2(1f, 1f);
			scrollbarRect.pivot = new Vector2(1f, 1f);
			scrollbarRect.offsetMin = new Vector2(-scrollbarSize, 0f);
			scrollbarRect.offsetMax = Vector2.zero;
			scrollbar.direction = Scrollbar.Direction.BottomToTop;
		}
		else
		{
			scrollbarRect.anchorMin = new Vector2(0f, 0f);
			scrollbarRect.anchorMax = new Vector2(1f, 0f);
			scrollbarRect.pivot = new Vector2(0f, 0f);
			scrollbarRect.offsetMin = Vector2.zero;
			scrollbarRect.offsetMax = new Vector2(0f, scrollbarSize);
			scrollbar.direction = Scrollbar.Direction.LeftToRight;
		}

		Transform slidingArea = scrollbarObject.transform.Find(SlidingAreaName);

		if (slidingArea == null)
		{
			GameObject obj = new GameObject(SlidingAreaName, typeof(RectTransform));
			obj.transform.SetParent(scrollbarObject.transform, false);
			slidingArea = obj.transform;
		}

		RectTransform slidingRect = slidingArea.GetComponent<RectTransform>();
		slidingRect.anchorMin = Vector2.zero;
		slidingRect.anchorMax = Vector2.one;
		slidingRect.offsetMin = Vector2.zero;
		slidingRect.offsetMax = Vector2.zero;

		Transform handle = slidingArea.Find(HandleName);

		if (handle == null)
		{
			GameObject obj = new GameObject(HandleName, typeof(RectTransform));
			obj.transform.SetParent(slidingArea, false);
			handle = obj.transform;
		}

		RectTransform handleRect = handle.GetComponent<RectTransform>();
		handleRect.anchorMin = Vector2.zero;
		handleRect.anchorMax = Vector2.one;
		handleRect.offsetMin = Vector2.zero;
		handleRect.offsetMax = Vector2.zero;

		Image handleImage = handle.GetComponent<Image>();

		if (handleImage == null)
		{
			handleImage = handle.gameObject.AddComponent<Image>();
		}

		handleImage.color = scrollbarHandleColor;
		handleImage.raycastTarget = true;

		scrollbar.handleRect = handleRect;

		return scrollbar;
	}

	private void ApplyRootOptions()
	{
		RectTransform root = GetComponent<RectTransform>();

		if (root == null)
		{
			return;
		}

		if (applyViewSize)
		{
			root.sizeDelta = viewSize;
		}
	}

	private void ApplyScrollViewOptions()
	{
		if (scrollView == null || scrollRect == null)
		{
			return;
		}

		scrollView.anchorMin = Vector2.zero;
		scrollView.anchorMax = Vector2.one;
		scrollView.pivot = new Vector2(0.5f, 0.5f);
		scrollView.offsetMin = Vector2.zero;
		scrollView.offsetMax = Vector2.zero;

		scrollRect.viewport = viewport;
		scrollRect.content = content;

		scrollRect.horizontal = direction == AutoScrollViewDirection.Horizontal;
		scrollRect.vertical = direction == AutoScrollViewDirection.Vertical;

		scrollRect.movementType = ScrollRect.MovementType.Elastic;
		scrollRect.inertia = true;
		scrollRect.scrollSensitivity = 1f;

		scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
		scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
	}

	private void ApplyViewportOptions(bool verticalScrollbarVisible, bool horizontalScrollbarVisible)
	{
		if (viewport == null)
		{
			return;
		}

		float right = scrollbarTakesViewportSpace && verticalScrollbarVisible ? -scrollbarSize : 0f;
		float bottom = scrollbarTakesViewportSpace && horizontalScrollbarVisible ? scrollbarSize : 0f;

		viewport.anchorMin = Vector2.zero;
		viewport.anchorMax = Vector2.one;
		viewport.pivot = new Vector2(0.5f, 0.5f);
		viewport.offsetMin = new Vector2(0f, bottom);
		viewport.offsetMax = new Vector2(right, 0f);
	}

	private void ApplyContentOptions()
	{
		if (content == null)
		{
			return;
		}

		if (direction == AutoScrollViewDirection.Vertical)
		{
			content.anchorMin = new Vector2(0f, 1f);
			content.anchorMax = new Vector2(1f, 1f);
			content.pivot = new Vector2(0.5f, 1f);
			content.anchoredPosition = Vector2.zero;
		}
		else
		{
			content.anchorMin = new Vector2(0f, 0f);
			content.anchorMax = new Vector2(0f, 1f);
			content.pivot = new Vector2(0f, 0.5f);
			content.anchoredPosition = Vector2.zero;
		}
	}

	private void ApplyLayoutOptions()
	{
		if (content == null)
		{
			return;
		}

		AutoScrollViewLayoutMode resolvedLayout = GetResolvedLayoutMode();

		if (resolvedLayout == AutoScrollViewLayoutMode.None)
		{
			RemoveComponent<VerticalLayoutGroup>(content.gameObject);
			RemoveComponent<HorizontalLayoutGroup>(content.gameObject);
			RemoveComponent<GridLayoutGroup>(content.gameObject);
			return;
		}

		if (resolvedLayout == AutoScrollViewLayoutMode.Vertical)
		{
			RemoveComponent<HorizontalLayoutGroup>(content.gameObject);
			RemoveComponent<GridLayoutGroup>(content.gameObject);

			VerticalLayoutGroup layout = EnsureComponent<VerticalLayoutGroup>(content.gameObject);
			layout.padding = CopyPadding();
			layout.spacing = spacing;
			layout.childAlignment = childAlignment;
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandWidth = stretchItemCrossAxis;
			layout.childForceExpandHeight = false;
			layout.childScaleWidth = false;
			layout.childScaleHeight = false;
			return;
		}

		if (resolvedLayout == AutoScrollViewLayoutMode.Horizontal)
		{
			RemoveComponent<VerticalLayoutGroup>(content.gameObject);
			RemoveComponent<GridLayoutGroup>(content.gameObject);

			HorizontalLayoutGroup layout = EnsureComponent<HorizontalLayoutGroup>(content.gameObject);
			layout.padding = CopyPadding();
			layout.spacing = spacing;
			layout.childAlignment = childAlignment;
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = stretchItemCrossAxis;
			layout.childScaleWidth = false;
			layout.childScaleHeight = false;
		}
	}

	private void ApplyContentSizeOptions()
	{
		if (content == null)
		{
			return;
		}

		if (contentSizeMode == AutoScrollViewContentSizeMode.None ||
			contentSizeMode == AutoScrollViewContentSizeMode.ManualByItems)
		{
			RemoveComponent<ContentSizeFitter>(content.gameObject);
			RefreshContentSizeByItems();
			return;
		}

		ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(content.gameObject);

		if (direction == AutoScrollViewDirection.Vertical)
		{
			fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}
		else
		{
			fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
		}
	}

	private void ApplyScrollbarOptions()
	{
		if (scrollRect == null)
		{
			return;
		}

		bool verticalAllowed =
			scrollbarMode == AutoScrollViewScrollbarMode.Both ||
			scrollbarMode == AutoScrollViewScrollbarMode.DirectionOnly && direction == AutoScrollViewDirection.Vertical;

		bool horizontalAllowed =
			scrollbarMode == AutoScrollViewScrollbarMode.Both ||
			scrollbarMode == AutoScrollViewScrollbarMode.DirectionOnly && direction == AutoScrollViewDirection.Horizontal;

		if (verticalAllowed)
		{
			if (verticalScrollbar == null)
			{
				verticalScrollbar = EnsureVerticalScrollbar();
			}

			scrollRect.verticalScrollbar = verticalScrollbar;
		}
		else
		{
			scrollRect.verticalScrollbar = null;
		}

		if (horizontalAllowed)
		{
			if (horizontalScrollbar == null)
			{
				horizontalScrollbar = EnsureHorizontalScrollbar();
			}

			scrollRect.horizontalScrollbar = horizontalScrollbar;
		}
		else
		{
			scrollRect.horizontalScrollbar = null;
		}

		if (verticalScrollbar != null)
		{
			verticalScrollbar.gameObject.SetActive(verticalAllowed && !showScrollbarOnlyWhenOverflow);
		}

		if (horizontalScrollbar != null)
		{
			horizontalScrollbar.gameObject.SetActive(horizontalAllowed && !showScrollbarOnlyWhenOverflow);
		}
	}

	private void RefreshScrollbarVisibilityByOverflow()
	{
		if (scrollRect == null || viewport == null || content == null)
		{
			return;
		}

		Canvas.ForceUpdateCanvases();

		bool verticalAllowed =
			scrollbarMode == AutoScrollViewScrollbarMode.Both ||
			scrollbarMode == AutoScrollViewScrollbarMode.DirectionOnly && direction == AutoScrollViewDirection.Vertical;

		bool horizontalAllowed =
			scrollbarMode == AutoScrollViewScrollbarMode.Both ||
			scrollbarMode == AutoScrollViewScrollbarMode.DirectionOnly && direction == AutoScrollViewDirection.Horizontal;

		bool verticalOverflow = content.rect.height > viewport.rect.height + 0.5f;
		bool horizontalOverflow = content.rect.width > viewport.rect.width + 0.5f;

		bool showVertical = verticalAllowed && (!showScrollbarOnlyWhenOverflow || verticalOverflow);
		bool showHorizontal = horizontalAllowed && (!showScrollbarOnlyWhenOverflow || horizontalOverflow);

		if (verticalScrollbar != null)
		{
			verticalScrollbar.gameObject.SetActive(showVertical);
		}

		if (horizontalScrollbar != null)
		{
			horizontalScrollbar.gameObject.SetActive(showHorizontal);
		}

		ApplyViewportOptions(showVertical, showHorizontal);

		if (scrollbarTakesViewportSpace)
		{
			Canvas.ForceUpdateCanvases();
			RefreshContentSizeByItems();

			verticalOverflow = content.rect.height > viewport.rect.height + 0.5f;
			horizontalOverflow = content.rect.width > viewport.rect.width + 0.5f;

			showVertical = verticalAllowed && (!showScrollbarOnlyWhenOverflow || verticalOverflow);
			showHorizontal = horizontalAllowed && (!showScrollbarOnlyWhenOverflow || horizontalOverflow);

			if (verticalScrollbar != null)
			{
				verticalScrollbar.gameObject.SetActive(showVertical);
			}

			if (horizontalScrollbar != null)
			{
				horizontalScrollbar.gameObject.SetActive(showHorizontal);
			}

			ApplyViewportOptions(showVertical, showHorizontal);
		}

		scrollRect.verticalScrollbar = showVertical ? verticalScrollbar : null;
		scrollRect.horizontalScrollbar = showHorizontal ? horizontalScrollbar : null;
	}

	private void ApplyExistingItemLayouts()
	{
		if (content == null)
		{
			return;
		}

		for (int i = 0; i < content.childCount; i++)
		{
			GameObject child = content.GetChild(i).gameObject;
			ResetItemRect(child);
			ApplyItemLayout(child);
		}
	}

	private void ApplyItemLayout(GameObject itemObject)
	{
		if (itemObject == null)
		{
			return;
		}

		LayoutElement layout = EnsureComponent<LayoutElement>(itemObject);
		layout.ignoreLayout = false;

		if (direction == AutoScrollViewDirection.Vertical)
		{
			layout.minWidth = itemSize.x;
			layout.preferredWidth = itemSize.x;
			layout.flexibleWidth = stretchItemCrossAxis ? 1f : 0f;

			layout.minHeight = itemSize.y;
			layout.preferredHeight = itemSize.y;
			layout.flexibleHeight = 0f;
		}
		else
		{
			layout.minWidth = itemSize.x;
			layout.preferredWidth = itemSize.x;
			layout.flexibleWidth = 0f;

			layout.minHeight = itemSize.y;
			layout.preferredHeight = itemSize.y;
			layout.flexibleHeight = stretchItemCrossAxis ? 1f : 0f;
		}
	}

	private void ResetItemRect(GameObject itemObject)
	{
		if (itemObject == null)
		{
			return;
		}

		RectTransform rect = itemObject.GetComponent<RectTransform>();

		if (rect == null)
		{
			rect = itemObject.AddComponent<RectTransform>();
		}

		rect.localScale = Vector3.one;
		rect.localRotation = Quaternion.identity;

		if (direction == AutoScrollViewDirection.Vertical)
		{
			rect.anchorMin = new Vector2(0f, 1f);
			rect.anchorMax = new Vector2(1f, 1f);
			rect.pivot = new Vector2(0.5f, 1f);
		}
		else
		{
			rect.anchorMin = new Vector2(0f, 0f);
			rect.anchorMax = new Vector2(0f, 1f);
			rect.pivot = new Vector2(0f, 0.5f);
		}

		rect.anchoredPosition = Vector2.zero;
	}

	private void RefreshContentSizeByItems()
	{
		if (content == null || viewport == null)
		{
			return;
		}

		if (contentSizeMode != AutoScrollViewContentSizeMode.ManualByItems)
		{
			return;
		}

		int itemCount = 0;

		for (int i = 0; i < content.childCount; i++)
		{
			LayoutElement layout = content.GetChild(i).GetComponent<LayoutElement>();

			if (layout != null && layout.ignoreLayout)
			{
				continue;
			}

			itemCount++;
		}

		float safeSpacing = itemCount > 1 ? spacing * (itemCount - 1) : 0f;

		if (direction == AutoScrollViewDirection.Vertical)
		{
			float height = padding.top + padding.bottom + itemSize.y * itemCount + safeSpacing;
			height = Mathf.Max(height, viewport.rect.height);

			content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewport.rect.width);
			content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		}
		else
		{
			float width = padding.left + padding.right + itemSize.x * itemCount + safeSpacing;
			width = Mathf.Max(width, viewport.rect.width);

			content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, viewport.rect.height);
		}
	}

	private AutoScrollViewLayoutMode GetResolvedLayoutMode()
	{
		if (layoutMode != AutoScrollViewLayoutMode.Direction)
		{
			return layoutMode;
		}

		return direction == AutoScrollViewDirection.Vertical
			? AutoScrollViewLayoutMode.Vertical
			: AutoScrollViewLayoutMode.Horizontal;
	}

	private void ResetScrollPositionToStart()
	{
		if (scrollRect == null)
		{
			return;
		}

		if (direction == AutoScrollViewDirection.Vertical)
		{
			scrollRect.verticalNormalizedPosition = 1f;
		}
		else
		{
			scrollRect.horizontalNormalizedPosition = 0f;
		}
	}

	private T EnsureComponent<T>(GameObject target) where T : Component
	{
		if (target == null)
		{
			return null;
		}

		T component = target.GetComponent<T>();

		if (component == null)
		{
			component = target.AddComponent<T>();
		}

		return component;
	}

	private void RemoveComponent<T>(GameObject target) where T : Component
	{
		if (target == null)
		{
			return;
		}

		T component = target.GetComponent<T>();

		if (component == null)
		{
			return;
		}

		DestroySafe(component);
	}

	private RectOffset CopyPadding()
	{
		EnsureDefaultValues();

		return new RectOffset(
			padding.left,
			padding.right,
			padding.top,
			padding.bottom
		);
	}

	private void RebuildLayout()
	{
		Canvas.ForceUpdateCanvases();

		if (content != null)
		{
			if (contentSizeMode == AutoScrollViewContentSizeMode.ManualByItems)
			{
				RefreshContentSizeByItems();
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(content);
		}

		if (viewport != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
		}

		if (scrollView != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(scrollView);
		}

		Canvas.ForceUpdateCanvases();
	}

	private void DestroySafe(UnityEngine.Object target)
	{
		if (target == null)
		{
			return;
		}

		if (Application.isPlaying)
		{
			Destroy(target);
		}
		else
		{
			DestroyImmediate(target);
		}
	}
}