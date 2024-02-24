namespace nexx.Manager
{
    using nexx.Individual;

    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class DeckGenerator
    {
        public const int CARDSINDECK = 52;

        public static CardScript[] ConvertDeckToReal(CardScript template, ref CardData[] data)
        {
            CardScript[] irl = new CardScript[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                irl[i] = CardScript.Instantiate(template);
                irl[i].GenerateCardData(data[i]);
            }

            return irl;
        }


        public static void ServeDeck(ref CardData[] deck, int combinedDecks)
        {
            combinedDecks = Mathf.Max(combinedDecks, 1);
            deck = new CardData[CARDSINDECK * combinedDecks];

            for (int i = 0; i < combinedDecks; i++)
            {
                CardData[] temp = GiveDeck();

                for (int q = 0; q < temp.Length; q++)
                    deck[(i * combinedDecks) + q] = temp[q];
            }
        }

        private static CardData[] GiveDeck()
        {
            // 1- ace, 2 - 10, 11 = jack, 12 = queen = 13 = king
            CardData[] toReturn = new CardData[CARDSINDECK];

            int pointer = 0;
            for (int suit = 0; suit < 4; suit++)
                for (int num = 1; num <= 13; num++, pointer++)
                    toReturn[pointer] = new CardData(num, suit);

            return toReturn;
        }



        public static void ShuffleDeck(ref CardData[] deck)
        {
            List<CardData> temp = new List<CardData>(deck);

            for (int i = 0; i < deck.Length; i++)
            {
                int newPos = Random.Range(0, temp.Count);
                deck[i] = temp[newPos];

                temp.RemoveAt(newPos);
            }
        }
    }
}