using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Poker
{
    [Serializable]
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades,
        Null
    }

    [Serializable]
    public enum Rank
    {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    [Serializable]
    public class Card 
    {
        [SerializeField] private Suit _suit;
        [SerializeField] private Rank _rank;
        [SerializeField] private bool _visible;

        #region Operator Override

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Card card = (Card)obj;
            return Suit == card.Suit && Rank == card.Rank;
        }

        public override int GetHashCode()
        {
            return (Suit, Rank).GetHashCode();
        }

        #endregion

        public Card(Suit suit, Rank rank, bool visible = false)
        {
            _suit = suit;
            _rank = rank;
            _visible = visible;
        }
        
        public Card(Card card)
        {
            _suit = card.Suit;
            _rank = card.Rank;
            _visible = card.Visible;
        }

        public Suit Suit
        {
            get => _suit;
            private set => _suit = value;
        }

        public Rank Rank
        {
            get => _rank;
            private set => _rank = value;
        }

        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        public Card Clone()
        {
            return new Card(this);
        }

    }

    [Serializable]
    public enum ActionType
    {
        Null,
        Fold,
        Check,
        Call,
        Raise
    }

    [Serializable]
    public class PlayerAction 
    {
        [SerializeField] private long _money;
        [SerializeField] private ActionType _actionType;

        public PlayerAction(ActionType actionType, long money)
        {
            _actionType = actionType;
            _money = money;
        }
        
        public PlayerAction(PlayerAction other)
        {
            _actionType = other.ActionType;
            _money = other.Money;
        }

        public long Money
        {
            get => _money;
            private set => _money = value;
        }

        public ActionType ActionType
        {
            get => _actionType;
            private set => _actionType = value;
        }

        public PlayerAction Clone()
        {
            return new PlayerAction(this);
        }
    }

    [Serializable]
    public class Player 
    {
        [SerializeField] private string _name;
        [SerializeField] private List<Card> _cards;
        [SerializeField] private long _money;
        [SerializeField] private bool _folded;
        [SerializeField] private List<PlayerAction> _actionLog;
        [SerializeField] private int next = 0;

        public Player(string name, List<Card> cards, long money, bool folded = false, List<PlayerAction> actionLog = null, int next = 0)
        {
            _name = name;
            _cards = cards ?? new List<Card>();
            _money = money;
            _folded = folded;
            _actionLog = actionLog ?? new List<PlayerAction>();
            this.next = next;
        }
        
        public Player(Player other)
        {
            _name = other.Name;
            _cards = other.Cards.Select(card => card.Clone()).ToList();
            _money = other.Money;
            _folded = other.Folded;
            _actionLog = other.ActionLog.Select(action => action.Clone()).ToList();
            next = other.next;
        }

        public string Name
        {
            get => _name;
            private set => _name = value;
        }

        public List<Card> Cards
        {
            get => _cards;
            set => _cards = value;
        }

        public long Money
        {
            get => _money;
            set => _money = value;
        }

        public bool Folded
        {
            get => _folded;
            set => _folded = value;
        }

        public List<PlayerAction> ActionLog
        {
            get => _actionLog;
            set => _actionLog = value;
        }

        public PlayerAction LastAction()
        {
            return ActionLog.LastOrDefault();
        }

        public PlayerAction NextAction()
        {
            if (next >= ActionLog.Count) return null;
            PlayerAction nextAction = ActionLog[next];
            next++;
            return nextAction;
        }

        public void UseMoney(long amount)
        {
            Money -= amount;
        }

        public Player Clone()
        {
            return new Player(this);
        }
    }

    [Serializable]
    public class Game
    {
        [SerializeField] private List<Card> _deck;
        [SerializeField] private List<Card> _communityCards;
        [SerializeField] private List<Player> _players;
        [SerializeField] private Player _user;
        [SerializeField] private Player _lastRaiser;

        [SerializeField] private int _numPlayers;
        [SerializeField] private long _pot;

        public Game(int numPlayers)
        {
            LastRaiser = null;
            NumPlayers = numPlayers;
            Pot = 0;

            // Deck
            Deck = Utilities.NewDeck();
            Deck.Shuffle();
            
            // Community Cards
            CommunityCards = Utilities.DeckTakeAmount(ref _deck, Utilities.RandomInt(3, 5));
            Utilities.ShowCards(ref _communityCards);

            // Players
            Players = new();
            // User
            User = new("You", Utilities.DeckTakeTwo(ref _deck), Utilities.RandomInt(0, 1000));
            List<Card> userCards = User.Cards;
            Utilities.ShowCards(ref userCards);
            Players.Add(User);

            // Other Players
            for (int i = 0; i < NumPlayers; i++)
            {
                string name = Utilities.RandomName();
                while (Players.Any(p => p.Name == name))
                {
                    name = Utilities.RandomName();
                }

                Player p = new Player(name, Utilities.DeckTakeTwo(ref _deck), Utilities.RandomInt(0, 1000));
                Players.Add(p);
            }

            Players.Shuffle();
        }

        public Game(int numPlayers, long potSize, List<Card> deck, List<Card> communityCards, Player user, int userPosition, List<Player> players)
        {
            LastRaiser = null;
            NumPlayers = numPlayers;
            Pot = potSize;
            Deck = deck.Select(card => card.Clone()).ToList();
            CommunityCards = communityCards.Select(card => card.Clone()).ToList();
            Players = players.Select(player => player.Clone()).ToList();
            User = user;

            List<Card> userCards = User.Cards;

            Utilities.ShowCards(ref userCards);

            Players.Add(User);
            Players.TrySwap(Players.IndexOf(User), userPosition, out Exception err);

            if (err != null)
            {
                Debug.LogError($"Failed swapping user to position {userPosition}: {err.Message}");
            }
        }

        public List<Card> Deck
        {
            get => _deck;
            set => _deck = value;
        }

        public List<Card> CommunityCards
        {
            get => _communityCards;
            set => _communityCards = value;
        }

        public List<Player> Players
        {
            get => _players;
            set => _players = value;
        }

        public Player User
        {
            get => _user;
            set => _user = value;
        }

        public Player LastRaiser
        {
            get => _lastRaiser;
            set => _lastRaiser = value;
        }

        public int NumPlayers
        {
            get => _numPlayers;
            set => _numPlayers = value;
        }

        public long Pot
        {
            get => _pot;
            set => _pot = value;
        }
    }
}