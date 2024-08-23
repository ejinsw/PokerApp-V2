using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float duration = 1f;
    [SerializeField] Image glow;

    [SerializeField] GameObject shadowPrefab;
    [SerializeField] Transform shadowTransform;
    
    private RectTransform rectTransform;
    private Vector2 startPosition;
    float endY;

    Tween idleTween;
    Tween glowTween;
    Coroutine shadowCoroutine;

    public bool pointerDown = false;

    void Start()
    {
        // Get the RectTransform component
        rectTransform = GetComponent<RectTransform>();
        // Store the initial position of the UI element
        startPosition = rectTransform.position;
        endY = startPosition.y + 1000;
    }

    void Update()
    {
        if (pointerDown) {
            idleTween.Kill();
            glowTween.Kill();
            rectTransform.position = new Vector3(rectTransform.position.x, Mathf.Clamp(Input.mousePosition.y, startPosition.y, endY), rectTransform.position.z);
            if (shadowCoroutine == null) shadowCoroutine = StartCoroutine(ShadowCoroutine());
        }
        else {
            if (rectTransform.position.y >= endY - 600) {
                StartGame();
            }
            else {
                rectTransform.position = startPosition;
                if (!idleTween.IsActive() && !glowTween.IsActive() || !idleTween.IsPlaying() && !glowTween.IsPlaying()) {
                    StartAnimation();
                }
            }
            if (shadowCoroutine != null) {
                StopCoroutine(shadowCoroutine);
                shadowCoroutine = null;
            }
        }
    }

    IEnumerator ShadowCoroutine()
    {
        while (true) {
            Instantiate(shadowPrefab, transform.position, Quaternion.identity, shadowTransform);
            yield return new WaitForSeconds(0.03f);
        }
    }
    
    void StartAnimation()
    {
        // Animate to the end position and then reset back to the start
        idleTween = rectTransform.DOMoveY(startPosition.y + 20, duration)
            .SetEase(Ease.InOutQuad) // Use a linear movement
            .OnComplete(() => rectTransform.position = startPosition) // Reset the position
            .SetLoops(-1, LoopType.Yoyo); // Loop indefinitely
        
        glowTween = glow.DOFade(0.03f, duration)
            .SetEase(Ease.InOutQuad) // Use a linear movement
            .SetLoops(-1, LoopType.Yoyo); // Loop indefinitely
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
