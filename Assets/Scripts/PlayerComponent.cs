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

    public void UpdateUI() {
        money.text = "$" + Player.Money;
    }

    public IEnumerator DoTurn() {
        // TODO: Replace probability with folding frequency
        if (Utilities.TrueWithProbability(0.3f) && GameManager.instance.game.LastRaiser != null) {
            yield return StartCoroutine(Fold(true));
        }
        else {
            Player lastRaiser = GameManager.instance.game.LastRaiser;
            if (lastRaiser != null && lastRaiser != Player) {
                // Call or reraise
                // TODO: Change the probability of calling vs reraising
                if (Utilities.TrueWithProbability(0.5f)) {
                    yield return StartCoroutine(Call(lastRaiser.LastAction().Money, true));
                }
                else {
                    // TODO: Change the bounds for reraising from just +10 to +100
                    long raiseAmount = lastRaiser.LastAction() != null
                        ? Utilities.RandomInt((int)lastRaiser.LastAction().Money + 10, (int)lastRaiser.LastAction().Money + 100)
                        : Utilities.RandomInt(10, 100);
                    yield return StartCoroutine(Raise(raiseAmount, true));
                }
            }
            else {
                // Check or raise
                // TODO: Change the probability of checking vs raising
                if (Utilities.TrueWithProbability(0.5f)) {
                    yield return StartCoroutine(Check(true));
                }
                else {
                    // TODO: Change the bounds for raising 
                    yield return StartCoroutine(Raise(Utilities.RandomInt(10, 100), true));
                }
            }
        }

        yield return null;
    }

    public IEnumerator DoTurn(PlayerAction action) {
        switch (action.ActionType) {
            case ActionType.Fold:
                yield return StartCoroutine(Fold(false));
                break;
            case ActionType.Check:
                yield return StartCoroutine(Check(false));
                break;
            case ActionType.Call:
                yield return StartCoroutine(Call(action.Money, false));
                break;
            case ActionType.Raise:
                yield return StartCoroutine(Raise(action.Money, false));
                break;
        }

        yield return null;
    }

    public IEnumerator Fold(bool logAction) {
        if (logAction)
            Player.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
        Player.Folded = true;
        yield return null;
    }

    public IEnumerator Check(bool logAction) {
        if (logAction)
            Player.ActionLog.Add(new PlayerAction(ActionType.Check, 0));
        yield return null;
    }

    public IEnumerator Call(long callAmount, bool logAction) {
        // TODO: For continuous games DON'T DELETE
        // long useAmount = Player.LastAction() != null
        //     ? callAmount - Player.LastAction().Money
        // : callAmount;
        
        Player.UseMoney(callAmount);
        if (logAction)
            Player.ActionLog.Add(new PlayerAction(ActionType.Call, callAmount));
        yield return null;
    }

    public IEnumerator Raise(long raiseAmount, bool logAction) {
        // TODO: For continuous games DON'T DELETE
        // long useAmount = Player.LastAction() != null
        //     ? raiseAmount - Player.LastAction().Money
        //     : raiseAmount;

        Player.UseMoney(raiseAmount);
        if (logAction)
            Player.ActionLog.Add(new PlayerAction(ActionType.Raise, raiseAmount));
        GameManager.instance.game.LastRaiser = Player;
        yield return null;
    }
}