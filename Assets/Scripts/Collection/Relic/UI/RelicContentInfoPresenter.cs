using Item;
using Presentation;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Item.UI
{
    public sealed class RelicContentInfoPresenter : UIComponent
    {
        [Header("View")]
        [AutoBind("UIContentInfoView_Relic")]
        [SerializeField] private UIContentInfoView contentView;

        [Header("Relic")]
        [SerializeField] private RelicSO relic;
        [SerializeField] private bool buildOnStart = true;

        public RelicSO Relic => relic;

        private void Start()
        {
            if (buildOnStart && relic != null)
            {
                BuildPresentation();
            }
        }

        [ContextMenu("Build Relic Presentation")]
        public void BuildPresentation()
        {
            if (!CanBuildPresentation())
            {
                return;
            }

            if (relic == null)
            {
                Debug.LogError(
                    "[RelicContentInfoPresenter] RelicSO is not assigned.",
                    this);
                ClearPresentation();
                return;
            }

            ContentPresentationData content =
                new RelicPresentationResolver().ResolveForPlayerDisplay(
                    relic,
                    PresentationContext.Preview);
            Bind(content);
        }

        public void SetRelic(RelicSO value, bool rebuild = true)
        {
            relic = value;

            if (rebuild && Application.isPlaying)
            {
                BuildPresentation();
            }
        }

        public void ShowRelic(RelicSO value)
        {
            relic = value;
            if (relic == null)
            {
                HidePresentation();
                return;
            }

            gameObject.SetActive(true);
            BuildPresentation();
        }

        public void ShowRelic(RelicEntry runtime)
        {
            relic = runtime?.relic;
            if (runtime == null || relic == null)
            {
                HidePresentation();
                return;
            }

            gameObject.SetActive(true);

            if (!CanBuildPresentation())
            {
                return;
            }

            ContentPresentationData content =
                new RelicPresentationResolver().ResolveForPlayerDisplay(
                    runtime,
                    PresentationContext.Runtime);
            Bind(content);
        }

        public void HidePresentation()
        {
            ClearPresentation();
            relic = null;
        }

        public void ClearPresentation()
        {
            if (contentView != null)
            {
                contentView.Bind(null);
            }
        }

        private bool CanBuildPresentation()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[RelicContentInfoPresenter] Enter Play Mode before building presentation data.",
                    this);
                return false;
            }

            if (contentView == null)
            {
                Debug.LogError(
                    "[RelicContentInfoPresenter] RelicContentInfoView is not assigned.",
                    this);
                return false;
            }

            if (EventSystem.current == null)
            {
                Debug.LogWarning(
                    "[RelicContentInfoPresenter] No active EventSystem was found. " +
                    "The content can be displayed, but ScrollRect input will not work.",
                    this);
            }

            return true;
        }

        private void Bind(ContentPresentationData content)
        {
            contentView.SetFormatter(
                PresentationTextFormatter.CreatePlayerFormatter(
                    PresentationLocalizedTextResolver.ResolveLabel));
            contentView.Bind(content);
        }
    }
}
