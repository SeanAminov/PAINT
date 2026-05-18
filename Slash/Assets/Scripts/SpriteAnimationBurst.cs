using UnityEngine;

namespace Slash
{
    // Plays a sprite array as a one-shot animation on a SpriteRenderer, then
    // destroys the GameObject. Used by the ult charge explosion which is
    // built from a sprite sheet without an Animator.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimationBurst : MonoBehaviour
    {
        public Sprite[] frames;
        public float fps = 14f;
        public bool destroyOnEnd = true;

        SpriteRenderer _sr;
        float _startTime;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _startTime = Time.time;
        }

        void Update()
        {
            if (frames == null || frames.Length == 0 || _sr == null) return;

            float elapsed = Time.time - _startTime;
            int frame = Mathf.FloorToInt(elapsed * fps);

            if (frame >= frames.Length)
            {
                if (destroyOnEnd) Destroy(gameObject);
                else _sr.sprite = frames[frames.Length - 1];
                return;
            }

            _sr.sprite = frames[frame];
        }
    }
}
