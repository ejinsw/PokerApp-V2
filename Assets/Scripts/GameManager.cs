using System;
using System.Collections;
using System.Collections.Generic;
using Poker;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager instance;

    private void Awake() {
        if (instance == null) instance = this;
    }

    #endregion

    #region Members

    private List<Card> m_deck = new();

    private List<Card> m_communityCards = new();

    private List<Player> m_players = new();

    #region Serialize Fields

    [SerializeField] private int m_numPlayers = 2;

    #endregion
    
    #endregion

    #region Initialization

    private void Start() {
        Initialize();
    }

    #region Initialization

    private List<Card> ResetDeck() {
        List<Card> deck = new();
        foreach (Suit s in Enum.GetValues(typeof(Suit))) {
            foreach (Rank r in Enum.GetValues(typeof(Rank))) {
                deck.Add(new Card(s, r));
            }
        }

        return deck;
    }

    private List<Card> DeckTakeOne(ref List<Card> deck) {
        List<Card> cards = new();
        if (deck.Count == 0) return cards;

        cards.Add(deck[0]);
        deck.RemoveAt(0);
        
        return cards;
    }

    private List<Card> DeckTakeTwo(ref List<Card> deck) {
        List<Card> cards = new();
        cards.AddRange(DeckTakeOne(ref deck));
        cards.AddRange(DeckTakeOne(ref deck));
        return cards;
    }

    #endregion
    private void Initialize() {
        m_deck = ResetDeck();
        m_deck.Shuffle();

        m_players.Clear();
        for (int i = 0; i < m_numPlayers; i++) {
            m_players.Add(new Player(i.ToString(), DeckTakeTwo(ref m_deck)));
            m_players[i].PrintCards();
        }
    }

    #endregion
}
