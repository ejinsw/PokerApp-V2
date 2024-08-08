using System.Collections.Generic;
using System.Linq;

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

        // public static List<Card> BackDoorFlushDrawCc(ref List<Card> deck, int size)
        // {
        //     List<Card> cards = Utilities.HandSuited(ref deck);
        //     for (int i = 0; i < 1; i++)
        //     {
        //         foreach (Card c in deck)
        //         {
        //             if (cards[0].Suit != c.Suit)
        //             {
        //                 cards.Add(c);
        //                 Utilities.DeckTakeCards(ref deck, cards);
        //                 break;
        //             }
        //         }
        //     }
        //     return cards;
        // }

        // public static List<Card> BackDoorFlushDrawP(ref List<Card> deck, List<Card> cc)
        // {
        //     if (cc.Count == 0)
        //     {
        //         return null;
        //     }
        //     return Utilities.HandSuited(ref deck, cc[0].Suit);
        // }
        #endregion
    }
}