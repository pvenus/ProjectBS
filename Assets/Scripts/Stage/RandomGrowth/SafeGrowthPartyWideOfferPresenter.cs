using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Character.ProgressionBridge;
using Party;
using Progression;
using Skill;

namespace Stage
{
    public enum SafeGrowthPartyWideOfferOpenResult
    { Opened, AlreadyOpen, AlreadyApplied, MissingOpportunity, AttachFailed, InvalidProjection, ViewUnavailable }

    public enum SafeGrowthTerminalNavigationDecision { KeepPopup, CloseStagePopup, OpenGrowthOffer }

    public static class SafeGrowthTerminalNavigationPolicy
    {
        public static SafeGrowthTerminalNavigationDecision Resolve(
            SafeGrowthPresentationActionIntent intent, bool offerOpened) => intent switch
        {
            SafeGrowthPresentationActionIntent.OpenGrowthOffer => offerOpened
                ? SafeGrowthTerminalNavigationDecision.OpenGrowthOffer
                : SafeGrowthTerminalNavigationDecision.KeepPopup,
            SafeGrowthPresentationActionIntent.ContinueStage => SafeGrowthTerminalNavigationDecision.CloseStagePopup,
            _ => SafeGrowthTerminalNavigationDecision.KeepPopup
        };
    }

    public interface ISafeGrowthPartyWideOfferViewHost
    {
        bool IsOpen { get; }
        bool Open(UIFramework.Data.SkillUpgradeViewData data, Action<int> selected);
        void Close();
    }

    public sealed class SafeGrowthPartyWideOfferPresenter
    {
        private readonly RunProgressionLedger ledger; private readonly PartyRuntimeData party;
        private readonly EquipmentSkillSO[] catalog; private readonly ISafeGrowthPartyWideOfferViewHost host;
        private SafeGrowthPartyWideOfferViewProjection projection;
        private ProgressionOpportunitySnapshot opportunity;

        public SafeGrowthPartyWideOfferPresenter(RunProgressionLedger ledger, PartyRuntimeData party,
            IEnumerable<EquipmentSkillSO> catalog, ISafeGrowthPartyWideOfferViewHost host)
        { this.ledger=ledger;this.party=party;this.catalog=(catalog??Array.Empty<EquipmentSkillSO>()).ToArray();this.host=host; }

        public SafeGrowthPartyWideOfferOpenResult Open()
        {
            if (host?.IsOpen == true) return SafeGrowthPartyWideOfferOpenResult.AlreadyOpen;
            opportunity = ledger?.GetSnapshots().SingleOrDefault(x =>
                x.SourceType == ProgressionSourceType.RandomEventSafe
                && x.SourceId == ProgressionSourceRegistry.RandomGrowthSafeSource);
            if (opportunity == null) return SafeGrowthPartyWideOfferOpenResult.MissingOpportunity;
            if (opportunity.State == ProgressionOpportunityState.Applied)
                return SafeGrowthPartyWideOfferOpenResult.AlreadyApplied;
            SafeGrowthEligibilitySnapshot eligible = new PartyWideSafeGrowthEligibilityQuery().Query(party, catalog);
            if (eligible.Status != SafeGrowthEligibilityStatus.Eligible)
                return SafeGrowthPartyWideOfferOpenResult.InvalidProjection;
            ProgressionSkillCandidateSnapshot[] candidates = eligible.Targets.Select(x =>
                new ProgressionSkillCandidateSnapshot(x.OwnerCharacterId, x.EquipmentId,
                    x.CanonicalSkillId, x.CurrentLevel, x.MaxLevel)).ToArray();
            ProgressionOfferAttachResult attached = new FixedOfferService().GetOrCreate(ledger,
                opportunity.OpportunityId, opportunity.Revision, candidates, 2, out opportunity);
            if (attached != ProgressionOfferAttachResult.Attached
                && attached != ProgressionOfferAttachResult.AlreadyAttached)
                return SafeGrowthPartyWideOfferOpenResult.AttachFailed;
            if (!new SafeGrowthPartyWideOfferViewDataBuilder().TryBuild(party, catalog, opportunity, out projection))
                return SafeGrowthPartyWideOfferOpenResult.InvalidProjection;
            return host.Open(projection.ViewData, Select)
                ? SafeGrowthPartyWideOfferOpenResult.Opened : SafeGrowthPartyWideOfferOpenResult.ViewUnavailable;
        }

        private void Select(int index)
        {
            if (projection == null || opportunity == null || index < 0 || index >= projection.Candidates.Count) return;
            ProgressionSkillCandidateSnapshot candidate = projection.Candidates[index];
            var gateway = new CharacterRuntimeSkillLevelGateway(
                party.Members.Cast<CharacterRuntimeData>().ToArray(), projection.Descriptors);
            ProgressionApplyResult result = new ProgressionConsumeService(ledger, gateway).TryApplyCandidate(
                new ProgressionApplyCandidateCommand(opportunity.OpportunityId,
                    opportunity.Offer.Fingerprint, candidate.OwnerCharacterId, candidate.SkillInstanceId,
                    candidate.CanonicalSkillId, candidate.CurrentLevel, opportunity.Revision));
            if (result.Code == ProgressionApplyResultCode.Applied
                || result.Code == ProgressionApplyResultCode.AlreadyApplied)
            { opportunity=result.Opportunity;host.Close(); }
        }
    }

    public sealed class SafeGrowthSkillUpgradeViewHost : ISafeGrowthPartyWideOfferViewHost
    {
        private SkillUpgradeView view;
        private Action<int> selectionHandler;
        public bool IsOpen => view != null;
        public bool Open(UIFramework.Data.SkillUpgradeViewData data, Action<int> selected)
        {
            view = UIPopupViewController.Instance?.Open<SkillUpgradeView>(PopupType.SkillUpgrade);
            if (view == null) return false;
            selectionHandler = selected;
            view.SetData(data); view.OnOptionClicked += selectionHandler; view.SetCloseButtonVisible(false); return true;
        }
        public void Close()
        {
            if (view == null) return;
            if (selectionHandler != null) view.OnOptionClicked -= selectionHandler;
            UIPopupViewController.Instance?.Close(view); view = null;
            selectionHandler = null;
        }
    }
}
