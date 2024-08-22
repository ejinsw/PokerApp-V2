using System;
using DG.Tweening;
using UnityEngine;

public class GestureAnimation : MonoBehaviour
{
    [SerializeField] StartButton startButton;
    [SerializeField] GameObject image;
    [SerializeField] float duration = 1.0f; 
    private RectTransform rectTransform;
    private Vector2 startPosition;
    private float endY;
    private Tween moveTween;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.position;
        endY = startPosition.y + 400;

        rectTransform.position = startPosition;
        StartGestureAnimation();
    }

    void Update()
    {
        if (startButton.pointerDown) {
            image.SetActive(false);
            // Stop the animation if it's running and reset the position
            moveTween.Kill();
            rectTransform.position = startPosition;
        }
        else {
            image.SetActive(true);
            if (!moveTween.IsActive() || !moveTween.IsPlaying()) {
                StartGestureAnimation();
            }
        }
    }

    void StartGestureAnimation()
    {
        // Animate to the end position and then reset back to the start
        moveTween = rectTransform.DOMoveY(endY, duration)
            .SetEase(Ease.InOutQuad) // Use a linear movement
            .OnComplete(() => rectTransform.position = startPosition) // Reset the position
            .SetLoops(-1, LoopType.Restart); // Loop indefinitely
    }
}
