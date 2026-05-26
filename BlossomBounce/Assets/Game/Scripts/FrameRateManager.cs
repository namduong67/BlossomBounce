using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    [Header("Frame Settings")]
    int maxRate = 9999;
    public float targetFrameRate = 150.0f;
    float currentFrameRate;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxRate;
        currentFrameRate = Time.realtimeSinceStartup;
        StartCoroutine(WaitForNextFrame());

        IEnumerator WaitForNextFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                currentFrameRate += 1.0f / targetFrameRate;
                var t = Time.realtimeSinceStartup;
                var sleepTime = currentFrameRate - t - 0.01f;
                if (sleepTime > 0)
                    Thread.Sleep((int)(sleepTime * 1000));
                while (t < currentFrameRate)
                    t = Time.realtimeSinceStartup;
            }
        }
    }
}
