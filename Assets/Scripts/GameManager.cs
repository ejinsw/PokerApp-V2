using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Poker;
using TMPro;
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

    private long userRaiseAmount = 0;

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

    [SerializeField] private Slider raiseSlider;
    [SerializeField] private TMP_Text raiseText;
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

        raiseSlider.onValueChanged.RemoveAllListeners();
        raiseSlider.onValueChanged.AddListener((value) => {
            userRaiseAmount = (long)value;
            raiseText.text = $"${userRaiseAmount}";
        });
        raiseText.text = $"${userRaiseAmount}";

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

    private void InitializeScenario(BluffCases.BluffCase scenario) {
        List<BluffCases.BluffCase> bluffCases = new();
        List<Card> deck = null;
        List<Card> communityCards = null;
        Player user = null;
        List<Player> players = new();

        int attempts = 0;
        for (attempts = 0; !(bluffCases.Contains(scenario) && bluffCases.Count == 1) && attempts < 1000; attempts++) {
            players.Clear();
            deck = Utilities.NewDeck();
            deck.Shuffle();

            communityCards = BluffCases.ScenarioCC(scenario, ref deck, gameSettings.communityCardSize);
            Utilities.ShowCards(ref communityCards);
            user = new("You", BluffCases.ScenarioP(scenario, ref deck, communityCards), gameSettings.userStartingMoney);

            players.Add(new(Utilities.RandomName(), Utilities.DeckTakeTwo(ref deck), gameSettings.playerStartingMoney));

            // Check bluff cases
            bluffCases = BluffCases.GetAllBluffCases(communityCards, user.Cards);
            
            if (bluffCases.Contains(scenario) && bluffCases.Count == 1) {
                attempts = 0;
                break;
            }
        }

        // Fallback
        if (attempts >= 1000) {
            Debug.LogWarning("Exceeded 1000 attempts to initialize. Using fallback...");
            players.Clear();
            deck = Utilities.NewDeck();
            deck.Shuffle();
            
            communityCards = new List<Card>(Utilities.DeckTakeCards(ref deck, gameSettings.customCommunityCards));
            Utilities.ShowCards(ref communityCards);
            user = new("You", Utilities.DeckTakeCards(ref deck, gameSettings.customUserHand), gameSettings.userStartingMoney);
            
            players.Add(new(Utilities.RandomName(), Utilities.DeckTakeTwo(ref deck), gameSettings.playerStartingMoney));
        }

        game = new(gameSettings.numberOfPlayers, gameSettings.startingPotSize, deck, communityCards, user, gameSettings.userPosition, players);
    }

    /// <summary>
    /// Initializes the game & components with a given player count.
    /// </summary>
    private void Initialize(GameSettings gameSettings) {
        Debug.Log("Initializing game...");

        #region Initialize Game

        if (gameSettings.scenario != BluffCases.BluffCase.None) {
            InitializeScenario(gameSettings.scenario);
        }
        else if (gameSettings.randomGame) {
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
                players = gameSettings.customPlayerList.Select(player => player.Clone()).ToList();

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
        
        List<BluffCases.BluffCase> bluffCases = BluffCases.GetAllBluffCases(game.CommunityCards, game.User.Cards);
        foreach (BluffCases.BluffCase b in bluffCases) {
            Debug.Log(Enum.GetName(typeof(BluffCases.BluffCase), b));
        }

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
            // TODO: Finish implementing logic for continuous games
            // TODO: DON'T DELETE THIS!
            // if (p.Folded) {
            //     p.ActionLog.Add(new PlayerAction(ActionType.Fold, 0));
            //     Debug.Log(p.Name + " " + Enum.GetName(typeof(ActionType), p.LastAction().ActionType) + ": " + p.LastAction().Money);
            //     continue;
            // } // skip folders
            //
            // if (game.LastRaiser == p) continue; // skip last raiser
            //
            // if (p.LastAction() != null
            //     && game.LastRaiser != null
            //     && p.LastAction().Money == game.LastRaiser.LastAction().Money) continue; // skip ppl who called already

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

                if (gameSettings.enableCustomPlayerActionLog) {
                    PlayerAction action = p.NextAction();

                    if (action == null) {
                        yield return StartCoroutine(playerComponents[p].DoTurn());
                    }
                    else {
                        yield return StartCoroutine(playerComponents[p].DoTurn(action));
                    }
                }
                else {
                    yield return StartCoroutine(playerComponents[p].DoTurn());
                }
            }

            playerComponents[p].UpdateUI();

            Debug.Log(p.Name + " " + Enum.GetName(typeof(ActionType), p.LastAction().ActionType) + ": " + p.LastAction().Money);
        }

        // TODO: Finish implementing logic for continuous games
        // TODO: DON'T DELETE THIS!
        // if (!Utilities.AllResponded(game.LastRaiser, game.Players))
        //     yield return GameStart();

        // game.LastRaiser = null;

        
        ResultsManager.instance.InitializeResults(game, gameSettings);
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
                yield return StartCoroutine(Raise(userRaiseAmount));
                break;
        }

        ActivateButtons(false);
    }

    #endregion

    #region User Actions

    private void ActivateButtons(bool activate) {
        long minRaise = 0;
        if (game.LastRaiser != null && game.LastRaiser.LastAction() != null) {
            minRaise = game.LastRaiser.LastAction().Money + 1;
        }

        raiseSlider.minValue = Math.Min(0, game.User.Money - minRaise);
        raiseSlider.maxValue = Math.Min(200, game.User.Money - minRaise);

        raiseSlider.gameObject.SetActive(false);
        raiseText.gameObject.SetActive(false);
        foldButton.gameObject.SetActive(false);
        checkButton.gameObject.SetActive(false);
        callButton.gameObject.SetActive(false);
        raiseButton.gameObject.SetActive(false);
        if (activate) {
            if (game.LastRaiser != null && game.LastRaiser != game.User) {
                // Fold, Call, Raise
                foldButton.gameObject.SetActive(true);
                callButton.gameObject.SetActive(true);
            }
            else {
                // Check, Raise
                checkButton.gameObject.SetActive(true);
            }

            raiseButton.gameObject.SetActive(true);
            raiseSlider.gameObject.SetActive(true);
            raiseText.gameObject.SetActive(true);
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

    public IEnumerator Call(long amount) {
        #region Null Check

        if (game.LastRaiser == null || game.LastRaiser.LastAction() == null) {
            Debug.LogError("LastRaiser or LastRaiser's LastAction is null in Call method.");
            yield break; // exit coroutine if there's an issue
        }

        #endregion

        long useAmount = game.User.LastAction() != null
            ? amount - game.User.LastAction().Money
            : amount;

        game.User.UseMoney(Math.Min(useAmount, game.User.Money));
        game.User.ActionLog.Add(new PlayerAction(ActionType.Call, Math.Min(useAmount, game.User.Money)));
        yield return null;
    }

    public IEnumerator Raise(long amount) {
        long useAmount = game.LastRaiser.LastAction() != null
            ? amount + game.LastRaiser.LastAction().Money
            : amount;
        game.User.UseMoney(useAmount);
        game.User.ActionLog.Add(new PlayerAction(ActionType.Raise, amount));
        game.LastRaiser = game.User;
        yield return null;
    }

    #endregion
}