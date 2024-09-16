using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HistoryItem : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text moneyText;

    public void SetName(string text)
    {
        nameText.text = text;
    }
    public void SetMoney(string text)
    {
        moneyText.text = text;
    }
}
