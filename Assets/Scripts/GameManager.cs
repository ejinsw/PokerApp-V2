using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Poker;
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

    [HideInInspector] public Game game;

    private Dictionary<Player, PlayerComponent> playerComponents = new();

    private bool userTurn = false;

    private ActionType userAction = ActionType.Null;


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
        foldButton.onClick.AddListener((() => UserAction(ActionType.Fold)));
        checkButton.onClick.AddListener((() => UserAction(ActionType.Check)));
        callButton.onClick.AddListener((() => UserAction(ActionType.Call)));
        raiseButton.onClick.AddListener((() => UserAction(ActionType.Raise)));

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
                yield return StartCoroutine(UserTurn());
            }
            else {
                yield return StartCoroutine(playerComponents[p].DoTurn());
            }

            Debug.Log(p.Name + " " + Enum.GetName(typeof(ActionType), p.ActionLog[game.Round].ActionType) + ": " + p.ActionLog[game.Round].Money);
        }

        game.Round++;

        yield return null;
    }

    private IEnumerator UserTurn() {
        userTurn = true;
        ActivateButtons(true);
        while (userTurn) {
            yield return null;
        }

        switch (userAction) {
            case ActionType.Fold:
                yield return StartCoroutine(Fold());
                break;
            case ActionType.Check:
                yield return StartCoroutine(Check());
                break;
            case ActionType.Call:
                yield return StartCoroutine(Call());
                break;
            case ActionType.Raise:
                yield return StartCoroutine(Raise());
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
            if (Utilities.ContainsRaise(game.User, game.Players, game.Round)) {
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

    public IEnumerator Call() {
        game.User.ActionLog.Add(new PlayerAction(ActionType.Call, 0));
        yield return null;
    }

    public IEnumerator Raise() {
        game.User.ActionLog.Add(new PlayerAction(ActionType.Raise, 0));
        yield return null;
    }

    #endregion
}