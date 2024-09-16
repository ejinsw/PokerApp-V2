using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryManager : MonoBehaviour
{
    [SerializeField] Transform content;
    [SerializeField] GameObject historyPrefab;
    public void AddHistory(string name, string money)
    {
        GameObject newHistory = Instantiate(historyPrefab, content);
        HistoryItem historyItem = newHistory.GetComponent<HistoryItem>();
        historyItem.SetName(name);
        historyItem.SetMoney(money);
    }

    public void Clear()
    {
        for (int i = 1; i < content.childCount; i++) {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
