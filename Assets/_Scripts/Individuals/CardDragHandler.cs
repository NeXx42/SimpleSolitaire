namespace nexx.Individual
{
    using nexx.Manager;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private CardDropPoint lastDropPoint;
        private int lastSiblingPos;

        private Transform repositionTo;
        private CanvasGroup group;

        private CardScript master;
        private Vector2 cardOffset;

        public CardDragHandler Setup(CardScript card)
        {
            master = card;

            group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();

            repositionTo = GenericGameHandler.getCardDragContainer;
            return this;
        }

        public void SetCanDrag(bool to)
        {
            group.blocksRaycasts = to;
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            group.blocksRaycasts = false;
            cardOffset = new Vector2(Input.mousePosition.x - transform.position.x, Input.mousePosition.y - transform.position.y);

            SavePositions();
            transform.SetParent(repositionTo);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(!master.skipReturnToDefault)
                ResetPosition();

            group.blocksRaycasts = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - cardOffset;
        }



        private void ResetPosition()
        {
            if (!master) return;

            master.Reposition(lastDropPoint.getStackPoint);
            transform.SetSiblingIndex(lastSiblingPos);
        }


        private void SavePositions()
        {
            lastDropPoint = master.getParent;
            lastSiblingPos = transform.GetSiblingIndex();
        }
    }
}