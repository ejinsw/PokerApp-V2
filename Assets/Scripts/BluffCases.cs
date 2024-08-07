using System.Collections.Generic;
using System.Linq;

namespace Poker {
    public static class BluffCases {
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
                if ((ranks[i] + 2 == ranks[i + 2] && ranks.Skip(i).Take(3).Distinct().Count() == 3) ||
                    (i < ranks.Count - 3 && (ranks[i] + 3 == ranks[i + 3] && ranks.Skip(i).Take(4).Distinct().Count() == 4 && ranks.Contains(ranks[i] + 2)))) {
                    return true;
                }
            }

            return false;
        }
    }
}