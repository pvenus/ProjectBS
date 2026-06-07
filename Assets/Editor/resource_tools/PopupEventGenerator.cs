using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Stage;
using UnityEditor;
using UnityEngine;

public static class PopupEventGenerator
{
    private const string MenuRoot = "Assets/PopupEvent";

    [MenuItem(MenuRoot + "/Generate PopupEventSO From Json", false, 2000)]
    public static void GenerateFromSelectedJson()
    {
        UnityEngine.Object selected = Selection.activeObject;

        if (selected == null)
        {
            Debug.LogWarning("[PopupEventGenerator] Select a popup event json file first.");
            return;
        }

        string jsonPath = AssetDatabase.GetAssetPath(selected);

        if (string.IsNullOrEmpty(jsonPath) || !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"[PopupEventGenerator] Selected asset is not a json file. path={jsonPath}");
            return;
        }

        GenerateFromJsonPath(jsonPath);
    }

    [MenuItem(MenuRoot + "/Generate PopupEventSO All From Folder", false, 2001)]
    public static void GenerateAllInSelectedFolder()
    {
        UnityEngine.Object selected = Selection.activeObject;

        if (selected == null)
        {
            Debug.LogWarning("[PopupEventGenerator] Select a folder first.");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(selected);

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            folderPath = Path.GetDirectoryName(folderPath);
        }

        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"[PopupEventGenerator] Invalid folder. path={folderPath}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folderPath });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            GenerateFromJsonPath(path);
            count++;
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PopupEventGenerator] Generated popup events. count={count} folder={folderPath}");
    }

    public static PopupEventSO GenerateFromJsonPath(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath))
        {
            Debug.LogWarning("[PopupEventGenerator] Json path is empty.");
            return null;
        }

        TextAsset jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);

        if (jsonAsset == null)
        {
            Debug.LogWarning($"[PopupEventGenerator] Json asset not found. path={jsonPath}");
            return null;
        }

        PopupEventJsonData data = JsonUtility.FromJson<PopupEventJsonData>(jsonAsset.text);

        if (data == null || string.IsNullOrEmpty(data.eventId))
        {
            Debug.LogWarning($"[PopupEventGenerator] Invalid popup event json. path={jsonPath}");
            return null;
        }

        string outputFolder = Path.GetDirectoryName(jsonPath);

        string popupAssetPath = NormalizeAssetPath(
            Path.Combine(outputFolder, $"{data.eventId}.asset"));

        string nodeId = ResolveNodeId(data);
        string nodeAssetPath = NormalizeAssetPath(
            Path.Combine(outputFolder, $"{nodeId}.node.asset"));

        DeleteAssetIfExists(nodeAssetPath);
        DeleteAssetIfExists(popupAssetPath);

        RoundNodeSO roundNodeSO = CreateAsset<RoundNodeSO>(nodeAssetPath);
        PopupEventSO eventSO = CreateAsset<PopupEventSO>(popupAssetPath);

        if (roundNodeSO == null || eventSO == null)
        {
            Debug.LogError(
                $"[PopupEventGenerator] Failed to create assets. popup={popupAssetPath} node={nodeAssetPath}");
            return null;
        }

        SetupPopupEventSO(
            eventSO,
            data,
            outputFolder);

        EditorUtility.SetDirty(eventSO);
        FlushAssetDatabase(popupAssetPath);

        eventSO = AssetDatabase.LoadAssetAtPath<PopupEventSO>(popupAssetPath);
        roundNodeSO = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(nodeAssetPath);

        if (eventSO == null || roundNodeSO == null)
        {
            Debug.LogError(
                $"[PopupEventGenerator] Failed to reload created assets. popup={popupAssetPath} node={nodeAssetPath}");
            return null;
        }

        SetupRoundNodeSO(
            roundNodeSO,
            data,
            eventSO);

        EditorUtility.SetDirty(roundNodeSO);
        FlushAssetDatabase(nodeAssetPath);

        Debug.Log($"[PopupEventGenerator] Generated PopupEventSO and RoundNodeSO. eventId={data.eventId} popupPath={popupAssetPath} nodePath={nodeAssetPath}");

        return eventSO;
    }


    private static string ResolveNodeId(PopupEventJsonData data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(data.nodeId)
            ? data.eventId
            : data.nodeId;
    }

    private static void SetupPopupEventSO(
        PopupEventSO eventSO,
        PopupEventJsonData data,
        string outputFolder)
    {
        if (eventSO == null || data == null)
        {
            return;
        }

        eventSO.eventId = data.eventId;

        SetFieldOrProperty(
            eventSO,
            "mainImage",
            FindSpriteInFolder(data.mainImage, outputFolder));

        SetFieldOrProperty(
            eventSO,
            "tags",
            data.tags ?? new List<string>());

        SetFieldOrProperty(
            eventSO,
            "choices",
            BuildChoices(data, outputFolder));
    }

    private static void SetupRoundNodeSO(
        RoundNodeSO roundNodeSO,
        PopupEventJsonData data,
        PopupEventSO popupEventSO)
    {
        if (roundNodeSO == null || data == null)
        {
            return;
        }

        roundNodeSO.nodeId = ResolveNodeId(data);
        roundNodeSO.nodeType = ParseEnum(
            data.nodeType,
            RoundNodeType.Event);
        roundNodeSO.popupEvent = popupEventSO;
        roundNodeSO.isRequired = data.isRequired;

        SetFieldOrProperty(
            roundNodeSO,
            "tags",
            data.tags ?? new List<string>());
    }

    private static List<PopupEventChoice> BuildChoices(
        PopupEventJsonData data,
        string baseFolder)
    {
        List<PopupEventChoice> choices = new();

        if (data.choices == null)
        {
            return choices;
        }

        for (int i = 0; i < data.choices.Count; i++)
        {
            PopupEventChoiceJsonData choiceData = data.choices[i];

            if (choiceData == null)
            {
                continue;
            }

            PopupEventChoice choice = new()
            {
                choiceId = choiceData.choiceId,
                rewards = BuildRewards(choiceData, baseFolder)
            };

            SetFieldOrProperty(choice, "nextEvent", FindPopupEvent(choiceData.nextEventId, baseFolder));
            SetFieldOrProperty(choice, "completesEvent", choiceData.completesEvent);
            SetFieldOrProperty(choice, "weight", choiceData.weight);
            SetFieldOrProperty(choice, "tag", choiceData.tag);

            choices.Add(choice);
        }

        return choices;
    }

    private static List<PopupEventRewardData> BuildRewards(
        PopupEventChoiceJsonData choiceData,
        string baseFolder)
    {
        List<PopupEventRewardData> rewards = new();

        if (choiceData.rewards == null)
        {
            return rewards;
        }

        for (int i = 0; i < choiceData.rewards.Count; i++)
        {
            PopupEventRewardJsonData rewardData = choiceData.rewards[i];

            if (rewardData == null)
            {
                continue;
            }

            PopupEventRewardData reward = new()
            {
                rewardType = ParseEnum(
                    rewardData.rewardType,
                    PopupEventRewardType.None),
                value = rewardData.value,
                tag = rewardData.tag,
                targetData = FindTargetData(rewardData, baseFolder)
            };

            SetFieldOrProperty(
                reward,
                "godType",
                ParseEnumByFieldType(
                    typeof(PopupEventRewardData),
                    "godType",
                    rewardData.godType));

            rewards.Add(reward);
        }

        return rewards;
    }

    private static ScriptableObject FindTargetData(
        PopupEventRewardJsonData rewardData,
        string baseFolder)
    {
        if (!string.IsNullOrEmpty(rewardData.targetPath))
        {
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(rewardData.targetPath);

            if (asset != null)
            {
                return asset;
            }
        }

        if (string.IsNullOrEmpty(rewardData.targetId))
        {
            return null;
        }

        ScriptableObject localAsset = FindScriptableObjectByNameInFolder(
            rewardData.targetId,
            baseFolder);

        if (localAsset != null)
        {
            return localAsset;
        }

        return FindScriptableObjectByName(rewardData.targetId);
    }

    private static PopupEventSO FindPopupEvent(
        string eventId,
        string baseFolder)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return null;
        }

        string localPath = NormalizeAssetPath(
            Path.Combine(baseFolder, $"{eventId}.asset"));

        PopupEventSO localEvent = AssetDatabase.LoadAssetAtPath<PopupEventSO>(localPath);

        if (localEvent != null)
        {
            return localEvent;
        }

        string[] guids = AssetDatabase.FindAssets($"{eventId} t:PopupEventSO");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PopupEventSO eventSO = AssetDatabase.LoadAssetAtPath<PopupEventSO>(path);

            if (eventSO != null && eventSO.eventId == eventId)
            {
                return eventSO;
            }
        }

        return null;
    }

    private static ScriptableObject FindScriptableObjectByNameInFolder(
        string assetName,
        string folder)
    {
        if (string.IsNullOrEmpty(assetName) || string.IsNullOrEmpty(folder))
        {
            return null;
        }

        string directPath = NormalizeAssetPath(
            Path.Combine(folder, $"{assetName}.asset"));

        ScriptableObject directAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(directPath);

        if (directAsset != null)
        {
            return directAsset;
        }

        string[] guids = AssetDatabase.FindAssets(assetName, new[] { folder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (asset != null && asset.name.Equals(assetName, StringComparison.OrdinalIgnoreCase))
            {
                return asset;
            }
        }

        return null;
    }

    private static ScriptableObject FindScriptableObjectByName(string assetName)
    {
        string[] guids = AssetDatabase.FindAssets(assetName);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (asset != null && asset.name.Equals(assetName, StringComparison.OrdinalIgnoreCase))
            {
                return asset;
            }
        }

        Debug.LogWarning($"[PopupEventGenerator] Target asset not found. targetId={assetName}");
        return null;
    }


    private static Sprite FindSpriteInFolder(
        string spriteName,
        string folder)
    {
        if (string.IsNullOrEmpty(spriteName) ||
            string.IsNullOrEmpty(folder))
        {
            return null;
        }

        string[] guids = AssetDatabase.FindAssets(
            spriteName,
            new[] { folder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            UnityEngine.Object[] assets =
                AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite &&
                    sprite.name.Equals(
                        spriteName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return sprite;
                }
            }

            Sprite directSprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (directSprite != null &&
                directSprite.name.Equals(
                    spriteName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return directSprite;
            }
        }

        Debug.LogWarning(
            $"[PopupEventGenerator] Sprite not found in folder. sprite={spriteName} folder={folder}");

        return null;
    }

    private static TEnum ParseEnum<TEnum>(
        string value,
        TEnum fallback)
        where TEnum : struct
    {
        if (string.IsNullOrEmpty(value))
        {
            return fallback;
        }

        if (Enum.TryParse(value, true, out TEnum result))
        {
            return result;
        }

        Debug.LogWarning($"[PopupEventGenerator] Failed to parse enum. type={typeof(TEnum).Name} value={value}");
        return fallback;
    }

    private static object ParseEnumByFieldType(
        Type ownerType,
        string fieldName,
        string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        FieldInfo field = ownerType.GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field == null || !field.FieldType.IsEnum)
        {
            return null;
        }

        if (Enum.TryParse(field.FieldType, value, true, out object result))
        {
            return result;
        }

        Debug.LogWarning($"[PopupEventGenerator] Failed to parse enum. type={field.FieldType.Name} value={value}");
        return null;
    }

    private static void SetFieldOrProperty(
        object target,
        string memberName,
        object value)
    {
        if (target == null || string.IsNullOrEmpty(memberName))
        {
            return;
        }

        Type type = target.GetType();
        BindingFlags flags = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        FieldInfo field = type.GetField(memberName, flags);

        if (field != null)
        {
            if (value != null && !field.FieldType.IsAssignableFrom(value.GetType()))
            {
                if (field.FieldType.IsEnum && value is string enumText)
                {
                    value = Enum.Parse(field.FieldType, enumText, true);
                }
                else
                {
                    return;
                }
            }

            field.SetValue(target, value);
            return;
        }

        PropertyInfo property = type.GetProperty(memberName, flags);

        if (property == null || !property.CanWrite)
        {
            return;
        }

        if (value != null && !property.PropertyType.IsAssignableFrom(value.GetType()))
        {
            return;
        }

        property.SetValue(target, value);
    }

    private static string NormalizeAssetPath(string path)
    {
        return path.Replace("\\", "/");
    }



    private static void DeleteAssetIfExists(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return;
        }

        UnityEngine.Object existingAsset =
            AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

        if (existingAsset == null)
        {
            return;
        }

        AssetDatabase.DeleteAsset(assetPath);
        FlushAssetDatabase(assetPath);
    }

    private static T CreateAsset<T>(string assetPath)
        where T : ScriptableObject
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, assetPath);
        FlushAssetDatabase(assetPath);

        return AssetDatabase.LoadAssetAtPath<T>(assetPath);
    }

    private static void FlushAssetDatabase(string assetPath)
    {
        AssetDatabase.SaveAssets();

        if (!string.IsNullOrEmpty(assetPath))
        {
            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.Refresh();
    }

    [Serializable]
    private class PopupEventJsonData
    {
        public string eventId;
        public string nodeId;
        public string nodeType;
        public bool isRequired;
        public string mainImage;
        public List<string> tags = new();
        public List<PopupEventChoiceJsonData> choices = new();
    }

    [Serializable]
    private class PopupEventChoiceJsonData
    {
        public string choiceId;
        public string nextEventId;
        public bool completesEvent = true;
        public int weight = 1;
        public string tag;
        public List<PopupEventRewardJsonData> rewards = new();
    }

    [Serializable]
    private class PopupEventRewardJsonData
    {
        public string rewardType;
        public int value;
        public string godType;
        public string targetId;
        public string targetPath;
        public string tag;
    }
}
