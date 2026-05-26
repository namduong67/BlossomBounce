using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T m_Instance = null;
    public static T instance
    {
        get
        {
            // Instance requiered for the first time, we look for it
            if (m_Instance == null)
            {
                m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (m_Instance != null)
                {
                    m_Instance.Init();
                }
            }
            return m_Instance;
        }
    }

    public static bool Exists()
    {
        return (m_Instance != null);
    }

    // If no other monobehaviour request the instance in an awake function
    // executing before this one, no need to search the object.
    protected virtual void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this as T;
            m_Instance.Init();
        }
    }

    // This function is called when the instance is used the first time
    // Put all the initializations you need here, as you would do in Awake
    public virtual void Init() { }

    // Make sure the instance isn't referenced anymore when the user quit, just in case.
    private void OnApplicationQuit()
    {
        m_Instance = null;
    }
}