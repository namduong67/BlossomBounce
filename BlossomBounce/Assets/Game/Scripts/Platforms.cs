using ObjectPooler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platforms : MonoBehaviour
{
    public static Platforms instance;

    public Player player;
    public float speed = 100f;

    public List<GameObject> gameObjectList = new List<GameObject>();
    public int activeCount = 3;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        //for(int i=0;i<transform.childCount;i++)
        //{
        //    transform.GetChild(i).gameObject.SetActive(false);
        //    Debug.LogError(transform.GetChild(i).name + ": " + transform.GetChild(i).gameObject.activeSelf);
        //}

        for(int i=0;i<transform.childCount;i++)
        {
            gameObjectList.Add(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < activeCount && i < gameObjectList.Count; i++)
        {
            gameObjectList[i].SetActive(true);
        }

        for (int i = activeCount; i < gameObjectList.Count; i++)
        {
            gameObjectList[i].SetActive(false);
        }
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
        for (int i = 0; i < activeCount && i < gameObjectList.Count; i++)
        {
            if (gameObjectList[i].transform.childCount == 0)
            {
                gameObjectList[i].SetActive(false);

                GameObject deactivatedObject = gameObjectList[i];
                gameObjectList.RemoveAt(i);
                gameObjectList.Add(deactivatedObject);

                if (activeCount < gameObjectList.Count)
                {
                    gameObjectList[activeCount - 1].SetActive(true);
                }

                break;
            }
        }
    }
}
