using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Poker;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of GameManager found!");
            Destroy(gameObject);
        }
    }

    #endregion

    #region Members

    [HideInInspector] public Game game;

    private Dictionary<Player, PlayerComponent> playerComponents = new();

    public bool userTurn = false;

    public ActionType userAction = ActionType.Null;

    public long userRaiseAmount = 0;
    [HideInInspector] public GameSettings selectedGameSettings;

    #region Serialize Fields

    [SerializeField] public List<GameSettings> gameSettings;

    [SerializeField] public Sprite IconClubs;
    [SerializeField] public Sprite IconDiamonds;
    [SerializeField] public Sprite IconHearts;
    [SerializeField] public Sprite IconSpades;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<Transform> playersTransforms;
    [SerializeField] UserComponent userComponent;

    [SerializeField] public GameObject cardPrefab;
    [SerializeField] private GameObject cardHorizontalPrefab;
    [SerializeField] private Transform communityCardsTransform;

    [SerializeField] private Button foldButton;
    [SerializeField] private Button checkButton;
    [SerializeField] private Button callButton;

    [SerializeField] private Slider raiseSlider;
    [SerializeField] private TMP_Text raiseText;
    [SerializeField] private Button raiseButton;

    [SerializeField] private TMP_Text potText;

    [SerializeField] TMP_Text hintText;
    [SerializeField] GameObject hintObject;

    #endregion

    #endregion

    public const int STEP = 5;
    public void OnRaiseSliderChanged(float value)
    {
        userRaiseAmount = (long)value * STEP;
        raiseText.text = $"${userRaiseAmount}";
    }

    #region Initialization

    private void Start()
    {
        ActivateButtons(false);

        // Button callbacks
        foldButton.onClick.RemoveAllListeners();
        foldButton.onClick.AddListener(() => UserAction(ActionType.Fold));

        checkButton.onClick.RemoveAllListeners();
        checkButton.onClick.AddListener(() => UserAction(ActionType.Check));

        callButton.onClick.RemoveAllListeners();
        callButton.onClick.AddListener(() => UserAction(ActionType.Call));

        raiseSlider.onValueChanged.RemoveAllListeners();
        raiseSlider.onValueChanged.AddListener(OnRaiseSliderChanged);
        raiseText.text = $"${userRaiseAmount}";

        raiseButton.onClick.RemoveAllListeners();
        raiseButton.onClick.AddListener(() => UserAction(ActionType.Raise));

        PreInitialization();
    }

    #region Utility Methods

    /// <summary>
    /// Initializes a corresponding CardComponent for a Card instance.
    /// </summary>
    public void CreateCard(Card card, GameObject prefab, Transform hand, float scale = 1)
    {
        GameObject cardObject = Instantiate(prefab, hand);
        CardComponent component = cardObject.GetComponent<CardComponent>();
        component.Initialize(card);
        component.SetScale(scale);
    }

    /// <summary>
    /// Initializes a corresponding PlayerComponent for a Player instance.
    /// </summary>
    private PlayerComponent CreatePlayer(Player p, Transform parent, float scale = 1)
    {
        GameObject playerObject = Instantiate(playerPrefab, parent);
        PlayerComponent playerComponent = playerObject.GetComponent<PlayerComponent>();
        playerComponent.Initialize(p, scale);

        return playerComponent;
    }

    #endregion

    public void ResetGame()
    {
        foreach (Transform t in communityCardsTransform)
        {
            Destroy(t.gameObject);
        }
        
        foreach (PlayerComponent p in playerComponents.Values)
        {
            Destroy(p.gameObject);
        }

        playerComponents.Clear();
    }

    public void PreInitialization()
    {
        ResetGame();
        
        selectedGameSettings = gameSettings[Utilities.RandomInt(0, gameSettings.Count - 1)];
        Debug.Log($"Initializing {selectedGameSettings.settingsName}");
        Initialize(selectedGameSettings);
    }

    private void InitializeScenario(BluffCases.BluffCase scenario)
    {
        List<BluffCases.BluffCase> bluffCases = new();
        List<Card> deck = null;
        List<Card> communityCards = null;
        Player user = null;
        List<Player> players = new();

        int attempts = 0;
        for (attempts = 0; !(bluffCases.Contains(scenario) && bluffCases.Count == 1) && attempts < 100; attempts++)
        {
            players.Clear();
            deck = Utilities.NewDeck();
            deck.Shuffle();

            communityCards = BluffCases.ScenarioCC(scenario, ref deck, selectedGameSettings.communityCardSize);
            Utilities.ShowCards(ref communityCards);
            user = new("You", BluffCases.ScenarioP(scenario, ref deck, communityCards), selectedGameSettings.userStartingMoney);

            if (selectedGameSettings.enableCustomPlayerList)
            {
                players = selectedGameSettings.customPlayerList.Select(player => player.Clone()).ToList();

                foreach (Player p in players)
                {
                    if (!p.Cards.Any())
                    {
                        p.Cards = Utilities.DeckTakeTwo(ref deck);
                    }
                    else if (p.Cards.Count == 1)
                    {
                        p.Cards.AddRange(Utilities.DeckTakeOne(ref deck));
                    }
                    else
                    {
                        Utilities.DeckTakeCards(ref deck, p.Cards);
                    }
                }
            }
            else
            {
                for (int i = 0; i < selectedGameSettings.numberOfPlayers; i++)
                {
                    players.Add(new(Utilities.RandomName(), Utilities.DeckTakeTwo(ref deck), selectedGameSettings.playerStartingMoney));
                }
            }

            // Check bluff cases
            bluffCases = BluffCases.GetAllBluffCases(communityCards, user.Cards);

            if (bluffCases.Contains(scenario) && bluffCases.Count == 1)
            {
                attempts = 0;
                break;
            }
        }

        // Fallback
        if (attempts >= 100)
        {
            Debug.LogWarning("Exceeded 100 attempts to initialize. Using fallback...");
            players.Clear();
            deck = Utilities.NewDeck();
            deck.Shuffle();

            communityCards = new List<Card>(Utilities.DeckTakeCards(ref deck, selectedGameSettings.customCommunityCards));
            Utilities.ShowCards(ref communityCards);
            user = new("You", Utilities.DeckTakeCards(ref deck, selectedGameSettings.customUserHand), selectedGameSettings.userStartingMoney);

            if (selectedGameSettings.enableCustomPlayerList)
            {
                players = selectedGameSettings.customPlayerList.Select(player => player.Clone()).ToList();

                foreach (Player p in players)
                {
                    if (!p.Cards.Any())
                    {
                        p.Cards = Utilities.DeckTakeTwo(ref deck);
                    }
                    else if (p.Cards.Count == 1)
                    {
                        p.Cards.AddRange(Utilities.DeckTakeOne(ref deck));
                    }
                    else
                    {
                        Utilities.DeckTakeCards(ref deck, p.Cards);
                    }
                }
            }
            else
            {
                for (int i = 0; i < selectedGameSettings.numberOfPlayers; i++)
                {
                    players.Add(new(Utilities.RandomName(), Utilities.DeckTakeTwo(ref deck), selectedGameSettings.playerStartingMoney));
                }
            }
        }

        game = new(selectedGameSettings.numberOfPlayers, selectedGameSettings.startingPotSize, deck, communityCards, user, selectedGameSettings.userPosition, players);
    }

    /// <summary>
    /// Initializes the game & components with a given player count.
    /// </summary>
    public void Initialize(GameSettings gameSettings)
    {
        Debug.Log("Initializing game...");

        #region Initialize Game

        if (gameSettings.scenario != BluffCases.BluffCase.None)
        {
            InitializeScenario(gameSettings.scenario);
        }
        else if (gameSettings.randomGame)
        {
            game = new(gameSettings.numberOfPlayers);
        }
        else
        {
            List<Card> deck = Utilities.NewDeck();
            deck.Shuffle();
            List<Card> communityCards = null;
            Player user = null;
            List<Player> players = null;

            // Custom Selections First (to prevent duplicates)
            if (gameSettings.enableCustomCommunityCards)
            {
                communityCards = new List<Card>(Utilities.DeckTakeCards(ref deck, gameSettings.customCommunityCards.Select(card => card.Clone()).ToList()));
            }

            if (gameSettings.enableCustomUserHand)
            {
                user = new("You", Utilities.DeckTakeCards(ref deck, gameSettings.customUserHand.Select(card => card.Clone()).ToList()), gameSettings.userStartingMoney);
            }

            if (gameSettings.enableCustomPlayerList)
            {
                players = gameSettings.customPlayerList.Select(player => player.Clone()).ToList();

                foreach (Player p in players)
                {
                    if (!p.Cards.Any())
                    {
                        p.Cards = Utilities.DeckTakeTwo(ref deck);
                    }
                    else if (p.Cards.Count == 1)
                    {
                        p.Cards.AddRange(Utilities.DeckTakeOne(ref deck));
                    }
                    else
                    {
                        Utilities.DeckTakeCards(ref deck, p.Cards);
                    }
                }
            }

            // Community cards
            if (!gameSettings.enableCustomCommunityCards)
            {
                communityCards = Utilities.DeckTakeAmount(ref deck, Utilities.RandomInt(3, 5));
            }

            // User
            if (!gameSettings.enableCustomUserHand)
            {
                if (gameSettings.randomUserHand)
                {
                    user = new("You", Utilities.DeckTakeTwo(ref deck), gameSettings.userStartingMoney);
                }
                else if (gameSettings.userHandPaired)
                {
                    user = new("You", Utilities.HandPaired(ref deck), gameSettings.userStartingMoney);
                    if (user.Cards == null) user.Cards = Utilities.DeckTakeTwo(ref deck);
                }
                else if (gameSettings.userHandSuited)
                {
                    user = new("You", Utilities.HandSuited(ref deck), gameSettings.userStartingMoney);
                    if (user.Cards == null) user.Cards = Utilities.DeckTakeTwo(ref deck);
                }
                else
                {
                    user = new("You", Utilities.DeckTakeTwo(ref deck), gameSettings.userStartingMoney);
                }
            }

            // Players
            if (!gameSettings.enableCustomPlayerList)
            {
                players = new();
                if (gameSettings.randomPlayerHand)
                {
                    for (int i = 0; i < gameSettings.numberOfPlayers; i++)
                    {
                        players.Add(new(Utilities.RandomName(), Utilities.DeckTakeTwo(ref deck), gameSettings.playerStartingMoney));
                    }
                }
                else if (gameSettings.playerHandPaired)
                {
                    for (int i = 0; i < gameSettings.numberOfPlayers; i++)
                    {
                        players.Add(new(Utilities.RandomName(), Utilities.HandPaired(ref deck), gameSettings.playerStartingMoney));
                    }
                }
                else if (gameSettings.playerHandSuited)
                {
                    for (int i = 0; i < gameSettings.numberOfPlayers; i++)
                    {
                        players.Add(new(Utilities.RandomName(), Utilities.HandSuited(ref deck), gameSettings.playerStartingMoney));
                    }
                }
                else
                {
                    for (int i = 0; i < gameSettings.numberOfPlayers; i++)
                    {
                        players.Add(new(Utilities.RandomName(), Utilities.DeckTakeTwo(ref deck), gameSettings.playerStartingMoney));
                    }
                }
            }

            game = new(gameSettings.numberOfPlayers, gameSettings.startingPotSize, deck, communityCards, user, gameSettings.userPosition, players);
        }

        #endregion

        List<BluffCases.BluffCase> bluffCases = BluffCases.GetAllBluffCases(game.CommunityCards, game.User.Cards);
        foreach (BluffCases.BluffCase b in bluffCases)
        {
            Debug.Log(Enum.GetName(typeof(BluffCases.BluffCase), b));
        }

        #region Initialize Components

        // Community Card Components
        foreach (Card c in game.CommunityCards)
        {
            CreateCard(c, cardPrefab, communityCardsTransform, 2f);
        }

        // Player Components
        int index = 0;
        foreach (Player p in game.Players)
        {
            if (p == game.User)
            {
                userComponent.Initialize(game.User);
            }
            else
            {
                playerComponents.Add(p, CreatePlayer(p, playersTransforms[index]));
                index++;
            }
        }
        
        ToggleHint(false);
        SetHint();

        #endregion

        // Start
        StartCoroutine(GameStart());
    }

    #endregion

    #region Game Cycle

    private IEnumerator GameStart()
    {
        foreach (Player p in game.Players)
        {
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

            if (p == game.User)
            {
                yield return StartCoroutine(userComponent.UserTurn());
                
                userComponent.UpdateUI();
            }
            else
            {
                #region Null Check

                if (!playerComponents.ContainsKey(p) || playerComponents[p] == null)
                {
                    Debug.LogError($"PlayerComponent for player {p.Name} is null in GameStart method.");
                    continue;
                }

                #endregion

                if (selectedGameSettings.enableCustomPlayerActionLog)
                {
                    PlayerAction action = p.NextAction();


                    if (action == null)
                    {
                        yield return StartCoroutine(playerComponents[p].DoTurn());
                    }
                    else
                    {
                        if (action.Money == -1)
                        {
                            action.Money = Utilities.RandomInt(5, (int)game.Pot, STEP);
                        }
                        yield return StartCoroutine(playerComponents[p].DoTurn(action));
                    }
                }
                else
                {
                    yield return StartCoroutine(playerComponents[p].DoTurn());
                }
                playerComponents[p].UpdateUI();
            }

            potText.text = $"${game.GetPot()}";
            
            SetHint();

            Debug.Log(p.Name + " " + Enum.GetName(typeof(ActionType), p.LastAction().ActionType) + ": " + p.LastAction().Money);
        }

        // TODO: Finish implementing logic for continuous games
        // TODO: DON'T DELETE THIS!
        // if (!Utilities.AllResponded(game.LastRaiser, game.Players))
        //     yield return GameStart();

        // game.LastRaiser = null;

        yield return new WaitForSeconds(1);

        ResultsManager.instance.InitializeResults(game, selectedGameSettings);
        yield return null;
    }

    

    #endregion

    #region User Actions

    public void ActivateButtons(bool activate)
    {
        long minRaise = 0;
        if (game.LastRaiser != null && game.LastRaiser.LastAction() != null)
        {
            minRaise = game.LastRaiser.LastAction().Money * 2 / STEP;
        }

        raiseSlider.minValue = minRaise;
        raiseSlider.maxValue = game.User.Money / STEP;

        // raiseSlider.gameObject.SetActive(false);
        // raiseText.gameObject.SetActive(false);
        foldButton.gameObject.SetActive(false);
        checkButton.gameObject.SetActive(false);
        callButton.gameObject.SetActive(false);
        // raiseButton.gameObject.SetActive(false);
        if (activate)
        {
            if (game.LastRaiser != null && game.LastRaiser != game.User)
            {
                // Fold, Call, Raise
                foldButton.gameObject.SetActive(true);

                if (game.LastRaiser.LastAction().Money >= game.User.Money)
                {
                    callButton.GetComponentInChildren<TMP_Text>().text = "All In";
                }
                else
                {
                    callButton.GetComponentInChildren<TMP_Text>().text = $"Call ${game.LastRaiser.LastAction().Money}";

                    if (game.LastRaiser.LastAction().Money * 2 <= game.User.Money)
                    {
                        raiseButton.gameObject.SetActive(true);
                        raiseSlider.gameObject.SetActive(true);
                        // raiseText.gameObject.SetActive(true);
                    }
                }

                callButton.gameObject.SetActive(true);
            }
            else
            {
                // Check, Raise
                checkButton.gameObject.SetActive(true);
                raiseButton.gameObject.SetActive(true);
                raiseSlider.gameObject.SetActive(true);
                // raiseText.gameObject.SetActive(true);
            }
        }
    }

    public void ToggleHint(bool toggle = true)
    {
        hintObject.SetActive(toggle ? !hintObject.activeInHierarchy : false);
    }
    
    public void SetHint()
    {
        int handEq = Utilities.HandEquity(selectedGameSettings.scenario, game.CommunityCards.Count);
        int raise = game.LastRaiser != null && game.LastRaiser.LastAction() != null ? (int)game.LastRaiser.LastAction().Money : 0;
        Utilities.Odds potOdds = Utilities.PotOdds((int)game.Pot, raise);
        
        int gcd = Utilities.GCD(potOdds.first, potOdds.second);
        potOdds.first /= gcd != 0 ? gcd : 1;
        potOdds.second /= gcd != 0 ? gcd : 1;
        
        string text = $"<b>Straight Draw</b>\n\nHand Equity:\t<b>{handEq}%</b>\nPot Odds:\t\t<b>{potOdds.first}:{potOdds.second}</b>\n";
        hintText.text = text;
    }
    
    public void UserAction(ActionType action)
    {
        userAction = action;
        userTurn = false;
    }

    #endregion
}
