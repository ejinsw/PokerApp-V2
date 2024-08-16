using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaiseComponent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Slider raiseSlider;
    [SerializeField] RectTransform sliderRect;
    [SerializeField] RectTransform handleRect;
    
    [SerializeField] TMP_Text raiseMoneyText;
    [SerializeField] TMP_Text confirmText;
    [SerializeField] Button raiseButton;


    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        confirmText.gameObject.SetActive(true);
        raiseMoneyText.gameObject.SetActive(false);
        
        sliderRect.sizeDelta = new Vector2(sliderRect.sizeDelta.x, 40);
        handleRect.sizeDelta = new Vector2(40, handleRect.sizeDelta.y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        raiseMoneyText.gameObject.SetActive(true);
        confirmText.gameObject.SetActive(false);

        sliderRect.DOSizeDelta(new Vector2(sliderRect.sizeDelta.x, 60), 0.1f);
        handleRect.DOSizeDelta(new Vector2(60, handleRect.sizeDelta.y), 0.1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        confirmText.gameObject.SetActive(true);
        raiseMoneyText.gameObject.SetActive(false);
        
        // Animate back to original sizes when releasing
        sliderRect.DOSizeDelta(new Vector2(sliderRect.sizeDelta.x, 40), 0.1f);
        handleRect.DOSizeDelta(new Vector2(40, handleRect.sizeDelta.y), 0.1f);
    }
}
