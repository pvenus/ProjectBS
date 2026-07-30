using System;
using System.Collections.Generic;

namespace Stage
{
    [Serializable]
    public class StageMapImportReport
    {
        public bool isSuccess;

        public int totalSlotsParsed;
        public int storySlotsParsed;
        public int randomSlotsParsed;
        public int totalConnectionsParsed;

        public int storyBindingsCount;
        public int matchedStoryBindingsCount;
        public int missingStoryBindingsCount;

        public int randomSectionsCount;
        public int totalRandomSlotsInSection;

        public List<string> warningMessages = new List<string>();
        public List<string> errorMessages = new List<string>();
        public string rawImportLog = string.Empty;
    }
}
