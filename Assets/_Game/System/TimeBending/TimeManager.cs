using UnityEngine;
using UnityEngine.Rendering;

namespace TimeBending
{
    public class TimeManager : MonoBehaviour
    {
        public float slowdownFactor = 0.05f;
        public bool isActive = false;

        [SerializeField] private bool _isPaused;
        public bool IsPaused => _isPaused;

        public void StartSlowMotion ()
        {
            if (_isPaused) return;
            
            int fps = F3PS.GameManager.Instance.Fps;
            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale /fps; // timeScale divided by 60fps
            Debug.Log($"Slow motion initiated by a factor of {(1f/slowdownFactor).ToString("0.001")}");
            isActive = true;
        }
        
        public void StopSlowMotion ()
        {
            if (_isPaused) return;

            int fps = F3PS.GameManager.Instance.Fps;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = (1f / fps);
            Debug.Log("Slow motion is stopped");
            isActive = false;
        }

        public void PauseTime()
        {
            Debug.Log("PAUSE");
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;
            _isPaused = true;
        }
        
        public void ResumeTime()
        {
            Debug.Log("RESUME");
            Time.timeScale = isActive ? slowdownFactor : 1f;
            Time.fixedDeltaTime = Time.timeScale / F3PS.GameManager.Instance.Fps;
            _isPaused = false;
        }
    }
}
