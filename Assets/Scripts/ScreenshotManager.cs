using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager instance;
    int count = 0;
    void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(instance);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            ScreenCapture.CaptureScreenshot($"/Users/ejinsw/Desktop/poker_app_screenshot_{count++}.png");
        }
    }
}
