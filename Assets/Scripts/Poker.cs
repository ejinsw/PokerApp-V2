using System;
using System.Collections;
using System.Collections.Generic;
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
        public Player(string name, List<Card> cards, double money, bool folded = false) {
            Name = name;
            Cards = cards;
            Money = money;
            Folded = folded;
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
}