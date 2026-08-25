using System.Collections.Generic;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectBS.UI.EditorTools
{
    public static class SkillUpgradeViewPrefabBuilder
    {
        private const string SkillUpgradeViewPath =
            "Assets/Prefabs/UI/Fixed/SkillUpgradeView.prefab";
        private const string OptionCardPath =
            "Assets/Prefabs/UI/Fixed/UISkillUpButton.prefab";
        private const string SkillContentInfoPath =
            "Assets/Prefabs/UI/Fixed/Content/UIContentInfoView_Skill.prefab";
        private const string PopupRegistryPath =
            "Assets/Resources/ui/PopupViewRegistrySO.asset";

        [MenuItem(
            "Tools/ProjectBS/UI/Skill Upgrade/Rebuild Prefabs (Overwrite Layout)")]
        public static void RebuildPrefabs()
        {
            GameObject contentInfoPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(SkillContentInfoPath);

            if (contentInfoPrefab == null
                || contentInfoPrefab.GetComponent<UIContentInfoView>() == null)
            {
                throw new System.InvalidOperationException(
                    $"Required UIContentInfoView prefab is missing or invalid: {SkillContentInfoPath}");
            }

            GameObject optionCardPrefab =
                BuildOptionCardPrefab(contentInfoPrefab);
            BuildSkillUpgradeViewPrefab(optionCardPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            VerifySavedPrefabs();

            Debug.Log(
                "[SkillUpgradeViewPrefabBuilder] Skill upgrade prefabs rebuilt and verified. "
                + "The previous prefab layouts were intentionally overwritten.");
        }

        [MenuItem("Tools/ProjectBS/UI/Skill Upgrade/Verify Prefabs")]
        public static void VerifyPrefabs()
        {
            VerifySavedPrefabs();
            Debug.Log(
                "[SkillUpgradeViewPrefabBuilder] Skill upgrade prefab verification passed.");
        }

        private static GameObject BuildOptionCardPrefab(
            GameObject contentInfoPrefab)
        {
            GameObject root =
                CreateRect(
                    "UISkillUpButton",
                    null,
                    new Vector2(540f, 760f));

            try
            {
                Image background = root.AddComponent<Image>();
                background.sprite = BuiltinSprite();
                background.type = Image.Type.Sliced;
                background.color = new Color(0.11f, 0.095f, 0.075f, 0.98f);
                background.raycastTarget = true;

                Button button = root.AddComponent<Button>();
                button.targetGraphic = background;
                button.transition = Selectable.Transition.ColorTint;
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 0.90f, 0.64f, 1f);
                colors.pressedColor = new Color(0.82f, 0.68f, 0.42f, 1f);
                colors.selectedColor = colors.highlightedColor;
                colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
                button.colors = colors;

                LayoutElement rootLayout = root.AddComponent<LayoutElement>();
                rootLayout.preferredWidth = 540f;
                rootLayout.preferredHeight = 760f;
                rootLayout.flexibleWidth = 1f;

                VerticalLayoutGroup layout =
                    root.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(18, 18, 18, 18);
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;

                UISkillUpgradeButton cardView =
                    root.AddComponent<UISkillUpgradeButton>();

                TMP_Text memberNameText =
                    CreateText(
                        "Bind_MemberNameText",
                        root.transform,
                        "Character",
                        28f,
                        TextAlignmentOptions.Center);
                AddLayoutElement(memberNameText.gameObject, 42f);

                TMP_Text levelText =
                    CreateText(
                        "Bind_LevelText",
                        root.transform,
                        "Lv.1 → Lv.2",
                        24f,
                        TextAlignmentOptions.Center);
                AddLayoutElement(levelText.gameObject, 38f);

                GameObject contentInfoObject =
                    (GameObject)PrefabUtility.InstantiatePrefab(
                        contentInfoPrefab,
                        root.transform);
                contentInfoObject.name = "Bind_ContentInfoView";
                ResetRect(contentInfoObject.GetComponent<RectTransform>());
                LayoutElement contentLayout =
                    contentInfoObject.GetComponent<LayoutElement>()
                    ?? contentInfoObject.AddComponent<LayoutElement>();
                contentLayout.minHeight = 500f;
                contentLayout.preferredHeight = 560f;
                contentLayout.flexibleHeight = 1f;
                contentLayout.flexibleWidth = 1f;

                UIContentInfoView contentInfoView =
                    contentInfoObject.GetComponent<UIContentInfoView>();

                Transform contentBody =
                    FindDescendantByName(
                        contentInfoObject.transform,
                        "Body");
                Transform scrollRect =
                    FindDescendantByName(
                        contentInfoObject.transform,
                        "Info_ScrollRect");
                Transform groupRoot =
                    FindDescendantByName(
                        contentInfoObject.transform,
                        "Info_GroupRoot");
                Transform statusText =
                    FindDescendantByName(
                        contentInfoObject.transform,
                        "Info_StatusText");

                if (contentBody == null
                    || scrollRect == null
                    || groupRoot == null
                    || statusText == null)
                {
                    throw new System.InvalidOperationException(
                        "UIContentInfoView_Skill Body hierarchy does not match the expected layout.");
                }

                scrollRect.gameObject.SetActive(false);
                groupRoot.gameObject.SetActive(false);
                statusText.gameObject.SetActive(false);

                SetObjectReferences(
                    contentInfoView,
                    new Dictionary<string, Object>
                    {
                        { "scrollRect", null },
                        { "groupRoot", null },
                        { "statusText", null }
                    });

                TMP_Text comparisonText =
                    CreateText(
                        "Bind_StatComparisonText",
                        contentBody,
                        "Upgrade comparison",
                        19f,
                        TextAlignmentOptions.TopLeft);
                comparisonText.textWrappingMode = TextWrappingModes.Normal;
                LayoutElement comparisonLayout =
                    AddLayoutElement(comparisonText.gameObject, 180f);
                comparisonLayout.minHeight = 120f;
                comparisonLayout.flexibleHeight = 1f;

                SetObjectReferences(
                    cardView,
                    new Dictionary<string, Object>
                    {
                        { "button", button },
                        { "backgroundImage", background },
                        { "contentInfoView", contentInfoView },
                        { "memberNameText", memberNameText },
                        { "levelText", levelText },
                        { "statComparisonText", comparisonText }
                    });

                return PrefabUtility.SaveAsPrefabAsset(root, OptionCardPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void BuildSkillUpgradeViewPrefab(
            GameObject optionCardPrefab)
        {
            if (optionCardPrefab == null
                || optionCardPrefab.GetComponent<UISkillUpgradeButton>() == null)
            {
                throw new System.InvalidOperationException(
                    $"Option card prefab is missing or invalid: {OptionCardPath}");
            }

            GameObject root =
                CreateRect(
                    "SkillUpgradeView",
                    null,
                    new Vector2(2560f, 1440f));
            SetStretch(root.GetComponent<RectTransform>());

            try
            {
                SkillUpgradeView view = root.AddComponent<SkillUpgradeView>();

                Image dimmer =
                    CreateImage(
                        "Dimmer",
                        root.transform,
                        new Color(0.015f, 0.012f, 0.01f, 0.82f),
                        true);
                SetStretch(dimmer.rectTransform);

                GameObject panelRoot =
                    CreateRect(
                        "Bind_PanelRoot",
                        root.transform,
                        new Vector2(1880f, 1080f));
                Image panelBackground = panelRoot.AddComponent<Image>();
                panelBackground.sprite = BuiltinSprite();
                panelBackground.type = Image.Type.Sliced;
                panelBackground.color = new Color(0.08f, 0.065f, 0.05f, 1f);
                panelBackground.raycastTarget = true;

                VerticalLayoutGroup panelLayout =
                    panelRoot.AddComponent<VerticalLayoutGroup>();
                panelLayout.padding = new RectOffset(48, 48, 38, 38);
                panelLayout.spacing = 24f;
                panelLayout.childAlignment = TextAnchor.UpperCenter;
                panelLayout.childControlWidth = true;
                panelLayout.childControlHeight = true;
                panelLayout.childForceExpandWidth = true;
                panelLayout.childForceExpandHeight = false;

                GameObject header =
                    CreateRect(
                        "Header",
                        panelRoot.transform,
                        new Vector2(1784f, 108f));
                AddLayoutElement(header, 108f);
                VerticalLayoutGroup headerLayout =
                    header.AddComponent<VerticalLayoutGroup>();
                headerLayout.spacing = 8f;
                headerLayout.childAlignment = TextAnchor.MiddleCenter;
                headerLayout.childControlWidth = true;
                headerLayout.childControlHeight = true;
                headerLayout.childForceExpandWidth = true;
                headerLayout.childForceExpandHeight = false;

                TMP_Text titleText =
                    CreateText(
                        "Bind_TitleText",
                        header.transform,
                        "스킬 업그레이드 선택",
                        46f,
                        TextAlignmentOptions.Center);
                AddLayoutElement(titleText.gameObject, 58f);

                TMP_Text statusText =
                    CreateText(
                        "Bind_StatusText",
                        header.transform,
                        "업그레이드할 스킬을 선택하세요.",
                        24f,
                        TextAlignmentOptions.Center);
                statusText.color = new Color(0.78f, 0.72f, 0.62f, 1f);
                AddLayoutElement(statusText.gameObject, 36f);

                GameObject optionContainerObject =
                    CreateRect(
                        "Bind_OptionContainer",
                        panelRoot.transform,
                        new Vector2(1784f, 820f));
                LayoutElement optionLayoutElement =
                    optionContainerObject.AddComponent<LayoutElement>();
                optionLayoutElement.minHeight = 760f;
                optionLayoutElement.preferredHeight = 820f;
                optionLayoutElement.flexibleHeight = 1f;

                HorizontalLayoutGroup optionLayout =
                    optionContainerObject.AddComponent<HorizontalLayoutGroup>();
                optionLayout.spacing = 24f;
                optionLayout.childAlignment = TextAnchor.MiddleCenter;
                optionLayout.childControlWidth = true;
                optionLayout.childControlHeight = true;
                optionLayout.childForceExpandWidth = true;
                optionLayout.childForceExpandHeight = true;

                GameObject closeButtonObject =
                    CreateRect(
                        "Bind_CloseButton",
                        panelRoot.transform,
                        new Vector2(240f, 64f));
                Image closeBackground = closeButtonObject.AddComponent<Image>();
                closeBackground.sprite = BuiltinSprite();
                closeBackground.type = Image.Type.Sliced;
                closeBackground.color = new Color(0.26f, 0.20f, 0.13f, 1f);
                Button closeButton = closeButtonObject.AddComponent<Button>();
                closeButton.targetGraphic = closeBackground;
                AddLayoutElement(closeButtonObject, 64f);

                TMP_Text closeLabel =
                    CreateText(
                        "Label",
                        closeButtonObject.transform,
                        "닫기",
                        24f,
                        TextAlignmentOptions.Center);
                SetStretch(closeLabel.rectTransform);
                closeButtonObject.SetActive(false);

                SetObjectReferences(
                    view,
                    new Dictionary<string, Object>
                    {
                        { "panelRoot", panelRoot },
                        { "titleText", titleText },
                        { "statusText", statusText },
                        { "optionContainer", optionContainerObject.transform },
                        {
                            "optionCardPrefab",
                            optionCardPrefab.GetComponent<UISkillUpgradeButton>()
                        },
                        { "closeButton", closeButton }
                    });

                PrefabUtility.SaveAsPrefabAsset(root, SkillUpgradeViewPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void VerifySavedPrefabs()
        {
            VerifyOptionCardPrefab();
            VerifySkillUpgradeViewPrefab();
            VerifyPopupRegistry();
        }

        private static void VerifyOptionCardPrefab()
        {
            GameObject root =
                PrefabUtility.LoadPrefabContents(OptionCardPath);

            try
            {
                ThrowIfMissingScripts(root, OptionCardPath);

                UISkillUpgradeButton card =
                    root.GetComponent<UISkillUpgradeButton>();
                Button button = root.GetComponent<Button>();
                UIContentInfoView contentView =
                    root.GetComponentInChildren<UIContentInfoView>(true);

                if (card == null || button == null || contentView == null)
                {
                    throw new System.InvalidOperationException(
                        "Saved skill upgrade option card is missing its View, Button, or UIContentInfoView.");
                }

                SerializedObject cardObject = new SerializedObject(card);
                RequireReference(cardObject, "button");
                RequireReference(cardObject, "backgroundImage");
                RequireReference(cardObject, "contentInfoView");
                RequireReference(cardObject, "memberNameText");
                RequireReference(cardObject, "levelText");
                RequireReference(cardObject, "statComparisonText");

                Object source =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        contentView.gameObject);
                string sourcePath = AssetDatabase.GetAssetPath(source);

                if (sourcePath != SkillContentInfoPath)
                {
                    throw new System.InvalidOperationException(
                        $"Nested content View source is '{sourcePath}', expected '{SkillContentInfoPath}'.");
                }

                SerializedObject contentObject =
                    new SerializedObject(contentView);
                RequireReference(contentObject, "tagPrefab");
                RequireReference(contentObject, "groupPrefab");
                RequireNullReference(contentObject, "scrollRect");
                RequireNullReference(contentObject, "groupRoot");
                RequireNullReference(contentObject, "statusText");

                Transform comparisonTransform =
                    cardObject.FindProperty("statComparisonText")
                        .objectReferenceValue is TMP_Text comparison
                        ? comparison.transform
                        : null;
                Transform body =
                    FindDescendantByName(contentView.transform, "Body");
                Transform scroll =
                    FindDescendantByName(
                        contentView.transform,
                        "Info_ScrollRect");
                Transform group =
                    FindDescendantByName(
                        contentView.transform,
                        "Info_GroupRoot");
                Transform status =
                    FindDescendantByName(
                        contentView.transform,
                        "Info_StatusText");

                if (body == null
                    || comparisonTransform == null
                    || comparisonTransform.parent != body
                    || scroll == null
                    || scroll.gameObject.activeSelf
                    || group == null
                    || group.gameObject.activeSelf
                    || status == null
                    || status.gameObject.activeSelf)
                {
                    throw new System.InvalidOperationException(
                        "Saved option card must show only Bind_StatComparisonText in the content Body.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void VerifySkillUpgradeViewPrefab()
        {
            GameObject root =
                PrefabUtility.LoadPrefabContents(SkillUpgradeViewPath);

            try
            {
                ThrowIfMissingScripts(root, SkillUpgradeViewPath);

                SkillUpgradeView view = root.GetComponent<SkillUpgradeView>();
                if (view == null)
                {
                    throw new System.InvalidOperationException(
                        "Saved SkillUpgradeView component is missing.");
                }

                SerializedObject viewObject = new SerializedObject(view);
                RequireReference(viewObject, "panelRoot");
                RequireReference(viewObject, "titleText");
                RequireReference(viewObject, "statusText");
                RequireReference(viewObject, "optionContainer");
                RequireReference(viewObject, "optionCardPrefab");
                RequireReference(viewObject, "closeButton");

                Transform optionContainer =
                    viewObject.FindProperty("optionContainer")
                        .objectReferenceValue as Transform;

                if (optionContainer == null || optionContainer.childCount != 0)
                {
                    throw new System.InvalidOperationException(
                        "Saved option container must exist and start without preview children.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void VerifyPopupRegistry()
        {
            PopupViewRegistrySO registry =
                AssetDatabase.LoadAssetAtPath<PopupViewRegistrySO>(
                    PopupRegistryPath);
            SkillUpgradeView savedView =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    SkillUpgradeViewPath)
                    ?.GetComponent<SkillUpgradeView>();

            if (registry == null
                || !registry.TryGetConfig(
                    PopupType.SkillUpgrade,
                    out PopupViewConfig config)
                || config.prefab != savedView)
            {
                throw new System.InvalidOperationException(
                    "Popup registry no longer points to the saved SkillUpgradeView prefab.");
            }
        }

        private static GameObject CreateRect(
            string name,
            Transform parent,
            Vector2 size)
        {
            GameObject gameObject =
                new GameObject(name, typeof(RectTransform));
            gameObject.layer = 5;

            RectTransform rect =
                gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            return gameObject;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Color color,
            bool raycastTarget)
        {
            GameObject gameObject =
                CreateRect(name, parent, Vector2.zero);
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            string text,
            float fontSize,
            TextAlignmentOptions alignment)
        {
            GameObject gameObject =
                CreateRect(name, parent, new Vector2(100f, 40f));
            TextMeshProUGUI label =
                gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = new Color(0.94f, 0.88f, 0.72f, 1f);
            label.raycastTarget = false;
            return label;
        }

        private static LayoutElement AddLayoutElement(
            GameObject target,
            float preferredHeight)
        {
            LayoutElement layout =
                target.GetComponent<LayoutElement>()
                ?? target.AddComponent<LayoutElement>();
            layout.preferredHeight = preferredHeight;
            return layout;
        }

        private static void ResetRect(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void SetStretch(RectTransform rect)
        {
            ResetRect(rect);
        }

        private static Sprite BuiltinSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>(
                "UI/Skin/UISprite.psd");
        }

        private static void SetObjectReferences(
            Object target,
            IReadOnlyDictionary<string, Object> references)
        {
            SerializedObject serializedObject =
                new SerializedObject(target);

            foreach (KeyValuePair<string, Object> pair in references)
            {
                SerializedProperty property =
                    serializedObject.FindProperty(pair.Key);

                if (property == null)
                {
                    throw new System.InvalidOperationException(
                        $"Serialized property '{pair.Key}' was not found on {target.GetType().Name}.");
                }

                property.objectReferenceValue = pair.Value;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RequireReference(
            SerializedObject target,
            string propertyName)
        {
            SerializedProperty property =
                target.FindProperty(propertyName);

            if (property == null
                || property.objectReferenceValue == null)
            {
                throw new System.InvalidOperationException(
                    $"Required reference '{propertyName}' is missing on {target.targetObject.name}.");
            }
        }

        private static void RequireNullReference(
            SerializedObject target,
            string propertyName)
        {
            SerializedProperty property =
                target.FindProperty(propertyName);

            if (property == null
                || property.objectReferenceValue != null)
            {
                throw new System.InvalidOperationException(
                    $"Reference '{propertyName}' must be empty on {target.targetObject.name}.");
            }
        }

        private static Transform FindDescendantByName(
            Transform root,
            string objectName)
        {
            if (root == null || string.IsNullOrWhiteSpace(objectName))
                return null;

            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(true);

            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == objectName)
                    return transforms[index];
            }

            return null;
        }

        private static void ThrowIfMissingScripts(
            GameObject root,
            string assetPath)
        {
            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(true);

            for (int index = 0; index < transforms.Length; index++)
            {
                GameObject current = transforms[index].gameObject;
                int missingCount =
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        current);

                if (missingCount > 0)
                {
                    throw new System.InvalidOperationException(
                        $"Prefab '{assetPath}' contains {missingCount} missing script(s) on '{current.name}'.");
                }
            }
        }
    }
}
