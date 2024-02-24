namespace nexx.Individual
{
    using nexx.Manager;
    using nexx.Saving;
    using nexx.Solitaire;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.EventSystems;


    public class CardDropPoint : MonoBehaviour, IDropHandler
    {
        [Header("Stacking")]
        [SerializeField] private int onlyAllowOfID = -1;
        [SerializeField] private StackTypes stackTypes;
        [SerializeField] private Transform cardStackPoint;

        protected CardDropPoint harbouring;
        protected CardDropPoint childOf;

        protected bool blockStacking = false;

        public CardDropPoint getParent => childOf;
        public CardDropPoint getHarbouring => harbouring;
        public Transform getStackPoint => cardStackPoint;


        public bool skipReturnToDefault
        {
            get
            {
                bool b = m_skipReturnToDefault;
                m_skipReturnToDefault = false;

                return b;
            }
        }
        private bool m_skipReturnToDefault = false;



        public virtual void SetShowStatus(bool setUp)
        {

        }


        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag && eventData.pointerDrag.TryGetComponent(out CardScript card))
            {
                if (card == this || !CanStack(this, card)) return;

                HandleOnDrop(card);
            }
        }


        public virtual void HandleOnDrop(CardScript card, bool isSave = true)
        {
            GenericGameHandler.GetManagedReference<SolitaireController>().CheckForComplete();

            card.m_skipReturnToDefault = true;
            StackCard(card, isSave);
        }


        public virtual void StackCard(CardScript card, bool isSave = true)
        {
            if (isSave && GenericGameHandler.TryGetManagedReference(out SolitaireController solitaire)) solitaire.CreateSnapShot(card, this, null);

            if (card.childOf && card.childOf != this)
            {
                card.childOf.SetShowStatus(true);
                card.childOf.harbouring = null;
            }

            harbouring = card;
            card.Reposition(cardStackPoint);
            card.childOf = this;

            card.onCardParentChange?.Invoke(card);
            card.SetPassThrough(null);
            card.onCardParentChange = null;
        }


        public virtual bool CanStack(CardDropPoint drop, CardScript card)
        {
            return !blockStacking && (onlyAllowOfID == -1 || card.getData.id == onlyAllowOfID);
        }

        public virtual void ClearStacker()
        {
            if (childOf)
            {
                childOf.ClearChild(this);
                childOf = null;
            }
        }

        public virtual void ClearChild(CardDropPoint child)
        {
            if (harbouring == child) harbouring = null;
        }

        public virtual void BlockStacking(bool to) => blockStacking = to;

        public virtual string Serialize()
        {
            if (TryGetComponent(out SerializableGameobject obj)) return obj.getIdentity;

            Debug.Log("Error serialized, either no serializable identity exists or this is not implemented appropriatly");
            return "";
        }

        public virtual void Reset()
        {
            if (harbouring)
            {
                harbouring.childOf = null;
                harbouring = null;
            }

            childOf = null;
        }
    }

    public enum StackTypes
    {
        None,

        Squential,
        SquentialSameSuit,
        SquentialSameColour,
        SquentialAlternateColour,

        NegativeSquential,
        NegativeSquentialSameSuit,
        NegativeSquentialSameColour,
        NegativeSquentialAlternateColour,

        SameColour,
        AlernativeColour,
    }
}