using System.Collections.Generic;
using Skill;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.UI
{
    public sealed class CharacterSkillContentInfoPresenter : UIComponent
    {
        [Header("References")]
        [AutoBind("SkillContentInfoPresenter")]
        [SerializeField] private SkillContentInfoPresenter skillPresenter;

        [AutoBind("CharacterSkillTabRoot")]
        [SerializeField] private RectTransform skillTabRoot;

        [SerializeField] private SkillContentInfoTabButton skillTabPrefab;

        [Header("Character Presentation")]
        [SerializeField] private CharacterContentInfoPresenter characterPresenter;

        [Header("Character Navigation")]
        [SerializeField] private List<CharacterSO> characters = new();
        [SerializeField] private CharacterSO character;
        [SerializeField, Min(0)] private int initialCharacterIndex;
        [SerializeField] private bool loopCharacterSelection;

        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;

        [FormerlySerializedAs("initialSelectedIndex")]
        [SerializeField, Min(0)] private int initialSelectedSkillIndex;

        private readonly List<SkillContentInfoTabButton> spawnedTabs = new();
        private readonly List<CharacterSO> availableCharacters = new();
        private int selectedCharacterIndex = -1;

        public CharacterSO Character => character;
        public IReadOnlyList<CharacterSO> Characters => characters;
        public int CharacterCount => availableCharacters.Count;
        public int SelectedCharacterIndex => selectedCharacterIndex;
        public bool CanShowPreviousCharacter =>
            availableCharacters.Count > 1 &&
            selectedCharacterIndex >= 0 &&
            (loopCharacterSelection || selectedCharacterIndex > 0);
        public bool CanShowNextCharacter =>
            availableCharacters.Count > 1 &&
            selectedCharacterIndex >= 0 &&
            (loopCharacterSelection ||
             selectedCharacterIndex < availableCharacters.Count - 1);
        public EquipmentSkillSO SelectedSkill { get; private set; }
        public int SpawnedTabCount => spawnedTabs.Count;

        private void Start()
        {
            RebuildAvailableCharacters();

            if (availableCharacters.Count > 0)
            {
                selectedCharacterIndex = Mathf.Clamp(
                    initialCharacterIndex,
                    0,
                    availableCharacters.Count - 1);
                character = availableCharacters[selectedCharacterIndex];
            }

            if (buildOnStart)
            {
                BuildSelectedCharacter();
            }
        }

        [ContextMenu("Build Selected Character")]
        public void BuildSelectedCharacter()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[CharacterSkillContentInfoPresenter] Enter Play Mode before building character presentation.",
                    this);
                return;
            }

            if (character == null)
            {
                ClearSkillTabs();
                skillPresenter?.ClearPresentation();
                characterPresenter?.ClearPresentation();
                return;
            }

            BuildSkillTabs();

            if (characterPresenter != null)
            {
                characterPresenter.SetCharacter(character);
            }

        }

        [ContextMenu("Build Character Skill Tabs")]
        public void BuildSkillTabs()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[CharacterSkillContentInfoPresenter] Enter Play Mode before building skill tabs.",
                    this);
                return;
            }

            ClearSkillTabs();

            if (!ValidateReferences())
            {
                return;
            }

            IReadOnlyList<CharacterSkillEntry> skillEntries = character.Skills;
            if (skillEntries == null)
            {
                skillPresenter.ClearPresentation();
                return;
            }

            for (int index = 0; index < skillEntries.Count; index++)
            {
                CharacterSkillEntry entry = skillEntries[index];
                EquipmentSkillSO skill = entry?.skillSo;
                if (skill == null)
                {
                    Debug.LogWarning(
                        $"[CharacterSkillContentInfoPresenter] Ignored null skill at index {index} " +
                        $"for CharacterSO '{character.name}'.",
                        character);
                    continue;
                }

                SkillContentInfoTabButton tab = Instantiate(skillTabPrefab, skillTabRoot);
                tab.name = BuildTabName(entry, skill, index);
                tab.Bind(skill, () => SelectSkill(tab));
                spawnedTabs.Add(tab);
            }

            if (spawnedTabs.Count == 0)
            {
                skillPresenter.ClearPresentation();
                return;
            }

            SelectSkill(Mathf.Clamp(initialSelectedSkillIndex, 0, spawnedTabs.Count - 1));
        }

        public void SetCharacter(CharacterSO value, bool rebuild = true)
        {
            character = value;
            RebuildAvailableCharacters();
            selectedCharacterIndex = availableCharacters.IndexOf(value);

            if (rebuild && Application.isPlaying)
            {
                BuildSkillTabs();
            }
        }

        public void SetCharacters(
            IEnumerable<CharacterSO> values,
            int selectedIndex = 0,
            bool rebuild = true)
        {
            List<CharacterSO> replacements = values != null
                ? new List<CharacterSO>(values)
                : new List<CharacterSO>();

            characters.Clear();
            characters.AddRange(replacements);
            character = null;

            RebuildAvailableCharacters();

            if (availableCharacters.Count == 0)
            {
                selectedCharacterIndex = -1;
                character = null;

                if (rebuild && Application.isPlaying)
                {
                    BuildSelectedCharacter();
                }

                return;
            }

            SelectCharacter(
                Mathf.Clamp(selectedIndex, 0, availableCharacters.Count - 1),
                rebuild);
        }

        public void SelectCharacter(int index, bool rebuild = true)
        {
            if (index < 0 || index >= availableCharacters.Count)
            {
                Debug.LogWarning(
                    $"[CharacterSkillContentInfoPresenter] Character index is out of range: {index}.",
                    this);
                return;
            }

            selectedCharacterIndex = index;
            character = availableCharacters[index];

            if (rebuild && Application.isPlaying)
            {
                BuildSelectedCharacter();
            }
        }

        public void ShowPreviousCharacter()
        {
            MoveCharacter(-1);
        }

        public void ShowNextCharacter()
        {
            MoveCharacter(1);
        }

        public void SelectSkill(int index)
        {
            if (index < 0 || index >= spawnedTabs.Count)
            {
                Debug.LogWarning(
                    $"[CharacterSkillContentInfoPresenter] Skill tab index is out of range: {index}.",
                    this);
                return;
            }

            SelectSkill(spawnedTabs[index]);
        }

        public void ClearSkillTabs()
        {
            SelectedSkill = null;

            for (int index = spawnedTabs.Count - 1; index >= 0; index--)
            {
                SkillContentInfoTabButton tab = spawnedTabs[index];
                if (tab == null)
                {
                    continue;
                }

                tab.gameObject.SetActive(false);
                Destroy(tab.gameObject);
            }

            spawnedTabs.Clear();
        }

        private void SelectSkill(SkillContentInfoTabButton selectedTab)
        {
            if (selectedTab == null || selectedTab.Skill == null)
            {
                return;
            }

            for (int index = 0; index < spawnedTabs.Count; index++)
            {
                SkillContentInfoTabButton tab = spawnedTabs[index];
                tab?.SetSelected(tab == selectedTab);
            }

            SelectedSkill = selectedTab.Skill;
            skillPresenter.ShowSkill(SelectedSkill);
        }

        private void MoveCharacter(int direction)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[CharacterSkillContentInfoPresenter] Enter Play Mode before changing characters.",
                    this);
                return;
            }

            if (availableCharacters.Count == 0)
            {
                return;
            }

            int currentIndex = selectedCharacterIndex >= 0
                ? selectedCharacterIndex
                : 0;
            int targetIndex = currentIndex + direction;

            if (loopCharacterSelection)
            {
                targetIndex = (targetIndex + availableCharacters.Count) %
                              availableCharacters.Count;
            }
            else
            {
                targetIndex = Mathf.Clamp(
                    targetIndex,
                    0,
                    availableCharacters.Count - 1);
            }

            if (targetIndex != selectedCharacterIndex)
            {
                SelectCharacter(targetIndex);
            }
        }

        private void RebuildAvailableCharacters()
        {
            availableCharacters.Clear();

            for (int index = 0; index < characters.Count; index++)
            {
                CharacterSO candidate = characters[index];
                if (candidate == null)
                {
                    Debug.LogWarning(
                        $"[CharacterSkillContentInfoPresenter] Ignored null CharacterSO at list index {index}.",
                        this);
                    continue;
                }

                availableCharacters.Add(candidate);
            }

            if (availableCharacters.Count == 0 && character != null)
            {
                availableCharacters.Add(character);
            }
        }

        private bool ValidateReferences()
        {
            if (skillPresenter == null)
            {
                Debug.LogError(
                    "[CharacterSkillContentInfoPresenter] SkillContentInfoPresenter is not assigned.",
                    this);
                return false;
            }

            if (skillTabRoot == null)
            {
                Debug.LogError(
                    "[CharacterSkillContentInfoPresenter] CharacterSkillTabRoot is not assigned.",
                    this);
                return false;
            }

            if (skillTabPrefab == null)
            {
                Debug.LogError(
                    "[CharacterSkillContentInfoPresenter] SkillContentInfoTabButton prefab is not assigned.",
                    this);
                return false;
            }

            if (character == null)
            {
                Debug.LogError(
                    "[CharacterSkillContentInfoPresenter] CharacterSO is not assigned.",
                    this);
                skillPresenter.ClearPresentation();
                return false;
            }

            return true;
        }

        private static string BuildTabName(
            CharacterSkillEntry entry,
            EquipmentSkillSO skill,
            int index)
        {
            string key = !string.IsNullOrWhiteSpace(entry?.slotKey)
                ? entry.slotKey
                : skill.EquipmentId;

            return string.IsNullOrWhiteSpace(key)
                ? $"SkillTab_{index}"
                : $"SkillTab_{key}";
        }
    }
}
