using UnityEngine;
using UnityEngine.Rendering;

namespace TimeBending
{
    public class TimeManager : MonoBehaviour
    {
        public float slowdownFactor = 0.05f;
        public float slowdownLength = 2f;

        void Update()
        {
            Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        }

        public void StartSlowMotion ()
        {
            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * (float)(1f/60f); // timeScale divided by 60fps
            Debug.Log($"Slow motion initiated by a factor of {1/slowdownFactor}");
        }
    }

}
