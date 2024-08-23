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
    [SerializeField] TMP_Text money;
    [SerializeField] CardComponent card1;
    [SerializeField] CardComponent card2;
    [SerializeField] RaiseComponent raiseComponent;

    public void Initialize(Player user)
    {
        User = user;
        // money.text = "$" + User.Money;
        if (User.Cards.Count > 0 && User.Cards[0] != null)
            card1.Initialize(User.Cards[0]);
        if (User.Cards.Count > 1 && User.Cards[1] != null)
            card2.Initialize(User.Cards[1]);

        raiseComponent.Initialize();
    }

    public void UpdateUI()
    {
        // money.text = "$" + User.Money;
    }
    
    public IEnumerator UserTurn()
    {
        Debug.Log("User's turn started.");
        GameManager.instance.userTurn = true;
        GameManager.instance.DisableUserButtons(false);
        GameManager.instance.ActivateButtons(true);
        yield return new WaitUntil(() => !GameManager.instance.userTurn);

        Debug.Log($"User action: {GameManager.instance.userAction}");

        switch (GameManager.instance.userAction)
        {
        case ActionType.Fold:
            yield return StartCoroutine(Fold());
            break;
        case ActionType.Check:
            yield return StartCoroutine(Check());
            break;
        case ActionType.Call:
            if (GameManager.instance.game.LastRaiser != null && GameManager.instance.game.LastRaiser.LastAction() != null)
            {
                yield return StartCoroutine(Call(GameManager.instance.game.LastRaiser.LastAction().Money));
            }
            else
            {
                Debug.LogError("LastRaiser or LastRaiser's LastAction is null in UserTurn.");
            }

            break;
        case ActionType.Raise:
            yield return StartCoroutine(Raise(GameManager.instance.userRaiseAmount));
            break;
        }

        if (GameManager.instance.game.User.LastAction() != null)
            GameManager.instance.game.ActionLog.Add(GameManager.instance.game.User.LastAction());
    }

    public IEnumerator Fold()
    {
        GameManager.instance.game.User.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
        GameManager.instance.game.User.Folded = true;
        yield return null;
    }

    public IEnumerator Check()
    {
        GameManager.instance.game.User.ActionLog.Add(new PlayerAction(ActionType.Check, 0));
        yield return null;
    }

    public IEnumerator Call(long amount)
    {
        #region Null Check

        if (GameManager.instance.game.LastRaiser == null || GameManager.instance.game.LastRaiser.LastAction() == null)
        {
            Debug.LogError("LastRaiser or LastRaiser's LastAction is null in Call method.");
            yield break; // exit coroutine if there's an issue
        }

        #endregion

        long useAmount = Math.Min(amount, GameManager.instance.game.User.Money);

        GameManager.instance.game.User.UseMoney(useAmount);
        GameManager.instance.game.User.ActionLog.Add(new PlayerAction(ActionType.Call, useAmount));
        yield return null;
    }

    public IEnumerator Raise(long amount)
    {
        if (amount == GameManager.instance.game.LastRaiser.LastAction().Money)
        {
            yield return StartCoroutine(Call(amount));
            yield break;
        }
        GameManager.instance.game.User.UseMoney(amount);
        GameManager.instance.game.User.ActionLog.Add(new PlayerAction(ActionType.Raise, amount));
        GameManager.instance.game.LastRaiser = GameManager.instance.game.User;
        yield return null;
    }
}
