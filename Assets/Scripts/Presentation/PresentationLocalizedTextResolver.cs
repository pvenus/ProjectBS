using String;

namespace Presentation
{
    public static class PresentationLocalizedTextResolver
    {
        public static string ResolveName(
            string unavailableFallback,
            params string[] localizationMainKeys)
        {
            return ResolveRequired(
                "name",
                unavailableFallback,
                localizationMainKeys);
        }

        public static string ResolveLabel(string localizationMainKey)
        {
            return ResolveRequired(
                "name",
                string.Empty,
                localizationMainKey);
        }

        public static string ResolveRequired(
            string subKey,
            params string[] localizationMainKeys)
        {
            return ResolveRequired(
                subKey,
                string.Empty,
                localizationMainKeys);
        }

        public static string ResolveOptional(
            string subKey,
            params string[] localizationMainKeys)
        {
            StringManager manager = StringManager.Instance;
            if (manager == null || localizationMainKeys == null)
            {
                return string.Empty;
            }

            foreach (string mainKey in localizationMainKeys)
            {
                if (string.IsNullOrWhiteSpace(mainKey))
                {
                    continue;
                }

                string resolved = manager.Get(
                    mainKey,
                    subKey,
                    returnNullIfMissing: true);
                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    return resolved;
                }
            }

            return string.Empty;
        }

        private static string ResolveRequired(
            string subKey,
            string unavailableFallback,
            params string[] localizationMainKeys)
        {
            StringManager manager = StringManager.Instance;
            if (manager == null)
            {
                return unavailableFallback ?? string.Empty;
            }

            if (localizationMainKeys == null)
            {
                return string.Empty;
            }

            string firstMainKey = string.Empty;
            foreach (string mainKey in localizationMainKeys)
            {
                if (string.IsNullOrWhiteSpace(mainKey))
                {
                    continue;
                }

                firstMainKey = string.IsNullOrWhiteSpace(firstMainKey)
                    ? mainKey
                    : firstMainKey;
                string resolved = manager.Get(
                    mainKey,
                    subKey,
                    returnNullIfMissing: true);
                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    return resolved;
                }
            }

            return string.IsNullOrWhiteSpace(firstMainKey)
                ? string.Empty
                : manager.Get(firstMainKey, subKey);
        }
    }
}
