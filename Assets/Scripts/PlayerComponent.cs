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
            GameManager.instance.CreateCard(c, GameManager.instance.cardPrefab, hand, scale);
        }
    }

    public IEnumerator DoTurn() {
        // TODO: Replace probability with folding frequency
        if (Utilities.TrueWithProbability(0.5f)) {
            yield return StartCoroutine(Fold());
        }
        else {
            if (Utilities.ContainsRaise(Player, GameManager.instance.game.Players, GameManager.instance.game.Round)) {
                int choice = Utilities.RandomInt(1, 2);
            }
        }

        yield return null;
    }

    public IEnumerator Fold() {
        Player.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
        yield return null;
    }

    public IEnumerator Check() {
        Player.ActionLog.Add(new PlayerAction(ActionType.Check, 0));
        yield return null;
    }

    public IEnumerator Call(float callAmount) {
        Player.ActionLog.Add(new PlayerAction(ActionType.Call, callAmount));
        yield return null;
    }

    public IEnumerator Raise(float raiseAmount) {
        Player.ActionLog.Add(new PlayerAction(ActionType.Raise, raiseAmount));
        yield return null;
    }
}