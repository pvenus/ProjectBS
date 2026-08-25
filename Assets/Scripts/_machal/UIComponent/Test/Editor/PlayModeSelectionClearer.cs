using UnityEditor;

[InitializeOnLoad]
public static class PlayModeSelectionClearer
{
    static PlayModeSelectionClearer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // 플레이 모드 진입 직전에 선택된 오브젝트를 강제로 해제합니다.
            // (파괴될 오브젝트를 인스펙터가 그리려고 시도하다가 발생하는 SerializedObjectNotCreatableException 방지)
            Selection.activeObject = null;
        }
    }
}
