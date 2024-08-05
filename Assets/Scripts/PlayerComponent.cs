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
    

    public void Initialize(Player player) {
        Player = player;
        playerName.text = Player.Name;
        money.text = "$" + Player.Money;
        foreach (Card c in Player.Cards) {
            GameManager.instance.CreateCard(c.Suit, c.Rank, hand);
        }
    }
}