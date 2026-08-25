using System;

public interface ILobbyUITestable<TData, TResult>
{
    void Show(TData data, Action<TResult> onResult);
    void Hide();
}
