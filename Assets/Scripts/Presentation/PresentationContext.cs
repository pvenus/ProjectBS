using System;

namespace Presentation
{
    public enum PresentationContextMode
    {
        Preview = 0,
        Runtime = 100,
    }

    [Serializable]
    public sealed class PresentationContext
    {
        public static PresentationContext Preview { get; } =
            new PresentationContext(PresentationContextMode.Preview);

        public static PresentationContext Runtime { get; } =
            new PresentationContext(PresentationContextMode.Runtime);

        public PresentationContextMode Mode { get; }

        public PresentationContext(PresentationContextMode mode)
        {
            Mode = mode;
        }
    }
}
