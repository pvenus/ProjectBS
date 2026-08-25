using UnityEngine;

public abstract class UITestScenarioBase : MonoBehaviour
{
    protected LobbyUITestPanel testPanel;
    protected LobbyUIDebugLogPanel logger;

    public void Initialize(LobbyUITestPanel panel, LobbyUIDebugLogPanel logger)
    {
        this.testPanel = panel;
        this.logger = logger;
        Setup();
    }

    /// <summary>
    /// 이 시나리오에 필요한 테스트 버튼(testPanel.AddButton)을 등록합니다.
    /// </summary>
    protected abstract void Setup();

    protected void Log(string msg)
    {
        Debug.Log($"[{GetType().Name}] {msg}");
        if (logger != null) logger.AddLog(msg);
    }
}
