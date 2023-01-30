using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScreenshotMaker : MonoBehaviour {
#if UNITY_EDITOR

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            int i = 0;
            string name;
            do {
                name = "Screen_" + (i++) + ".png";
            } while (File.Exists(name));

            ScreenCapture.CaptureScreenshot(name);
        }
    }
#endif
}

