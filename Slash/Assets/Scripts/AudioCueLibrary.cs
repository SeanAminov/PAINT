using UnityEngine;

namespace Slash
{
    // Scene-level registry of audio clips and per-cue volume. Self-registers
    // with the AudioCues facade so gameplay scripts can fire cues without
    // caching individual references.
    public class AudioCueLibrary : MonoBehaviour
    {
        [Header("Player")]
        public AudioClip slash;
        public float slashVolume = 0.85f;
        public AudioClip playerHurt1;
        public AudioClip playerHurt2;
        public float playerHurtVolume = 0.7f;
        public AudioClip metalStep;
        public float metalStepVolume = 0.25f;

        [Header("Ult")]
        public AudioClip timeStop;
        public float timeStopVolume = 1f;

        [Header("Enemies")]
        public AudioClip enemyDeath;
        public float enemyDeathVolume = 0.45f;
        public AudioClip arrow;
        public float arrowVolume = 0.45f;
        public AudioClip bigGuySound;
        public float bigGuySoundVolume = 0.9f;
        public AudioClip bigGuyHurt;
        public float bigGuyHurtVolume = 1.65f;

        [Header("Pickups")]
        public AudioClip coin;
        public float coinVolume = 0.3f;
        public AudioClip heartDrop;
        public float heartDropVolume = 0.35f;
        public AudioClip heartGain;
        public float heartGainVolume = 1f;

        [Header("Cart")]
        public AudioClip princessHurt;
        public float princessHurtVolume = 1f;
        public AudioClip princessSayHi;
        public float princessSayHiVolume = 0.55f;

        [Header("Menu")]
        public AudioClip menuClick;
        public float menuClickVolume = 0.5f;

        [Header("Spatial Falloff")]
        public float minDistance = 5f;
        public float maxDistance = 30f;

        void Awake()
        {
            AudioCues.Init(this);
        }

        void OnDestroy()
        {
            AudioCues.Clear(this);
        }
    }
}
