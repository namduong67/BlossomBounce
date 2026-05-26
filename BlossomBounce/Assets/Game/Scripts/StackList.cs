using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Stack List")]
public class StackList : ScriptableObject
{
    public List<StackObject> stackList = new List<StackObject>();

    public void RandomEasyStack(int index, GameObject[] platforms)
    {
        for (int i = 0; i < stackList[index].easyStack.Length; i++)
        {
            platforms[i] = stackList[index].easyStack[i];
        }
    }

    public void RandomHardStack(int index, GameObject[] platforms)
    {
        for (int i = 0; i < stackList[index].hardStack.Length; i++)
        {
            platforms[i] = stackList[index].hardStack[i];
        }
    }

    public ColorAndBG RandomPair(int index)
    {
        ColorAndBG color = stackList[index].colorAndBG[Random.Range(0, stackList[index].colorAndBG.Length)];
        return color;
    }
}

[System.Serializable]
public class StackObject
{
    public int id;
    public ColorAndBG[] colorAndBG;
    public GameObject[] easyStack;
    public GameObject[] hardStack;
}

[System.Serializable]
public class ColorAndBG
{
    public Material material;
    public Sprite background;
}
