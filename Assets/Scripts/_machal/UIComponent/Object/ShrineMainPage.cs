using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shrine;

[AutoBindPrefix("UI")]
public class ShrineMainPage : UIPage
{
        [Header("Root")]
        [Tooltip("실제 UI 패널들을 감싸는 루트. 창 자체는 켜둔 채로 이 루트만 켜고 끕니다.")]
        [SerializeField] private GameObject pageRoot;

        [Header("UI Components")]
        [AutoBind]
        [SerializeField] private TMP_Text titleText;

        [AutoBind]
        [SerializeField] private TMP_Text descText;

        [AutoBind]
        [SerializeField] private Image illust;

        [AutoBind]
        [SerializeField] private TMP_Text hpText;

        [AutoBind]
        [SerializeField] private TMP_Text goldText;

        [AutoBind]
        [SerializeField] private UIButtonList buttonList;

        [Header("Sub Pages")]
        [AutoBind]
        [SerializeField] private ShrineGodSelectPage godSelectPage;

        private void Awake()
        {
            
        }

        protected virtual void Start()
        {
            if (ShrineManager.Instance != null)
            {
                ShrineManager.Instance.OnShrineOpened += HandleShrineOpened;
                ShrineManager.Instance.OnShrineRefreshed += HandleShrineRefreshed;
                ShrineManager.Instance.OnShrineCompleted += HandleShrineCompleted;

                // 이미 열려있는 상태라면 바로 표시
                if (ShrineManager.Instance.HasShrine && ShrineManager.Instance.IsOpened)
                {
                    HandleShrineOpened(ShrineManager.Instance.CurrentShrine);
                }
            }
            else
            {
                Debug.LogWarning("[ShrineMainPage] ShrineManager.Instance is null at Start.");
            }

			// 최초 실행 시 숨김 처리
			Hide();
		}

        protected virtual void OnDestroy()
        {
            if (ShrineManager.Instance != null)
            {
                ShrineManager.Instance.OnShrineOpened -= HandleShrineOpened;
                ShrineManager.Instance.OnShrineRefreshed -= HandleShrineRefreshed;
                ShrineManager.Instance.OnShrineCompleted -= HandleShrineCompleted;
            }
        }

        public override void Show()
        {
            if (pageRoot != null)
            {
                pageRoot.SetActive(true);
            }
        }

        public override void Hide()
        {
            if (pageRoot != null)
            {
                pageRoot.SetActive(false);
            }
        }

        private void HandleShrineOpened(ShrineRuntimeData data)
        {
            Show();
            Refresh(data);
        }

        private void HandleShrineRefreshed(ShrineRuntimeData data)
        {
            Refresh(data);
        }

        private void HandleShrineCompleted(ShrineRuntimeData data)
        {
            Hide();
        }

        public override void Refresh()
        {
            if (ShrineManager.Instance != null && ShrineManager.Instance.HasShrine)
            {
                Refresh(ShrineManager.Instance.CurrentShrine);
            }
        }

        private void Refresh(ShrineRuntimeData data)
        {
            if (data == null)
            {
                return;
            }

            UpdateCommonUI(data);

            // 서브 페이지 숨기기 초기화 (필요시 각 Show 메서드에서 활성화)
            if (godSelectPage != null && data.flowState != ShrineFlowState.GodSelection)
            {
                godSelectPage.Hide();
            }

            switch (data.flowState)
            {
                case ShrineFlowState.Enter:
                case ShrineFlowState.MainSelection:
                    ShowMainSelection();
                    break;

                case ShrineFlowState.BlessingSelection:
                    ShowBlessSelection(data);
                    break;

                case ShrineFlowState.GodSelection:
                    ShowGodSelection(data);
                    break;

                case ShrineFlowState.FaithActionSelection:
                    ShowFaithActionSelection(data);
                    break;

                case ShrineFlowState.Reward:
                    ShowReward();
                    break;

                case ShrineFlowState.Complete:
                    HandleShrineCompleted(data);
                    break;
            }
        }

