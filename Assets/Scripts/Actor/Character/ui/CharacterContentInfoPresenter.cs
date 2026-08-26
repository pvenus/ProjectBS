using Presentation;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Character.UI
{
    public sealed class CharacterContentInfoPresenter : UIComponent
    {
        [Header("View")]
        [AutoBind("CharacterContentInfoView")]
        [SerializeField] private UIContentInfoView contentView;

        [Header("Character")]
        [SerializeField] private CharacterSO character;
        [SerializeField] private bool buildOnStart = true;
        private CharacterRuntimeData characterRuntime;

        [Header("Optional Skill Tabs")]
        [SerializeField] private CharacterSkillContentInfoPresenter skillTabs;

        public CharacterSO Character => character;
        public CharacterRuntimeData CharacterRuntime => characterRuntime;

        private void Start()
        {
            if (buildOnStart)
            {
                BuildPresentation();
            }
        }

        [ContextMenu("Build Character Presentation")]
        public void BuildPresentation()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[CharacterContentInfoPresenter] Enter Play Mode before building presentation data.",
                    this);
                return;
            }

            if (contentView == null)
            {
                Debug.LogError(
                    "[CharacterContentInfoPresenter] CharacterContentInfoView is not assigned.",
                    this);
                return;
            }

            if (character == null)
            {
                Debug.LogError(
                    "[CharacterContentInfoPresenter] CharacterSO is not assigned.",
                    this);
                contentView.Bind(null);
                return;
            }

            if (EventSystem.current == null)
            {
                Debug.LogWarning(
                    "[CharacterContentInfoPresenter] No active EventSystem was found. " +
                    "The content can be displayed, but ScrollRect input will not work.",
                    this);
            }

            ContentPresentationData content = characterRuntime != null
                ? new CharacterPresentationResolver().ResolveForPlayerDisplay(
                    characterRuntime,
                    PresentationContext.Runtime)
                : new CharacterPresentationResolver().ResolveForPlayerDisplay(
                    character,
                    PresentationContext.Preview);

            contentView.SetFormatter(
                PresentationTextFormatter.CreatePlayerFormatter(
                    PresentationLocalizedTextResolver.ResolveLabel));
            contentView.Bind(content);

            if (skillTabs != null && skillTabs.Character != character)
            {
                if (characterRuntime != null)
                {
                    skillTabs.SetCharacter(characterRuntime);
                }
                else
                {
                    skillTabs.SetCharacter(character);
                }
            }
        }

        public void SetCharacter(CharacterSO value, bool rebuild = true)
        {
            characterRuntime = null;
            character = value;

            if (rebuild && Application.isPlaying)
            {
                BuildPresentation();
            }
        }

        public void SetCharacter(CharacterRuntimeData value, bool rebuild = true)
        {
            characterRuntime = value;
            character = value?.characterSO;

            if (rebuild && Application.isPlaying)
            {
                BuildPresentation();
            }
        }

        public void ClearPresentation()
        {
            character = null;
            characterRuntime = null;
            contentView?.Bind(null);
        }
    }
}
