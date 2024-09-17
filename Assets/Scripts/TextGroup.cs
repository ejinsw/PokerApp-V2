using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextGroup : MonoBehaviour
{
    [SerializeField] List<TMP_Text> textFields;

    public void SetColor(Color color)
    {
        foreach (TMP_Text text in textFields) {
            text.color = color;
        }
    }
}
