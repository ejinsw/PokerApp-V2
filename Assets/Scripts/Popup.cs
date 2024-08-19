using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] GameObject arrowBL;
    [SerializeField] GameObject arrowBR;
    [SerializeField] GameObject arrowTL;
    [SerializeField] GameObject arrowTR;
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
    public IEnumerator Activate(Vector2 position, string msg, bool bottom, bool left, bool typeText = false)
    {
        // Convert screen position to canvas position
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            position,
            cam, // Assuming it's a Screen Space - Overlay canvas; pass the camera if Screen Space - Camera
            out canvasPosition
        );

        // Set the position of the popup
        rectTransform.anchoredPosition = position;


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

        arrowBL.SetActive(bottom && left);
        arrowBR.SetActive(bottom && !left);
        arrowTL.SetActive(!bottom && left);
        arrowTR.SetActive(!bottom && !left);

        yield return new WaitUntil(() => userInput);

        gameObject.SetActive(false);
    }
}
