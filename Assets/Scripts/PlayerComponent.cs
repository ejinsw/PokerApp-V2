using System.Collections;
using System.Collections.Generic;
using Poker;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerComponent : MonoBehaviour {
    public Player Player;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text money;
    [SerializeField] private Transform hand;

    public void Initialize(Player player, float scale) {
        Player = player;
        playerName.text = Player.Name;
        money.text = "$" + Player.Money;
        foreach (Card c in Player.Cards) {
            GameManager.instance.CreateCard(c,GameManager.instance.cardPrefab, hand, scale);
        }
    }

    public IEnumerator DoTurn() {
        yield return null;
    }

    public IEnumerator Fold() {
        yield return null;
    }

    public IEnumerator Check() {
        yield return null;
    }

    public IEnumerator Call() {
        yield return null;
    }

    public IEnumerator Raise() {
        yield return null;
    }
}