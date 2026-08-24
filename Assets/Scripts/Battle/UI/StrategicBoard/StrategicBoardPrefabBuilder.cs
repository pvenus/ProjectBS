#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI.StrategicBoard.Editor
{
    public static class StrategicBoardPrefabBuilder
    {
        private const string OutputFolder = "Assets/Prefabs/UI/Fixed/Battle/StrategicBoard";
        private const string GaugePath = OutputFolder + "/StrategicGauge.prefab";
        private const string SlotPath = OutputFolder + "/StrategicSkillSlot.prefab";
        private const string BoardPath = OutputFolder + "/StrategicBoard.prefab";
        private const string TargetingGuidePath = OutputFolder + "/StrategicSkillTargetingGuide.prefab";

        [MenuItem("Tools/Battle/Build Strategic Board Prototype")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI/Fixed/Battle"))
            {
                throw new System.InvalidOperationException(
                    "Shared parent Assets/Prefabs/UI/Fixed/Battle must exist before building the strategic board.");
            }

            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs/UI/Fixed/Battle", "StrategicBoard");
            }

            GameObject gaugePrefab = BuildGaugePrefab();
            GameObject slotPrefab = BuildSlotPrefab();
            BuildBoardPrefab(gaugePrefab, slotPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Battle/Repair Strategic Board References")]
        public static void RepairBoardReferences()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(BoardPath);

            try
            {
                EnsureBoardReferences(root);
                PrefabUtility.SaveAsPrefabAsset(root, BoardPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            VerifySavedBoardReferences();
            Debug.Log("[StrategicBoardPrefabBuilder] Strategic board references repaired and verified.");
        }

        private static GameObject BuildGaugePrefab()
        {
            GameObject root = CreateRect("StrategicGauge", null, new Vector2(240f, 240f));
            StrategicGaugeView view = root.AddComponent<StrategicGaugeView>();
            StrategicGaugeBinder binder = root.AddComponent<StrategicGaugeBinder>();

            Image frame = CreateImage("GaugeFrame", root.transform, new Vector2(226f, 226f),
                new Color(0.30f, 0.20f, 0.09f, 1f), BuiltinKnob());
            Image background = CreateImage("GaugeBackground", root.transform, new Vector2(208f, 208f),
                new Color(0.055f, 0.06f, 0.055f, 1f), BuiltinKnob());
            Image fill = CreateImage("GaugeFill", root.transform, new Vector2(190f, 190f),
                new Color(0.80f, 0.30f, 0.12f, 1f), BuiltinKnob());
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Top;
            fill.fillClockwise = true;
            fill.fillAmount = 0.72f;

            TMP_Text valueText = CreateText("GaugeValueText", root.transform, "72 / 100", 30f,
                new Vector2(180f, 50f), new Vector2(0f, -6f));
            TMP_Text rateText = CreateText("ChargePerSecondText", root.transform, "+2/s", 22f,
                new Vector2(160f, 40f), new Vector2(0f, -52f));

            SetObjectReferences(view, new Dictionary<string, Object>
            {
                { "backgroundImage", background },
                { "fillImage", fill },
                { "frameImage", frame },
                { "currentMaxText", valueText },
                { "chargePerSecondText", rateText }
            });
            SetObjectReferences(binder, new Dictionary<string, Object>
            {
                { "gaugeView", view }
            });
            SetFloat(binder, "chargePerSecond", 2f);

            view.SetGauge(72, 100);
            view.SetChargePerSecond(2f);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, GaugePath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildSlotPrefab()
        {
            GameObject root = CreateRect("StrategicSkillSlot", null, new Vector2(184f, 246f));
            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            StrategicSkillSlotView view = root.AddComponent<StrategicSkillSlotView>();

            Image background = CreateImage("SlotBackground", root.transform, new Vector2(184f, 246f),
                new Color(0.12f, 0.105f, 0.08f, 1f), BuiltinSprite());
            Image icon = CreateImage("SkillIcon", root.transform, new Vector2(142f, 154f),
                new Color(0.72f, 0.69f, 0.59f, 1f), BuiltinSprite());
            SetAnchoredPosition(icon.rectTransform, new Vector2(0f, 23f));

            Image costPlate = CreateImage("CostPlate", root.transform, new Vector2(96f, 42f),
                new Color(0.06f, 0.055f, 0.045f, 0.96f), BuiltinSprite());
            SetAnchoredPosition(costPlate.rectTransform, new Vector2(0f, -91f));
            TMP_Text costText = CreateText("CostText", costPlate.transform, string.Empty, 27f,
                new Vector2(90f, 38f), Vector2.zero);

            Image selection = CreateImage("SelectionOverlay", root.transform, new Vector2(174f, 236f),
                new Color(0.95f, 0.72f, 0.20f, 0.28f), BuiltinSprite());
            Image insufficient = CreateImage("InsufficientResourceOverlay", root.transform,
                new Vector2(174f, 236f), new Color(0.55f, 0.04f, 0.03f, 0.48f), BuiltinSprite());
            Image empty = CreateImage("EmptySlotOverlay", root.transform, new Vector2(174f, 236f),
                new Color(0.06f, 0.06f, 0.06f, 0.66f), BuiltinSprite());
            Image locked = CreateImage("LockOverlay", root.transform, new Vector2(174f, 236f),
                new Color(0.015f, 0.015f, 0.015f, 0.82f), BuiltinSprite());

            selection.enabled = false;
            insufficient.enabled = false;
            locked.enabled = false;

            SetObjectReferences(view, new Dictionary<string, Object>
            {
                { "backgroundImage", background },
                { "iconImage", icon },
                { "costText", costText },
                { "selectionImage", selection },
                { "insufficientResourceImage", insufficient },
                { "emptySlotImage", empty },
                { "lockImage", locked },
                { "canvasGroup", canvasGroup },
                { "dragVisual", root.GetComponent<RectTransform>() }
            });

            view.ClearContent();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SlotPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BuildBoardPrefab(GameObject gaugePrefab, GameObject slotPrefab)
        {
            GameObject root = CreateRect("StrategicBoard", null, new Vector2(1500f, 320f));
            StrategicBoardView boardView = root.AddComponent<StrategicBoardView>();
            root.AddComponent<StrategicBoardPresenter>();

            Image frame = CreateImage("BoardFrame", root.transform, new Vector2(1500f, 320f),
                new Color(0.25f, 0.17f, 0.085f, 1f), BuiltinSprite());
            Image background = CreateImage("BoardBackground", root.transform, new Vector2(1468f, 288f),
                new Color(0.035f, 0.035f, 0.03f, 0.96f), BuiltinSprite());

            GameObject gauge = (GameObject)PrefabUtility.InstantiatePrefab(gaugePrefab, root.transform);
            gauge.name = "SharedGauge";
            RectTransform gaugeRect = gauge.GetComponent<RectTransform>();
            SetAnchoredPosition(gaugeRect, new Vector2(-570f, 0f));

            GameObject slotRootObject = CreateRect("StrategicSkillSlots", root.transform, new Vector2(900f, 250f));
            RectTransform slotRoot = slotRootObject.GetComponent<RectTransform>();
            SetAnchoredPosition(slotRoot, new Vector2(230f, 0f));

            var slots = new List<StrategicSkillSlotView>();
            StrategicSkillSlotState[] states =
            {
                StrategicSkillSlotState.Ready,
                StrategicSkillSlotState.InsufficientResource,
                StrategicSkillSlotState.Empty,
                StrategicSkillSlotState.Locked
            };
            int[] costs = { 30, 50, 40, 0 };

            for (int i = 0; i < states.Length; i++)
            {
                GameObject slotObject = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, slotRoot);
                slotObject.name = $"StrategicSkillSlot_{i + 1}";
                RectTransform slotRect = slotObject.GetComponent<RectTransform>();
                SetAnchoredPosition(slotRect, new Vector2(-300f + i * 205f, 0f));

                StrategicSkillSlotView slot = slotObject.GetComponent<StrategicSkillSlotView>();
                slot.SetSlotId($"strategic-slot-{i + 1}");

                if (states[i] != StrategicSkillSlotState.Empty)
                {
                    slot.SetContent(BuiltinSprite(), costs[i]);
                }

                slot.SetState(states[i]);
                slots.Add(slot);
            }

            SetObjectReferences(boardView, new Dictionary<string, Object>
            {
                { "backgroundImage", background },
                { "frameImage", frame },
                { "gaugeView", gauge.GetComponent<StrategicGaugeView>() },
                { "slotRoot", slotRoot }
            });
            EnsureBoardReferences(root);

            PrefabUtility.SaveAsPrefabAsset(root, BoardPath);
            Object.DestroyImmediate(root);
            VerifySavedBoardReferences();
        }

        private static void EnsureBoardReferences(GameObject root)
        {
            StrategicBoardView boardView = root.GetComponent<StrategicBoardView>();

            if (boardView == null || boardView.SlotRoot == null)
            {
                throw new System.InvalidOperationException(
                    "StrategicBoardView or its slotRoot reference is missing.");
            }

            var slots = new List<StrategicSkillSlotView>(
                boardView.SlotRoot.GetComponentsInChildren<StrategicSkillSlotView>(true));
            slots.Sort((left, right) =>
                left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex()));

            if (slots.Count != 4)
            {
                throw new System.InvalidOperationException(
                    $"Expected exactly four strategic skill slots, but found {slots.Count}.");
            }

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].SetSlotId($"strategic-slot-{i + 1}");
            }

            SetObjectList(boardView, "slots", slots);

            StrategicGaugeBinder binder = root.GetComponentInChildren<StrategicGaugeBinder>(true);

            if (binder == null)
            {
                throw new System.InvalidOperationException(
                    "The nested StrategicGaugeBinder is missing.");
            }

            var binderObject = new SerializedObject(binder);
            binderObject.FindProperty("boardView").objectReferenceValue = boardView;
            binderObject.FindProperty("managerOverride").objectReferenceValue = null;
            binderObject.FindProperty("findManagerInScene").boolValue = true;
            binderObject.ApplyModifiedPropertiesWithoutUndo();

            StrategicBoardPresenter presenter = root.GetComponent<StrategicBoardPresenter>();

            if (presenter == null)
            {
                presenter = root.AddComponent<StrategicBoardPresenter>();
            }

            GameObject guidePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TargetingGuidePath);
            var presenterObject = new SerializedObject(presenter);
            presenterObject.FindProperty("boardView").objectReferenceValue = boardView;
            presenterObject.FindProperty("targetingGuidePrefab").objectReferenceValue =
                guidePrefab != null
                    ? guidePrefab.GetComponent<StrategicSkillTargetingGuideView>()
                    : null;
            presenterObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void VerifySavedBoardReferences()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(BoardPath);

            try
            {
                StrategicBoardView boardView = root.GetComponent<StrategicBoardView>();

                if (boardView == null)
                {
                    throw new System.InvalidOperationException("Saved StrategicBoardView is missing.");
                }

                var boardObject = new SerializedObject(boardView);
                SerializedProperty slotsProperty = boardObject.FindProperty("slots");
                var slotIds = new HashSet<string>();

                if (slotsProperty.arraySize != 4)
                {
                    throw new System.InvalidOperationException(
                        $"Saved slots array size is {slotsProperty.arraySize}, expected 4.");
                }

                for (int i = 0; i < slotsProperty.arraySize; i++)
                {
                    var slot = slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue
                        as StrategicSkillSlotView;

                    if (slot == null)
                    {
                        throw new System.InvalidOperationException(
                            $"Saved slot reference {i + 1} is null.");
                    }

                    string expectedId = $"strategic-slot-{i + 1}";

                    if (slot.SlotId != expectedId || !slotIds.Add(slot.SlotId))
                    {
                        throw new System.InvalidOperationException(
                            $"Saved slot {i + 1} has invalid or duplicate id '{slot.SlotId}'.");
                    }
                }

                StrategicGaugeBinder binder = root.GetComponentInChildren<StrategicGaugeBinder>(true);

                if (binder == null)
                {
                    throw new System.InvalidOperationException("Saved StrategicGaugeBinder is missing.");
                }

                var binderObject = new SerializedObject(binder);

                if (binderObject.FindProperty("boardView").objectReferenceValue != boardView ||
                    binderObject.FindProperty("managerOverride").objectReferenceValue != null ||
                    !binderObject.FindProperty("findManagerInScene").boolValue)
                {
                    throw new System.InvalidOperationException(
                        "Saved gauge binder references are invalid.");
                }

                StrategicBoardPresenter presenter = root.GetComponent<StrategicBoardPresenter>();
                GameObject guidePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TargetingGuidePath);
                var presenterObject = presenter != null ? new SerializedObject(presenter) : null;

                if (presenterObject == null ||
                    presenterObject.FindProperty("boardView").objectReferenceValue != boardView ||
                    guidePrefab == null ||
                    presenterObject.FindProperty("targetingGuidePrefab").objectReferenceValue !=
                    guidePrefab.GetComponent<StrategicSkillTargetingGuideView>())
                {
                    throw new System.InvalidOperationException(
                        "Saved strategic board presenter references are invalid.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 size)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.layer = 5;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return gameObject;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Vector2 size,
            Color color,
            Sprite sprite)
        {
            GameObject gameObject = CreateRect(name, parent, size);
            var image = gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            string text,
            float fontSize,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            GameObject gameObject = CreateRect(name, parent, size);
            SetAnchoredPosition(gameObject.GetComponent<RectTransform>(), anchoredPosition);
            var label = gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.94f, 0.87f, 0.70f, 1f);
            label.raycastTarget = false;
            return label;
        }

        private static Sprite BuiltinSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static Sprite BuiltinKnob()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        private static void SetAnchoredPosition(RectTransform rect, Vector2 position)
        {
            rect.anchoredPosition = position;
        }

        private static void SetObjectReferences(Object target, Dictionary<string, Object> references)
        {
            var serializedObject = new SerializedObject(target);

            foreach (KeyValuePair<string, Object> pair in references)
            {
                serializedObject.FindProperty(pair.Key).objectReferenceValue = pair.Value;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectList<T>(Object target, string propertyName, List<T> values)
            where T : Object
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Count;

            for (int i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
