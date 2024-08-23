using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Poker;
using UnityEngine;

public class HomeManager : MonoBehaviour
{
    [SerializeField] List<CardComponent> cards;
    void Start()
    {
        int i = 0;
        List<Card> deck = Utilities.NewDeck();
        deck.Shuffle();
        foreach (CardComponent c in cards) {
            c.Initialize(Utilities.DeckTakeOne(ref deck)[0]);
            c.SetContentVisible(true);
            
            RectTransform r = c.gameObject.GetComponent<RectTransform>();
            Vector3 start = r.localPosition;
            
            r.DOLocalMoveY(start.y + 8, 1)
                .SetEase(Ease.InOutQuad) // Use a linear movement
                .OnComplete(() => r.position = start)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(i * 0.2f); 

            i++;
        }
    }
}
