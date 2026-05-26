using Nami.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockSkin : MonoBehaviour
{
    public static UnlockSkin instance;

    [SerializeField] GameUI gameUi;
    public List<int> unlockedList = new List<int>();

    public bool newSkinUnlocked = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        gameUi = GameObject.Find("GameUI").GetComponent<GameUI>();

        BallSelector ballSelector = gameUi.ballView.GetComponent<BallSelector>();

        for (int i = 0; i < ballSelector.ballView.listData.Count; i++)
            if (!ballSelector.ballView.listData[i].isLocked)
                unlockedList.Add(ballSelector.ballView.listData[i].id);
    }

    void Update()
    {
        if (gameUi == null)
        {
            gameUi = GameObject.Find("GameUI").GetComponent<GameUI>();
        }


        if (unlockedList.Count > ScoreHandler.instance.unlockedSkin)
            newSkinUnlocked = true;
        else
            newSkinUnlocked = false;
    }

    public void OpenStore()
    {
        newSkinUnlocked = false;
        ScoreHandler.instance.unlockedSkin = unlockedList.Count;
    }
}
