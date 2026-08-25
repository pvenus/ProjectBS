using System;
using System.Collections.Generic;

[Serializable]
public class UIImageSpriteEntry
{
    public string path;
    public string objectName;
    public string spriteName;
    public string spritePath;
}

[Serializable]
public class UIImageSpriteProfile
{
    public string spriteFolder;
    public List<UIImageSpriteEntry> entries = new List<UIImageSpriteEntry>();
}

public class UIImageSpriteApplyReport
{
    public int targetRootCount;
    public int entryCount;
    public int appliedCount;
    public int missingObjectCount;
    public int duplicateObjectCount;
    public int missingImageComponentCount;
    public int missingSpriteCount;
    public int duplicateSpriteCount;
    public int skippedCount;
    public int changedPrefabCount;
    public List<string> failedPrefabs = new List<string>();
    public List<string> logs = new List<string>();

    public void AddLog(string log)
    {
        logs.Add(log);
    }
    
    public void Merge(UIImageSpriteApplyReport other)
    {
        targetRootCount += other.targetRootCount;
        entryCount += other.entryCount;
        appliedCount += other.appliedCount;
        missingObjectCount += other.missingObjectCount;
        duplicateObjectCount += other.duplicateObjectCount;
        missingImageComponentCount += other.missingImageComponentCount;
        missingSpriteCount += other.missingSpriteCount;
        duplicateSpriteCount += other.duplicateSpriteCount;
        skippedCount += other.skippedCount;
        changedPrefabCount += other.changedPrefabCount;
        failedPrefabs.AddRange(other.failedPrefabs);
        logs.AddRange(other.logs);
    }
}
