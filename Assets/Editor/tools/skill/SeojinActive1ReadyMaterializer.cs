#if UNITY_EDITOR
using Skill;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    public static class SeojinActive1ReadyMaterializer
    {
        internal const string JsonPath="Assets/Contents/Skill/json/skill.character.seojin.1.active_1.active_1.json";

        [MenuItem("Tools/ProjectBS/Skill/Materialize Seojin G1 Active1 Ready",false,2228)]
        public static EquipmentSkillSO Materialize()
        {
            EquipmentSkillSO result=EquipmentSkillJsonGenerator.GenerateFromJsonPath(JsonPath);
            if(result==null||result.ActiveInputMode==null||!result.ActiveInputMode.Enabled)
            {
                Debug.LogError("[SeojinActive1ReadyMaterializer] Ready graph is incomplete; existing skill remains fail-closed.");
                return null;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(result),ImportAssetOptions.ForceSynchronousImport);
            EquipmentSkillSO persisted=AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(AssetDatabase.GetAssetPath(result));
            if(persisted?.ActiveInputMode?.ActionHit==null)
            {
                Debug.LogError("[SeojinActive1ReadyMaterializer] Persisted actionHit reference is missing.");
                return null;
            }
            return persisted;
        }
    }
}
#endif
