using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shrine;

[AutoBindPrefix("God")]
public class ShrineGodSelectPage : UIComponent
{
		[Header("References")]
		[AutoBind]
		[SerializeField] private Transform cardRoot;

		[AutoBind]
		[SerializeField] private RectTransform focusPoint;

		[AutoBind]
		[SerializeField] private UIButtonList actionButtonList;

		[AutoBind]
		[SerializeField] private ShrineGodInfoPanel infoPanel;

		[Header("Prefab")]
		[SerializeField] private ShrineGodCard cardPrefab;

		[Header("Sequence")]
		[SerializeField] private float cardShowInterval = 0.12f;
		[SerializeField] private float selectDelay = 0.45f;

		private readonly List<ShrineGodCard> spawnedCards = new();

		private ShrineManager shrineManager;
		private Action<ShrineGodType, ShrineActionType> onGodActionSelected;

		private ShrineGodType selectedGodType;
		private ShrineGodCard selectedCardInstance;

		private Coroutine showRoutine;
		private Coroutine selectRoutine;
		private bool isSelecting;

		public void Show(
			ShrineManager manager,
			IReadOnlyList<ShrineGodType> godTypes,
			Action<ShrineGodType, ShrineActionType> selectedCallback)
		{
			shrineManager = manager;
			onGodActionSelected = selectedCallback;

			gameObject.SetActive(true);

			if (actionButtonList != null)
			{
				actionButtonList.Clear();
				actionButtonList.gameObject.SetActive(false);
			}

			if (infoPanel != null)
			{
				infoPanel.HideImmediate();
			}

			BuildCards(godTypes);

			if (showRoutine != null)
			{
				StopCoroutine(showRoutine);
			}

			showRoutine = StartCoroutine(PlayCardsSequentially());
		}

		public void Hide()
		{
			if (showRoutine != null)
			{
				StopCoroutine(showRoutine);
				showRoutine = null;
			}

			if (selectRoutine != null)
			{
				StopCoroutine(selectRoutine);
				selectRoutine = null;
			}

			ClearCards();

			isSelecting = false;
			gameObject.SetActive(false);
		}

		private void BuildCards(IReadOnlyList<ShrineGodType> godTypes)
		{
			ClearCards();

			if (godTypes == null || cardPrefab == null)
			{
				return;
			}

			Transform root = cardRoot != null
				? cardRoot
				: transform;

			// 1. 카드 먼저 생성 바인딩
			for (int i = 0; i < godTypes.Count; i++)
			{
				ShrineGodType godType = godTypes[i];

				if (godType == ShrineGodType.None)
				{
					continue;
				}

				ShrineGodType capturedGodType = godType;

				ShrineGodSO godSo = shrineManager != null
					? shrineManager.GetGodSO(capturedGodType)
					: null;

				ShrineGodCard card = Instantiate(cardPrefab, root);

				card.Bind(
					capturedGodType,
					godSo,
					clickedCard => HandleCardClicked(clickedCard, capturedGodType));

				spawnedCards.Add(card);
			}

			// 2. Unity LayoutGroup이 즉시 위치를 계산하도록 강제 업데이트
			Canvas.ForceUpdateCanvases();
			UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(root as RectTransform);

			// 3. LayoutGroup이 강제한 위치를 애니메이션용으로 사용하기 위해 LayoutGroup 비활성화
			// (안 끄면 FlyAnimator가 위치를 변경해도 LayoutGroup이 매 프레임 원상복구시켜버립니다)
			var layoutGroup = root.GetComponent<UnityEngine.UI.LayoutGroup>();
			if (layoutGroup != null)
			{
				layoutGroup.enabled = false;
			}

			// 4. 이제 올바른 Layout 위치가 잡혔으므로, Cache 및 Hide 처리
			foreach (var card in spawnedCards)
			{
				card.HideImmediate();
			}

			isSelecting = true;
		}

		private IEnumerator PlayCardsSequentially()
		{
			for (int i = 0; i < spawnedCards.Count; i++)
			{
				ShrineGodCard card = spawnedCards[i];

				if (card != null)
				{
					card.PlayShow();
				}

				yield return new WaitForSeconds(cardShowInterval);
			}

			showRoutine = null;
		}

