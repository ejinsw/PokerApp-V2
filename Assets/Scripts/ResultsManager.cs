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

    [SerializeField] Popup popup;

    [Header("Tip Buttons")]
    [SerializeField]
    Button handEqTip;
    [SerializeField] Button villianFoldEqTip;
    [SerializeField] Button potOddsTip;
    [SerializeField] Button evTip;
    [SerializeField] Button mdfTip;

    [SerializeField] TextGroup foldTextGroup;
    [SerializeField] TextGroup callTextGroup;
    [SerializeField] TextGroup raiseTextGroup;

    #endregion

    #endregion

    private void Start()
    {
        resultsScreen.SetActive(false);

        handEqTip.onClick.AddListener(() => StartCoroutine(
            popup.Activate("Probability of improving to a winning hand.")));
        villianFoldEqTip.onClick.AddListener(() => StartCoroutine(
            popup.Activate("Assuming the villain will not fold.")));
        potOddsTip.onClick.AddListener(() => StartCoroutine(
            popup.Activate("The ratio X : Y, where X is the pot size and Y is the villain’s bet, represents the risk (Y) and reward (X) of calling. Your hand equity must exceed X/(X+Y) for a profitable call.")));
        evTip.onClick.AddListener(() => StartCoroutine(
            popup.Activate("Expected value is calculated by Win probability * Win amount - Lose probability * Lose amount. This figure represents what you expect to win/lose from taking this action.")));
        mdfTip.onClick.AddListener(() => StartCoroutine(
            popup.Activate("Max fold frequency without giving opponent profitable bluff opportunities.")));
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
        potOdds.text = $"{potOddsRatio1}:{potOddsRatio2} PO";
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
        int maxEv = Math.Max(0, Math.Max(results.callEv, results.reraiseEv));
        int minEv = Math.Min(0, Math.Min(results.callEv, results.reraiseEv));

        int[] colorCutoffs = { minEv, minEv + (maxEv - minEv / 2), maxEv };

        if (actionEV < colorCutoffs[0] + 10)
        {
            userAction.color = Color.red;
            userEv.color = Color.red;
        }
        else if (colorCutoffs[2] - 10 <= actionEV)
        {
            userAction.color = new Color(111f / 255f, 229f / 255f, 111f / 255f);
            userEv.color = new Color(111f / 255f, 229f / 255f, 111f / 255f);
        }
        else
        {
            userAction.color = new Color(255f / 255f, 200f / 255f, 0f / 255f);
            userEv.color = new Color(255f / 255f, 200f / 255f, 0f / 255f);
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
            reraiseAmount.text = $"(${results.reraiseAmount})";
        }
        foldAmount.text = $"({Math.Round(100 * (game.Pot / (double)(game.Pot + game.Players[0].LastAction().Money)))}% MDF)";
        callAmount.text = $"(${game.Players[0].LastAction().Money})";

        if (0 > results.callEv && 0 > results.reraiseEv)
        {
            foldTextGroup.SetColor(new Color(111f / 255f, 229f / 255f, 111f / 255f));
            callTextGroup.SetColor(Color.white);
            raiseTextGroup.SetColor(Color.white);
        }
        else if (results.callEv > 0 && results.callEv > results.reraiseEv)
        {
            callTextGroup.SetColor(new Color(111f / 255f, 229f / 255f, 111f / 255f));
            foldTextGroup.SetColor(Color.white);
            raiseTextGroup.SetColor(Color.white);
        }
        else if (results.reraiseEv > 0 && results.reraiseEv > results.callEv)
        {
            raiseTextGroup.SetColor(new Color(111f / 255f, 229f / 255f, 111f / 255f));
            callTextGroup.SetColor(Color.white);
            foldTextGroup.SetColor(Color.white);
        }

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
