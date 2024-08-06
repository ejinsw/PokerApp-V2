using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Poker {
    public static class Utilities {
        public static Random rng = new Random((uint)Guid.NewGuid().GetHashCode());

        public static List<string> player_names = new List<string> {
            "David", "Tom", "Will", "Julie", "Wanda", "Winston", "Brook", "Thomas",
            "John", "Jane", "Alice", "Bob", "Charlie", "Michael", "Sarah", "Emily",
            "James", "Linda", "Daniel", "Laura", "Matthew", "Jessica", "Joshua", "Karen",
            "Andrew", "Barbara", "Kevin", "Helen", "Paul", "Donna", "Mark", "Lisa",
            "Jason", "Nancy", "Ryan", "Betty", "Adam", "Sandra", "Nathan", "Carol",
            "Brian", "Ruth", "Scott", "Sharon", "Aaron", "Michelle", "Gary", "Deborah",
            "Frank", "Shirley"
        };

        public static Dictionary<Rank, string> rank_as_string = new() {
            { Rank.Ace, "A" },
            { Rank.Two, "2" },
            { Rank.Three, "3" },
            { Rank.Four, "4" },
            { Rank.Five, "5" },
            { Rank.Six, "6" },
            { Rank.Seven, "7" },
            { Rank.Eight, "8" },
            { Rank.Nine, "9" },
            { Rank.Ten, "10" },
            { Rank.Jack, "J" },
            { Rank.Queen, "Q" },
            { Rank.King, "K" }
        };

        public static string RandomName() {
            return player_names[rng.NextInt(0, player_names.Count)];
        }

        public static double RandomDouble(double min, double max, int cutoff = 2) {
            double num = rng.NextDouble(min, max);
            num *= Math.Pow(10, cutoff);
            num = Math.Floor(num);
            num /= Math.Pow(10, cutoff);
            return num;
        }

        public static int RandomInt(int min, int max) {
            return rng.NextInt(min, max + 1);
        }

        public static bool TrueWithProbability(double p) {
            return rng.NextDouble(0, 1) < p;
        }

        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.NextInt(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void PrintCards(List<Card> cards) {
            string msg = "";
            foreach (Card c in cards) {
                msg += Enum.GetName(typeof(Suit), c.Suit) + " " + Enum.GetName(typeof(Rank), c.Rank) + " ";
            }

            Debug.Log(msg);
        }

        public static List<Card> NewDeck() {
            List<Card> deck = new();
            foreach (Suit s in Enum.GetValues(typeof(Suit))) {
                foreach (Rank r in Enum.GetValues(typeof(Rank))) {
                    deck.Add(new Card(s, r));
                }
            }

            return deck;
        }

        public static List<Card> DeckTakeOne(ref List<Card> deck) {
            List<Card> cards = new();
            if (deck.Count <= 0) return cards;

            cards.Add(deck[0]);
            deck.RemoveAt(0);

            return cards;
        }

        public static List<Card> DeckTakeTwo(ref List<Card> deck) {
            List<Card> cards = new();
            cards.AddRange(DeckTakeOne(ref deck));
            cards.AddRange(DeckTakeOne(ref deck));
            return cards;
        }

        public static List<Card> DeckTakeAmount(ref List<Card> deck, int amount) {
            List<Card> cards = new();
            for (int i = 0; i < amount; i++) {
                cards.AddRange(DeckTakeOne(ref deck));
            }

            if (cards.Count < amount) {
                Debug.LogWarning("Requested more cards than are available in the deck.");
            }

            return cards;
        }

        public static void ShowCards(ref List<Card> cards) {
            foreach (Card c in cards) {
                c.Visible = true;
            }
        }

        public static bool ContainsRaise(Player self, List<Player> players, int currentRound) {
            foreach (Player p in players) {
                if (p == self) continue; // skip self
                if (!PlayerWent(p, currentRound)) continue; // skip non-initialized
                if (p.ActionLog[currentRound].ActionType == ActionType.Raise) return true;
            }

            return false;
        }

        public static Player HighestRaiser(Player self, List<Player> players, int currentRound) {
            if (!ContainsRaise(self, players, currentRound)) return self;

            Player highest = self;
            
            foreach (Player p in players) {
                if (!PlayerWent(p, currentRound)) continue; // skip non-initialized
                if (p.ActionLog[currentRound].ActionType == ActionType.Raise 
                    && !PlayerWent(highest, currentRound) 
                    || p.ActionLog[currentRound].ActionType == ActionType.Raise 
                    && p.ActionLog[currentRound].Money > highest.ActionLog[currentRound].Money) {
                    highest = p;
                }
            }
            
            return highest;
        }

        public static bool PlayerWent(Player player, int round) {
            return round < player.ActionLog.Count;
        }

        #region Bluff Cases

        public enum BluffCase {
            Pure,
            FrontStraightOpen,
            FrontStraightOne,
            FlushDrawOne
        }

        public static BluffCase GetBluffCase(List<Card> hand, List<Card> board) {
            List<Card> allCards = hand.Concat(board).ToList();

            // Check for Flush Draw (One-ended)
            if (IsFlushDrawOneEnded(allCards)) {
                return BluffCase.FlushDrawOne;
            }

            // Check for Frontdoor Straight Draw (Open-ended)
            if (IsFrontStraightDrawOpen(allCards)) {
                return BluffCase.FrontStraightOpen;
            }

            // Check for Frontdoor Straight Draw (One-ended)
            if (IsFrontStraightDrawOne(allCards)) {
                return BluffCase.FrontStraightOne;
            }

            // Default to Pure Bluff if no other conditions are met
            return BluffCase.Pure;
        }

        private static bool IsFlushDrawOneEnded(List<Card> cards) {
            var suitGroups = cards.GroupBy(card => card.Suit)
                .Where(group => group.Count() == 4);
            return suitGroups.Any(); // True if there"s a four-card flush draw
        }

        private static bool IsFrontStraightDrawOpen(List<Card> cards) {
            var ranks = cards.Select(card => card.Rank).Distinct().OrderBy(rank => rank).ToList();
            for (int i = 0; i < ranks.Count - 3; i++) {
                if (ranks[i] + 3 == ranks[i + 3] && ranks.Skip(i).Take(4).Distinct().Count() == 4) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFrontStraightDrawOne(List<Card> cards) {
            var ranks = cards.Select(card => card.Rank).Distinct().OrderBy(rank => rank).ToList();
            for (int i = 0; i < ranks.Count - 2; i++) {
                Debug.Log($"i is currently at {i} and the size of ranks is {ranks.Count}");
                if ((ranks[i] + 2 == ranks[i + 2] && ranks.Skip(i).Take(3).Distinct().Count() == 3) ||
                    (i < ranks.Count - 3 && (ranks[i] + 3 == ranks[i + 3] && ranks.Skip(i).Take(4).Distinct().Count() == 4 && ranks.Contains(ranks[i] + 2)))) {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}