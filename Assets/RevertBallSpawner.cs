using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RevertBallSpawner : MonoBehaviour
{
    public GameObject revertBallPrefab;
    public List<RevertBall> revertBalls;
    public Transform targetPlacePoint;
    public Transform ballSpawnPoint;
    public int ballCount;
    public float time;
    public float spawnEverySeconds;
    public float ballSpeed;
    public int currentlySpawnedBallIndex = 0;

    private void Awake()
    {
        for (int i = 0; i < ballCount; i++)
        {
            var spawnPosition = ballSpawnPoint.position;
            var ball = Instantiate(revertBallPrefab, spawnPosition, ballSpawnPoint.rotation, transform.parent);
            var revertBall = ball.GetComponent<RevertBall>();
            revertBall.Init(targetPlacePoint);
            revertBalls.Add(revertBall);
        }
        time = spawnEverySeconds;
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time < spawnEverySeconds)
        {
            return;
        }
        time %= spawnEverySeconds;

        var nextBall = revertBalls[currentlySpawnedBallIndex];
        nextBall.StartRun(ballSpeed);
        currentlySpawnedBallIndex++;
        currentlySpawnedBallIndex %= ballCount;
    }
}
