using System;

namespace Presentation
{
    public enum PresentationValueKind
    {
        Number = 0,
        Token = 100,
    }

    public enum PresentationValueUnit
    {
        None = 0,
        Flat = 100,
        Ratio = 200,
        Percent = 300,
        Seconds = 400,
        Meters = 500,
        Force = 600,
        Count = 700,
        Degrees = 800,
        MetersPerSecond = 900,
        DegreesPerSecond = 1000,
    }

    [Serializable]
    public sealed class PresentationValueData
    {
        public PresentationValueKind Kind { get; }
        public double NumericValue { get; }
        public string Token { get; }
        public PresentationValueUnit Unit { get; }
        public PresentationProvenanceData Provenance { get; }

        private PresentationValueData(
            PresentationValueKind kind,
            double numericValue,
            string token,
            PresentationValueUnit unit,
            PresentationProvenanceData provenance)
        {
            Kind = kind;
            NumericValue = numericValue;
            Token = token ?? string.Empty;
            Unit = unit;
            Provenance = provenance;
        }

        public static PresentationValueData Number(
            double value,
            PresentationValueUnit unit,
            PresentationProvenanceData provenance = null)
        {
            return new PresentationValueData(
                PresentationValueKind.Number,
                value,
                string.Empty,
                unit,
                provenance);
        }

        public static PresentationValueData SemanticToken(
            string token,
            PresentationProvenanceData provenance = null)
        {
            return new PresentationValueData(
                PresentationValueKind.Token,
                0d,
                token,
                PresentationValueUnit.None,
                provenance);
        }
    }
}
