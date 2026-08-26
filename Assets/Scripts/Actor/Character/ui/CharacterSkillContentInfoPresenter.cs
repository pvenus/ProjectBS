using System.Collections.Generic;
using Battle.UI.PartyHud;
using Skill;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

        [Header("Party Tabs")]
        [SerializeField] private RectTransform characterTabRoot;
        [SerializeField] private GameObject characterTabPrefab;

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
        private readonly List<GameObject> spawnedCharacterTabs = new();
        private readonly List<CharacterSO> availableCharacters = new();
        private readonly List<CharacterRuntimeData> runtimeCharacters = new();
        private CharacterRuntimeData currentRuntimeCharacter;
        private int selectedCharacterIndex = -1;

        public CharacterSO Character => character;
        public CharacterRuntimeData CurrentRuntimeCharacter => currentRuntimeCharacter;
        public IReadOnlyList<CharacterSO> Characters => characters;
        public IReadOnlyList<CharacterRuntimeData> RuntimeCharacters => runtimeCharacters;
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
        public int SpawnedCharacterTabCount => spawnedCharacterTabs.Count;

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
                ClearCharacterTabs();
                ClearSkillTabs();
                skillPresenter?.ClearPresentation();
                characterPresenter?.ClearPresentation();
                return;
            }

            BuildCharacterTabs();
            BuildSkillTabs();

            if (characterPresenter != null)
            {
                if (currentRuntimeCharacter != null)
                {
                    characterPresenter.SetCharacter(currentRuntimeCharacter);
                }
                else
                {
                    characterPresenter.SetCharacter(character);
                }
            }

        }

        [ContextMenu("Build Character Party Tabs")]
        public void BuildCharacterTabs()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[CharacterSkillContentInfoPresenter] Enter Play Mode before building character tabs.",
                    this);
                return;
            }

            ClearCharacterTabs();

            if (characterTabRoot == null || characterTabPrefab == null)
            {
                Debug.LogWarning(
                    "[CharacterSkillContentInfoPresenter] Character party-tab root or template is not assigned.",
                    this);
                return;
            }

            characterTabPrefab.SetActive(false);

            for (int index = 0; index < availableCharacters.Count; index++)
            {
                CharacterSO candidate = availableCharacters[index];
                GameObject tabObject = Instantiate(characterTabPrefab, characterTabRoot);
                int capturedIndex = index;

                tabObject.name = BuildCharacterTabName(candidate, index);

                Button button = tabObject.GetComponent<Button>();
                PartyHudMemberView portraitView =
                    tabObject.GetComponentInChildren<PartyHudMemberView>(true);
                Image portraitImage = portraitView == null
                    ? FindPartyPortraitImage(tabObject.transform)
                    : null;

                if (button == null
                    || (portraitView == null && portraitImage == null))
                {
                    Debug.LogError(
                        "[CharacterSkillContentInfoPresenter] Character party-tab template must contain "
                        + "a root Button and either a PartyHudMemberView or a supported Portrait Image.",
                        tabObject);
                    tabObject.SetActive(false);
                    Destroy(tabObject);
                    continue;
                }

                EnsureButtonHitArea(button);
                if (portraitView != null)
                {
                    portraitView.RenderPortraitOnly(candidate.Portrait);
                }
                else
                {
                    BindPartyPortraitImage(portraitImage, candidate.Portrait);
                }
                button.onClick.AddListener(() => SelectCharacter(capturedIndex));

                spawnedCharacterTabs.Add(tabObject);
                tabObject.SetActive(true);
            }

            UpdateCharacterTabSelection();
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

            skillTabPrefab.gameObject.SetActive(false);

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

                if (currentRuntimeCharacter != null
                    && currentRuntimeCharacter.GetSkillInstance(skill.EquipmentId) == null)
                {
                    continue;
                }

                SkillContentInfoTabButton tab = Instantiate(skillTabPrefab, skillTabRoot);
                tab.name = BuildTabName(entry, skill, index);
                EnsureButtonHitArea(tab.GetComponent<Button>());
                tab.Bind(skill, skill.Icon, () => SelectSkill(tab));
                spawnedTabs.Add(tab);
                tab.gameObject.SetActive(true);
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
            runtimeCharacters.Clear();
            currentRuntimeCharacter = null;
            character = value;
            RebuildAvailableCharacters();
            selectedCharacterIndex = availableCharacters.IndexOf(value);

            if (rebuild && Application.isPlaying)
            {
                BuildSelectedCharacter();
            }
        }

        public void SetCharacter(CharacterRuntimeData value, bool rebuild = true)
        {
            runtimeCharacters.Clear();
            if (value != null)
            {
                runtimeCharacters.Add(value);
            }

            currentRuntimeCharacter = value;
            character = value?.characterSO;
            RebuildAvailableCharacters();
            selectedCharacterIndex = availableCharacters.IndexOf(character);

            if (rebuild && Application.isPlaying)
            {
                BuildSelectedCharacter();
            }
        }

        public void SetCharacters(
            IEnumerable<CharacterSO> values,
            int selectedIndex = 0,
            bool rebuild = true)
        {
            runtimeCharacters.Clear();
            currentRuntimeCharacter = null;

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

        public void SetCharacters(
            IEnumerable<CharacterRuntimeData> values,
            int selectedIndex = 0,
            bool rebuild = true)
        {
            runtimeCharacters.Clear();
            if (values != null)
            {
                runtimeCharacters.AddRange(values);
            }

            characters.Clear();
            for (int i = 0; i < runtimeCharacters.Count; i++)
            {
                CharacterRuntimeData r = runtimeCharacters[i];
                if (r?.characterSO != null)
                {
                    characters.Add(r.characterSO);
                }
            }

            character = null;
            currentRuntimeCharacter = null;

            RebuildAvailableCharacters();

            if (availableCharacters.Count == 0)
            {
                selectedCharacterIndex = -1;
                character = null;
                currentRuntimeCharacter = null;

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

            if (runtimeCharacters.Count == availableCharacters.Count)
            {
                currentRuntimeCharacter = runtimeCharacters[index];
            }
            else if (runtimeCharacters.Count > 0)
            {
                currentRuntimeCharacter = runtimeCharacters.Find(r => r != null && r.characterSO == character);
            }
            else
            {
                currentRuntimeCharacter = null;
            }

            UpdateCharacterTabSelection();

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

        public void ClearCharacterTabs()
        {
            for (int index = spawnedCharacterTabs.Count - 1; index >= 0; index--)
            {
                GameObject tab = spawnedCharacterTabs[index];
                if (tab == null)
                {
                    continue;
                }

                tab.SetActive(false);
                Destroy(tab);
            }

            spawnedCharacterTabs.Clear();
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
            EquipmentSkillInstanceData skillInstance =
                currentRuntimeCharacter?.GetSkillInstance(
                    SelectedSkill?.EquipmentId);
            skillPresenter.ShowSkill(SelectedSkill, skillInstance);
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

        private void UpdateCharacterTabSelection()
        {
            for (int index = 0; index < spawnedCharacterTabs.Count; index++)
            {
                GameObject tab = spawnedCharacterTabs[index];
                if (tab == null)
                {
                    continue;
                }

                bool selected = index == selectedCharacterIndex;
                Transform selectedState = tab.transform.Find("State_Selected");
                Transform unselectedState = tab.transform.Find("State_Unselected");
                Button button = tab.GetComponent<Button>();

                if (selectedState != null)
                {
                    selectedState.gameObject.SetActive(selected);
                }

                if (unselectedState != null)
                {
                    unselectedState.gameObject.SetActive(!selected);
                }

                if (button != null)
                {
                    button.interactable = !selected;
                }
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

        private static string BuildCharacterTabName(CharacterSO value, int index)
        {
            string key = value != null ? value.CharacterId : string.Empty;
            return string.IsNullOrWhiteSpace(key)
                ? $"PartyTab_{index + 1:00}"
                : $"PartyTab_{index + 1:00}_{key}";
        }

        private static Image FindPartyPortraitImage(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            string[] supportedNames =
            {
                "Portrait",
                "Bind_Portrait",
                "PortraitImage",
                "Image_Portrait",
                "(img)portrait",
            };

            Image[] images = root.GetComponentsInChildren<Image>(true);
            for (int nameIndex = 0; nameIndex < supportedNames.Length; nameIndex++)
            {
                for (int imageIndex = 0; imageIndex < images.Length; imageIndex++)
                {
                    Image image = images[imageIndex];
                    if (image != null && image.name == supportedNames[nameIndex])
                    {
                        return image;
                    }
                }
            }

            return null;
        }

        private static void BindPartyPortraitImage(Image image, Sprite portrait)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = portrait;
            image.preserveAspect = true;
            image.enabled = portrait != null;
            image.gameObject.SetActive(true);
        }

        private static void EnsureButtonHitArea(Button button)
        {
            if (button == null)
            {
                return;
            }

            Image hitArea = button.GetComponent<Image>();
            if (hitArea == null)
            {
                hitArea = button.gameObject.AddComponent<Image>();
                hitArea.sprite = null;
                hitArea.color = Color.clear;
            }

            hitArea.raycastTarget = true;
            button.targetGraphic = hitArea;
        }
    }
}
