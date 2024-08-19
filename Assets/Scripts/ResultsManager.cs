using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Poker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsManager : MonoBehaviour
{
    #region Singleton

    public static ResultsManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    #endregion

    #region Members

    #region Serializable Fields

    [SerializeField] private GameObject resultsScreen;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform userHand;
    [SerializeField] private Transform communityCards;

    [SerializeField] private TMP_Text pot;
    [SerializeField] private TMP_Text handEquity;
    [SerializeField] private TMP_Text villainFoldEquity;
    [SerializeField] private TMP_Text villainRaise;
    [SerializeField] private TMP_Text potOdds;
    [SerializeField] private TMP_Text userAction;
    [SerializeField] private TMP_Text userEv;
    [SerializeField] private TMP_Text foldEv;
    [SerializeField] private TMP_Text callEv;
    [SerializeField] private TMP_Text reraiseEv;

    [SerializeField] private TMP_Text foldAmount;
    [SerializeField] private TMP_Text callAmount;
    [SerializeField] private TMP_Text reraiseAmount;

    #endregion

    #endregion

    private void Start()
    {
        resultsScreen.SetActive(false);
    }

    public void InitializeResults(Game game, GameSettings gameSettings)
    {
        foreach (Transform t in communityCards)
        {
            Destroy(t.gameObject);
        }

        foreach (Transform t in userHand)
        {
            Destroy(t.gameObject);
        }

        foreach (PlayerAction p in game.ActionLog)
        {
            Debug.Log(Enum.GetName(typeof(ActionType), p.ActionType) + ": " + p.Money);
        }

        // Initialize Cards
        List<Card> hand = game.User.Cards.Select(card => card.Clone()).ToList();
        foreach (Card c in hand)
        {
            GameManager.instance.CreateCard(c, cardPrefab, userHand, 1.6f);
        }

        List<Card> cc = game.CommunityCards.Select(card => card.Clone()).ToList();
        foreach (Card c in cc)
        {
            GameManager.instance.CreateCard(c, cardPrefab, communityCards, 1.6f);
        }

        int handEq = Utilities.HandEquity(gameSettings.scenario, game.CommunityCards.Count);
        int potOddsRatio1 = Utilities.PotOdds((int)game.Pot, (int)game.Players[0].LastAction().Money).first;
        int potOddsRatio2 = Utilities.PotOdds((int)game.Pot, (int)game.Players[0].LastAction().Money).second;
        int gcd = Utilities.GCD(potOddsRatio1, potOddsRatio2);
        potOddsRatio1 /= gcd;
        potOddsRatio2 /= gcd;
        int actionEV = (int)Math.Round(handEq / 100.0f * (game.Pot + 2 * game.User.LastAction().Money) - game.User.LastAction().Money);
        if (game.User.LastAction().Money == 0) actionEV = 0;
        Utilities.Statistics results = Utilities.Options(handEq, (int)game.Pot, (int)game.Players[0].LastAction().Money);

        pot.text = $"Pot: ${game.Pot}";
        handEquity.text = $"Hand Equity: {handEq}%";
        villainFoldEquity.text = $"Villain Fold Equity: 0";
        villainRaise.text = $"Villain Raise: ${game.Players[0].LastAction().Money}";
        potOdds.text = $"Pot Odds: {potOddsRatio1}:{potOddsRatio2}";
        if (game.User.LastAction().Money == 0)
        {
            userAction.text = "Hero Folds";
        }
        else if (game.User.LastAction().Money == game.Players[0].LastAction().Money)
        {
            userAction.text = $"Hero {Enum.GetName(typeof(ActionType), game.User.LastAction().ActionType)}: ${game.User.LastAction().Money}";
        }
        else
        {
            userAction.text = $"Hero Re{Enum.GetName(typeof(ActionType), game.User.LastAction().ActionType)}: ${game.User.LastAction().Money}";
        }
        userEv.text = $"{actionEV} EV";
        foldEv.text = $"0 EV";
        callEv.text = $"{results.callEv} EV";
        if (2 * (int)game.Players[0].LastAction().Money == results.reraiseAmount)
        {
            reraiseEv.text = results.reraiseEv + " EV";
            reraiseAmount.text = "($" + results.reraiseAmount + ")";
        }
        else
        {
            reraiseEv.text = (gameSettings.userStartingMoney < 2 * game.Players[0].LastAction().Money ? "N/A" : "0 -> " + results.reraiseEv + " EV");
            if (results.reraiseEv == 0)
            {
                reraiseEv.text = (gameSettings.userStartingMoney < 2 * game.Players[0].LastAction().Money ? "N/A" : "0" + " EV");
            }
            reraiseAmount.text = $"(${2 * (int)game.Players[0].LastAction().Money} -> ${results.reraiseAmount})";
        }
        foldAmount.text = $"({Math.Round(100 * (game.Pot / (double)(game.Pot + game.Players[0].LastAction().Money)))}% MDF)";
        callAmount.text = $"(${game.Players[0].LastAction().Money})";
        resultsScreen.SetActive(true);
    }

    public void Retry()
    {
        foreach (Transform t in communityCards)
        {
            Destroy(t.gameObject);
        }

        foreach (Transform t in userHand)
        {
            Destroy(t.gameObject);
        }
        GameManager.instance.ResetGame();
        GameManager.instance.Initialize(GameManager.instance.selectedGameSettings);
        resultsScreen.SetActive(false);
    }

    public void Continue()
    {
        foreach (Transform t in communityCards)
        {
            Destroy(t.gameObject);
        }

        foreach (Transform t in userHand)
        {
            Destroy(t.gameObject);
        }
        GameManager.instance.PreInitialization();
        resultsScreen.SetActive(false);
    }
}