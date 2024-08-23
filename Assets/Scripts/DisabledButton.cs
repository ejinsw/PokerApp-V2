using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisabledButton : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    Coroutine coroutine;
    void OnEnable()
    {
        if (coroutine == null) coroutine = StartCoroutine(TypeText());
    }

    void OnDisable()
    {
        if (coroutine != null) StopCoroutine(coroutine);
    }

    IEnumerator TypeText()
    {
        text.text = ".";
        while (true) {
            if (text.text == ".....") {
                text.text = ".";
            }
            else {
                text.text += ".";
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
