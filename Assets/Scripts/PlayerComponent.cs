using System.Collections;
using System.Collections.Generic;
using Poker;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = Unity.Mathematics.Random;

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
        List<Player> players = GameManager.instance.game.Players;
        int round = GameManager.instance.game.Round;

        // TODO: Replace probability with folding frequency
        if (Utilities.TrueWithProbability(0.3f)) {
            yield return StartCoroutine(Fold());
        }
        else {
            Player highestRaiser = Utilities.HighestRaiser(Player, players, round);
            if (Utilities.ContainsRaise(Player, players, round) && highestRaiser != Player) {
                // Call or reraise
                // TODO: Change the probability of calling vs reraising
                if (Utilities.TrueWithProbability(0.5f)) {
                    yield return StartCoroutine(Call(highestRaiser.ActionLog[round].Money));
                }
                else {
                    // TODO: Change the bounds for reraising from just +100
                    yield return StartCoroutine(Raise((float)Utilities.RandomDouble(highestRaiser.ActionLog[round].Money, highestRaiser.ActionLog[round].Money + 100)));
                }
            }
            else {
                // Check or raise
                // TODO: Change the probability of checking vs raising
                if (Utilities.TrueWithProbability(0.5f)) {
                    yield return StartCoroutine(Check());
                }
                else {
                    // TODO: Change the bounds for raising 
                    yield return StartCoroutine(Raise((float)Utilities.RandomDouble(10, 100)));
                }
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