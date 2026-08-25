#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIFramework.Editor
{
    [Serializable]
    public class UISpriteMappingEntry
    {
        [Tooltip("Root 기준 하이어라키 경로 (중복 objectName 방지용)")]
        public string path;

        [Tooltip("Image GameObject 이름")]
        public string objectName;

        [Tooltip("적용할 Sprite 직접 참조")]
        public Sprite sprite;

        [Tooltip("이 매핑을 적용할지 여부")]
        public bool enabled = true;

        [Tooltip("메모")]
        public string memo;
    }

    [CreateAssetMenu(menuName = "UI/Sprite Mapping Profile", fileName = "UISpriteMappingProfile")]
    public class UISpriteMappingProfileSO : ScriptableObject
    {
        public string profileId;

        [Tooltip("이 프로필이 관리하는 기본 이미지 폴더")]
        public DefaultAsset baseImageFolder;

        public bool usePathFirst = true;
        public bool allowObjectNameFallback = true;
        public bool skipIfTargetSpriteAlreadyAssigned = false;

        public List<UISpriteMappingEntry> entries = new List<UISpriteMappingEntry>();
    }
}
#endif
