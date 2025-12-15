using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RevertBallSpawner : MonoBehaviour
{
    public GameObject revertBallPrefab;
    public List<RevertBall> revertBalls;
    public Transform revertPlayerToPoint;
    public Transform ballSpawnPoint;
    public int ballCount;
    public float time;
    public float spawnEverySeconds;
    public float ballSpeed;

    private void Awake()
    {
        time = spawnEverySeconds;
    }

    private RevertBall SpawnBall()
    {
        var spawnPosition = ballSpawnPoint.position;
        var ball = Instantiate(revertBallPrefab, spawnPosition, ballSpawnPoint.rotation, transform.parent);
        var revertBall = ball.GetComponent<RevertBall>();
        revertBall.Init(revertPlayerToPoint);
        revertBalls.Add(revertBall);
        ball.SetActive(false);
        return revertBall;
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time < spawnEverySeconds)
        {
            return;
        }
        time %= spawnEverySeconds;
        var nextBall = revertBalls.FirstOrDefault(b => !b.gameObject.activeSelf);
        if (nextBall == null)
        {
            nextBall = SpawnBall();
        }
        nextBall.gameObject.SetActive(true);
        nextBall.StartRun(ballSpeed);
    }
}
