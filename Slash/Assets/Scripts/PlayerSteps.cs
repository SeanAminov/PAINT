using UnityEngine;

namespace Slash
{
    // Fires the metal step cue on a cadence while the player's X is changing.
    // Reads position deltas, not input, so cart shoves and ult zips also step.
    public class PlayerSteps : MonoBehaviour
    {
        public float stepInterval = 0.32f;
        public float speedThreshold = 0.5f;

        Vector3 _lastPos;
        float _nextStepAt;

        void Start()
        {
            _lastPos = transform.position;
            _nextStepAt = Time.time + stepInterval * 0.5f;
        }

        void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            float speed = Mathf.Abs(transform.position.x - _lastPos.x) / dt;
            _lastPos = transform.position;

            if (speed < speedThreshold) return;
            if (Time.time < _nextStepAt) return;

            AudioCues.PlayMetalStep(transform.position);
            _nextStepAt = Time.time + stepInterval;
        }
    }
}
