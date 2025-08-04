using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FallingCubeController : MonoBehaviour
{
    public List<FallingCube> fallingCubes;
    public bool isInitialized = false;
    public float delayBetweenCubes = 0.33f;
    public float resetAfterSeconds = 5f;

    private void Update()
    {
        if (isInitialized)
        {
            return;
        }

        int touchedCubeIndex = -1;
        for (int i = 0; i < fallingCubes.Count; i++)
        {
            if (fallingCubes[i].isTouchedByPlayer)
            {
                touchedCubeIndex = i;
                break;
            }
        }

        if (touchedCubeIndex == -1)
        {
            return;
        }

        isInitialized = true;
        for (int i = 0; i < touchedCubeIndex; i++)
        {
            if (!fallingCubes[i].isTouchedByPlayer)
            {

                fallingCubes[i].isTouchedByPlayer = true;
                fallingCubes[i].GetComponent<Rigidbody>().useGravity = true;
                break;
            }
        }

        for (int i = touchedCubeIndex; i < fallingCubes.Count; i++)
        {
            StartCoroutine(FallCubeWithDelay(i - touchedCubeIndex + 1, fallingCubes[i]));
        }

        StartCoroutine(ResetFallingCubesAfterDelay());
    }

    private System.Collections.IEnumerator FallCubeWithDelay(int index, FallingCube cube)
    {
        yield return new WaitForSeconds(delayBetweenCubes + index);
        if (!cube.isTouchedByPlayer)
        {
            cube.isTouchedByPlayer = true;
            cube.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    private System.Collections.IEnumerator ResetFallingCubesAfterDelay()
    {
        yield return new WaitForSeconds(resetAfterSeconds);
        foreach (var cube in fallingCubes)
        {
            cube.Reset();
        }
        isInitialized = false;
    }
}
