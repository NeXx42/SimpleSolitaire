namespace nexx.Individual
{
    using nexx.Manager;
    using nexx.Saving;
    using nexx.Solitaire;

    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class CardData
    {
        public int id;
        public CardSuit suit;


        public CardData(SaveData_Solitaire_Card save) => Generate(int.Parse(save.flatVal.Substring(1)));
        public CardData(int flatVal) => Generate(flatVal);
        public CardData(int i, int s) => Generate(i, s);


        public void Generate(int flat)
        {
            int s = (flat - 1) / (DeckGenerator.CARDSINDECK / 4);
            id = flat - (s * (DeckGenerator.CARDSINDECK / 4));

            suit = (CardSuit)s;
        }

        public void Generate(int i, int s)
        {
            id = i;
            suit = (CardSuit)s;
        }


        public int SerializeCard() => ((DeckGenerator.CARDSINDECK / 4) * (int)suit) + id;


        public static string CardIDToName(int at)
        {
            switch (at)
            {
                case 1: return "Ace";

                case 11: return "Jack";
                case 12: return "Queen";
                case 13: return "King";

                default: return at.ToString();
            }
        }

        public static string CardIDToAbbriviation(int at) => CardIDToName(at)[0].ToString();

        public static Color GetColourPerSuit(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.Spades: return Color.black;
                case CardSuit.Clubs: return Color.black;
                case CardSuit.Hearts: return Color.red;
                case CardSuit.Diamonds: return Color.red;

                default: return Color.green;
            }
        }


        public override string ToString() => $"{CardIDToName(id)} of {suit}";


        public int GetSequentialVal(CardScript source) => source.getData.id - id;
        /// 0 = same, 1 = same colour, 2 = alternative
        public int GetSuitType(CardScript source) => suit == source.getData.suit ? 0 : GetColourPerSuit(suit) == GetColourPerSuit(source.getData.suit) ? 1 : 2;


        public bool CanStack(StackTypes stack, CardScript source)
        {
            int sequientialVal = GetSequentialVal(source);
            int suitType = GetSuitType(source);

            switch (stack)
            {
                case StackTypes.Squential: return sequientialVal == 1;
                case StackTypes.SquentialSameSuit: return sequientialVal == 1 && suitType == 0;
                case StackTypes.SquentialSameColour: return sequientialVal == 1 && suitType == 1;
                case StackTypes.SquentialAlternateColour: return sequientialVal == 1 && suitType == 2;

                case StackTypes.NegativeSquential: return sequientialVal == -1;
                case StackTypes.NegativeSquentialSameSuit: return sequientialVal == -1 && suitType == 0;
                case StackTypes.NegativeSquentialSameColour: return sequientialVal == -1 && suitType == 2;
                case StackTypes.NegativeSquentialAlternateColour: return sequientialVal == -1 && suitType == 2;

                case StackTypes.SameColour: return suitType == 2;
                case StackTypes.AlernativeColour: return suitType == 2;

                default: return true;
            }
        }
    }




    public class CardScript : CardDropPoint, IPointerClickHandler
    {
        [Header("Faces")]
        [SerializeField] private Image card_Up;
        [SerializeField] private GameObject card_Down;


        public UnityAction<CardScript> onCardParentChange;


        private CardData data;
        private CardDragHandler dragger;
        private CardDropPoint passthrough;


        private bool isShown = false;


        private Transform trans
        {
            get
            {
                if (!m_trans) m_trans = transform;
                return m_trans;
            }
        }
        private Transform m_trans;


        public CardData getData => data;
        public bool getIsShown => isShown;


        public void SetCanDragTo(bool to)
        {
            if (dragger) dragger.SetCanDrag(to);
        }


        public void SetPassThrough(CardDropPoint dropPoint) => passthrough = dropPoint;


        public void GenerateCardData(CardData data)
        {
            this.data = data;
            DrawCard();
        }


        public void DrawCard()
        {
            card_Up.sprite = GenericGameHandler.GetCardDecal(data.id, data.suit);
            SetShowStatus(false);
        }

        public override void SetShowStatus(bool setUp)
        {
            if (setUp != isShown)
            {
                if (setUp)
                    dragger = trans.AddComponent<CardDragHandler>().Setup(this);
                else
                    Destroy(dragger);
            }

            isShown = setUp;

            card_Up.gameObject.SetActive(setUp);
            card_Down.SetActive(!setUp);
        }


        public void Reposition(Transform to)
        {
            trans.SetParent(GenericGameHandler.getCardDragContainer, true);
            SetPosition(to, .25f);
        }


        public void SetPosition(Transform to, float inTime)
        {
            LeanTween.cancel(trans.gameObject);
            LeanTween.move(trans.gameObject, to.position, inTime).setEaseOutBack().setOnComplete(() =>
            {
                trans.SetParent(to, true);

                trans.localPosition = Vector3.zero;
                trans.eulerAngles = new Vector3(0, 0, Random.Range(-.5f, .5f));
                trans.localScale = Vector3.one;
            });
        }



        public override void HandleOnDrop(CardScript card, bool isSave = true)
        {
            if (passthrough)
            {
                passthrough.HandleOnDrop(card);
                return;
            }

            if (!isShown)
                return;

            base.HandleOnDrop(card, isSave);
        }


        public override bool CanStack(CardDropPoint drop, CardScript card)
        {
            if (blockStacking) return false;
            if (passthrough) return passthrough.CanStack(drop, card);
            return data.CanStack(StackTypes.NegativeSquentialAlternateColour, card);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (GenericGameHandler.TryGetManagedReference(out SolitaireController helper))
            {
                helper.TryAutoPlace(this, false, out bool success);

                if (success)
                {
                    GenericGameHandler.GetManagedReference<SolitaireController>().CheckForComplete();
                }
            }
        }


        public override string Serialize()
        {
            return "#" + data.SerializeCard().ToString();
        }

        public override void Reset()
        {
            onCardParentChange = null;

            SetCanDragTo(true);
            SetShowStatus(false);
            BlockStacking(false);

            SetPassThrough(null);

            base.Reset();
        }
    }



    public enum CardSuit
    { 
        Clubs,
        Diamonds,
        Hearts,
        Spades,
    }

}