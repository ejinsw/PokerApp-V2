using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Dictionary<Player, PlayerComponent> m_playerComponents = new();

    private Player User;

    #region Serialize Fields

    [SerializeField] public Sprite IconClubs;
    [SerializeField] public Sprite IconDiamonds;
    [SerializeField] public Sprite IconHearts;
    [SerializeField] public Sprite IconSpades;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<Transform> playersTransforms;
    [SerializeField] private Transform userTransform;
    
    [SerializeField] public GameObject cardPrefab;
    [SerializeField] private GameObject cardHorizontalPrefab;
    [SerializeField] private Transform communityCardsTransform;

    [SerializeField] private int m_numPlayers = 3;

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

    public void ShowCards(ref List<Card> cards) {
        foreach (Card c in cards) {
            c.Visible = true;
        }
    }

    public void CreateCard(Card card, GameObject prefab, Transform hand, float scale = 1) {
        GameObject cardObject = Instantiate(prefab, hand);
        CardComponent component = cardObject.GetComponent<CardComponent>();
        component.Initialize(card);
        component.SetScale(scale);
    }

    /// <summary>
    /// Initializes a corresponding PlayerComponent for a Player instance.
    /// </summary>
    private PlayerComponent CreatePlayer(Player p, Transform parent, float scale = 1) {
        GameObject playerObject = Instantiate(playerPrefab, parent);
        PlayerComponent playerComponent = playerObject.GetComponent<PlayerComponent>();
        playerComponent.Initialize(p, scale);

        return playerComponent;
    }

    #endregion

    private void Initialize() {
        // Deck
        m_deck = ResetDeck();
        m_deck.Shuffle();

        // Community Cards
        m_communityCards.Clear();
        m_communityCards = DeckTakeAmount(ref m_deck, Utilities.RandomInt(3, 5));
        ShowCards(ref m_communityCards);

        foreach (Card c in m_communityCards) {
            CreateCard(c,cardHorizontalPrefab, communityCardsTransform, 2);
        }

        m_players.Clear();
        // User
        User = new("You", DeckTakeTwo(ref m_deck), Utilities.RandomDouble(0, 1000));
        List<Card> userCards = User.Cards;
        ShowCards(ref userCards);
        m_players.Add(User);

        // Other Players
        for (int i = 0; i < m_numPlayers; i++) {
            string name = Utilities.RandomName();
            while (m_players.Any(p => p.Name == name)) {
                name = Utilities.RandomName();
            }

            Player p = new Player(name, DeckTakeTwo(ref m_deck), Utilities.RandomDouble(0, 1000));
            m_players.Add(p);
        }

        m_players.Shuffle();

        int index = 0;
        // Player Components
        foreach (Player p in m_players) {
            if (p == User) {
                m_playerComponents.Add(p, CreatePlayer(p, userTransform, 1.4f));
            }
            else {
                m_playerComponents.Add(p, CreatePlayer(p, playersTransforms[index]));
                index++;
            }
        }

        // Start
        StartCoroutine(GameStart());
    }

    #endregion

    #region Game Cycle

    private IEnumerator GameStart() {
        foreach (Player p in m_players) {
            if (p.Folded) continue;
            yield return m_playerComponents[p].DoTurn();
        }

        yield return null;
    }

    #endregion
}