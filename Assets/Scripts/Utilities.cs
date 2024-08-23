using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

namespace Poker
{
    public static class Utilities
    {
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


        public static string RandomName()
        {
            return player_names[rng.NextInt(0, player_names.Count)];
        }

        public static double RandomDouble(double min, double max, int cutoff = 2)
        {
            double num = rng.NextDouble(min, max);
            num *= Math.Pow(10, cutoff);
            num = Math.Floor(num);
            num /= Math.Pow(10, cutoff);
            return num;
        }

        public static int RandomInt(int min, int max, int step = 1)
        {
            // Calculate the number of steps between min and max
            int stepCount = (max - min) / step + 1;

            // Generate a random index within the range of steps
            int randomStepIndex = rng.NextInt(0, stepCount);

            // Calculate the random value based on the step size
            return min + (randomStepIndex * step);
        }

        public static bool TrueWithProbability(double p)
        {
            return rng.NextDouble(0, 1) < p;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            // To ensure random shuffle every time
            int seed = Environment.TickCount;
            Random shuffleRng = new Random((uint)seed);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = shuffleRng.NextInt(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void PrintCards(List<Card> cards)
        {
            string msg = "";
            foreach (Card c in cards)
            {
                msg += Enum.GetName(typeof(Suit), c.Suit) + " " + Enum.GetName(typeof(Rank), c.Rank) + " ";
            }

            Debug.Log(msg);
        }

        public static List<Card> NewDeck()
        {
            List<Card> deck = new();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
            {
                if (s == Suit.Null) continue;
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                {
                    deck.Add(new Card(s, r));
                }
            }

            return deck;
        }


        public static List<Card> DeckTakeOne(ref List<Card> deck)
        {
            List<Card> cards = new();
            if (deck.Count <= 0) return cards;

            cards.Add(deck[0].Clone());
            deck.RemoveAt(0);

            return cards;
        }

        public static List<Card> DeckTakeTwo(ref List<Card> deck)
        {
            List<Card> cards = new();
            cards.AddRange(DeckTakeOne(ref deck));
            cards.AddRange(DeckTakeOne(ref deck));
            return cards;
        }

        public static List<Card> DeckTakeAmount(ref List<Card> deck, int amount)
        {
            List<Card> cards = new();
            for (int i = 0; i < amount; i++)
            {
                cards.AddRange(DeckTakeOne(ref deck));
            }

            if (cards.Count < amount)
            {
                Debug.LogWarning("Requested more cards than are available in the deck.");
            }

            return cards;
        }

        public static List<Card> DeckTakeCards(ref List<Card> deck, List<Card> cards)
        {
            HashSet<Card> cardsToRemove = new HashSet<Card>(cards);

            deck = deck.Where(card => !cardsToRemove.Contains(card)).ToList();

            return cards;
        }

        public static bool DeckInsert(ref List<Card> deck, ref List<Card> toInsert)
        {
            bool alreadyHas = false;
            foreach (Card c in toInsert)
            {
                if (deck.Contains(c))
                {
                    alreadyHas = true;
                    continue;
                }
                deck.Add(c);
            }
            toInsert.Clear();
            return !alreadyHas;
        }

        public static List<Card> HandSuited(ref List<Card> deck, Suit suit = Suit.Null)
        {
            if (deck.Count < 2) return null;
            if (suit != Suit.Null)
            {
                List<Card> cards = new();
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < deck.Count; j++)
                    {
                        if (deck[j].Suit == suit && cards.Count != 0 && deck[j] != cards[0] || deck[j].Suit == suit && cards.Count == 0)
                        {
                            cards.Add(deck[j]);
                            break;
                        }
                    }
                }
                return DeckTakeCards(ref deck, cards); ;
            }
            else
            {
                for (int i = 0; i < deck.Count; i++)
                {
                    List<Card> cards = new();
                    cards.Add(deck[i]);

                    if (cards.Count < 1) return null;

                    for (int j = i + 1; j < deck.Count; j++)
                    {
                        if (deck[j].Suit == cards[0].Suit)
                        {
                            cards.Add(deck[j]);
                            return DeckTakeCards(ref deck, cards);
                        }
                    }
                }
            }
            return null;
        }

