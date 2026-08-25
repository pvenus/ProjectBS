using UnityEngine;

public class LobbyUITestRunner : MonoBehaviour
{
    public LobbyUITestPanel testPanel;
    public LobbyUIDebugLogPanel debugLogPanel;

    private void Start()
    {
        Log("Lobby UI Test Environment Initialized.");
        testPanel.AddButton("Clear Log", () => debugLogPanel?.Clear());
        testPanel.AddButton("Close All UIs", () => Log("Close All UIs: Not fully implemented yet."));

        // 현재 GameObject에 붙어있는 모든 시나리오 컴포넌트를 찾아서 초기화합니다.
        var scenarios = GetComponentsInChildren<UITestScenarioBase>();
        foreach (var scenario in scenarios)
        {
            scenario.Initialize(testPanel, debugLogPanel);
        }
    }

    private void Log(string msg)
    {
        Debug.Log($"[UITest] {msg}");
        if (debugLogPanel != null) debugLogPanel.AddLog(msg);
    }
}
