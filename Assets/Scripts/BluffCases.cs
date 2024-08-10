using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Poker
{
    public static class BluffCases
    {
        #region Bluff Case Checks

        public enum BluffCase
        {
            None,
            Pair,
            UnderPair,
            OverPair,
            DoublePair,
            Triples,
            FourOfAKind,
            FullHouse,
            StraightDraw,
            GutShot,
            BackdoorFlushDraw,
            FlushDraw,
            StraightFlushDraw,
        }

        public static List<BluffCase> GetAllBluffCases(List<Card> cc, List<Card> hand) {
            List<Card> cards = new List<Card>(cc);
            cards.AddRange(hand);
            
            List<BluffCase> bluffCases = new();

            if (IsUnderPair(cc, hand))
                bluffCases.Add(BluffCase.UnderPair);
            
            if (IsOverPair(cc, hand))
                bluffCases.Add(BluffCase.OverPair);
            
            if (NumberOfPairs(cards) == 2)
                bluffCases.Add(BluffCase.DoublePair);

            if (IsTriples(cards))
                bluffCases.Add(BluffCase.Triples);

            if (IsFourOfAKind(cards))
                bluffCases.Add(BluffCase.FourOfAKind);

            if (IsFullHouse(cards))
                bluffCases.Add(BluffCase.FullHouse);
            
            if (IsBackdoorFlushDraw(cards) && !IsStraightFlushDraw(cards))
                bluffCases.Add(BluffCase.BackdoorFlushDraw);

            if (IsFlushDraw(cards) && !IsStraightFlushDraw(cards))
                bluffCases.Add(BluffCase.FlushDraw);

            if (IsStraightDraw(cards) && !IsStraightFlushDraw(cards))
                bluffCases.Add(BluffCase.StraightDraw);
            
            if (IsGutshot(cards))
                bluffCases.Add(BluffCase.GutShot);

            if (IsStraightFlushDraw(cards))
                bluffCases.Add(BluffCase.StraightFlushDraw);

            return bluffCases;
        }

        public static bool IsFlushDraw(List<Card> cards)
        {
            // There aren't enough cards
            if (cards.Count < 4)
            {
                return false;
            }

            // This creates lists for every situation where there's four same suited cards (this will always be 1 or 0)
            IEnumerable<IGrouping<Suit, Card>> suitGroups = cards.GroupBy(card => card.Suit)
                .Where(group => group.Count() == 4);

            return suitGroups.Any(); // True the enumerable isn't empty a four-card flush draw
        }
        public static bool IsBackdoorFlushDraw(List<Card> cards)
        {
            // There aren't enough cards
            if (cards.Count < 3)
            {
                return false;
            }

            IEnumerable<IGrouping<Suit, Card>> suitGroups = cards.GroupBy(card => card.Suit)
                .Where(group => group.Count() == 3);

            return suitGroups.Any(); // True the enumerable isn't empty a four-card flush draw
        }

        private static bool IsNeighborRank(Rank a, Rank b)
        {
            // King - Ace is 12 - 0
            return Mathf.Abs((int)a - (int)b) == 1 || Mathf.Abs((int)a - (int)b) == 12;
        }
        
        public static bool IsStraightDraw(List<Card> cards)
        {
            // There aren't enough cards
            if (cards.Count < 4)
            {
                return false;
            }

            List<Rank> ranks = cards.Select(card => card.Rank).Distinct().OrderBy(rank => rank).ToList();
            // Only consider the first 4 distinct ranks
            if (ranks.Count < 4)
            {
                return false;
            }

            IEnumerable<IGrouping<int, Rank>> rankGroups = ranks
                .Select((rank, index) => new { Rank = rank, GroupKey = (int)rank - index })
                .GroupBy(x => x.GroupKey, x => x.Rank)
                .Where(group => group.Count() == 4);
            
            return rankGroups.Any();
        }
        
        public static bool IsStraightFlushDraw(List<Card> cards)
        {
            // There aren't enough cards
            if (cards.Count < 4)
            {
                return false;
            }

            return IsStraightDraw(cards) && (IsFlushDraw(cards) || IsBackdoorFlushDraw(cards));

            // // This Checks for a straight flush draw not a "straight draw" + "flush draw"
            // var suitGroups = cards.GroupBy(card => card.Suit).ToList();
            //
            // foreach (var suitGroup in suitGroups)
            // {
            //     if (suitGroup.Count() < 4)
            //     {
            //         continue;
            //     }
            //
            //     List<Rank> ranks = suitGroup.Select(card => card.Rank).Distinct().OrderBy(rank => rank).ToList();
            //
            //     // Only consider the first 4 distinct ranks
            //     if (ranks.Count < 4)
            //     {
            //         continue;
            //     }
            //
            //     IEnumerable<IGrouping<int, Rank>> rankGroups = ranks
            //         .Select((rank, index) => new { Rank = rank, GroupKey = (int)rank - index })
            //         .GroupBy(x => x.GroupKey, x => x.Rank)
            //         .Where(group => group.Count() == 4);
            //
            //     foreach (var g in rankGroups)
            //     {
            //         List<Rank> group = g.ToList();
            //
            //         if (g.First() != Rank.Ace && g.Last() != Rank.Ace)
            //         {
            //             return true;
            //         }
            //     }
            // }
            //
            // return false;
        }

        public static bool IsGutshot(List<Card> cards)
        {
            // There aren't enough cards
            if (cards.Count < 4)
            {
                return false;
            }

            List<Rank> ranks = cards.Select(card => card.Rank).Distinct().OrderBy(rank => rank).ToList();
            // Only consider the first 4 distinct ranks
            if (ranks.Count < 4)
            {
                return false;
            }

            IEnumerable<IGrouping<int, Rank>> neighbors = ranks
                .Select((rank, index) => new { Rank = rank, GroupKey = (int)rank - index })
                .GroupBy(x => x.GroupKey, x => x.Rank);

            if (neighbors.Count() == 2)
            {
                List<Rank> firstGroup = neighbors.First().ToList();
                List<Rank> secondGroup = neighbors.Last().ToList();

                if (IsNeighborRank(firstGroup.Last() + 1, secondGroup.First()))
                {
                    return true;
                }
            }

            return false;
        }

        public static int NumberOfPairs(List<Card> cards)
        {
            var ranks = cards.Select(card => card.Rank).GroupBy(rank => rank).Where(group => group.Count() == 2);
            return ranks.Count();
        }
        
        public static bool IsUnderPair(List<Card> cc, List<Card> hand) {
            if (NumberOfPairs(hand) == 0) return false;

            List<Card> orderedCC = cc.OrderBy(card => card.Rank).Distinct().ToList();

            return hand[0].Rank < orderedCC[0].Rank;
        }
        
        public static bool IsOverPair(List<Card> cc, List<Card> hand) {
            if (NumberOfPairs(hand) == 0) return false;

            List<Card> orderedCC = cc.OrderBy(card => card.Rank).Distinct().ToList();

            return hand[0].Rank > orderedCC.Last().Rank;
        }
        
        public static bool IsTriples(List<Card> cards)
        {
            var ranks = cards.Select(card => card.Rank).GroupBy(rank => rank).Where(group => group.Count() == 3);
            return ranks.Count() != 0;
        }

        public static bool IsFourOfAKind(List<Card> cards)
        {
            var ranks = cards.Select(card => card.Rank).GroupBy(rank => rank).Where(group => group.Count() == 4);
            return ranks.Count() != 0;
        }

        public static bool IsFullHouse(List<Card> cards)
        {
            if (NumberOfPairs(cards) != 0 && IsTriples(cards))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Scenarios

        public static List<Card> ScenarioCC(BluffCase scenario, ref List<Card> deck, int size) {
            List<Card> cc = new();
            switch (scenario) {
                case BluffCase.None:
                    break;
                case BluffCase.UnderPair:
                    cc = UnderPairCc(ref deck, size);
                    break;
                case BluffCase.OverPair:
                    cc = OverPairCc(ref deck, size);
                    break;
                case BluffCase.StraightDraw:
                    cc = StraightDrawCc(ref deck, size);
                    break;
                case BluffCase.GutShot:
                    cc = StraightDrawCc(ref deck, size);
                    break;
                case BluffCase.BackdoorFlushDraw:
                    cc = BackDoorFlushDrawCc(ref deck, size);
                    break;
                case BluffCase.FlushDraw:
                    cc = FlushDrawCc(ref deck, size);
                    break;
                case BluffCase.StraightFlushDraw:
                    cc = StraightDrawCc(ref deck, size);
                    break;
            }

            return cc;
        }
        
        public static List<Card> ScenarioP(BluffCase scenario, ref List<Card> deck, List<Card> cc) {
            List<Card> cards = new();
            switch (scenario) {
                case BluffCase.None:
                    break;
                case BluffCase.UnderPair:
                    cards = UnderPairP(ref deck, cc);
                    break;
                case BluffCase.OverPair:
                    cards = OverPairP(ref deck, cc);
                    break;
                case BluffCase.StraightDraw:
                    cards = StraightDrawP(ref deck, cc);
                    break;
                case BluffCase.GutShot:
                    cards = GutShotP(ref deck, cc);
                    break;
                case BluffCase.BackdoorFlushDraw:
                    cards = BackDoorFlushDrawP(ref deck, cc);
                    break;
                case BluffCase.FlushDraw:
                    cards = FlushDrawP(ref deck, cc);
                    break;
                case BluffCase.StraightFlushDraw:
                    cards = StraightFlushDrawP(ref deck, cc);
                    break;
            }

            return cards;
        }
        
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
            min = sortedCards[0].Rank == (sortedCards[1].Rank - 1)
                ? (int)sortedCards[0].Rank
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

        public static List<Card> GutShotP(ref List<Card> deck, List<Card> cc)
        {
            if (cc.Count == 0)
            {
                return null;
            }

            List<Card> cards = new();
            int min = 0;
            List<Card> sortedCards = cc.OrderBy(card => card.Rank).ToList();
            min = sortedCards[0].Rank == (sortedCards[1].Rank - 1)
                ? (int)sortedCards[0].Rank
                : (int)sortedCards[1].Rank;
            if (min == 1 || min == 0)
            {
                min += 4;
            }
            else
            {
                min += Utilities.RandomInt(0, 1) == 1 ? 4 : -2;
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
                if ((int)c.Rank != min + 1 && (int)c.Rank != min - 1)
                {
                    cards.Add(c);
                    Utilities.DeckTakeCards(ref deck, cards);
                    break;
                }
            }

            return cards;
        }
        public static List<Card> UnderPairCc(ref List<Card> deck, int size)
        {
            List<Card> cards = new();

            for (int i = 0; i < size; i++)
            {
                foreach (Card c in deck)
                {
                    if ((int)c.Rank != 1)
                    {
                        cards.Add(c);
                        Utilities.DeckTakeCards(ref deck, cards);
                        break;
                    }
                }
            }
            return cards;
        }

        public static List<Card> UnderPairP(ref List<Card> deck, List<Card> cc)
        {
            if (cc.Count == 0)
            {
                return null;
            }

            List<Card> cards = new();
            Rank min = 0;
            List<Card> sortedCards = cc.OrderBy(card => card.Rank).ToList();
            min = sortedCards[0].Rank;
            int i = 0;
            while (min == 0)
            {
                min = sortedCards[++i].Rank;
            }
            Rank r = 0;
            foreach (Card c in deck)
            {
                if (c.Rank < min && c.Rank != 0)
                {
                    cards.Add(c);
                    Utilities.DeckTakeCards(ref deck, cards);
                    r = c.Rank;
                    break;
                }
            }
            foreach (Card c in deck)
            {
                if (c.Rank == r)
                {
                    cards.Add(c);
                    Utilities.DeckTakeCards(ref deck, cards);
                    break;
                }
            }

            return cards;
        }
        public static List<Card> OverPairCc(ref List<Card> deck, int size)
        {
            List<Card> cards = new();

            for (int i = 0; i < size; i++)
            {
                foreach (Card c in deck)
                {
                    if ((int)c.Rank > 1)
                    {
                        cards.Add(c);
                        Utilities.DeckTakeCards(ref deck, cards);
                        break;
                    }
                }
            }
            return cards;
        }

        public static List<Card> OverPairP(ref List<Card> deck, List<Card> cc)
        {
            if (cc.Count == 0)
            {
                return null;
            }

            List<Card> cards = new();
            Rank max = 0;
            List<Card> sortedCards = cc.OrderBy(card => card.Rank).ToList();
            max = sortedCards[sortedCards.Count - 1].Rank;
            Rank r = 0;

            foreach (Card c in deck)
            {
                if (c.Rank > max || c.Rank == 0)
                {
                    cards.Add(c);
                    Utilities.DeckTakeCards(ref deck, cards);
                    r = c.Rank;
                    break;
                }
            }
            foreach (Card c in deck)
            {
                if (c.Rank == r)
                {
                    cards.Add(c);
                    Utilities.DeckTakeCards(ref deck, cards);
                    break;
                }
            }

            return cards;
        }

        public static List<Card> StraightFlushDrawCc(ref List<Card> deck, int size)
        {
            List<Card> cards = new();
            int rand = Utilities.RandomInt(1, 9);
            int pos1 = Utilities.RandomInt(0, size - 1);
            int pos2;
            int pos3 = 0;
            Suit s = 0;
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
            int first = Math.Min(pos1, Math.Min(pos2, pos3));
            int second;
            if (size == 3)
            {
                second = 2;
            }
            else
            {
                second = Utilities.RandomInt(2, 3);
            }

            Rank r = (Rank)rand;
            for (int i = 0; i < size; i++)
            {
                if (i == second)
                {
                    if (i == pos1)
                    {
                        foreach (Card c in deck)
                        {
                            if (c.Suit == s && c.Rank == r)
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
                            if (c.Suit == s && c.Rank == r + 1)
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
                            if (c.Suit == s && c.Rank == r + 2)
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
                            if (c.Suit == s && c.Rank != r + 3 && c.Rank != r - 1 && c.Rank != r + 4 && c.Rank != r - 2)
                            {
                                cards.Add(c);
                                Utilities.DeckTakeCards(ref deck, cards);
                                break;
                            }
                        }
                    }
                }
                else //if i is not second
                {
                    if (i == pos1)
                    {
                        foreach (Card c in deck)
                        {
                            if (c.Rank == r)
                            {
                                if (i == first)
                                {
                                    s = c.Suit;
                                }
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
                                if (i == first)
                                {
                                    s = c.Suit;
                                }
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
                                if (i == first)
                                {
                                    s = c.Suit;
                                }
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
            }

            return cards;
        }

        public static List<Card> StraightFlushDrawP(ref List<Card> deck, List<Card> cc)
        {
            if (cc.Count == 0)
            {
                return null;
            }
            Suit s = cc[0].Suit;
            List<Card> cards = new();
            int min = 0;
            List<Card> sortedCards = cc.OrderBy(card => card.Rank).ToList();
            min = sortedCards[0].Rank == (sortedCards[1].Rank - 1)
                ? (int)sortedCards[0].Rank
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
                if (c.Suit == s && (int)c.Rank == min)
                {
                    cards.Add(c);
                    Utilities.DeckTakeCards(ref deck, cards);
                    break;
                }
            }

            foreach (Card c in deck)
            {
                if (c.Suit == s && (int)c.Rank != min - 1 && (int)c.Rank != min + 1 && (int)c.Rank != min + 4 && (int)c.Rank != min - 4)
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