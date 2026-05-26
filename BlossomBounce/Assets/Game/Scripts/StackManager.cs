using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackManager : MonoBehaviour
{
    public GameObject[] platformArray;
    public int sideNumber = 0;

    public void SetRandomPlatform(Material mat)
    {
        for (int i = 0; i < platformArray.Length; i++)
        {
            platformArray[i].GetComponent<NewPlatformController>().SetRandomPlatformMat(mat);
        }
    }

    public void SetPlatformRotation()
    {
        if (platformArray != null && platformArray.Length > 1)
        {
            for (int i = 1; i < platformArray.Length; i++)
            {
                platformArray[i].transform.localEulerAngles = platformArray[i - 1].transform.localEulerAngles - Vector3.up * 5;
                platformArray[i].transform.position = platformArray[i - 1].transform.position - Vector3.up * 0.5f;
            }
        }
    }

    public void InitPlatforms()
    {
        for (int i = 0; i < platformArray.Length; i++)
        {
            platformArray[i].GetComponent<NewPlatformController>().InitPlatforms();
        }
    }

    public float LastPlatformPosition()
    {
        float lastY = platformArray[platformArray.Length - 1].transform.position.y;
        return lastY;
    }

    public float LastPlatformRotation()
    {
        float lastY = platformArray[platformArray.Length - 1].transform.localEulerAngles.y;
        float lastRotation = transform.localEulerAngles.y + lastY;
        return lastRotation;
    }
}
