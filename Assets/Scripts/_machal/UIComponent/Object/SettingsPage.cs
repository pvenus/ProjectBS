using System;
using UnityEngine;
using UnityEngine.UI;

[AutoBindPrefix("Settings")]
public class SettingsPage : UIPage
{
    [Header("Root")]
    [AutoBind] [SerializeField] private GameObject panelRoot;
    
    [Header("Toggle")]
    [AutoBind] [SerializeField] private Button toggleButton;

    [Header("Menu Buttons (세로 배치용)")]
    [AutoBind] [SerializeField] private Button audioButton;
    [AutoBind] [SerializeField] private Button graphicsButton;
    [AutoBind] [SerializeField] private Button gameplayButton;
    [AutoBind] [SerializeField] private Button languageButton;
    [AutoBind] [SerializeField] private Button quitButton;

    // 외부(매니저 등)에서 각 버튼의 동작을 구독할 수 있는 이벤트
    public event Action OnAudioClicked;
    public event Action OnGraphicsClicked;
    public event Action OnGameplayClicked;
    public event Action OnLanguageClicked;
    public event Action OnQuitClicked;

    private void Awake()
    {
        if (panelRoot == null)
        {
            Debug.LogWarning("[SettingsPage] PanelRoot가 할당되지 않았습니다. 현재 오브젝트의 자식으로 패널을 구성해주세요.");
        }

        // 설정 창 열기/닫기 토글 동작 연결
        if (toggleButton != null) 
        {
            toggleButton.onClick.AddListener(Toggle);
        }
        
        // 각 메뉴 버튼 클릭 이벤트 연결
        if (audioButton != null) audioButton.onClick.AddListener(() => { Debug.Log("오디오 설정 클릭됨"); OnAudioClicked?.Invoke(); });
        if (graphicsButton != null) graphicsButton.onClick.AddListener(() => { Debug.Log("그래픽 설정 클릭됨"); OnGraphicsClicked?.Invoke(); });
        if (gameplayButton != null) gameplayButton.onClick.AddListener(() => { Debug.Log("게임플레이 설정 클릭됨"); OnGameplayClicked?.Invoke(); });
        if (languageButton != null) languageButton.onClick.AddListener(() => { Debug.Log("언어 설정 클릭됨"); OnLanguageClicked?.Invoke(); });
        if (quitButton != null) quitButton.onClick.AddListener(() => { Debug.Log("게임 종료 클릭됨"); OnQuitClicked?.Invoke(); });

        Hide(); // 시작 시 숨김
    }

    public override void Show()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        else base.Show();
    }

    public override void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        else base.Hide();
    }

    public void Toggle()
    {
        if (panelRoot != null)
        {
            if (panelRoot.activeSelf) Hide();
            else Show();
        }
        else
        {
            if (gameObject.activeSelf) Hide();
            else Show();
        }
    }
}
