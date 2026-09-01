using System;

namespace Battle.Presentation.SkillFocus
{
    // Compatibility shell for generated project files. Camera presentation never slows execution.
    [Obsolete("Main3 camera presentation no longer owns a local clock.")]
    public sealed class SkillPresentationClock
    {
        public float Scale => 1f;

        public IDisposable Acquire(float ignoredScale) => NoOpToken.Instance;

        public void Restore()
        {
        }

        private sealed class NoOpToken : IDisposable
        {
            public static readonly NoOpToken Instance = new();
            public void Dispose()
            {
            }
        }
    }
}
