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

        public Card(Suit suit, Rank rank) {
            Suit = suit;
            Rank = rank;
        }

        public Suit Suit {
            get => _suit;
            private set => _suit = value;
        }
        
        public Rank Rank {
            get => _rank;
            private set => _rank = value;
        }
    }

    public class Player {
        private string _name;
        private List<Card> _cards;
        private double _money;
        public Player(string name, List<Card> cards, double money) {
            Name = name;
            Cards = cards;
            Money = money;
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