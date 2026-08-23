using System.Collections.Generic;
using UnityEngine;

namespace Battle.UI.PartyHud
{
    [DisallowMultipleComponent]
    public sealed class PartyHudView : MonoBehaviour
    {
        public const int MaxPartyMemberCount = 4;

        [Header("Layout")]
        [SerializeField] private RectTransform memberRoot;
        [SerializeField] private PartyHudMemberView memberPrefab;

        [Header("Options")]
        [Tooltip("Keeps the basic-attack slot in the layout but allows it to be hidden without changing the data contract.")]
        [SerializeField] private bool showBasicAttack = true;

        private readonly List<PartyHudMemberView> memberViews =
            new List<PartyHudMemberView>(MaxPartyMemberCount);

        private bool missingPrefabReported;

        public bool IsBasicAttackVisible => showBasicAttack;

        private void Awake()
        {
            EnsureMemberViews();
        }

        public void Render(PartyHudViewData viewData)
        {
            EnsureMemberViews();

            IReadOnlyList<PartyHudMemberData> members = viewData?.Members;
            int memberCount =
                members == null
                    ? 0
                    : Mathf.Min(members.Count, MaxPartyMemberCount);

            for (int index = 0; index < memberViews.Count; index++)
            {
                PartyHudMemberData member =
                    index < memberCount
                        ? members[index]
                        : null;

                memberViews[index].Render(member, showBasicAttack);
            }

            if (members != null && members.Count > MaxPartyMemberCount)
            {
                Debug.LogWarning(
                    $"[PartyHudView] Only the first {MaxPartyMemberCount} party members are displayed.",
                    this);
            }
        }

        public void SetBasicAttackVisible(bool visible)
        {
            if (showBasicAttack == visible)
            {
                return;
            }

            showBasicAttack = visible;

            for (int index = 0; index < memberViews.Count; index++)
            {
                memberViews[index].SetBasicAttackVisible(visible);
            }
        }

        public void Clear()
        {
            Render(null);
        }

        private void EnsureMemberViews()
        {
            if (memberPrefab == null || memberRoot == null)
            {
                if (!missingPrefabReported)
                {
                    Debug.LogError(
                        "[PartyHudView] Member root or member prefab is not assigned.",
                        this);
                    missingPrefabReported = true;
                }

                return;
            }

            while (memberViews.Count < MaxPartyMemberCount)
            {
                PartyHudMemberView memberView =
                    Instantiate(memberPrefab, memberRoot);
                memberView.name = $"PartyMember_{memberViews.Count + 1}";
                memberView.gameObject.SetActive(false);
                memberViews.Add(memberView);
            }
        }
    }
}
