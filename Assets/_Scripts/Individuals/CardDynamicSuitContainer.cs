namespace nexx.Individual
{
    using nexx.Manager;
    using nexx.Solitaire;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class CardDynamicSuitContainer : CardDropPoint
    {
        [Header("Main")]
        [SerializeField] private int startCardID = 0;

        private List<CardScript> storedCards = new List<CardScript>();



        public CardScript getFrontCard => storedCards.Count == 0 ? null : storedCards[storedCards.Count - 1];
        public List<CardScript> getAllCards => storedCards;




        public override bool CanStack(CardDropPoint drop, CardScript card)
        {
            if (!getFrontCard)
            {
                return (startCardID == -1 || card.getData.id == startCardID);
            }

            return getFrontCard.getData.CanStack(StackTypes.SquentialSameSuit, card);
        }

        public override void HandleOnDrop(CardScript card, bool isSave = true)
        {
            base.HandleOnDrop(card, isSave);

            card.SetPassThrough(this);
            card.onCardParentChange += ClearChild;

            storedCards.Add(card);
        }

        public override void ClearChild(CardDropPoint child)
        {
            storedCards.Remove(child as CardScript);
        }


        public override void Reset()
        {
            base.Reset();
            storedCards = new List<CardScript>();
        }

        public void LoadData(List<CardScript> cards)
        {
            foreach (CardScript c in cards.OrderBy(x => x.getData.id))
            {
                HandleOnDrop(c, false);
                c.gameObject.SetActive(true);
            }
        }
    }
}