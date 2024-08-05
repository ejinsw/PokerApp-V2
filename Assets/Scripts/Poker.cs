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
        One,
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

        public Player(string name, List<Card> cards) {
            Name = name;
            Cards = cards;
        }

        public string Name {
            get => _name;
            private set => _name = value;
        }
        public List<Card> Cards {
            get => _cards;
            private set => _cards = value;
        }

        #region Debugging

        public void PrintCards() {
            string msg = Name;
            foreach (Card c in Cards) {
                msg += " " + Enum.GetName(typeof(Suit), c.Suit) + " " + Enum.GetName(typeof(Rank), c.Rank);
            }
            Debug.Log(msg);
        }

        #endregion
    }
}