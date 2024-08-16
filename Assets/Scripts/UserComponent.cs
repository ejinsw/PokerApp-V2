using System;
using System.Collections;
using System.Collections.Generic;
using Poker;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public class UserComponent : MonoBehaviour
{
    public Player User;
    [SerializeField] private TMP_Text money;
    [SerializeField] CardComponent card1;
    [SerializeField] CardComponent card2;

    public void Initialize(Player user)
    {
        User = user;
        money.text = "$" + User.Money;
        if (User.Cards.Count > 0 && User.Cards[0] != null)
            card1.Initialize(User.Cards[0]);
        if (User.Cards.Count > 1 && User.Cards[1] != null)
            card1.Initialize(User.Cards[1]);
    }

    public void UpdateUI()
    {
        money.text = "$" + User.Money;
    }

    public IEnumerator DoTurn()
    {
        // TODO: Replace probability with folding frequency
        if (Utilities.TrueWithProbability(0.3f) && GameManager.instance.game.LastRaiser != null) {
            yield return StartCoroutine(Fold(true));
        }
        else {
            Player lastRaiser = GameManager.instance.game.LastRaiser;
            if (lastRaiser != null && lastRaiser != User) {
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

        if (User.LastAction() != null)
            GameManager.instance.game.ActionLog.Add(User.LastAction());

        yield return null;
    }

    public IEnumerator DoTurn(PlayerAction action)
    {
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

        if (User.LastAction() != null)
            GameManager.instance.game.ActionLog.Add(User.LastAction());

        yield return null;
    }

    public IEnumerator Fold(bool logAction)
    {
        if (logAction)
            User.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
        User.Folded = true;
        yield return null;
    }

    public IEnumerator Check(bool logAction)
    {
        if (logAction)
            User.ActionLog.Add(new PlayerAction(ActionType.Check, 0));
        yield return null;
    }

    public IEnumerator Call(long callAmount, bool logAction)
    {
        // TODO: For continuous games DON'T DELETE
        // long useAmount = Player.LastAction() != null
        //     ? callAmount - Player.LastAction().Money
        // : callAmount;

        User.UseMoney(callAmount);
        if (logAction)
            User.ActionLog.Add(new PlayerAction(ActionType.Call, callAmount));
        yield return null;
    }

    public IEnumerator Raise(long raiseAmount, bool logAction)
    {
        // TODO: For continuous games DON'T DELETE
        // long useAmount = Player.LastAction() != null
        //     ? raiseAmount - Player.LastAction().Money
        //     : raiseAmount;

        User.UseMoney(raiseAmount);
        if (logAction)
            User.ActionLog.Add(new PlayerAction(ActionType.Raise, raiseAmount));
        GameManager.instance.game.LastRaiser = User;
        yield return null;
    }
}
