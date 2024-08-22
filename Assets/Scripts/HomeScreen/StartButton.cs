using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Vector2 startPosition;
    float endY;

    public bool pointerDown = false;

    void Start()
    {
        // Get the RectTransform component
        rectTransform = GetComponent<RectTransform>();
        // Store the initial position of the UI element
        startPosition = rectTransform.position;
        endY = startPosition.y + 400;
    }

    void Update()
    {
        if (pointerDown) {
            rectTransform.position = new Vector3(rectTransform.position.x, Mathf.Clamp(Input.mousePosition.y, startPosition.y, endY), rectTransform.position.z);
        }
        else {
            if (rectTransform.position.y >= endY - 30) {
                StartGame();
            }
            else {
                rectTransform.position = startPosition;
            }
        }
    }

    void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
    }
}
