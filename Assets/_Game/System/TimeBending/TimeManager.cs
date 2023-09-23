using UnityEngine;
using DarkTonic.MasterAudio;
using System.Collections;

namespace TimeBending
{
    public class TimeManager : MonoBehaviour
    {
        public float slowdownFactor = 0.05f;
        public float slowdownPitch = 0.6f;
        public bool isActive = false;
        public float duration = 0.5f;
        private float _pitchTime = 0f;

        private float _fps = 60f;
        public float lookRotationSpeed = 0.6f;  

        public void StartSlowMotion ()
        { 
            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * (float)(1f/_fps); // timeScale divided by 60fps
            MasterAudio.PlaySoundAndForget("SlowMo_init");
            Debug.Log($"Slow motion initiated by a factor of {1/slowdownFactor}");
            isActive = true;
            PitchSoundtrack_Co(1f, slowdownPitch);
            MasterAudio.ChangeBusPitch("Weapon", slowdownPitch);
            MasterAudio.ChangeBusPitch("SFX", slowdownPitch);
            MasterAudio.ChangeBusPitch("Enemy", slowdownPitch);
            MasterAudio.ChangeBusPitch("Player", slowdownPitch);
        }

        Coroutine PitchSoundtrackCo = null;
        private void PitchSoundtrack_Co(float pitch_src, float pitch_dst)
        {
            if (PitchSoundtrackCo != null)
            {
                StopCoroutine(PitchSoundtrackCo);
            }
            PitchSoundtrackCo = StartCoroutine(PitchSoundtrack(pitch_src, pitch_dst));
        }

        public void StopSlowMotion ()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = (1f / _fps);
            Debug.Log("Slow motion is stopped");
            MasterAudio.PlaySoundAndForget("SlowMo_init");
            isActive = false;
            PitchSoundtrack_Co(slowdownPitch, 1f);
            MasterAudio.ChangeBusPitch("Weapon", 1f);
            MasterAudio.ChangeBusPitch("SFX", 1f);
            MasterAudio.ChangeBusPitch("Enemy", 1f);
            MasterAudio.ChangeBusPitch("Player", 1f);
        }

        public IEnumerator PitchSoundtrack (float pitch_src, float pitch_dst)
        {
            PlaylistController pc = FindObjectOfType<PlaylistController>();
            AudioSource audio = pc.ActiveAudioSource;
            while (_pitchTime < duration)
            {
                var newPitch = Mathf.Lerp(pitch_src, pitch_dst, _pitchTime / duration);
                audio.pitch = newPitch;
                _pitchTime += Time.unscaledDeltaTime;
                yield return null;
            };
            audio.pitch = pitch_dst;
            _pitchTime = 0f;
        }

    }



}
