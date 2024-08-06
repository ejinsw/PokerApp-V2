using System;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Poker {
    public static class Utilities {
        public static Random rng = new Random((uint)Guid.NewGuid().GetHashCode());

        public static List<string> player_names = new List<string>
        {
            "David", "Tom", "Will", "Julie", "Wanda", "Winston", "Brook", "Thomas",
            "John", "Jane", "Alice", "Bob", "Charlie", "Michael", "Sarah", "Emily",
            "James", "Linda", "Daniel", "Laura", "Matthew", "Jessica", "Joshua", "Karen",
            "Andrew", "Barbara", "Kevin", "Helen", "Paul", "Donna", "Mark", "Lisa",
            "Jason", "Nancy", "Ryan", "Betty", "Adam", "Sandra", "Nathan", "Carol",
            "Brian", "Ruth", "Scott", "Sharon", "Aaron", "Michelle", "Gary", "Deborah",
            "Frank", "Shirley"
        };

        public static Dictionary<Rank, string> rank_as_string = new()
        {
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
    }
}