        private void UpdateCommonUI(ShrineRuntimeData data)
        {
            if (titleText != null)
            {
                titleText.text = data.shrineName;
			}

            if(descText != null)
            {
				descText.text = data.selectedAction == ShrineActionType.None
					? "Choose your action."
					: data.selectedAction.ToString();
			}

            if (ShrineManager.Instance == null) return;

            if (hpText != null)
            {
                hpText.text = $"HP: {ShrineManager.Instance.PartyCurrentHp} / {ShrineManager.Instance.PartyMaxHp}";
            }

            if (goldText != null)
            {
                goldText.text = $"Gold: {ShrineManager.Instance.CurrentGold}";
            }
        }

        private void ShowMainSelection()
        {
            if (descText != null)
            {
                descText.text = "Choose Your Action";
            }

            List<UIButtonData> buttons = new List<UIButtonData>
            {
                new UIButtonData("btn_heal_bless", "heal and bless", ShrineManager.Instance.SelectHealAndBless),
                new UIButtonData("btn_enter_faith", "enter faith", ShrineManager.Instance.SelectPray)
            };

            if (buttonList != null)
            {
                buttonList.SetButtons(buttons);
            }
        }

        private void ShowBlessSelection(ShrineRuntimeData data)
        {
            if (descText != null)
            {
                descText.text = data.selectedAction == ShrineActionType.None
                    ? "Heal And Bless"
                    : data.selectedAction.ToString();
            }

            List<UIButtonData> buttons = new List<UIButtonData>();

            if (data.HasBlessingCandidates)
            {
                foreach (var candidate in data.blessingCandidates)
                {
                    if (candidate == null) continue;

                    string blessName = candidate.DisplayName;
                    

					buttons.Add(new UIButtonData(
                        $"btn_bless_{candidate.slotIndex}",
                        $"{candidate.slotIndex}: {blessName}",
                        () => ShrineManager.Instance.SelectBlessingBySlot(candidate.slotIndex)
                    ));
                }
            }

            if (buttonList != null)
            {
                buttonList.SetButtons(buttons);
            }
        }

        private void SetTitle(string title, string desc)
        {
            if (titleText != null) titleText.text = title;
            if (descText != null) descText.text = desc;
        }

		private void ShowGodSelection(ShrineRuntimeData data)
		{
			SetTitle("신 선택", "신앙 활동을 진행할 신을 선택하세요.");

			if (buttonList != null)
			{
				buttonList.Clear();
			}

			if (godSelectPage == null)
			{
				Debug.LogWarning("[ShrineMainPage] GodSelectPage is null.");
				return;
			}

			godSelectPage.Show(
				ShrineManager.Instance,
				data.availableGods,
				(godType, actionType) =>
				{
					ShrineManager.Instance.SelectGod(godType);
					
					if (actionType == ShrineActionType.Pray)
					{
						ShrineManager.Instance.ConfirmPray();
					}
					else if (actionType == ShrineActionType.Donate)
					{
						ShrineManager.Instance.ConfirmDonate();
					}
				});
		}

        private void ShowFaithActionSelection(ShrineRuntimeData data)
        {
            if (descText != null)
            {
				descText.text = data.selectedAction == ShrineActionType.None
					? "Select Faith Action"
					: data.selectedAction.ToString();
			}

			List<UIButtonData> buttons = new List<UIButtonData>();

            if (data.HasSelectedGod)
            {
				buttons.Add(new UIButtonData(
					"btn_confirm_pray",
					"Pray",
					() => ShrineManager.Instance.ConfirmPray()
				));

				int cost = ShrineManager.Instance.GetDonationCost(data.selectedGod);
				buttons.Add(new UIButtonData(
					"btn_confirm_donate",
					$"Donate ({cost} G)",
					() => ShrineManager.Instance.ConfirmDonate()
				));
			}

            if (buttonList != null)
            {
                buttonList.SetButtons(buttons);
            }
        }

        private void ShowReward()
        {
            if (descText != null)
            {
				//descText.text = data.flowState.ToString();
                descText.text = "보상을 받았습니다.";
            }

            List<UIButtonData> buttons = new List<UIButtonData>
            {
                new UIButtonData("btn_continue", "확인", () => ShrineManager.Instance.CompleteShrine())
            };

            if (buttonList != null)
            {
                buttonList.SetButtons(buttons);
            }
        }
    }
