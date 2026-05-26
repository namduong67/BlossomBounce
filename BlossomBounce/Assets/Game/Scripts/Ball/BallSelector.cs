using JetBrains.Annotations;
using Nami.Controller;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallSelector : MonoBehaviour
{
    public BallViewUI ballUiPrefab;
    public Transform ballUiContainer;
    public BallView ballView;
    public List<BallViewUI> listBallUi = new List<BallViewUI>();

    public Action<int> onSelectBall;

    GameUI gameUI;

    void Awake()
    {
        LoadFromData(ballView.listData);
        LoadSavedData();
        gameUI = transform.root.GetComponent<GameUI>();
    }

    public void LoadFromData(List<Ball> listBallData)
    {
        foreach (var ballData in listBallData)
        {
            BallViewUI viewUi = Instantiate(ballUiPrefab, ballUiContainer);
            listBallUi.Add(viewUi);

            viewUi.id = ballData.id;
            viewUi.SetIconSprite(ballData.icon);
            viewUi.SetSelectState(false);
            viewUi.SetLockState(ballData.isLocked, ballData);

            viewUi.onClick = () => ViewUi_OnClick(viewUi, ballData);
        }
    }

    private void LoadSavedData()
    {
        int id = PlayerPrefs.GetInt("ball_Id");
        listBallUi[id].SetSelectState(true);
        listBallUi[id].SetLockState(ballView.listData[id].isLocked, ballView.listData[id]);
    }

    private void ViewUi_OnClick(BallViewUI viewUi, Ball ballData)
    {
        if (viewUi.isLocked)
        {
            if (ballData.unlockType == UnlockType.Ads)
            {
                Unlock_WithAds(viewUi, ballData);
            }
        }
        else
        {
            Select(viewUi);
        }
    }

    private void Unlock_WithAds(BallViewUI viewUi, Ball ballData)
    {
        gameUI.loadingRewardAds.SetActive(true);
        for (int i = 0; i < listBallUi.Count; i++)
            listBallUi[i].GetComponent<Button>().interactable = false;
        GameAds.Get.LoadAndShowRewardAd((v) =>
        {
            for (int i = 0; i < listBallUi.Count; i++)
                listBallUi[i].GetComponent<Button>().interactable = true;
            if (v)
            {
                Debug.Log($"Rewarded");
                gameUI.loadingRewardAds.SetActive(false);
                viewUi.watchAds++;
                viewUi.itemLockState.text = viewUi.watchAds + "/" + ballData.unlockCondition;
                if (viewUi.watchAds >= ballData.unlockCondition)
                {
                    ballData.isLocked = false;
                    viewUi.SetLockState(false, ballData);
                    Select(viewUi);
                }
            }
            else
            {
                Debug.Log($"Fail");
                gameUI.CantGetRewardAd();
            }
        });
    }

    public void Select(BallViewUI ballUi)
    {
        for (int i = 0; i < listBallUi.Count; i++)
        {
            if (listBallUi[i] != ballUi)
            {
                listBallUi[i].SetSelectState(false);
            }
            else
            {
                listBallUi[i].SetSelectState(true);
                onSelectBall?.Invoke(ballUi.id);
            }
        }
    }
}
