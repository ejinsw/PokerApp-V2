using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Poker;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    #region Singleton

    public static GameManager instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Debug.LogError("Multiple instances of GameManager found!");
            Destroy(gameObject);
        }
    }

    #endregion

    #region Members

    [HideInInspector] public Game game;

    private Dictionary<Player, PlayerComponent> playerComponents = new();

    private bool userTurn = false;

    private ActionType userAction = ActionType.Null;


    #region Serialize Fields

    [SerializeField] private GameSettings gameSettings;

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

    [SerializeField] private Button foldButton;
    [SerializeField] private Button checkButton;
    [SerializeField] private Button callButton;
    [SerializeField] private Button raiseButton;

    #endregion

    #endregion

    #region Initialization

    private void Start() {
        ActivateButtons(false);

        // Button callbacks
        foldButton.onClick.RemoveAllListeners();
        foldButton.onClick.AddListener(() => UserAction(ActionType.Fold));

        checkButton.onClick.RemoveAllListeners();
        checkButton.onClick.AddListener(() => UserAction(ActionType.Check));

        callButton.onClick.RemoveAllListeners();
        callButton.onClick.AddListener(() => UserAction(ActionType.Call));

        raiseButton.onClick.RemoveAllListeners();
        raiseButton.onClick.AddListener(() => UserAction(ActionType.Raise));

        Initialize(gameSettings);
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
    private void Initialize(GameSettings gameSettings) {
        Debug.Log("Initializing game...");

        #region Initialize Game

        if (gameSettings.randomGame) {
            game = new(gameSettings.numberOfPlayers);
        }
        else {
            List<Card> deck = Utilities.NewDeck();
            deck.Shuffle();
            List<Card> communityCards = null;
            Player user = null;
            List<Player> players = null;

            // Custom Selections First (to prevent duplicates)
            if (gameSettings.enableCustomCommunityCards) {
                communityCards = new List<Card>(Utilities.DeckTakeCards(ref deck, gameSettings.customCommunityCards));
            }

            if (gameSettings.enableCustomUserHand) {
                user = new("You", Utilities.DeckTakeCards(ref deck, gameSettings.customUserHand), gameSettings.userStartingMoney);
            }

            if (gameSettings.enableCustomPlayerList) {
                players = new List<Player>(gameSettings.customPlayerList);
                foreach (Player p in players) {
                    Utilities.DeckTakeCards(ref deck, p.Cards);
                }
            }

            // Community cards
            if (!gameSettings.enableCustomCommunityCards) {
                communityCards = Utilities.DeckTakeAmount(ref deck, Utilities.RandomInt(3, 5));
            }

            // User
            if (!gameSettings.enableCustomUserHand) {
                if (gameSettings.randomUserHand) {
                    user = new("You", Utilities.DeckTakeTwo(ref deck), gameSettings.userStartingMoney);
                }
                else if (gameSettings.userHandPaired) {
                    user = new("You", Utilities.HandPaired(ref deck), gameSettings.userStartingMoney);
                    if (user.Cards == null) user.Cards = Utilities.DeckTakeTwo(ref deck);
                }
                else if (gameSettings.userHandSuited) {
                    user = new("You", Utilities.HandSuited(ref deck), gameSettings.userStartingMoney);
                    if (user.Cards == null) user.Cards = Utilities.DeckTakeTwo(ref deck);
                }
                else {
                    user = new("You", Utilities.DeckTakeTwo(ref deck), gameSettings.userStartingMoney);
                }
            }

            // Players
            if (!gameSettings.enableCustomPlayerList) {
                players = new();
                if (gameSettings.randomPlayerHand) {
                    for (int i = 0; i < gameSettings.numberOfPlayers; i++) {
                        players.Add(new(Utilities.RandomName(), Utilities.DeckTakeTwo(ref deck), gameSettings.playerStartingMoney));
                    }
                }
                else if (gameSettings.playerHandPaired) {
                    for (int i = 0; i < gameSettings.numberOfPlayers; i++) {
                        players.Add(new(Utilities.RandomName(), Utilities.HandPaired(ref deck), gameSettings.playerStartingMoney));
                    }
                }
                else if (gameSettings.playerHandSuited) {
                    for (int i = 0; i < gameSettings.numberOfPlayers; i++) {
                        players.Add(new(Utilities.RandomName(), Utilities.HandSuited(ref deck), gameSettings.playerStartingMoney));
                    }
                }
                else {
                    for (int i = 0; i < gameSettings.numberOfPlayers; i++) {
                        players.Add(new(Utilities.RandomName(), Utilities.DeckTakeTwo(ref deck), gameSettings.playerStartingMoney));
                    }
                }
            }

            game = new(gameSettings.numberOfPlayers, gameSettings.startingPotSize, deck, communityCards, user, gameSettings.userPosition, players);
        }

        #endregion

        #region Initialize Components

        // Community Card Components
        foreach (Card c in game.CommunityCards) {
            CreateCard(c, cardPrefab, communityCardsTransform, 1.4f);
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

        #endregion

        // Start
        StartCoroutine(GameStart());
    }

    #endregion

    #region Game Cycle

    private IEnumerator GameStart() {
        foreach (Player p in game.Players) {
            if (p.Folded) {
                p.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
                Debug.Log(p.Name + " " + Enum.GetName(typeof(ActionType), p.LastAction().ActionType) + ": " + p.LastAction().Money);
                continue;
            } // skip folders

            if (game.LastRaiser == p) continue; // skip last raiser

            if (p.LastAction() != null
                && game.LastRaiser != null
                && p.LastAction().Money == game.LastRaiser.LastAction().Money) continue; // skip ppl who called already

            if (p == game.User) {
                yield return StartCoroutine(UserTurn());
            }
            else {
                #region Null Check

                if (!playerComponents.ContainsKey(p) || playerComponents[p] == null) {
                    Debug.LogError($"PlayerComponent for player {p.Name} is null in GameStart method.");
                    continue;
                }

                #endregion

                yield return StartCoroutine(playerComponents[p].DoTurn());
            }

            playerComponents[p].UpdateUI();

            Debug.Log(p.Name + " " + Enum.GetName(typeof(ActionType), p.LastAction().ActionType) + ": " + p.LastAction().Money);
        }

        if (!Utilities.AllResponded(game.LastRaiser, game.Players))
            yield return GameStart();

        game.LastRaiser = null;

        yield return null;
    }

    private IEnumerator UserTurn() {
        Debug.Log("User's turn started.");
        userTurn = true;
        ActivateButtons(true);
        while (userTurn) {
            yield return null;
        }

        Debug.Log($"User action: {userAction}");

        switch (userAction) {
            case ActionType.Fold:
                yield return StartCoroutine(Fold());
                break;
            case ActionType.Check:
                yield return StartCoroutine(Check());
                break;
            case ActionType.Call:
                if (game.LastRaiser != null && game.LastRaiser.LastAction() != null) {
                    yield return StartCoroutine(Call(game.LastRaiser.LastAction().Money));
                }
                else {
                    Debug.LogError("LastRaiser or LastRaiser's LastAction is null in UserTurn.");
                }

                break;
            case ActionType.Raise:
                // TODO: Let player choose raise
                double raiseAmount = game.LastRaiser.LastAction() != null
                    ? Utilities.RandomDouble(game.LastRaiser.LastAction().Money + 10, game.LastRaiser.LastAction().Money + 100)
                    : Utilities.RandomDouble(10, 100);
                yield return StartCoroutine(Raise((float)raiseAmount));
                break;
        }

        ActivateButtons(false);
    }

    #endregion

    #region User Actions

    private void ActivateButtons(bool activate) {
        foldButton.gameObject.SetActive(false);
        checkButton.gameObject.SetActive(false);
        callButton.gameObject.SetActive(false);
        raiseButton.gameObject.SetActive(false);
        if (activate) {
            if (game.LastRaiser != null && game.LastRaiser != game.User) {
                // Fold, Call, Raise
                foldButton.gameObject.SetActive(true);
                callButton.gameObject.SetActive(true);
                raiseButton.gameObject.SetActive(true);
            }
            else {
                // Check, Raise
                checkButton.gameObject.SetActive(true);
                raiseButton.gameObject.SetActive(true);
            }
        }
    }

    public void UserAction(ActionType action) {
        userAction = action;
        userTurn = false;
    }

    public IEnumerator Fold() {
        game.User.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
        game.User.Folded = true;
        yield return null;
    }

    public IEnumerator Check() {
        game.User.ActionLog.Add(new PlayerAction(ActionType.Check, 0));
        yield return null;
    }

    public IEnumerator Call(float amount) {
        #region Null Check

        if (game.LastRaiser == null || game.LastRaiser.LastAction() == null) {
            Debug.LogError("LastRaiser or LastRaiser's LastAction is null in Call method.");
            yield break; // exit coroutine if there's an issue
        }

        #endregion

        float useAmount = game.User.LastAction() != null
            ? amount - game.User.LastAction().Money
            : amount;

        #region Null Check

        if (game.User == null) {
            Debug.LogError("game.User is null in Call method.");
            yield break;
        }

        #endregion

        game.User.UseMoney(useAmount);
        game.User.ActionLog.Add(new PlayerAction(ActionType.Call, game.LastRaiser.LastAction().Money));
        yield return null;
    }

    public IEnumerator Raise(float amount) {
        float useAmount = game.User.LastAction() != null
            ? amount - game.User.LastAction().Money
            : amount;
        game.User.UseMoney(useAmount);
        game.User.ActionLog.Add(new PlayerAction(ActionType.Raise, amount));
        game.LastRaiser = game.User;
        yield return null;
    }

    #endregion
}