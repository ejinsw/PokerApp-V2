using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Poker {
    public enum Suit {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    public enum Rank {
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

    public class Card {
        private Suit _suit;
        private Rank _rank;

        private bool _visible;

        public Card(Suit suit, Rank rank, bool visible = false) {
            Suit = suit;
            Rank = rank;
            Visible = visible;
        }

        public Suit Suit {
            get => _suit;
            private set => _suit = value;
        }

        public Rank Rank {
            get => _rank;
            private set => _rank = value;
        }

        public bool Visible {
            get => _visible;
            set => _visible = value;
        }
    }

    public enum ActionType {
        Null,
        Fold,
        Check,
        Call,
        Raise
    }
    
    public class PlayerAction {
        private float _money;
        private ActionType _actionType;

        public PlayerAction(ActionType actionType, float money) {
            ActionType = actionType;
            Money = money;
        }
        
        public float Money {
            get => _money;
            private set => _money = value;
        }

        public ActionType ActionType {
            get => _actionType;
            private set => _actionType = value;
        }
    }

    public class Player {
        private string _name;
        private List<Card> _cards;
        private double _money;
        private bool _folded;
        private List<PlayerAction> _actionLog;

        public Player(string name, List<Card> cards, double money, bool folded = false) {
            Name = name;
            Cards = cards;
            Money = money;
            Folded = folded;
            ActionLog = new();
        }

        public string Name {
            get => _name;
            private set => _name = value;
        }

        public List<Card> Cards {
            get => _cards;
            private set => _cards = value;
        }

        public double Money {
            get => _money;
            set => _money = value;
        }

        public bool Folded {
            get => _folded;
            set => _folded = value;
        }

        public List<PlayerAction> ActionLog {
            get => _actionLog;
            set => _actionLog = value;
        }

        public PlayerAction LastAction() {
            return ActionLog.LastOrDefault();
        }
    }

    public class Game {
        private List<Card> _deck;
        private List<Card> _communityCards;
        private List<Player> _players;
        private Player _user;
        private Player _lastRaiser;

        private int _numPlayers;
        private float _pot;

        public Game(int numPlayers) {
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
            User = new("You", Utilities.DeckTakeTwo(ref _deck), Utilities.RandomDouble(0, 1000));
            List<Card> userCards = User.Cards;
            Utilities.ShowCards(ref userCards);
            Players.Add(User);

            // Other Players
            for (int i = 0; i < NumPlayers; i++) {
                string name = Utilities.RandomName();
                while (Players.Any(p => p.Name == name)) {
                    name = Utilities.RandomName();
                }

                Player p = new Player(name, Utilities.DeckTakeTwo(ref _deck), Utilities.RandomDouble(0, 1000));
                Players.Add(p);
            }

            Players.Shuffle();
        }

        public List<Card> Deck {
            get => _deck;
            set => _deck = value;
        }

        public List<Card> CommunityCards {
            get => _communityCards;
            set => _communityCards = value;
        }

        public List<Player> Players {
            get => _players;
            set => _players = value;
        }

        public Player User {
            get => _user;
            set => _user = value;
        }

        public Player LastRaiser {
            get => _lastRaiser;
            set => _lastRaiser = value;
        }

        public int NumPlayers {
            get => _numPlayers;
            set => _numPlayers = value;
        }
        
        public float Pot {
            get => _pot;
            set => _pot = value;
        }
    }
}