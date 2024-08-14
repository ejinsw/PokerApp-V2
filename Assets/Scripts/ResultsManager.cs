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
        Utilities.Statistics results = Utilities.Options(handEq, (int)game.Pot, (int)game.Players[0].LastAction().Money);

        pot.text = $"Pot: ${game.Pot}";
        handEquity.text = $"Hand Equity: {handEq}%";
        villainFoldEquity.text = $"Villain Fold Equity: 0";
        villainRaise.text = $"Villain Raise: ${game.Players[0].LastAction().Money}";
        potOdds.text = $"Pot Odds: {potOddsRatio1}:{potOddsRatio2}";
        userAction.text = $"You Chose: {Enum.GetName(typeof(ActionType), game.User.LastAction().ActionType)} ${game.User.LastAction().Money}";
        userEv.text = $"User EV: ${actionEV}";
        foldEv.text = $"$0";
        callEv.text = $"${results.callEv}";
        reraiseEv.text = "$" + (gameSettings.userStartingMoney < 2 * game.Players[0].LastAction().Money ? "N/A" : results.reraiseEv);

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
        GameManager.instance.ResetGame();
        GameManager.instance.PreInitialization();
        resultsScreen.SetActive(false);
    }
}