using System;
using System.Collections.Generic;
using SecurIT_Memory.Models;

namespace SecurIT_Memory.Core
{
    public class GameManager
    {
        public List<Card> Cards { get; private set; }
        public int Attempts { get; private set; }
        public int PairsMatched { get; private set; }
        private Random _rng;

        public GameManager()
        {
            Cards = new List<Card>();
            Attempts = 0;
            PairsMatched = 0;
            _rng = new Random();
        }

        public void InitializeGame(List<string> imagePaths)
        {
            Cards.Clear();
            Attempts = 0;
            PairsMatched = 0;

            int currentId = 1;
            foreach (string path in imagePaths)
            {
                Cards.Add(new Card(currentId, path));
                Cards.Add(new Card(currentId, path));
                currentId++;
            }

            ShuffleCards();
        }

        public void ShuffleCards()
        {
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
        }

        public bool CheckMatch(Card card1, Card card2)
        {
            Attempts++;

            if (card1.Id == card2.Id)
            {
                card1.CurrentState = CardState.Matched;
                card2.CurrentState = CardState.Matched;
                PairsMatched++;
                return true;
            }

            return false;
        }
        
        public bool CheckVictory(int totalPairs)
        {
            return PairsMatched == totalPairs;
        }

        public void SwapUnmatchedCards()
        {
            List<int> unmatchedIndexes = new List<int>();
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].CurrentState != CardState.Matched)
                {
                    unmatchedIndexes.Add(i);
                }
            }

            int n = unmatchedIndexes.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                
                int indexA = unmatchedIndexes[k];
                int indexB = unmatchedIndexes[n];

                Card temp = Cards[indexA];
                Cards[indexA] = Cards[indexB];
                Cards[indexB] = temp;
            }
        }
    }
}