using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Shrine;
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(UIHoverScaler))]
[AutoBindPrefix("UI")]
public class ShrineGodCard : UIComponent,
	IPointerClickHandler,
	IPointerEnterHandler,
	IPointerExitHandler
{
	[Header("UI")]
	[AutoBind][SerializeField] private Image thumbButton;
	[AutoBind][SerializeField] private TMP_Text nameText;
	[AutoBind][SerializeField] private GameObject highlightObj;

	[Header("Tooltip")]
	[AutoBind][SerializeField] private GameObject tooltipRoot;
	[AutoBind][SerializeField] private TMP_Text tooltipText;

	[Header("Show Animation")]
	[SerializeField] private Vector2 showStartOffset = new(0f, 90f);
	[SerializeField] private float showDuration = 0.45f;

	[Header("Select Animation")]
	[SerializeField] private Vector2 selectedOffset = new(0f, 45f);
	[SerializeField] private Vector2 deselectedOffset = new(0f, -45f);
	[SerializeField] private float selectedDuration = 0.35f;
	[SerializeField] private float deselectedDuration = 0.25f;

	[Header("Fade")]
	[SerializeField] private float showFadeDuration = 0.45f;
	[SerializeField] private float deselectFadeDuration = 0.25f;

	public CanvasGroup CanvasGrp { get; private set; }

	private UIAnimationSequence sequence;
	private UIFlyAnimation flyAnim;
	private UIFadeAnimation fadeAnim;

	private Action<ShrineGodCard> onClick;

	private ShrineGodType godType;
	private string displayName;
	private string description;
	private Sprite godIcon;

	private RectTransform rectTransform;
	private Vector2 basePosition;

	protected void Awake()
	{
		CanvasGrp = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();

		sequence = gameObject.GetComponent<UIAnimationSequence>();
		if (sequence == null) sequence = gameObject.AddComponent<UIAnimationSequence>();

		flyAnim = gameObject.AddComponent<UIFlyAnimation>();
		fadeAnim = gameObject.AddComponent<UIFadeAnimation>();

		Button btn = GetComponent<Button>(); if (btn != null) { btn.onClick.AddListener(() => { if (CanvasGrp != null && !CanvasGrp.interactable) return; onClick?.Invoke(this); }); }
		SetSelected(false);
		SetTooltipVisible(false);
	}

	public void Bind(
		ShrineGodType godType,
		ShrineGodSO godSo,
		Action<ShrineGodCard> clickAction)
	{
		string godName = godSo != null
			? godSo.DisplayName
			: godType.ToString();

		string godDescription = godSo != null
			? godSo.Description
			: string.Empty;

		Sprite iconSprite = godSo != null
			? godSo.Icon
			: null;

		Bind(
			godType,
			godName,
			godDescription,
			iconSprite,
			clickAction);
	}

	private string GetGodIllustFromType(ShrineGodType type)
	{
		switch (type)
		{
		case ShrineGodType.Life:
			return "Images/Illust/LifeGod|Life_0";
		case ShrineGodType.War:
			return "Images/Illust/WarGod|WarGod (2)_0";
		case ShrineGodType.Dark:
			return "Images/Illust/DeathGod|DeathGod_0";
		default:
			return string.Empty;
		}
	}

	public void Bind(
		ShrineGodType godType,
		string godName,
		string godDescription,
		Sprite iconSprite,
		Action<ShrineGodCard> clickAction)
	{
		this.godType = godType;
		displayName = godName;
		description = godDescription;
		godIcon = iconSprite;

		// SO에서 이미지가 없었다면 수동으로 매칭된 이미지 로드 (Resources 폴더 기준)
		//if (godIcon == null)
		{
			string illustData = GetGodIllustFromType(godType);
			if (!string.IsNullOrEmpty(illustData))
			{
				string[] parts = illustData.Split('|');
				if (parts.Length == 2)
				{
					string texPath = parts[0];
					string spriteName = parts[1];

					// 멀티 스프라이트이므로 텍스처를 LoadAll 하고 이름으로 찾음
					Sprite[] sprites = Resources.LoadAll<Sprite>(texPath);
					foreach (Sprite s in sprites)
					{
						if (s.name == spriteName)
						{
							godIcon = s;
							break;
						}
					}
				}
				else
				{
					godIcon = Resources.Load<Sprite>(illustData);
				}
			}
		}

		onClick = clickAction;

		Refresh();
		Button btn = GetComponent<Button>(); if (btn != null) { btn.onClick.AddListener(() => { if (CanvasGrp != null && !CanvasGrp.interactable) return; onClick?.Invoke(this); }); }
		SetSelected(false);
		SetTooltipVisible(false);
	}

	public void Refresh()
	{
		if (nameText != null)
		{
			nameText.text = displayName;
		}

		if (thumbButton != null)
		{
			thumbButton.sprite = godIcon;
			thumbButton.enabled = godIcon != null;
		}

		if (tooltipText != null)
		{
			tooltipText.text = BuildTooltipText();
		}
	}

	public void HideImmediate()
	{
		CacheBasePosition();

		if (CanvasGrp != null)
		{
			CanvasGrp.alpha = 0f;
			CanvasGrp.interactable = false;
			CanvasGrp.blocksRaycasts = false;
		}

		if (rectTransform != null)
		{
			rectTransform.anchoredPosition = basePosition + showStartOffset;
		}

		SetTooltipVisible(false);
		Button btn = GetComponent<Button>(); if (btn != null) { btn.onClick.AddListener(() => { if (CanvasGrp != null && !CanvasGrp.interactable) return; onClick?.Invoke(this); }); }
		SetSelected(false);
	}

	public void PlayShow()
	{
		CacheBasePosition();

		if (CanvasGrp != null)
		{
			CanvasGrp.alpha = 0f;
			CanvasGrp.interactable = false;
			CanvasGrp.blocksRaycasts = false;
		}

		if (rectTransform != null)
		{
			Vector2 start = basePosition + showStartOffset;
			Vector2 target = basePosition;
			rectTransform.anchoredPosition = start;

			if (sequence != null)
			{
				sequence.Clear();

				flyAnim.targetRect = rectTransform;
				flyAnim.fromPosition = start;
				flyAnim.toPosition = target;
				flyAnim.duration = showDuration;
				flyAnim.delay = 0f;
				sequence.Append(flyAnim);

				if (CanvasGrp != null)
				{
					fadeAnim.targetGroup = CanvasGrp;
					fadeAnim.fromAlpha = 0f;
					fadeAnim.toAlpha = 1f;
					fadeAnim.duration = showFadeDuration;
					fadeAnim.delay = 0f;
					sequence.Join(fadeAnim);
				}

				sequence.Play(() => EnableInteraction());
			}
		}
		else
		{
			Invoke(nameof(EnableInteraction), showDuration);
		}
	}

	public void PlayFocus(Vector2 targetPosition)
	{
		SetSelected(true);
		SetTooltipVisible(false);

		if (CanvasGrp != null)
		{
			CanvasGrp.interactable = false;
			CanvasGrp.blocksRaycasts = false;
		}

		if (rectTransform != null && sequence != null)
		{
			Vector2 start = rectTransform.anchoredPosition;
			sequence.Clear();

			flyAnim.targetRect = rectTransform;
			flyAnim.fromPosition = start;
			flyAnim.toPosition = targetPosition;
			flyAnim.duration = selectedDuration;
			flyAnim.delay = 0f;
			sequence.Append(flyAnim);

			sequence.Play();
		}
	}

	public void PlayRestore()
	{
		SetSelected(false);

		if (sequence != null)
		{
			sequence.Clear();
			if (rectTransform != null)
			{
				flyAnim.targetRect = rectTransform;
				flyAnim.fromPosition = rectTransform.anchoredPosition;
				flyAnim.toPosition = basePosition;
				flyAnim.duration = selectedDuration;
				flyAnim.delay = 0f;
				sequence.Append(flyAnim);
			}

			if (CanvasGrp != null)
			{
				fadeAnim.targetGroup = CanvasGrp;
				fadeAnim.fromAlpha = CanvasGrp.alpha;
				fadeAnim.toAlpha = 1f;
				fadeAnim.duration = showFadeDuration;
				fadeAnim.delay = 0f;

				if (rectTransform != null) sequence.Join(fadeAnim);
				else sequence.Append(fadeAnim);
			}

			sequence.Play(() => EnableInteraction());
		}
	}

	public void PlayDeselected()
	{
		Button btn = GetComponent<Button>(); if (btn != null) { btn.onClick.AddListener(() => { if (CanvasGrp != null && !CanvasGrp.interactable) return; onClick?.Invoke(this); }); }
		SetSelected(false);
		SetTooltipVisible(false);

		if (CanvasGrp != null)
		{
			CanvasGrp.interactable = false;
			CanvasGrp.blocksRaycasts = false;
		}

		if (sequence != null)
		{
			sequence.Clear();
			if (rectTransform != null)
			{
				Vector2 start = rectTransform.anchoredPosition;
				Vector2 target = basePosition + deselectedOffset;

				flyAnim.targetRect = rectTransform;
				flyAnim.fromPosition = start;
				flyAnim.toPosition = target;
				flyAnim.duration = deselectedDuration;
				flyAnim.delay = 0f;
				sequence.Append(flyAnim);
			}

			if (CanvasGrp != null)
			{
				fadeAnim.targetGroup = CanvasGrp;
				fadeAnim.fromAlpha = CanvasGrp.alpha;
				fadeAnim.toAlpha = 0f;
				fadeAnim.duration = deselectFadeDuration;
				fadeAnim.delay = 0f;

				if (rectTransform != null) sequence.Join(fadeAnim);
				else sequence.Append(fadeAnim);
			}

			sequence.Play();
		}
	}

	public void SetSelected(bool selected)
	{
		if (highlightObj != null)
		{
			highlightObj.SetActive(selected);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log($"[ShrineGodCard] OnPointerClick triggered on {gameObject.name}");
		if (CanvasGrp != null && !CanvasGrp.interactable)
		{
			Debug.Log($"[ShrineGodCard] Click ignored because CanvasGrp is not interactable!");
			return;
		}

		if (onClick == null)
		{
			Debug.Log($"[ShrineGodCard] onClick action is null!");
		}
		else
		{
			Debug.Log($"[ShrineGodCard] Invoking onClick action!");
			onClick.Invoke(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (CanvasGrp != null && !CanvasGrp.interactable)
		{
			return;
		}

		SetTooltipVisible(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetTooltipVisible(false);
	}

	private void EnableInteraction()
	{
		Debug.Log($"[ShrineGodCard] EnableInteraction called on {gameObject.name}");
		if (CanvasGrp == null)
		{
			return;
		}

		CanvasGrp.alpha = 1f;
		CanvasGrp.interactable = true;
		CanvasGrp.blocksRaycasts = true;
	}

	private void CacheBasePosition()
	{
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}

		if (rectTransform != null)
		{
			basePosition = rectTransform.anchoredPosition;
		}
	}

	private string BuildTooltipText()
	{
		if (string.IsNullOrWhiteSpace(description))
		{
			return displayName;
		}

		return $"{displayName}\n{description}";
	}

	private void SetTooltipVisible(bool visible)
	{
		if (tooltipRoot != null)
		{
			tooltipRoot.SetActive(visible);
		}
	}
}