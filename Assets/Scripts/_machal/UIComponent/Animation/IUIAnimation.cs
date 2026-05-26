using System;
using System.Collections;

namespace UIFramework.Animation
{
    public interface IUIAnimation
    {
        float Duration { get; }
        float Delay { get; }
        bool IsPlaying { get; }

        IEnumerator PlayRoutine(Action onComplete = null);
        void Stop();
        void ResetToStart();
    }
}
