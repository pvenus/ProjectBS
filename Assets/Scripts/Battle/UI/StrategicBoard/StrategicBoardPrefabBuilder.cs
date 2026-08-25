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
            RepairGaugePrefabSettings();
            RepairSlotPrefabReferences();
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
            SetFloat(view, "increaseTweenDuration", 0.25f);
            SetFloat(binder, "fallbackChargePerSecond", 0f);

            view.SetGauge(72, 100);
            view.SetChargePerSecond(0f);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, GaugePath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildSlotPrefab()
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPath);

            if (existingPrefab != null)
            {
                RepairSlotPrefabReferences();
                return AssetDatabase.LoadAssetAtPath<GameObject>(SlotPath);
            }

            GameObject root = CreateRect("StrategicSkillSlot", null, new Vector2(184f, 246f));
            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            StrategicSkillSlotView view = root.AddComponent<StrategicSkillSlotView>();

            Image active = CreateImage("Active", root.transform, new Vector2(142f, 154f),
                new Color(0.72f, 0.69f, 0.59f, 1f), BuiltinSprite());
            SetAnchoredPosition(active.rectTransform, new Vector2(0f, 23f));

            Image empty = CreateImage("Empty", root.transform, new Vector2(174f, 236f),
                new Color(0.06f, 0.06f, 0.06f, 0.66f), BuiltinSprite());

            Image costPlate = CreateImage("CostPlate", root.transform, new Vector2(96f, 42f),
                new Color(0.06f, 0.055f, 0.045f, 0.96f), BuiltinSprite());
            SetAnchoredPosition(costPlate.rectTransform, new Vector2(0f, -91f));
            TMP_Text costText = CreateText("CostText", costPlate.transform, string.Empty, 27f,
                new Vector2(90f, 38f), Vector2.zero);

            GameObject overlay = CreateRect("Overlay", root.transform, new Vector2(174f, 236f));
            Image selection = CreateImage("Selection", overlay.transform, new Vector2(142f, 154f),
                new Color(0.95f, 0.72f, 0.20f, 0.28f), BuiltinSprite());
            Image insufficient = CreateImage("Insufficient", overlay.transform,
                new Vector2(174f, 236f), new Color(0.55f, 0.04f, 0.03f, 0.48f), BuiltinSprite());
            Image fillMask = CreateImage("FillMask", insufficient.transform,
                new Vector2(174f, 236f), Color.white, BuiltinSprite());
            fillMask.type = Image.Type.Filled;
            fillMask.fillMethod = Image.FillMethod.Vertical;
            fillMask.fillOrigin = (int)Image.OriginVertical.Bottom;
            fillMask.fillAmount = 1f;
            Mask mask = fillMask.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            CreateImage("InsufficientTint", fillMask.transform, new Vector2(174f, 236f),
                new Color(0.55f, 0.04f, 0.03f, 0.48f), BuiltinSprite());
            Image locked = CreateImage("Lock", overlay.transform, new Vector2(174f, 236f),
                new Color(0.015f, 0.015f, 0.015f, 0.82f), BuiltinSprite());

            SetObjectReferences(view, new Dictionary<string, Object>
            {
                { "activeRoot", active.rectTransform },
                { "iconImage", active },
                { "emptyRoot", empty.gameObject },
                { "selectionRoot", selection.gameObject },
                { "selectionImage", selection },
                { "insufficientRoot", insufficient.gameObject },
                { "insufficientFillImage", fillMask },
                { "overlayLockRoot", locked.gameObject },
                { "costText", costText },
                { "canvasGroup", canvasGroup },
                { "dragVisual", root.GetComponent<RectTransform>() }
            });

            view.ClearContent();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SlotPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void RepairGaugePrefabSettings()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(GaugePath);

            try
            {
                StrategicGaugeView view = root.GetComponent<StrategicGaugeView>();
                StrategicGaugeBinder binder = root.GetComponent<StrategicGaugeBinder>();

                if (view == null || binder == null)
                {
                    throw new System.InvalidOperationException(
                        "StrategicGaugeView or StrategicGaugeBinder is missing from the gauge prefab.");
                }

                var viewObject = new SerializedObject(view);
                Image fillImage = viewObject.FindProperty("fillImage").objectReferenceValue as Image;

                if (fillImage == null ||
                    viewObject.FindProperty("currentMaxText").objectReferenceValue == null ||
                    viewObject.FindProperty("chargePerSecondText").objectReferenceValue == null)
                {
                    throw new System.InvalidOperationException(
                        "Strategic gauge Fill or TMP references are missing.");
                }

                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Radial360;
                fillImage.fillOrigin = (int)Image.Origin360.Top;
                fillImage.fillClockwise = true;
                viewObject.FindProperty("animateIncreases").boolValue = true;
                viewObject.FindProperty("increaseTweenDuration").floatValue = 0.25f;
                viewObject.ApplyModifiedPropertiesWithoutUndo();

                var binderObject = new SerializedObject(binder);
                binderObject.FindProperty("gaugeView").objectReferenceValue = view;
                binderObject.FindProperty("fallbackChargePerSecond").floatValue = 0f;
                binderObject.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, GaugePath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            VerifySavedGaugeSettings();
        }

        private static void VerifySavedGaugeSettings()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(GaugePath);

            try
            {
                StrategicGaugeView view = root.GetComponent<StrategicGaugeView>();
                StrategicGaugeBinder binder = root.GetComponent<StrategicGaugeBinder>();

                if (view == null || binder == null)
                {
                    throw new System.InvalidOperationException(
                        "Saved strategic gauge components are missing.");
                }

                var viewObject = new SerializedObject(view);
                Image fillImage = viewObject.FindProperty("fillImage").objectReferenceValue as Image;
                var binderObject = new SerializedObject(binder);

                if (fillImage == null ||
                    viewObject.FindProperty("currentMaxText").objectReferenceValue == null ||
                    viewObject.FindProperty("chargePerSecondText").objectReferenceValue == null ||
                    fillImage.type != Image.Type.Filled ||
                    fillImage.fillMethod != Image.FillMethod.Radial360 ||
                    !viewObject.FindProperty("animateIncreases").boolValue ||
                    viewObject.FindProperty("increaseTweenDuration").floatValue < 0f ||
                    binderObject.FindProperty("gaugeView").objectReferenceValue != view ||
                    !Mathf.Approximately(
                        binderObject.FindProperty("fallbackChargePerSecond").floatValue,
                        0f))
                {
                    throw new System.InvalidOperationException(
                        "Saved strategic gauge fill, tween, or binder settings are invalid.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RepairSlotPrefabReferences()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SlotPath);

            try
            {
                EnsureSlotReferences(root);
                PrefabUtility.SaveAsPrefabAsset(root, SlotPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            VerifySavedSlotReferences();
        }

        private static void EnsureSlotReferences(GameObject root)
        {
            StrategicSkillSlotView view = root.GetComponent<StrategicSkillSlotView>();
            Transform active = RequireChild(root.transform, "Active");
            Transform empty = RequireChild(root.transform, "Empty");
            Transform selection = RequireChild(root.transform, "Overlay/Selection");
            Transform insufficient = RequireChild(root.transform, "Overlay/Insufficient");
            Transform fillMask = RequireChild(root.transform, "Overlay/Insufficient/FillMask");
            Transform overlayLock = RequireChild(root.transform, "Overlay/Lock");
            Transform costText = RequireChild(root.transform, "CostPlate/CostText");

            if (view == null)
            {
                throw new System.InvalidOperationException(
                    "StrategicSkillSlotView is missing from the slot prefab root.");
            }

            Image activeImage = active.GetComponent<Image>();
            Image selectionImage = selection.GetComponent<Image>();
            Image fillImage = fillMask.GetComponent<Image>();
            TMP_Text costLabel = costText.GetComponent<TMP_Text>();

            if (activeImage == null || selectionImage == null ||
                fillImage == null || costLabel == null)
            {
                throw new System.InvalidOperationException(
                    "Strategic skill slot state images or cost label are missing.");
            }

            fillImage.type = Image.Type.Filled;
            SetObjectReferences(view, new Dictionary<string, Object>
            {
                { "activeRoot", active as RectTransform },
                { "iconImage", activeImage },
                { "emptyRoot", empty.gameObject },
                { "selectionRoot", selection.gameObject },
                { "selectionImage", selectionImage },
                { "insufficientRoot", insufficient.gameObject },
                { "insufficientFillImage", fillImage },
                { "overlayLockRoot", overlayLock.gameObject },
                { "costText", costLabel },
                { "canvasGroup", root.GetComponent<CanvasGroup>() },
                { "dragVisual", root.GetComponent<RectTransform>() }
            });
        }

        private static void VerifySavedSlotReferences()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SlotPath);

            try
            {
                StrategicSkillSlotView view = root.GetComponent<StrategicSkillSlotView>();

                if (view == null)
                {
                    throw new System.InvalidOperationException(
                        "Saved StrategicSkillSlotView is missing.");
                }

                var serializedView = new SerializedObject(view);
                string[] requiredReferences =
                {
                    "activeRoot",
                    "iconImage",
                    "emptyRoot",
                    "selectionRoot",
                    "selectionImage",
                    "insufficientRoot",
                    "insufficientFillImage",
                    "overlayLockRoot",
                    "costText",
                    "canvasGroup",
                    "dragVisual"
                };

                foreach (string propertyName in requiredReferences)
                {
                    if (serializedView.FindProperty(propertyName).objectReferenceValue == null)
                    {
                        throw new System.InvalidOperationException(
                            $"Saved slot reference '{propertyName}' is null.");
                    }
                }

                Image icon = serializedView.FindProperty("iconImage").objectReferenceValue as Image;
                Image selection = serializedView.FindProperty("selectionImage").objectReferenceValue as Image;
                Image fill = serializedView.FindProperty("insufficientFillImage").objectReferenceValue as Image;
                GameObject lockRoot = serializedView.FindProperty("overlayLockRoot").objectReferenceValue as GameObject;

                if (icon == selection || fill == null || fill.type != Image.Type.Filled ||
                    lockRoot == null || lockRoot.transform.parent == null ||
                    lockRoot.transform.parent.name != "Overlay")
                {
                    throw new System.InvalidOperationException(
                        "Saved slot images, FillMask type, or Overlay/Lock hierarchy are invalid.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform RequireChild(Transform root, string path)
        {
            Transform child = root.Find(path);

            if (child == null)
            {
                throw new System.InvalidOperationException(
                    $"Required strategic slot child '{path}' is missing.");
            }

            return child;
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

            if (boardView == null || boardView.GaugeView == null || boardView.SlotRoot == null)
            {
                throw new System.InvalidOperationException(
                    "StrategicBoardView, gaugeView, or slotRoot reference is missing.");
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
                RevertSlotVisualStateOverrides(slots[i]);
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
            binderObject.FindProperty("gaugeView").objectReferenceValue = boardView.GaugeView;
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
            if (guidePrefab == null)
            {
                throw new System.InvalidOperationException(
                    "The strategic targeting guide prefab is missing.");
            }

            var presenterObject = new SerializedObject(presenter);
            presenterObject.FindProperty("boardView").objectReferenceValue = boardView;
            presenterObject.FindProperty("targetingGuidePrefab").objectReferenceValue =
                guidePrefab.GetComponent<StrategicSkillTargetingGuideView>();
            presenterObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RevertSlotVisualStateOverrides(StrategicSkillSlotView slot)
        {
            Transform active = RequireChild(slot.transform, "Active");
            Transform empty = RequireChild(slot.transform, "Empty");
            Transform overlay = RequireChild(slot.transform, "Overlay");
            var stateObjects = new HashSet<GameObject>();
            var stateImages = new HashSet<Image>();

            RevertStateTreeOverrides(active, stateObjects, stateImages);
            RevertStateTreeOverrides(empty, stateObjects, stateImages);
            RevertStateTreeOverrides(overlay, stateObjects, stateImages);
            RemoveStaleStatePropertyModifications(slot, stateObjects, stateImages);
        }

        private static void RevertStateTreeOverrides(
            Transform root,
            HashSet<GameObject> stateObjects,
            HashSet<Image> stateImages)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (stateObjects.Add(child.gameObject))
                {
                    RevertPropertyOverride(child.gameObject, "m_IsActive");
                }
            }

            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (stateImages.Add(image))
                {
                    RevertPropertyOverride(image, "m_Enabled");
                }
            }
        }

        private static void RevertPropertyOverride(Object target, string propertyPath)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyPath);

            if (property != null && property.prefabOverride)
            {
                PrefabUtility.RevertPropertyOverride(
                    property,
                    InteractionMode.AutomatedAction);
            }
        }

        private static void RemoveStaleStatePropertyModifications(
            StrategicSkillSlotView slot,
            HashSet<GameObject> stateObjects,
            HashSet<Image> stateImages)
        {
            GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(slot.gameObject);

            if (instanceRoot == null)
            {
                return;
            }

            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(instanceRoot);

            if (modifications == null || modifications.Length == 0)
            {
                return;
            }

            HashSet<Object> stateSourceTargets = GetStateSourceTargets(stateObjects, stateImages);
            var filteredModifications = new List<PropertyModification>(modifications.Length);
            bool removedAny = false;

            foreach (PropertyModification modification in modifications)
            {
                if (IsStateVisualModification(modification, stateSourceTargets))
                {
                    removedAny = true;
                    continue;
                }

                filteredModifications.Add(modification);
            }

            if (removedAny)
            {
                PrefabUtility.SetPropertyModifications(
                    instanceRoot,
                    filteredModifications.ToArray());
            }
        }

        private static HashSet<Object> GetStateSourceTargets(
            HashSet<GameObject> stateObjects,
            HashSet<Image> stateImages)
        {
            var sourceTargets = new HashSet<Object>();

            foreach (GameObject stateObject in stateObjects)
            {
                Object source = PrefabUtility.GetCorrespondingObjectFromSource(stateObject);
                if (source != null)
                {
                    sourceTargets.Add(source);
                }
            }

            foreach (Image stateImage in stateImages)
            {
                Object source = PrefabUtility.GetCorrespondingObjectFromSource(stateImage);
                if (source != null)
                {
                    sourceTargets.Add(source);
                }
            }

            return sourceTargets;
        }

        private static bool IsStateVisualModification(
            PropertyModification modification,
            HashSet<Object> stateSourceTargets)
        {
            bool isStateProperty = modification.propertyPath == "m_IsActive" ||
                modification.propertyPath == "m_Enabled";

            if (!isStateProperty)
            {
                return false;
            }

            return modification.target == null || stateSourceTargets.Contains(modification.target);
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

                if (boardView.GaugeView == null || boardView.SlotRoot == null)
                {
                    throw new System.InvalidOperationException(
                        "Saved board gaugeView or slotRoot reference is missing.");
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

                    VerifyNoSlotVisualStateOverrides(slot);
                }

                VerifyGaugeFill(boardView.GaugeView);

                StrategicGaugeBinder binder = root.GetComponentInChildren<StrategicGaugeBinder>(true);

                if (binder == null)
                {
                    throw new System.InvalidOperationException("Saved StrategicGaugeBinder is missing.");
                }

                var binderObject = new SerializedObject(binder);

                if (binderObject.FindProperty("gaugeView").objectReferenceValue != boardView.GaugeView ||
                    binderObject.FindProperty("boardView").objectReferenceValue != boardView ||
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

        private static void VerifyNoSlotVisualStateOverrides(StrategicSkillSlotView slot)
        {
            Transform[] stateRoots =
            {
                RequireChild(slot.transform, "Active"),
                RequireChild(slot.transform, "Empty"),
                RequireChild(slot.transform, "Overlay")
            };

            var stateObjects = new HashSet<GameObject>();
            var stateImages = new HashSet<Image>();

            foreach (Transform stateRoot in stateRoots)
            {
                foreach (Transform child in stateRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (stateObjects.Add(child.gameObject) &&
                        HasPropertyOverride(child.gameObject, "m_IsActive"))
                    {
                        throw new System.InvalidOperationException(
                            $"Saved slot '{slot.SlotId}' retains an m_IsActive state override at '{child.name}'.");
                    }
                }

                foreach (Image image in stateRoot.GetComponentsInChildren<Image>(true))
                {
                    if (stateImages.Add(image) && HasPropertyOverride(image, "m_Enabled"))
                    {
                        throw new System.InvalidOperationException(
                            $"Saved slot '{slot.SlotId}' retains an Image.m_Enabled state override at '{image.name}'.");
                    }
                }
            }

            GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(slot.gameObject);
            if (instanceRoot == null)
            {
                throw new System.InvalidOperationException(
                    $"Saved slot '{slot.SlotId}' is not a nested prefab instance.");
            }

            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(instanceRoot);
            HashSet<Object> stateSourceTargets = GetStateSourceTargets(stateObjects, stateImages);

            if (modifications != null)
            {
                foreach (PropertyModification modification in modifications)
                {
                    if (IsStateVisualModification(modification, stateSourceTargets))
                    {
                        throw new System.InvalidOperationException(
                            $"Saved slot '{slot.SlotId}' retains stale state visual property modifications.");
                    }
                }
            }
        }

        private static bool HasPropertyOverride(Object target, string propertyPath)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            return property != null && property.prefabOverride;
        }

        private static void VerifyGaugeFill(StrategicGaugeView gaugeView)
        {
            var gaugeObject = new SerializedObject(gaugeView);
            Image fillImage = gaugeObject.FindProperty("fillImage").objectReferenceValue as Image;

            if (fillImage == null ||
                fillImage.type != Image.Type.Filled ||
                fillImage.fillMethod != Image.FillMethod.Radial360)
            {
                throw new System.InvalidOperationException(
                    "Saved strategic gauge fill is missing or is not Filled/Radial360.");
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
