using System.Collections;
using System.Collections.Generic;
using Poker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardComponent : MonoBehaviour {
    [SerializeField] private TMP_Text rank;
    [SerializeField] private Image suit;

    public void SetScale(float f) {
        transform.localScale = new Vector3(f, f, 1);
    }
    
    public void SetRank(Rank r) {
        rank.text = Utilities.rank_as_string[r];
    }

    public void SetSuit(Suit s) {
        switch (s) {
            case Suit.Clubs:
                suit.sprite = GameManager.instance.IconClubs;
                break;
            case Suit.Diamonds:
                suit.sprite = GameManager.instance.IconDiamonds;
                break;
            case Suit.Hearts:
                suit.sprite = GameManager.instance.IconHearts;
                break;
            case Suit.Spades:
                suit.sprite = GameManager.instance.IconSpades;
                break;
        }
    }
}