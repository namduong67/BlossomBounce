using System;
using UnityEngine;

namespace Nami.Helper
{
    public class FPSDisplay : MonoBehaviour
    {
        float deltaTime = 0.0f;

        void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(200, 50, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 60;
            style.normal.textColor = Color.red;
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);

            if (fps < 10) Debug.LogError("low FPS  " + fps + " --  " + DateTime.Now.ToLongTimeString());
        }
    }
}