		private void HandleCardClicked(
			ShrineGodCard selectedCard,
			ShrineGodType godType)
		{
			if (!isSelecting)
			{
				return;
			}

			isSelecting = false;
			selectedGodType = godType;
			selectedCardInstance = selectedCard;

			Vector2 targetPos;
			RectTransform cardRect = selectedCard.GetComponent<RectTransform>();

			if (focusPoint != null)
			{
				// focusPoint가 하이어라키 상 어디에 있든 상관없이, 정확히 해당 월드 위치로 가도록 offset 계산
				Vector3 localFocusPos = cardRoot.InverseTransformPoint(focusPoint.position);
				Vector3 localCardPos = cardRoot.InverseTransformPoint(cardRect.position);
				
				Vector2 offset = (Vector2)(localFocusPos - localCardPos);
				targetPos = cardRect.anchoredPosition + offset;
			}
			else
			{
				targetPos = cardRect.anchoredPosition;
			}

			for (int i = 0; i < spawnedCards.Count; i++)
			{
				ShrineGodCard card = spawnedCards[i];

				if (card == null)
				{
					continue;
				}

				if (card == selectedCard)
				{
					card.PlayFocus(targetPos);
				}
				else
				{
					card.PlayDeselected();
				}
			}

			if (selectRoutine != null)
			{
				StopCoroutine(selectRoutine);
			}

			if (infoPanel != null)
			{
				ShrineGodSO godSo = shrineManager != null ? shrineManager.GetGodSO(godType) : null;
				ShrineRuntimeData shrineData = shrineManager != null ? shrineManager.CurrentShrine : null;
				infoPanel.Bind(godSo, shrineData, shrineManager);
				infoPanel.PlayShow();
			}

			selectRoutine = StartCoroutine(ShowActionButtonsAfterDelay(godType));
		}

		private IEnumerator ShowActionButtonsAfterDelay(ShrineGodType godType)
		{
			yield return new WaitForSeconds(selectDelay);
			ShowActionButtons(godType);
			selectRoutine = null;
		}

		private void ShowActionButtons(ShrineGodType godType)
		{
			if (actionButtonList == null) return;

			actionButtonList.gameObject.SetActive(true);

			List<UIButtonData> buttons = new List<UIButtonData>();

			int cost = shrineManager != null ? shrineManager.GetDonationCost(godType) : 0;

			buttons.Add(new UIButtonData("btn_pray", "Pray", () => 
			{
				onGodActionSelected?.Invoke(godType, ShrineActionType.Pray);
			}));

			buttons.Add(new UIButtonData("btn_donate", $"Donate ({cost} G)", () => 
			{
				onGodActionSelected?.Invoke(godType, ShrineActionType.Donate);
			}));

			buttons.Add(new UIButtonData("btn_cancel", "Back", () => 
			{
				RestoreCards();
			}));

			actionButtonList.SetButtons(buttons);
		}

		private void RestoreCards()
		{
			if (actionButtonList != null)
			{
				actionButtonList.Clear();
				actionButtonList.gameObject.SetActive(false);
			}

			if (infoPanel != null)
			{
				infoPanel.PlayHide();
			}

			for (int i = 0; i < spawnedCards.Count; i++)
			{
				ShrineGodCard card = spawnedCards[i];
				if (card != null)
				{
					card.PlayRestore();
				}
			}

			isSelecting = true;
		}

		private void ClearCards()
		{
			Transform root = cardRoot != null ? cardRoot : transform;
			var layoutGroup = root.GetComponent<UnityEngine.UI.LayoutGroup>();
			if (layoutGroup != null)
			{
				layoutGroup.enabled = true;
			}

			// 에디터에서 미리 배치해둔 더미 카드들도 모두 삭제
			for (int i = root.childCount - 1; i >= 0; i--)
			{
				Transform child = root.GetChild(i);
				// 프리팹 자체가 하위에 들어있을 수도 있으므로 (보통은 아님)
				if (cardPrefab != null && child.gameObject == cardPrefab.gameObject) continue;
				Destroy(child.gameObject);
			}

			spawnedCards.Clear();
		}
	}