using UnityEngine;

namespace Slash
{
    // Static facade for one-shot audio cues. The active AudioCueLibrary
    // registers itself on Awake; all calls are no-ops until a library is set.
    public static class AudioCues
    {
        static AudioCueLibrary _lib;

        public static void Init(AudioCueLibrary lib) { _lib = lib; }
        public static void Clear(AudioCueLibrary lib) { if (_lib == lib) _lib = null; }

        // Player-centric, so 2D guarantees full volume on every swing.
        public static void PlaySlash()
        {
            if (_lib == null) return;
            Play2D(_lib.slash, _lib.slashVolume);
        }
        public static void PlayTimeStop(Vector3 pos)     { PlayAt(_lib?.timeStop,    pos, _lib != null ? _lib.timeStopVolume    : 1f); }
        public static void PlayEnemyDeath(Vector3 pos)   { PlayAt(_lib?.enemyDeath,  pos, _lib != null ? _lib.enemyDeathVolume  : 1f); }
        public static void PlayArrow(Vector3 pos)        { PlayAt(_lib?.arrow,       pos, _lib != null ? _lib.arrowVolume       : 1f); }
        public static void PlayBigGuy(Vector3 pos)       { PlayAt(_lib?.bigGuySound, pos, _lib != null ? _lib.bigGuySoundVolume : 1f); }
        public static void PlayBigGuyHurt(Vector3 pos)   { PlayAt(_lib?.bigGuyHurt,  pos, _lib != null ? _lib.bigGuyHurtVolume  : 1f); }
        public static void PlayCoin(Vector3 pos)         { PlayAt(_lib?.coin,        pos, _lib != null ? _lib.coinVolume        : 1f); }
        public static void PlayHeartDrop(Vector3 pos)    { PlayAt(_lib?.heartDrop,   pos, _lib != null ? _lib.heartDropVolume   : 1f); }
        public static void PlayHeartGain(Vector3 pos)    { PlayAt(_lib?.heartGain,   pos, _lib != null ? _lib.heartGainVolume   : 1f); }
        // 2D so the player always hears the cart taking damage, no matter how
        // far they have wandered from it.
        public static void PlayPrincessHurt()
        {
            if (_lib == null) return;
            Play2D(_lib.princessHurt, _lib.princessHurtVolume);
        }
        public static void PlayPrincessSayHi(Vector3 pos){ PlayAt(_lib?.princessSayHi,pos,_lib != null ? _lib.princessSayHiVolume:1f); }
        public static void PlayMetalStep(Vector3 pos)    { PlayAt(_lib?.metalStep,   pos, _lib != null ? _lib.metalStepVolume   : 1f); }

        public static void PlayPlayerHurt(Vector3 pos)
        {
            if (_lib == null) return;
            AudioClip clip;
            if (_lib.playerHurt1 != null && _lib.playerHurt2 != null)
                clip = Random.value < 0.5f ? _lib.playerHurt1 : _lib.playerHurt2;
            else clip = _lib.playerHurt1 != null ? _lib.playerHurt1 : _lib.playerHurt2;
            PlayAt(clip, pos, _lib.playerHurtVolume);
        }

        public static void PlayMenuClick()
        {
            if (_lib == null) return;
            Play2D(_lib.menuClick, _lib.menuClickVolume);
        }

        static void PlayAt(AudioClip clip, Vector3 pos, float volume)
        {
            if (clip == null || _lib == null) return;

            var go = new GameObject("AudioOneShot");
            go.transform.position = pos;
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = volume;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = _lib.minDistance;
            src.maxDistance = _lib.maxDistance;
            src.dopplerLevel = 0f;
            src.Play();
            Object.Destroy(go, clip.length + 0.1f);
        }

        static void Play2D(AudioClip clip, float volume)
        {
            if (clip == null) return;

            var go = new GameObject("AudioOneShot2D");
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = volume;
            src.spatialBlend = 0f;
            src.Play();
            Object.Destroy(go, clip.length + 0.1f);
        }
    }
}