        public static List<Card> HandPaired(ref List<Card> deck)
        {
            if (deck.Count < 2) return null;

            for (int i = 0; i < deck.Count; i++)
            {
                List<Card> cards = new();
                cards.Add(deck[i]);

                if (cards.Count < 1) return null;

                for (int j = i + 1; j < deck.Count; j++)
                {
                    if (deck[j].Rank == cards[0].Rank)
                    {
                        cards.Add(deck[j]);
                        return DeckTakeCards(ref deck, cards);
                    }
                }
            }

            return null;
        }

        public static void ShowCards(ref List<Card> cards)
        {
            foreach (Card c in cards)
            {
                c.Visible = true;
            }
        }

        public static bool AllResponded(Player lastRaiser, List<Player> players)
        {
            foreach (Player p in players)
            {
                if (p.Folded) continue; // folders ok
                if (p.LastAction() == null) return false; // null if haven't gone yet

                // Not everyone called/folded to a raise
                if (lastRaiser != null)
                {
                    if (lastRaiser == p) continue; // skip last raiser
                    if (p.LastAction().ActionType != ActionType.Fold
                        && p.LastAction().ActionType != ActionType.Call
                        || p.LastAction().Money < lastRaiser.LastAction().Money)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static int HandEquity(BluffCases.BluffCase a, int numCc)
        {
            switch (a)
            {
                case BluffCases.BluffCase.FlushDraw:
                    return numCc == 3 ? 36 : 18;
                case BluffCases.BluffCase.BackdoorFlushDraw:
                    return 4;
                case BluffCases.BluffCase.OverPair:
                    return numCc == 3 ? RandomInt(55, 75) : RandomInt(70, 85);
                case BluffCases.BluffCase.UnderPair:
                    return numCc == 3 ? 8 : 4;
                case BluffCases.BluffCase.StraightDraw:
                    return numCc == 3 ? 32 : 17;
                case BluffCases.BluffCase.GutShot:
                    return numCc == 3 ? 17 : 9;
                case BluffCases.BluffCase.StraightFlushDraw:
                    return numCc == 3 ? 54 : 33;
                case BluffCases.BluffCase.StraightDrawPair:
                    return numCc == 3 ? 40 : 20;
                case BluffCases.BluffCase.FlushDrawPair:
                    return numCc == 3 ? 44 : 22;
                default:
                    return 0;
            }
        }
        public class Odds
        {
            public int first = 0;
            public int second = 0;
        }
        public static Odds PotOdds(int pot, int raise)
        {
            Odds odds = new();
            odds.first = pot + raise;
            odds.second = raise;
            return odds;

        }

        public class Statistics
        {
            public int callEv = 0;
            public int reraiseEv = 0;
            public int reraiseAmount = 0;
        }

        public static Statistics Options(int handEquity, int pot, int raise)
        {
            //result[call EV, reraise EV, optimal reraiseAmount]
            //reraiseAmount is the totla money player puts into pot
            Statistics result = new();
            double equityFactor = handEquity / 100.0f;
            if (equityFactor >= 50)
            {
                result.callEv = (int)Math.Round((double)(pot + 2 * raise) * equityFactor) - raise;
                result.reraiseAmount = (int)GameManager.instance.selectedGameSettings.userStartingMoney;
                result.reraiseEv = (int)Math.Round((double)(pot + 2 * result.reraiseAmount) * equityFactor) - raise;
                return result;
            }
            else
            {
                int callEV = (int)Math.Round(equityFactor * (pot + raise) - raise * (1.0f - equityFactor));
                result.callEv = callEV;
                double reraiseAmount = (double)(pot * equityFactor) / (1.0f - 2 * equityFactor);
                if (reraiseAmount < 2 * raise)
                {
                    reraiseAmount = raise * 2;
                }
                int raiseEV = (int)Math.Round(equityFactor * (pot + reraiseAmount * 2) - reraiseAmount);
                result.reraiseEv = raiseEV;
                result.reraiseAmount = (int)Math.Round(reraiseAmount);
                return result;
            }
        }
        public static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}


