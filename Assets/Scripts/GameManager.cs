using System;
using System.Collections;
using System.Collections.Generic;
using Poker;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
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

    [SerializeField] public Sprite IconClubs;
    [SerializeField] public Sprite IconDiamonds;
    [SerializeField] public Sprite IconHearts;
    [SerializeField] public Sprite IconSpades;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playersTransform;

    [SerializeField] private int m_numPlayers = 2;

    #endregion

    #endregion

    #region Initialization

    private void Start() {
        Initialize();
    }

    #region Utility Methods

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

    private List<Card> DeckTakeAmount(ref List<Card> deck, int amount) {
        List<Card> cards = new();
        for (int i = 0; i < amount; i++) {
            cards.AddRange(DeckTakeOne(ref deck));
        }

        return cards;
    }

    /// <summary>
    /// Creates a new Player instance, adds it to the player list,
    /// and initializes the corresponding PlayerComponent.
    /// </summary>
    private void CreatePlayer() {
        Player p = new Player(Utilities.RandomName(), DeckTakeTwo(ref m_deck), Utilities.RandomDouble(0, 1000));
        m_players.Add(p);
        GameObject playerObject = Instantiate(playerPrefab, playersTransform);
        PlayerComponent playerComponent = playerObject.GetComponent<PlayerComponent>();
        playerComponent.Initialize(p);
    }

    #endregion

    private void Initialize() {
        m_deck = ResetDeck();
        m_deck.Shuffle();

        m_communityCards.Clear();
        m_communityCards = DeckTakeAmount(ref m_deck, Utilities.RandomInt(3, 5));
        Utilities.PrintCards(m_communityCards);

        m_players.Clear();
        for (int i = 0; i < m_numPlayers; i++) {
            CreatePlayer();
        }
    }

    #endregion
}