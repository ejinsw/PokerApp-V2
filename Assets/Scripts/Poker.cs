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

    public class Player {
        private string _name;
        private List<Card> _cards;
        private double _money;
        private bool _folded;
        private List<float> _ins;

        public Player(string name, List<Card> cards, double money, bool folded = false) {
            Name = name;
            Cards = cards;
            Money = money;
            Folded = folded;
            Ins = new();
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

        public List<float> Ins {
            get => _ins;
            set => _ins = value;
        }

        #region Debugging

        public void PrintDetails() {
            string msg = Name + " " + Money;
            foreach (Card c in Cards) {
                msg += " " + Enum.GetName(typeof(Suit), c.Suit) + " " + Enum.GetName(typeof(Rank), c.Rank);
            }

            Debug.Log(msg);
        }

        #endregion
    }

    public class Game {
        private List<Card> _deck;
        private List<Card> _communityCards;
        private List<Player> _players;
        private Player _user;

        private int _numPlayers;
        private float _pot;
        private int _round;

        public Game(int numPlayers) {
            NumPlayers = numPlayers;
            Pot = 0;
            Round = 0;

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

        public int NumPlayers {
            get => _numPlayers;
            set => _numPlayers = value;
        }

        public int Round {
            get => _round;
            set => _round = value;
        }

        public float Pot {
            get => _pot;
            set => _pot = value;
        }
    }
}