using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Poker
{
    public static class BluffCases
    {
        public enum BluffCase
        {
            Pure,
            FrontStraightOpen,
            FrontStraightOne,
            FlushDrawOne
        }

        public static BluffCase GetBluffCase(List<Card> hand, List<Card> board)
        {
            List<Card> allCards = hand.Concat(board).ToList();

            // Check for Flush Draw (One-ended)
            if (IsFlushDrawOneEnded(allCards))
            {
                return BluffCase.FlushDrawOne;
            }

            // Check for Frontdoor Straight Draw (Open-ended)
            if (IsFrontStraightDrawOpen(allCards))
            {
                return BluffCase.FrontStraightOpen;
            }

            // Check for Frontdoor Straight Draw (One-ended)
            if (IsFrontStraightDrawOne(allCards))
            {
                return BluffCase.FrontStraightOne;
            }

            // Default to Pure Bluff if no other conditions are met
            return BluffCase.Pure;
        }

        private static bool IsFlushDrawOneEnded(List<Card> cards)
        {
            var suitGroups = cards.GroupBy(card => card.Suit)
                .Where(group => group.Count() == 4);
            return suitGroups.Any(); // True if there"s a four-card flush draw
        }

        private static bool IsFrontStraightDrawOpen(List<Card> cards)
        {
            if (cards.Count != 4)
            {
                return false;
            }
            var ranks = cards.Select(card => card.Rank).Distinct().OrderBy(rank => rank).ToList();
            for (int i = 0; i < ranks.Count - 3; i++)
            {
                if (ranks[i] + 3 == ranks[i + 3] && ranks.Skip(i).Take(4).Distinct().Count() == 4)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFrontStraightDrawOne(List<Card> cards)
        {
            var ranks = cards.Select(card => card.Rank).Distinct().OrderBy(rank => rank).ToList();
            for (int i = 0; i < ranks.Count - 2; i++)
            {
                if ((ranks[i] + 2 == ranks[i + 2] && ranks.Skip(i).Take(3).Distinct().Count() == 3) ||
                    (i < ranks.Count - 3 && (ranks[i] + 3 == ranks[i + 3] && ranks.Skip(i).Take(4).Distinct().Count() == 4 && ranks.Contains(ranks[i] + 2))))
                {
                    return true;
                }
            }

            return false;
        }
        #region Scenarios
        public static List<Card> FlushDrawCc(ref List<Card> deck, int size)
        {
            List<Card> cards = Utilities.HandSuited(ref deck);
            for (int i = 0; i < size - 2; i++)
            {
                foreach (Card c in deck)
                {
                    if (cards[0].Suit != c.Suit)
                    {
                        cards.Add(c);
                        Utilities.DeckTakeCards(ref deck, cards);
                        break;
                    }
                }
            }
            return cards;
        }

        public static List<Card> FlushDrawP(ref List<Card> deck, List<Card> cc)
        {
            if (cc.Count == 0)
            {
                return null;
            }
            return Utilities.HandSuited(ref deck, cc[0].Suit);
        }

        public static List<Card> BackDoorFlushDrawCc(ref List<Card> deck, int size)
        {
            List<Card> cards = new();
            cards.Add(deck[0]);
            Utilities.DeckTakeCards(ref deck, cards);
            for (int i = 0; i < 2; i++)
            {
                foreach (Card c in deck)
                {
                    if (cards[0].Suit != c.Suit)
                    {
                        cards.Add(c);
                        Utilities.DeckTakeCards(ref deck, cards);
                        break;
                    }
                }
            }
            return cards;
        }

        public static List<Card> BackDoorFlushDrawP(ref List<Card> deck, List<Card> cc)
        {
            if (cc.Count == 0)
            {
                return null;
            }
            int x = Utilities.RandomInt(0, 2);
            return Utilities.HandSuited(ref deck, cc[x].Suit);
        }

        public static List<Card> StraightDrawCc(ref List<Card> deck, int size)
        {
            List<Card> cards = new();
            int rand = Utilities.RandomInt(1, 9);
            int pos1 = Utilities.RandomInt(0, size - 1);
            int pos2;
            int pos3 = 0;
            do
            {
                pos2 = Utilities.RandomInt(0, size - 1);
            } while (pos1 == pos2);
            for (int i = 0; i < size; i++)
            {
                if (i != pos1 && i != pos2)
                {
                    pos3 = i;
                    break;
                }
            }
            Rank r = (Rank)rand;
            for (int i = 0; i < size; i++)
            {
                if (i == pos1)
                {
                    foreach (Card c in deck)
                    {
                        if (c.Rank == r)
                        {
                            cards.Add(c);
                            Utilities.DeckTakeCards(ref deck, cards);
                            break;
                        }
                    }
                }
                else if (i == pos2)
                {
                    foreach (Card c in deck)
                    {
                        if (c.Rank == r + 1)
                        {
                            cards.Add(c);
                            Utilities.DeckTakeCards(ref deck, cards);
                            break;
                        }
                    }
                }
                else if (i == pos3)
                {
                    foreach (Card c in deck)
                    {
                        if (c.Rank == r + 2)
                        {
                            cards.Add(c);
                            Utilities.DeckTakeCards(ref deck, cards);
                            break;
                        }
                    }
                }
                else
                {
                    foreach (Card c in deck)
                    {
                        if (c.Rank != r + 3 && c.Rank != r - 1 && c.Rank != r + 4 && c.Rank != r - 2)
                        {
                            cards.Add(c);
                            Utilities.DeckTakeCards(ref deck, cards);
                            break;
                        }
                    }
                }

            }
            return cards;
        }

        public static List<Card> StraightDrawP(ref List<Card> deck, List<Card> cc)
        {
            if (cc.Count == 0)
            {
                return null;
            }
            List<Card> cards = new();
            int min = 0;
            List<Card> sortedCards = cc.OrderBy(card => card.Rank).ToList();
            min = sortedCards[0].Rank == (sortedCards[1].Rank - 1) ? (int)sortedCards[0].Rank
                : (int)sortedCards[1].Rank;
            if (min == 1 || min == 0)
            {
                min += 3;
            }
            else
            {
                min += Utilities.RandomInt(0, 1) == 1 ? 3 : -1;
            }
            foreach (Card c in deck)
            {
                if ((int)c.Rank == min)
                {
                    cards.Add(c);
                    Utilities.DeckTakeCards(ref deck, cards);
                    break;
                }
            }
            foreach (Card c in deck)
            {
                if ((int)c.Rank != min - 1 && (int)c.Rank != min + 1 && (int)c.Rank != min + 4 && (int)c.Rank != min - 4)
                {
                    cards.Add(c);
                    Utilities.DeckTakeCards(ref deck, cards);
                    break;
                }
            }
            return cards;
        }
        #endregion
    }
}