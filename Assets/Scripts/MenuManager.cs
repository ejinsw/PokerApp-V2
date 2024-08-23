using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;
    void Awake()
    {
        if (instance == null) instance = this;
    }

    public void Play()
    {
        SceneManager.LoadScene(2);
    }
    
    public void Home()
    {
        SceneManager.LoadScene(1);
    }
}
