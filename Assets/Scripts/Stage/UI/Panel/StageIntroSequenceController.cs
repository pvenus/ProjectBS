using System;
using System.Collections;
using Session;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Stage.UI
{
	/// <summary>
	/// Owns the first-entry presentation for StageScene and releases the node map
	/// only after the player confirms the intro.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class StageIntroSequenceController : MonoBehaviour
	{
		[Header("Hierarchy Names")]
		[SerializeField] private string startAsNewButtonName = "(btn)StartAsNew";
		[SerializeField] private string startAsSaveButtonName = "(btn)StartAsSave";

		[Header("Initial Fade In")]
		[SerializeField, Min(0f)] private float initialBlackHoldDuration = 0.1f;
		[FormerlySerializedAs("blackFadeDuration")]
		[SerializeField, Min(0f)] private float introFadeInDuration = 0.65f;

		[Header("Start Click Fade Out")]
		[FormerlySerializedAs("mapRevealDuration")]
		[SerializeField, Min(0f)] private float introFadeOutDuration = 0.8f;

		private static bool introCompletedThisPlaySession;

		private CanvasGroup introGroup;
		private Image blackOverlay;
		private Button startAsNewButton;
		private Button startAsSaveButton;
		private bool transitionStarted;

		public static event Action IntroCompleted;
		public bool IsTransitioning => transitionStarted;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetPlaySessionState()
		{
			introCompletedThisPlaySession = false;
			IntroCompleted = null;
		}

		private static bool IsIntroAlreadyCompleted()
		{
			if (introCompletedThisPlaySession)
			{
				return true;
			}

			if (GameSession.Instance != null &&
			    GameSession.Instance.StageSession != null &&
			    GameSession.Instance.StageSession.isIntroCompleted)
			{
				return true;
			}

			return false;
		}

		private void Awake()
		{
			if (IsIntroAlreadyCompleted())
			{
				gameObject.SetActive(false);
				return;
			}

			introGroup = GetComponent<CanvasGroup>();
			if (introGroup == null)
			{
				introGroup = gameObject.AddComponent<CanvasGroup>();
			}

			if (introGroup == null)
			{
				Debug.LogError(
					"[StageIntroSequenceController] Failed to prepare the intro CanvasGroup.",
					this);
				enabled = false;
				return;
			}

			introGroup.alpha = 0f;
			introGroup.interactable = false;
			introGroup.blocksRaycasts = true;

			startAsNewButton = PrepareButton(startAsNewButtonName);
			startAsSaveButton = PrepareButton(startAsSaveButtonName);
			SetStartButtonsInteractable(false);

			blackOverlay = CreateBlackOverlay();
		}

		private IEnumerator Start()
		{
			if (IsIntroAlreadyCompleted())
			{
				ShowMapImmediately();
				yield break;
			}

			// Give all StageScene objects one frame to finish Awake and prefab activation.
			yield return null;

			if (IsIntroAlreadyCompleted())
			{
				ShowMapImmediately();
				yield break;
			}

			yield return PlayInitialFadeIn();

			introGroup.interactable = true;
			introGroup.blocksRaycasts = true;
			SetStartButtonsInteractable(true);
		}

		private void OnDestroy()
		{
			RemoveButtonListener(startAsNewButton);
			RemoveButtonListener(startAsSaveButton);
		}

		public void BeginStartTransition()
		{
			if (transitionStarted)
			{
				return;
			}

			transitionStarted = true;
			SetStartButtonsInteractable(false);
			introGroup.interactable = false;
			StartCoroutine(PlayIntroFadeOut());
		}

		private IEnumerator PlayInitialFadeIn()
		{
			if (initialBlackHoldDuration > 0f)
			{
				yield return new WaitForSecondsRealtime(initialBlackHoldDuration);
			}

			if (blackOverlay == null)
			{
				introGroup.alpha = 1f;
				yield break;
			}

			if (introFadeInDuration <= 0f)
			{
				introGroup.alpha = 1f;
				Destroy(blackOverlay.gameObject);
				blackOverlay = null;
				yield break;
			}

			float elapsed = 0f;
			while (elapsed < introFadeInDuration)
			{
				elapsed += Time.unscaledDeltaTime;
				float progress = Mathf.Clamp01(elapsed / introFadeInDuration);
				float fade = Smooth(progress);
				introGroup.alpha = fade;
				blackOverlay.color = new Color(0f, 0f, 0f, 1f - fade);
				yield return null;
			}

			introGroup.alpha = 1f;
			Destroy(blackOverlay.gameObject);
			blackOverlay = null;
		}

		private IEnumerator PlayIntroFadeOut()
		{
			if (introFadeOutDuration <= 0f)
			{
				CompleteTransition();
				yield break;
			}

			float elapsed = 0f;
			while (elapsed < introFadeOutDuration)
			{
				elapsed += Time.unscaledDeltaTime;
				float introProgress = Mathf.Clamp01(
					elapsed / introFadeOutDuration);
				introGroup.alpha = 1f - Smooth(introProgress);
				yield return null;
			}

			CompleteTransition();
		}

		private void CompleteTransition()
		{
			if (introGroup != null)
			{
				introGroup.alpha = 0f;
				introGroup.interactable = false;
				introGroup.blocksRaycasts = false;
			}

			introCompletedThisPlaySession = true;
			if (GameSession.Instance != null && GameSession.Instance.StageSession != null)
			{
				GameSession.Instance.StageSession.isIntroCompleted = true;
			}

			IntroCompleted?.Invoke();
			gameObject.SetActive(false);
		}

		private void ShowMapImmediately()
		{
			if (blackOverlay != null)
			{
				Destroy(blackOverlay.gameObject);
				blackOverlay = null;
			}

			if (introGroup != null)
			{
				introGroup.alpha = 0f;
				introGroup.blocksRaycasts = false;
			}

			introCompletedThisPlaySession = true;
			if (GameSession.Instance != null && GameSession.Instance.StageSession != null)
			{
				GameSession.Instance.StageSession.isIntroCompleted = true;
			}

			gameObject.SetActive(false);
		}

		private Button PrepareButton(string objectName)
		{
			Transform target = FindDescendant(transform, objectName);
			if (target == null)
			{
				Debug.LogWarning(
					$"[StageIntroSequenceController] Intro button '{objectName}' was not found.",
					this);
				return null;
			}

			Image targetImage = target.GetComponent<Image>();
			if (targetImage == null)
			{
				Debug.LogWarning(
					$"[StageIntroSequenceController] Intro button '{objectName}' has no Image.",
					target);
				return null;
			}

			targetImage.enabled = true;
			targetImage.raycastTarget = true;

			Button button = target.GetComponent<Button>();
			if (button == null)
			{
				button = target.gameObject.AddComponent<Button>();
			}

			if (button == null)
			{
				Debug.LogWarning(
					$"[StageIntroSequenceController] Failed to prepare Button '{objectName}'.",
					target);
				return null;
			}

			button.targetGraphic = targetImage;
			button.onClick.RemoveListener(BeginStartTransition);
			button.onClick.AddListener(BeginStartTransition);
			return button;
		}

		private void SetStartButtonsInteractable(bool value)
		{
			if (startAsNewButton != null && startAsNewButton.gameObject.activeInHierarchy)
			{
				startAsNewButton.interactable = value;
			}

			if (startAsSaveButton != null && startAsSaveButton.gameObject.activeInHierarchy)
			{
				startAsSaveButton.interactable = value;
			}
		}

		private void RemoveButtonListener(Button button)
		{
			if (button != null)
			{
				button.onClick.RemoveListener(BeginStartTransition);
			}
		}

		private Image CreateBlackOverlay()
		{
			GameObject overlay = new("Runtime_BlackFade", typeof(RectTransform));
			overlay.layer = gameObject.layer;
			RectTransform overlayRect = overlay.GetComponent<RectTransform>();
			Transform overlayParent = transform.parent != null
				? transform.parent
				: transform;
			overlayRect.SetParent(overlayParent, false);
			overlayRect.anchorMin = Vector2.zero;
			overlayRect.anchorMax = Vector2.one;
			overlayRect.offsetMin = Vector2.zero;
			overlayRect.offsetMax = Vector2.zero;
			overlayRect.SetAsLastSibling();

			Image image = overlay.AddComponent<Image>();
			image.color = Color.black;
			image.raycastTarget = true;
			return image;
		}

		private static Transform FindDescendant(Transform root, string objectName)
		{
			if (root == null || string.IsNullOrWhiteSpace(objectName))
			{
				return null;
			}

			Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
			for (int index = 0; index < descendants.Length; index++)
			{
				Transform candidate = descendants[index];
				if (candidate != null && candidate.name == objectName)
				{
					return candidate;
				}
			}

			return null;
		}

		private static float Smooth(float value)
		{
			return value * value * (3f - 2f * value);
		}

	}
}
