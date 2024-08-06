using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Poker;
using UnityEngine;

public class GameManager : MonoBehaviour {
    #region Singleton

    public static GameManager instance;

    private void Awake() {
        if (instance == null) instance = this;
    }

    #endregion

    #region Members

    [HideInInspector] public Game game;
    
    private Dictionary<Player, PlayerComponent> playerComponents = new();

    private bool userTurn = false;
    
    
    
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

    [SerializeField] private int numPlayers = 3;

    #endregion

    #endregion

    #region Initialization

    private void Start() {
        Initialize(numPlayers);
    }

    #region Utility Methods

    /// <summary>
    /// Initializes a corresponding CardComponent for a Card instance.
    /// </summary>
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

    /// <summary>
    /// Initializes the game & components with a given player count.
    /// </summary>
    private void Initialize(int numPlayers) {
        // Initialize game
        game = new(numPlayers);
        
        // Community Card Components
        foreach (Card c in game.CommunityCards) {
            CreateCard(c, cardHorizontalPrefab, communityCardsTransform, 1.4f);
        }

        // Player Components
        int index = 0;
        foreach (Player p in game.Players) {
            if (p == game.User) {
                playerComponents.Add(p, CreatePlayer(p, userTransform, 1.4f));
            }
            else {
                playerComponents.Add(p, CreatePlayer(p, playersTransforms[index]));
                index++;
            }
        }

        // Start
        StartCoroutine(GameStart());
    }

    #endregion

    #region Game Cycle

    private IEnumerator GameStart() {
        foreach (Player p in game.Players) {
            if (p.Folded) continue;

            if (p == game.User) {
                userTurn = true;
                while (userTurn) {
                    yield return null;
                }
            }
            else {
                yield return playerComponents[p].DoTurn();
            }
        }

        yield return null;
    }

    #endregion

    #region User Actions

    public void Fold() {
        game.User.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
        game.User.Folded = true;
        userTurn = false;
    }
    
    public void Check() {
        game.User.ActionLog.Add(new PlayerAction(ActionType.Check, 0));
        userTurn = false;
    }

    public void Call() {
        game.User.ActionLog.Add(new PlayerAction(ActionType.Call, 0));
        userTurn = false;
    }

    public void Raise() {
        game.User.ActionLog.Add(new PlayerAction(ActionType.Raise, 0));
        userTurn = false;
    }

    #endregion
}