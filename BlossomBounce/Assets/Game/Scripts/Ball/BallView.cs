using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Ball List")]
public class BallView : ScriptableObject
{
    public List<Ball> listData = new List<Ball>();

    public Ball GetCellDataById(int id)
    {
        for (int i = 0; i < listData.Count; i++)
        {
            Ball cellData = listData[i];

            if (id == cellData.id)
            {
                return cellData;
            }
        }
        return null;
    }
}

[System.Serializable]
public class Ball
{
    public int id;
    public UnlockType unlockType;
    public int unlockCondition;
    public Sprite icon;
    public Mesh mesh;
    public Material material;
    public bool isLocked = true;
    public int skinCount;
}

public enum UnlockType
{
    Score,
    Ads,
    Level
}
