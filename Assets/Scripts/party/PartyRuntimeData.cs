using System;
using System.Collections.Generic;
using Character;
using UnityEngine;

namespace Party
{
    [Serializable]
    public class PartyRuntimeData
    {
        public List<CharacterRuntimeData> Members = new();

        public void AddMember(CharacterRuntimeData characterRuntime)
        {
            if (characterRuntime == null)
            {
                Debug.LogError(
                    "[PartyRuntimeData] CharacterRuntimeData is null.");

                return;
            }

            Members.Add(characterRuntime);
        }

        public void Clear()
        {
            Members.Clear();
        }
    }
}
