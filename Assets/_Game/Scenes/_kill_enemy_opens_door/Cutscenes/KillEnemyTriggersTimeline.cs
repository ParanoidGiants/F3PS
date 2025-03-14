using F3PS.Enemy;
using UnityEngine;
using UnityEngine.Playables;

public class KillEnemyTriggersTimeline : MonoBehaviour
{
    [Space(10)]
    [Header("Kill Enemy Reference")]
    public BaseEnemy enemy;
    public PlayableDirector playableDirector;
    public void Start()
    {
        enemy.Dead += Play;
    }

    public void Play()
    {
        playableDirector.Play();
        Destroy(gameObject);
    }
}
