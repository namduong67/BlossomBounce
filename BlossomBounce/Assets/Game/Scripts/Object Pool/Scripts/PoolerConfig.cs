using System;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace ObjectPooler
{
    [Serializable]
    public class PoolerConfig
    {
        public string Tag;
        public GameObject Prefab;
        public int Size;
        
        public PoolerConfig(string tag, GameObject prefab, int size)
        {
            Tag = tag;
            Prefab = prefab;
            Size = size;
        }
    }

    [Serializable]
    public class PoolerPlatform
    {
        public string Tag;

        public PoolerPlatform(string tag)
        {
            Tag = tag;
        }
    }
}