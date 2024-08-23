using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    

    bool userInput = false;
    RectTransform rectTransform;
    Camera cam;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            userInput = true;
        }
    }
    public IEnumerator Activate(string msg, bool typeText = false)
    {
        userInput = false;
        gameObject.SetActive(true);

        text.text = "";
        if (!typeText) {
            text.text = msg;
        }
        else {
            foreach (char c in msg) {
                if (userInput) {
                    text.text = msg;
                    userInput = false;
                    break;
                }
                text.text += c;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetChild(0).gameObject.GetComponent<RectTransform>());
        
        yield return new WaitUntil(() => userInput);

        gameObject.SetActive(false);
    }
}
