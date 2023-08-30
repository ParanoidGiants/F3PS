using UnityEngine;
using UnityEngine.Rendering;

namespace TimeBending
{
    public class TimeManager : MonoBehaviour
    {
        public float slowdownFactor = 0.05f;
        public bool isActive = false;

        private float _fps = 60f;
        public float lookRotationSpeed = 0.6f;

        public void StartSlowMotion ()
        { 
            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * (float)(1f/_fps); // timeScale divided by 60fps
            Debug.Log($"Slow motion initiated by a factor of {1/slowdownFactor}");
            isActive = true;
        }
        
        public void StopSlowMotion ()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = (1f / _fps);
            Debug.Log("Slow motion is stopped");
            isActive = false;
        }
    }

}
