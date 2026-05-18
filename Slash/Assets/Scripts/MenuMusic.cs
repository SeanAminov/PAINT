using UnityEngine;

namespace Slash
{
    // 2D main-menu music loop. Thin AudioSource driver so the builder can
    // configure the clip and volume from a single inspector slot.
    [RequireComponent(typeof(AudioSource))]
    public class MenuMusic : MonoBehaviour
    {
        public AudioClip music;
        public float volume = 0.4f;

        void Start()
        {
            var src = GetComponent<AudioSource>();
            src.clip = music;
            src.loop = true;
            src.volume = volume;
            src.spatialBlend = 0f;
            src.playOnAwake = false;
            if (music != null) src.Play();
        }
    }
}
