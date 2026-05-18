using UnityEngine;

namespace Slash
{
    // Looped cart roll + one-shot princess greeting after greetingDelay.
    // Spatial 3D so the loop fades as the player drifts away from the cart.
    [RequireComponent(typeof(AudioSource))]
    public class CarriageAudio : MonoBehaviour
    {
        [Header("Loop")]
        public AudioClip loopClip;
        public float loopVolume = 0.45f;
        public float minDistance = 5f;
        public float maxDistance = 30f;

        [Header("Greeting")]
        public float greetingDelay = 1.25f;

        AudioSource _src;
        bool _greeted;
        float _sceneStartTime;

        void Awake()
        {
            _src = GetComponent<AudioSource>();
        }

        void Start()
        {
            _sceneStartTime = Time.time;

            _src.clip = loopClip;
            _src.loop = true;
            _src.volume = loopVolume;
            _src.spatialBlend = 1f;
            _src.rolloffMode = AudioRolloffMode.Linear;
            _src.minDistance = minDistance;
            _src.maxDistance = maxDistance;
            _src.dopplerLevel = 0f;
            _src.playOnAwake = false;
            if (loopClip != null) _src.Play();
        }

        void Update()
        {
            if (_greeted) return;
            if (Time.time - _sceneStartTime < greetingDelay) return;
            AudioCues.PlayPrincessSayHi(transform.position);
            _greeted = true;
        }
    }
}
