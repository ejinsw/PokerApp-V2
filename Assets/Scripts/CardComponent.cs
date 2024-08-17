using System.Collections;
using System.Collections.Generic;
using Poker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardComponent : MonoBehaviour {
    private Card Card;
    [SerializeField] private GameObject background;
    [SerializeField] private TMP_Text rank;
    [SerializeField] private Image suit;

    public void Initialize(Card c) {
        Card = c;
        
        rank.text = Utilities.rank_as_string[c.Rank];
        
        switch (c.Suit) {
            case Suit.Clubs:
                suit.sprite = GameManager.instance.IconClubs;
                suit.color = Color.black;
                rank.color = Color.black;
                break;
            case Suit.Diamonds:
                suit.sprite = GameManager.instance.IconDiamonds;
                suit.color = Color.red;
                rank.color = Color.red;
                break;
            case Suit.Hearts:
                suit.sprite = GameManager.instance.IconHearts;
                suit.color = Color.red;
                rank.color = Color.red;
                break;
            case Suit.Spades:
                suit.sprite = GameManager.instance.IconSpades;
                suit.color = Color.black;
                rank.color = Color.black;
                break;
        }
        
        SetContentVisible(c.Visible);
    }
    
    public void SetContentVisible(bool visible) {
        background.SetActive(!visible);
    }

    public void SetScale(float f) {
        transform.localScale = new Vector3(f, f, 1);
    }
}