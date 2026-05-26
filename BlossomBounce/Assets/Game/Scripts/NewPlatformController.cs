using ObjectPooler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlatformController : MonoBehaviour
{
    public byte[] platformsPartType;

    [SerializeField] private PlatformPartController[] _platforms = null;

    public void InitPlatforms()
    {
        for (int i = 0; i < platformsPartType.Length; i++)
        {
            switch (platformsPartType[i])
            {
                case 1:
                    _platforms[i].gameObject.tag = "GoodPlatform";
                    _platforms[i].GetComponent<MeshRenderer>().material.color = Color.white;
                    break;
                case 0:
                    _platforms[i].gameObject.tag = "BadPlatform";
                    _platforms[i].GetComponent<MeshRenderer>().material.color = new Color32(0, 47, 65, 255);
                    break;
            }
        }
    }

    public void SetRandomPlatformMat(Material mat)
    {
        for (int i = 0; i < _platforms.Length; i++)
        {
            _platforms[i].GetComponent<MeshRenderer>().material = mat;
        }
    }

    public void BreakAllPlatforms()
    {
        if (transform.parent != null)
        {
            transform.parent = null;
            Platforms.instance.player.IncreaseScore();
        }

        foreach (PlatformPartController p in _platforms)
        {
            p.BreakingPlatforms();
        }

        StartCoroutine(RemoveParts());
    }

    IEnumerator RemoveParts()
    {
        yield return new WaitForSeconds(1f);
        Pooler.AddPlatformToPool("Platforms", gameObject);
    }
}
