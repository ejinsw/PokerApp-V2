using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipShadow : MonoBehaviour
{
    void OnEnable()
    {
        StartCoroutine(KillSelf());
    }

    IEnumerator KillSelf()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
