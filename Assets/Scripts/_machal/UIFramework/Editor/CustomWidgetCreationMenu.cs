using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIFramework.Editor
{
    public static class CustomWidgetCreationMenu
    {
        private const string MenuPath = "GameObject/CustomWidget/";
        private const int Priority = 10;

        [MenuItem(MenuPath + "Grid Scroll Widget", false, Priority)]
        private static void CreateGridScrollWidget(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("UI_GridScrollWidget", typeof(RectTransform));
            var widget = go.AddComponent<UIGridScrollWidget>();
            widget.EnsureStructure();

            SetupUIElement(go, menuCommand);
        }

        [MenuItem(MenuPath + "Selectable Icon Button", false, Priority + 1)]
        private static void CreateSelectableIconButton(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("UI_SelectableIconButton", typeof(RectTransform));
            var image = go.AddComponent<Image>();
            var button = go.AddComponent<Button>();
            var canvasGroup = go.AddComponent<CanvasGroup>();
            var widget = go.AddComponent<UISelectableIconButton>();

            // Setup normal state sprite target
            button.targetGraphic = image;

            // Icon Image
            GameObject iconGo = CreateImageObject("UI_IconImage", go.transform);
            var iconImage = iconGo.GetComponent<Image>();

            // Selected Frame
            GameObject frameGo = CreateImageObject("UI_SelectedFrameImage", go.transform);
            var frameImage = frameGo.GetComponent<Image>();
            frameGo.SetActive(false);

            // Locked Overlay
            GameObject overlayGo = new GameObject("UI_LockedOverlay", typeof(RectTransform), typeof(Image));
            overlayGo.transform.SetParent(go.transform, false);
            StretchToParent(overlayGo.GetComponent<RectTransform>());
            var overlayImage = overlayGo.GetComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.5f);
            overlayGo.SetActive(false);

            // Use reflection or serialized properties to set references
            var serializedObj = new SerializedObject(widget);
            serializedObj.FindProperty("button").objectReferenceValue = button;
            serializedObj.FindProperty("iconImage").objectReferenceValue = iconImage;
            serializedObj.FindProperty("selectedFrameImage").objectReferenceValue = frameImage;
            serializedObj.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedObj.FindProperty("lockedOverlay").objectReferenceValue = overlayGo.GetComponent<RectTransform>();
            serializedObj.ApplyModifiedProperties();

            SetupUIElement(go, menuCommand);
        }

        [MenuItem(MenuPath + "Relic Grid Item View", false, Priority + 2)]
        private static void CreateRelicGridItemView(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("UI_RelicGridItemView", typeof(RectTransform));
            var view = go.AddComponent<RelicGridItemView>();

            // 자식으로 SelectableIconButton 생성
            GameObject buttonGo = new GameObject("UI_SelectableIconButton", typeof(RectTransform));
            buttonGo.transform.SetParent(go.transform, false);
            var image = buttonGo.AddComponent<Image>();
            var button = buttonGo.AddComponent<Button>();
            var canvasGroup = buttonGo.AddComponent<CanvasGroup>();
            var iconBtn = buttonGo.AddComponent<UISelectableIconButton>();

            button.targetGraphic = image;

            GameObject iconGo = CreateImageObject("UI_IconImage", buttonGo.transform);
            GameObject frameGo = CreateImageObject("UI_SelectedFrameImage", buttonGo.transform);
            frameGo.SetActive(false);

            GameObject overlayGo = new GameObject("UI_LockedOverlay", typeof(RectTransform), typeof(Image));
            overlayGo.transform.SetParent(buttonGo.transform, false);
            StretchToParent(overlayGo.GetComponent<RectTransform>());
            overlayGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            overlayGo.SetActive(false);

            var iconBtnSerialized = new SerializedObject(iconBtn);
            iconBtnSerialized.FindProperty("button").objectReferenceValue = button;
            iconBtnSerialized.FindProperty("iconImage").objectReferenceValue = iconGo.GetComponent<Image>();
            iconBtnSerialized.FindProperty("selectedFrameImage").objectReferenceValue = frameGo.GetComponent<Image>();
            iconBtnSerialized.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            iconBtnSerialized.FindProperty("lockedOverlay").objectReferenceValue = overlayGo.GetComponent<RectTransform>();
            iconBtnSerialized.ApplyModifiedProperties();

            var viewSerialized = new SerializedObject(view);
            viewSerialized.FindProperty("selectableIconButton").objectReferenceValue = iconBtn;
            viewSerialized.ApplyModifiedProperties();

            SetupUIElement(go, menuCommand);
        }

        [MenuItem(MenuPath + "Relic Info Panel View", false, Priority + 3)]
        private static void CreateRelicInfoPanelView(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("UI_RelicInfoPanelView", typeof(RectTransform));
            var canvasGroup = go.AddComponent<CanvasGroup>();
            var panel = go.AddComponent<RelicInfoPanelView>();

            // Icon Image
            GameObject iconGo = CreateImageObject("UI_IconImage", go.transform);
            var rt = iconGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.8f);
            rt.anchorMax = new Vector2(0.5f, 0.8f);
            rt.sizeDelta = new Vector2(120, 120);

            // Name Text
            GameObject nameGo = CreateTextObject("UI_NameText", "유물 이름", go.transform);
            var nameRt = nameGo.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 0.6f);
            nameRt.anchorMax = new Vector2(1f, 0.6f);
            nameRt.offsetMin = new Vector2(10, 0);
            nameRt.offsetMax = new Vector2(-10, 40);

            // Description Text
            GameObject descGo = CreateTextObject("UI_DescriptionText", "유물 설명이 이곳에 표시됩니다.", go.transform);
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0.2f);
            descRt.anchorMax = new Vector2(1f, 0.5f);
            descRt.offsetMin = new Vector2(10, 0);
            descRt.offsetMax = new Vector2(-10, 0);

            var serializedObj = new SerializedObject(panel);
            serializedObj.FindProperty("iconImage").objectReferenceValue = iconGo.GetComponent<Image>();
            serializedObj.FindProperty("nameText").objectReferenceValue = nameGo.GetComponent<TextMeshProUGUI>();
            serializedObj.FindProperty("descriptionText").objectReferenceValue = descGo.GetComponent<TextMeshProUGUI>();
            serializedObj.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedObj.ApplyModifiedProperties();

            SetupUIElement(go, menuCommand);
        }

        [MenuItem(MenuPath + "Relic Collection View", false, Priority + 4)]
        private static void CreateRelicCollectionView(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("UI_RelicCollectionView", typeof(RectTransform));
            var view = go.AddComponent<RelicCollectionView>();

            // Title Text
            GameObject titleGo = CreateTextObject("Txt_Title", "유물 도감", go.transform);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.95f);
            titleRt.anchorMax = new Vector2(0.5f, 0.95f);
            titleRt.sizeDelta = new Vector2(300, 50);

            // Count Text
            GameObject countGo = CreateTextObject("Txt_Count", "0 / 0", go.transform);
            var countRt = countGo.GetComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0.5f, 0.9f);
            countRt.anchorMax = new Vector2(0.5f, 0.9f);
            countRt.sizeDelta = new Vector2(200, 40);

            // UIGridScrollWidget (2단계 구조 그대로 생성 및 Ensure)
            GameObject gridGo = new GameObject("UI_RelicGridScrollWidget", typeof(RectTransform));
            gridGo.transform.SetParent(go.transform, false);
            var gridWidget = gridGo.AddComponent<UIGridScrollWidget>();
            gridWidget.EnsureStructure();
            var gridRt = gridGo.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.05f, 0.05f);
            gridRt.anchorMax = new Vector2(0.6f, 0.85f);
            gridRt.offsetMin = Vector2.zero;
            gridRt.offsetMax = Vector2.zero;

            // RelicInfoPanelView (3단계 구조 그대로 생성)
            GameObject infoGo = new GameObject("UI_RelicInfoPanelView", typeof(RectTransform));
            infoGo.transform.SetParent(go.transform, false);
            var infoCanvasGroup = infoGo.AddComponent<CanvasGroup>();
            var infoPanel = infoGo.AddComponent<RelicInfoPanelView>();
            var infoRt = infoGo.GetComponent<RectTransform>();
            infoRt.anchorMin = new Vector2(0.65f, 0.05f);
            infoRt.anchorMax = new Vector2(0.95f, 0.85f);
            infoRt.offsetMin = Vector2.zero;
            infoRt.offsetMax = Vector2.zero;

            // Info Panel Children
            GameObject infoIcon = CreateImageObject("UI_IconImage", infoGo.transform);
            infoIcon.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.8f);
            infoIcon.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.8f);
            infoIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

            GameObject infoName = CreateTextObject("UI_NameText", "유물 이름", infoGo.transform);
            infoName.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.6f);
            infoName.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.6f);
            infoName.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            infoName.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 40);

            GameObject infoDesc = CreateTextObject("UI_DescriptionText", "설명", infoGo.transform);
            infoDesc.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.1f);
            infoDesc.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
            infoDesc.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            infoDesc.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);

            var infoSerialized = new SerializedObject(infoPanel);
            infoSerialized.FindProperty("iconImage").objectReferenceValue = infoIcon.GetComponent<Image>();
            infoSerialized.FindProperty("nameText").objectReferenceValue = infoName.GetComponent<TextMeshProUGUI>();
            infoSerialized.FindProperty("descriptionText").objectReferenceValue = infoDesc.GetComponent<TextMeshProUGUI>();
            infoSerialized.FindProperty("canvasGroup").objectReferenceValue = infoCanvasGroup;
            infoSerialized.ApplyModifiedProperties();

            // Set main references
            var viewSerialized = new SerializedObject(view);
            viewSerialized.FindProperty("titleText").objectReferenceValue = titleGo.GetComponent<TextMeshProUGUI>();
            viewSerialized.FindProperty("countText").objectReferenceValue = countGo.GetComponent<TextMeshProUGUI>();
            viewSerialized.FindProperty("gridWidget").objectReferenceValue = gridWidget;
            viewSerialized.FindProperty("infoPanel").objectReferenceValue = infoPanel;
            viewSerialized.ApplyModifiedProperties();

            SetupUIElement(go, menuCommand);
        }

        #region Helper Methods

        private static GameObject CreateImageObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            StretchToParent(go.GetComponent<RectTransform>());
            return go;
        }

        private static GameObject CreateTextObject(string name, string text, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontSize = 20;

            StretchToParent(go.GetComponent<RectTransform>());
            return go;
        }

        private static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetupUIElement(GameObject go, MenuCommand menuCommand)
        {
            // Parent & Undo Setup
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            
            // 만약 부모가 Canvas 하위가 아니라면 자동으로 최적의 Canvas를 찾거나 생성해서 배치
            EnsureCanvasParent(go);

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeGameObject = go;
        }

        private static void EnsureCanvasParent(GameObject go)
        {
            // 이미 Canvas 하위이면 리턴
            if (go.GetComponentInParent<Canvas>() != null) return;

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                // 없으면 새 Canvas 생성
                GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas for Widget");
                
                // EventSystem 도 생성
                if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                    Undo.RegisterCreatedObjectUndo(eventSystemGo, "Create EventSystem");
                }
            }

            go.transform.SetParent(canvas.transform, false);
        }

        #endregion
    }
}
