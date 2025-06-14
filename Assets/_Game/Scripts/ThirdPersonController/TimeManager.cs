using UnityEngine;
using DarkTonic.MasterAudio;
using System.Collections;
using F3PS;
using UnityEngine.SceneManagement;

namespace TimeBending
{
    public class TimeManager : MonoBehaviour
    {
        [Space(10)]
        [Header("Settings")]
        public float slowdownFactor = 0.05f;
        public float slowdownPitch = 0.6f;
        public float duration = 0.5f;
        private float _pitchTime = 0f;
        public float lookRotationSpeed = 0.6f;

        [Space(10)]
        [Header("Watchers")]
        public bool slowMoToggle = false;
        public bool isSlowMoActive = false;
        public bool isPaused;

        private bool _wasPausePressedLastFrame = false;

        private void Update()
        {
            if (!GameManager.Instance.inputs.canControlPlayer) return;

            var isKeyDown = !GameManager.Instance.inputs.pause && _wasPausePressedLastFrame;
            if (isKeyDown)
            {
                if (isPaused)
                {
                    isPaused = false;
                    Time.timeScale = isSlowMoActive ? slowdownFactor : 1f;

                    GameManager.Instance.ResumeTime();
                }
                else
                {
                    isPaused = true;
                    Time.timeScale = 0f;

                    GameManager.Instance.PauseTime();
                }
            }
            _wasPausePressedLastFrame = GameManager.Instance.inputs.pause;

            if (isPaused) return;

            var slowMoInput = GameManager.Instance.inputs.slowmo;
            if (!slowMoToggle && slowMoInput)
            {
                isSlowMoActive = !isSlowMoActive;
                if (isSlowMoActive)
                {
                    StartSlowMotion();
                    GameManager.Instance.StartSlowMotion();
                }
                else
                {
                    StopSlowMotion();
                    GameManager.Instance.StopSlowMotion();
                }
            }
            slowMoToggle = slowMoInput;
        }

        public void StartSlowMotion ()
        {
            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale / GameManager.Instance.Fps;
            MasterAudio.PlaySoundAndForget("SlowMo_init");
            PitchSoundtrack_Co(1f, slowdownPitch);
            MasterAudio.ChangeBusPitch("Weapon", slowdownPitch);
            MasterAudio.ChangeBusPitch("SFX", slowdownPitch);
            MasterAudio.ChangeBusPitch("Enemy", slowdownPitch);
            MasterAudio.ChangeBusPitch("Player", slowdownPitch);
        }

        public void StopSlowMotion()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = (1f / GameManager.Instance.Fps);
            MasterAudio.PlaySoundAndForget("SlowMo_init");
            PitchSoundtrack_Co(slowdownPitch, 1f);
            MasterAudio.ChangeBusPitch("Weapon", 1f);
            MasterAudio.ChangeBusPitch("SFX", 1f);
            MasterAudio.ChangeBusPitch("Enemy", 1f);
            MasterAudio.ChangeBusPitch("Player", 1f);
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

        public IEnumerator PitchSoundtrack (float pitch_src, float pitch_dst)
        {
            PlaylistController pc = FindFirstObjectByType<PlaylistController>();
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

        public void PauseTime()
        {
            Time.timeScale = 0f;
        }
        
        public void ResumeTime()
        {
            if (isPaused)
                return;

            Time.timeScale = isSlowMoActive ? slowdownFactor : 1f;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResumeTime();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
