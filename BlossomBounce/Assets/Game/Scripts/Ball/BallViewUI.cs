using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallViewUI : MonoBehaviour
{
    public Button button;
    public int id;
    [SerializeField] Image icon;
    public GameObject markSelected;
    public TextMeshProUGUI itemSelectState;
    public TextMeshProUGUI itemLockState;
    public GameObject adsIcon;

    public Action onClick;
    public string condition;
    public bool isSelected = false;

    private Color32 outlineColor = new Color32(0, 89, 145, 255);
    public bool isLocked;

    public int watchAds = 0;

    void Awake()
    {
        button.onClick.AddListener(Button_OnClick);
        adsIcon.SetActive(false);
    }

    void Update()
    {
        if (isLocked)
        {
            itemLockState.gameObject.SetActive(true);
            itemSelectState.gameObject.SetActive(false);
        }
        else
        {
            itemLockState.gameObject.SetActive(false);
            itemSelectState.gameObject.SetActive(true);
            adsIcon.SetActive(false);
        }
    }

    private void Button_OnClick()
    {
        onClick?.Invoke();
    }

    public void SetIconSprite(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void SetSelectState(bool isSelected)
    {
        this.isSelected = isSelected;
        if (isSelected)
        {
            itemSelectState.text = "selected";
            GetComponent<Outline>().effectColor = Color.green;
        }
        else
        {
            itemSelectState.text = "unlocked";
            GetComponent<Outline>().effectColor = outlineColor;
        }
    }

    public void SetLockState(bool isLocked, Ball ballData)
    {
        this.isLocked = isLocked;
        if (isLocked)
        {
            if (ballData.unlockType == UnlockType.Score)
            {
                itemLockState.text = "scored " + ScoreHandler.instance.score + "/" + ballData.unlockCondition;
            }
            else if (ballData.unlockType == UnlockType.Ads)
            {
                itemLockState.text = watchAds + "/" + ballData.unlockCondition;
                itemLockState.rectTransform.anchoredPosition = new Vector3(-15, -5, 0);
                adsIcon.SetActive(true);
            }
            else
            {
                itemLockState.text = "level " + PlayerPrefs.GetInt("Level") + "/" + ballData.unlockCondition;
            }
        }
        else
        {
            
        }
    }
}
