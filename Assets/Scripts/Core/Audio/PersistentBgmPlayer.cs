using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Plays one looping BGM source and keeps it alive across scene transitions.
    /// Add this component to a GameObject in the initial scene.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class PersistentBgmPlayer : MonoBehaviour
    {
        private const string ClipResourcePath =
            "Audio/BGM/bgm_forgotten_mountain_shrine_loop";

        private static PersistentBgmPlayer instance;

        [SerializeField] private AudioClip bgmClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.55f;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            AudioClip clip = bgmClip != null
                ? bgmClip
                : Resources.Load<AudioClip>(ClipResourcePath);
            if (clip == null)
            {
                Debug.LogError(
                    $"[PersistentBgmPlayer] BGM clip was not found at Resources/{ClipResourcePath}.");
                return;
            }

            AudioSource source = GetComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.Play();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
