using UnityEngine;

public abstract class UIView : AutoBindBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void Refresh()
    {
    }

    /// <summary>
    /// 팝업이 닫힐 때 내부 UI 상태를 초기화한다. (동적 생성 위젯 제거, 텍스트 초기화 등)
    /// UIPopupViewController가 Close 시 OnCloseFromManager() → ClearCallbacks() 이후 호출한다.
    /// </summary>
    public virtual void Clear()
    {
    }

    /// <summary>
    /// 팝업이 닫히거나 풀로 반환되기 직전에 이벤트/콜백을 해제한다.
    /// 이전 요청자의 콜백이 풀링 재사용 시 중복 호출되는 것을 방지한다.
    /// 이벤트를 선언한 각 View에서 반드시 override하여 null 할당으로 해제한다.
    /// </summary>
    public virtual void ClearCallbacks()
    {
    }

    /// <summary>
    /// UIPopupViewController가 팝업을 열 때 호출한다.
    /// 기본 구현: Show() 호출.
    /// </summary>
    public virtual void OnOpenFromManager()
    {
        Show();
    }

    /// <summary>
    /// UIPopupViewController가 팝업을 닫을 때 호출한다.
    /// 순서: ClearCallbacks() → Clear() → Hide()
    /// </summary>
    public virtual void OnCloseFromManager()
    {
        ClearCallbacks();
        Clear();
        Hide();
    }
}
