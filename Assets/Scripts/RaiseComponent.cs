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
    
    [SerializeField] TMP_Text confirmText;
    [SerializeField] Button raiseButton;


    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        sliderRect.sizeDelta = new Vector2(sliderRect.sizeDelta.x, 60);
        handleRect.sizeDelta = new Vector2(60, handleRect.sizeDelta.y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        sliderRect.DOSizeDelta(new Vector2(sliderRect.sizeDelta.x, 80), 0.1f);
        handleRect.DOSizeDelta(new Vector2(80, handleRect.sizeDelta.y), 0.1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        sliderRect.DOSizeDelta(new Vector2(sliderRect.sizeDelta.x, 60), 0.1f);
        handleRect.DOSizeDelta(new Vector2(60, handleRect.sizeDelta.y), 0.1f);
    }
}
