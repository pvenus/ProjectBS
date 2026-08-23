using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    /// <summary>
    /// UnityEngine.UI.Image 컴포넌트를 래핑하여 에디터 전용 Sprite 자동 바인딩을 지원하는 컴포넌트입니다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class UIAutoImage : MonoBehaviour
    {
        [Tooltip("매핑 프로필에서 스프라이트를 찾기 위한 키값입니다. 비어있을 경우 게임오브젝트 이름이 기본 키로 사용됩니다.")]
        [SerializeField] private string spriteKey;

        public string SpriteKey
        {
            get => spriteKey;
            set => spriteKey = value;
        }

        private Image _image;

        public Image ImageComponent
        {
            get
            {
                if (_image == null)
                {
                    _image = GetComponent<Image>();
                }
                return _image;
            }
        }

        /// <summary>
        /// 에디터 혹은 런타임에서 사용할 스프라이트 직접 키값 반환 (비어있을 시 게임오브젝트 이름)
        /// </summary>
        public string GetEffectiveKey()
        {
            return string.IsNullOrEmpty(spriteKey) ? gameObject.name : spriteKey;
        }
    }
}
