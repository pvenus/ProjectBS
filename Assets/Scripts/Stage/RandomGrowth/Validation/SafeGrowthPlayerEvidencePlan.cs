using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Stage
{
    public enum SafeGrowthEvidenceLane { EditorG2 = 10, MacG2 = 20, MacG3 = 30 }

    public sealed class SafeGrowthPlayerEvidenceCase
    {
        public SafeGrowthPlayerEvidenceCase(string id, SafeGrowthEvidenceLane lane,
            int width, int height, SafeGrowthPresentationState expectedState, string routeScenario)
        {
            Id = id ?? string.Empty; Lane = lane; Width = width; Height = height;
            ExpectedState = expectedState; RouteScenario = routeScenario ?? string.Empty;
        }
        public string Id { get; }
        public SafeGrowthEvidenceLane Lane { get; }
        public int Width { get; }
        public int Height { get; }
        public SafeGrowthPresentationState ExpectedState { get; }
        public string RouteScenario { get; }
    }

    public sealed class SafeGrowthPlayerEvidencePlan
    {
        public const string Version = "chapter1.safe-growth.mac-player-evidence.v1";
        public const string Token = "projectbs-safe-mac-player-v1";
        public const string StageScenePath = "Assets/Scenes/StageScene.unity";
        public const string TempRootPrefix = "/private/tmp/projectbs-safe-mac-player.";
        public const string ImporterMetaPath =
            "Assets/ImagesGenerated/Stage/popup_main/node.act1.random_growth.02.windworn_sword_marks.intro.main.png.meta";
        public const string PresentationCatalogResource =
            "Stage/RandomGrowth/Presentation/event.act1.random_growth.02.windworn_sword_marks.ko-KR";

        private SafeGrowthPlayerEvidencePlan(IEnumerable<SafeGrowthPlayerEvidenceCase> cases)
        { Cases = new ReadOnlyCollection<SafeGrowthPlayerEvidenceCase>(new List<SafeGrowthPlayerEvidenceCase>(cases)); }

        public IReadOnlyList<SafeGrowthPlayerEvidenceCase> Cases { get; }
        public string Sha256 => ComputeSha(Cases);
        public int EditorG2Count => Count(SafeGrowthEvidenceLane.EditorG2);
        public int MacG2Count => Count(SafeGrowthEvidenceLane.MacG2);
        public int MacG3Count => Count(SafeGrowthEvidenceLane.MacG3);

        public static SafeGrowthPlayerEvidencePlan CreateCanonical()
        {
            List<SafeGrowthPlayerEvidenceCase> cases = new();
            Add(cases, SafeGrowthEvidenceLane.EditorG2, 960, 600,
                ("discovery-c2", SafeGrowthPresentationState.Discovery),
                ("preconfirm-c2", SafeGrowthPresentationState.Preconfirm),
                ("preconfirm-c1", SafeGrowthPresentationState.Preconfirm),
                ("disabled-c0", SafeGrowthPresentationState.DisabledNoCandidate),
                ("busy", SafeGrowthPresentationState.BusyApplying),
                ("pending-retry", SafeGrowthPresentationState.PendingRetry),
                ("terminal-success", SafeGrowthPresentationState.TerminalSafeGranted),
                ("terminal-decline", SafeGrowthPresentationState.TerminalDeclined),
                ("terminal-replay", SafeGrowthPresentationState.TerminalReplay));
            foreach (int width in new[] { 1920, 2560 })
                Add(cases, SafeGrowthEvidenceLane.MacG2, width, width == 1920 ? 1080 : 1440,
                    ("discovery-c2", SafeGrowthPresentationState.Discovery),
                    ("preconfirm-c1", SafeGrowthPresentationState.Preconfirm),
                    ("pending-retry", SafeGrowthPresentationState.PendingRetry),
                    ("terminal-success", SafeGrowthPresentationState.TerminalSafeGranted));
            Add(cases, SafeGrowthEvidenceLane.MacG3, 1920, 1080,
                ("success-route-entry", SafeGrowthPresentationState.Discovery),
                ("success-discovery", SafeGrowthPresentationState.Discovery),
                ("success-preconfirm", SafeGrowthPresentationState.Preconfirm),
                ("success-terminal", SafeGrowthPresentationState.TerminalSafeGranted),
                ("success-partywide", SafeGrowthPresentationState.TerminalReplay));
            Add(cases, SafeGrowthEvidenceLane.MacG3, 960, 600,
                ("opposite-placement", SafeGrowthPresentationState.Discovery),
                ("decline", SafeGrowthPresentationState.TerminalDeclined),
                ("retry", SafeGrowthPresentationState.PendingRetry),
                ("technical-failure", SafeGrowthPresentationState.TerminalReplay),
                ("candidate0-unavailable", SafeGrowthPresentationState.DisabledNoCandidate));
            return new SafeGrowthPlayerEvidencePlan(cases);
        }

        public static bool TryValidateOutputRoot(string value, out string fullPath)
        {
            fullPath = string.Empty;
            if (string.IsNullOrWhiteSpace(value) || value.IndexOf('\0') >= 0) return false;
            string normalized;
            try { normalized = Path.GetFullPath(value); }
            catch { return false; }
            if (!normalized.StartsWith(TempRootPrefix, StringComparison.Ordinal)
                || normalized.Length <= TempRootPrefix.Length
                || HasParentTraversal(value)) return false;
            DirectoryInfo cursor = new(normalized);
            while (cursor != null && !cursor.Exists) cursor = cursor.Parent;
            if (cursor == null || (cursor.Attributes & FileAttributes.ReparsePoint) != 0) return false;
            fullPath = normalized; return true;
        }

        private static void Add(List<SafeGrowthPlayerEvidenceCase> target, SafeGrowthEvidenceLane lane,
            int width, int height, params (string id, SafeGrowthPresentationState state)[] values)
        {
            foreach ((string id, SafeGrowthPresentationState state) in values)
                target.Add(new SafeGrowthPlayerEvidenceCase($"{lane.ToString().ToLowerInvariant()}-{width}x{height}-{id}",
                    lane, width, height, state, id));
        }

        private static string ComputeSha(IEnumerable<SafeGrowthPlayerEvidenceCase> cases)
        {
            using MemoryStream stream = new();
            void Write(string value)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
                stream.WriteByte((byte)(bytes.Length >> 24)); stream.WriteByte((byte)(bytes.Length >> 16));
                stream.WriteByte((byte)(bytes.Length >> 8)); stream.WriteByte((byte)bytes.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
            Write(Version);
            foreach (SafeGrowthPlayerEvidenceCase item in cases)
            { Write(item.Id); Write(((int)item.Lane).ToString()); Write(item.Width.ToString());
                Write(item.Height.ToString()); Write(((int)item.ExpectedState).ToString()); Write(item.RouteScenario); }
            using SHA256 sha = SHA256.Create();
            StringBuilder result = new();
            foreach (byte value in sha.ComputeHash(stream.ToArray())) result.Append(value.ToString("x2"));
            return result.ToString();
        }

        private int Count(SafeGrowthEvidenceLane lane)
        { int count = 0; foreach (SafeGrowthPlayerEvidenceCase item in Cases) if (item.Lane == lane) count++; return count; }

        private static bool HasParentTraversal(string value)
        { foreach (string part in value.Split(Path.DirectorySeparatorChar)) if (part == "..") return true; return false; }
    }
}
