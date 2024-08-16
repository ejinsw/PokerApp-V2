using System;
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
    [SerializeField] TMP_Text actionMessageText;

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

    public IEnumerator TypeText(string text)
    {
        actionMessageText.text = "";
        foreach (char c in text) {
            actionMessageText.text += c;
            yield return new WaitForSeconds(0.1f);
        }
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
                        ? Utilities.RandomInt((int)lastRaiser.LastAction().Money + 10, (int)lastRaiser.LastAction().Money + 100, GameManager.STEP)
                        : Utilities.RandomInt(10, 100, GameManager.STEP);
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
                    yield return StartCoroutine(Raise(Utilities.RandomInt(10, 100, GameManager.STEP), true));
                }
            }
        }
        
        if (Player.LastAction() != null)
            GameManager.instance.game.ActionLog.Add(Player.LastAction());

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
        
        if (Player.LastAction() != null)
            GameManager.instance.game.ActionLog.Add(Player.LastAction());

        yield return null;
    }

    public IEnumerator Fold(bool logAction) {
        if (logAction)
            Player.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
        Player.Folded = true;
        yield return StartCoroutine(TypeText("Folded..."));
    }

    public IEnumerator Check(bool logAction) {
        if (logAction)
            Player.ActionLog.Add(new PlayerAction(ActionType.Check, 0));
        yield return StartCoroutine(TypeText("Checked..."));
    }

    public IEnumerator Call(long callAmount, bool logAction) {
        // TODO: For continuous games DON'T DELETE
        // long useAmount = Player.LastAction() != null
        //     ? callAmount - Player.LastAction().Money
        // : callAmount;
        
        Player.UseMoney(callAmount);
        if (logAction)
            Player.ActionLog.Add(new PlayerAction(ActionType.Call, callAmount));
        yield return StartCoroutine(TypeText($"Called ${callAmount}..."));
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
        yield return StartCoroutine(TypeText($"Raised by ${raiseAmount}..."));
    }
}