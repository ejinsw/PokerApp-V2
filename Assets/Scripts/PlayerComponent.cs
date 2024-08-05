using System.Collections;
using System.Collections.Generic;
using Poker;
using TMPro;
using UnityEngine;

public class PlayerComponent : MonoBehaviour {
    public Player Player;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text money;
    [SerializeField] private Transform hand;
    [SerializeField] private GameObject cardPrefab;

    public void Initialize(Player player) {
        Player = player;
        playerName.text = Player.Name;
        money.text = "$" + Player.Money;
        foreach (Card c in Player.Cards) {
            GameObject cardObject = Instantiate(cardPrefab, hand);
            CardComponent component = cardObject.GetComponent<CardComponent>();
            component.SetRank(c.Rank); 
            component.SetSuit(c.Suit); 
        }
    }
}