using System.Collections.Generic;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace ObjectPooler
{
    public class Pooler : MonoBehaviour
    {
        #region Helper

        public enum InitOnHelper { Awake, Start, None }

        public const string ParentName = " Parent";

        #endregion

        #region Singleton

        [Header("[Destroy]")] public bool DontDestroy;

        private static Pooler instance;
        public static Pooler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<Pooler>();

                    if (instance == null)
                    {
                        var singletonObject = new GameObject($"Singleton - {nameof(Pooler)}");
                        instance = singletonObject.AddComponent<Pooler>();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Variables

        [Header("[Variables]")] 
        public InitOnHelper InitOn;
        [Space] 
        public List<PoolerConfig> ConfigPools;

        public List<PoolerPlatform> platformPooler;
        
        public Dictionary<string, Queue<GameObject>> PoolDictionary { get; private set; }

        #endregion

        #region Initialize
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                if (DontDestroy) DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("Another instance of " + GetType().Name + " is already exist! Destroying self...");
                Destroy(gameObject);
            }
            
            if (InitOn == InitOnHelper.Awake) Initialize();
        }
        
        private void Start()
        {
            if (InitOn == InitOnHelper.Start) Initialize();
        }

        private void Initialize()
        {
            if (PoolDictionary == null) PoolDictionary = new Dictionary<string, Queue<GameObject>>();
            else PoolDictionary.Clear();

            if (ConfigPools.Count > 0)
            {
                foreach (var pool in ConfigPools)
                {
                    var newPool = new Queue<GameObject>();

                    var parentPool = transform.Find(pool.Prefab.name + ParentName);
                    if (parentPool == null)
                    {
                        parentPool = new GameObject(pool.Prefab.name + ParentName).transform;
                        parentPool.SetParent(transform);
                    }

                    for (var i = 0; i < pool.Size; i++)
                    {
                        var obj = Instantiate(pool.Prefab, parentPool);
                        obj.name = pool.Prefab.name;
                        obj.SetActive(false);
                        newPool.Enqueue(obj);
                    }

                    PoolDictionary.Add(pool.Tag, newPool);
                }
            }
        }

        #endregion

        #region Functions

        public static void CreatePool(string tagValue, GameObject prefabValue, int sizeValue)
        {
            if (Instance.ConfigPools.Find(o => o.Tag.Equals(tagValue)) == null)
            {
                var newSerializePool = new PoolerConfig(tagValue, prefabValue, sizeValue);
                Instance.ConfigPools.Add(newSerializePool);
            }

            if (!Instance.PoolDictionary.ContainsKey(tagValue))
            {
                var newPool = new Queue<GameObject>();

                var parentPool = Instance.transform.Find(prefabValue.name + ParentName);
                if (parentPool == null)
                {
                    parentPool = new GameObject(prefabValue.name + ParentName).transform;
                    parentPool.SetParent(Instance.transform);
                }

                for (var i = 0; i < sizeValue; i++)
                {
                    var obj = Instantiate(prefabValue, parentPool);
                    obj.name = prefabValue.name;
                    obj.SetActive(false);
                    newPool.Enqueue(obj);
                }

                Instance.PoolDictionary.Add(tagValue, newPool);
            }
        }

        public static T SpawnFromPool<T>(string tagValue, Vector3 position, Quaternion rotation) where T : Component
        {
            if (Instance.PoolDictionary.ContainsKey(tagValue))
            {
                if (Instance.PoolDictionary[tagValue].Count == 0)
                {
                    var pool = Instance.ConfigPools.Find(p => p.Tag == tagValue);
                    var parentPool = Instance.transform.Find(pool.Prefab.name + ParentName);
                    if (parentPool == null)
                    {
                        parentPool = new GameObject(pool.Prefab.name + ParentName).transform;
                        parentPool.SetParent(Instance.transform);
                    }

                    for (var i = 0; i < pool.Size; i++)
                    {
                        var obj = Instantiate(pool.Prefab, parentPool);
                        obj.name = pool.Prefab.name;
                        obj.SetActive(false);
                        Instance.PoolDictionary[tagValue].Enqueue(obj);
                    }
                }

                var objToSpawn = Instance.PoolDictionary[tagValue].Dequeue();
                objToSpawn.SetActive(true);
                objToSpawn.transform.SetPositionAndRotation(position, rotation);
                var returnValue = objToSpawn.GetComponent<T>();
                return returnValue;
            }

            Debug.LogWarning("Tag not exist in pool.");
            return null;
        }

        public static T SpawnFromPool<T>(string tagValue, Transform parent) where T : Component
        {
            if (Instance.PoolDictionary.ContainsKey(tagValue))
            {
                if (Instance.PoolDictionary[tagValue].Count == 0)
                {
                    var pool = Instance.ConfigPools.Find(p => p.Tag == tagValue);
                    var parentPool = Instance.transform.Find(pool.Prefab.name + ParentName);
                    if (parentPool == null)
                    {
                        parentPool = new GameObject(pool.Prefab.name + ParentName).transform;
                        parentPool.SetParent(Instance.transform);
                    }

                    for (var i = 0; i < pool.Size; i++)
                    {
                        var obj = Instantiate(pool.Prefab, parentPool);
                        obj.name = pool.Prefab.name;
                        obj.SetActive(false);
                        Instance.PoolDictionary[tagValue].Enqueue(obj);
                    }
                }

                var objToSpawn = Instance.PoolDictionary[tagValue].Dequeue();
                objToSpawn.SetActive(true);
                objToSpawn.transform.SetParent(parent);
                objToSpawn.transform.SetPositionAndRotation(parent.position, parent.rotation);
                var returnValue = objToSpawn.GetComponent<T>();
                return returnValue;
            }

            Debug.LogWarning("Tag not exist in pool.");
            return null;
        }

        public static GameObject SpawnFromPool(string tagValue, Vector3 position, Quaternion rotation)
        {
            if (Instance.PoolDictionary.ContainsKey(tagValue))
            {
                if (Instance.PoolDictionary[tagValue].Count == 0)
                {
                    var pool = Instance.ConfigPools.Find(p => p.Tag == tagValue);
                    var parentPool = Instance.transform.Find(pool.Prefab.name + ParentName);
                    if (parentPool == null)
                    {
                        parentPool = new GameObject(pool.Prefab.name + ParentName).transform;
                        //parentPool.SetParent(Instance.transform);
                    }

                    for (var i = 0; i < pool.Size; i++)
                    {
                        var obj = Instantiate(pool.Prefab, parentPool);
                        obj.name = pool.Prefab.name;
                        obj.SetActive(false);
                        Instance.PoolDictionary[tagValue].Enqueue(obj);
                    }
                }

                var objToSpawn = Instance.PoolDictionary[tagValue].Dequeue();
                objToSpawn.SetActive(true);
                objToSpawn.transform.SetPositionAndRotation(position, rotation);
                return objToSpawn;
            }

            Debug.LogWarning("Tag not exist in pool.");
            return null;
        }

        public static GameObject SpawnFromPool(string tagValue, Transform parent)
        {
            if (Instance.PoolDictionary.ContainsKey(tagValue))
            {
                if (Instance.PoolDictionary[tagValue].Count == 0)
                {
                    var pool = Instance.ConfigPools.Find(p => p.Tag == tagValue);
                    var parentPool = Instance.transform.Find(pool.Prefab.name + ParentName);
                    if (parentPool == null)
                    {
                        parentPool = new GameObject(pool.Prefab.name + ParentName).transform;
                        parentPool.SetParent(Instance.transform);
                    }

                    for (var i = 0; i < pool.Size; i++)
                    {
                        var obj = Instantiate(pool.Prefab, parentPool);
                        obj.name = pool.Prefab.name;
                        obj.SetActive(false);
                        Instance.PoolDictionary[tagValue].Enqueue(obj);
                    }
                }

                var objToSpawn = Instance.PoolDictionary[tagValue].Dequeue();
                objToSpawn.SetActive(true);
                objToSpawn.transform.SetParent(parent);
                objToSpawn.transform.SetPositionAndRotation(parent.position, parent.rotation);
                return objToSpawn;
            }

            Debug.LogWarning("Tag not exist in pool.");
            return null;
        }

        public static void AddToPool(string tagValue, GameObject objToAdd)
        {
            if (Instance.PoolDictionary.ContainsKey(tagValue))
            {
                var pool = Instance.ConfigPools.Find(p => p.Tag == tagValue);
                var parentPool = Instance.transform.Find(pool.Prefab.name + " Parent");
                if (parentPool == null)
                {
                    parentPool = new GameObject(pool.Prefab.name + " Parent").transform;
                    parentPool.SetParent(Instance.transform);
                }

                objToAdd.SetActive(false);
                Instance.PoolDictionary[tagValue].Enqueue(objToAdd);
                objToAdd.transform.SetParent(parentPool);
            }
            else
            {
                Debug.LogWarning("Tag not exist in pool.");
            }
        }

        public static void AddPlatformToPool(string tagValue, GameObject objToAdd)
        {
            var pool = Instance.platformPooler.Find(p => p.Tag == tagValue);
            var parentPool = Instance.transform.Find(pool.Tag + " Parent");
            if (parentPool == null)
            {
                parentPool = new GameObject(pool.Tag + " Parent").transform;
                parentPool.SetParent(Instance.transform);
            }

            objToAdd.transform.SetParent(parentPool);
            objToAdd.SetActive(false);
            //Instance.PoolDictionary[tagValue].Enqueue(objToAdd);
        }

        #endregion
    }
}