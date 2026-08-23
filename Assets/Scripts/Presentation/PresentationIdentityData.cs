using System;
using UnityEngine;

namespace Presentation
{
    [Serializable]
    public sealed class PresentationIdentityData
    {
        public string ContentId { get; }
        public string DisplayName { get; }
        public Sprite Icon { get; }

        public PresentationIdentityData(
            string contentId,
            string displayName,
            Sprite icon = null)
        {
            ContentId = contentId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Icon = icon;
        }
    }
}
