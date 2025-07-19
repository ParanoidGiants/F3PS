using UnityEngine;

public class PlayMuzzle : MonoBehaviour
{
    public ParticleSystem[] muzzleFlashParticleSystems;

    public void Play(float timeScale)
    {
        foreach (var particleSystem in muzzleFlashParticleSystems)
        {
            // particleSystem.timeScale = timeScale;
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
        }
    }
}
