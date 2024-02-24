namespace nexx.Manager
{
    using nexx.Individual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;

    public abstract class GameHandlerLoadable : MonoBehaviour
    {
        public virtual T Setup<T>() where T : GameHandlerLoadable
        {
            return (T)this;
        }
    }


    public class GenericGameHandler : MonoBehaviour
    {
        public static GenericGameHandler instance;

        [Header("References")]
        [SerializeField] private CardScript cardTemplate;
        [SerializeField] private Transform cardDragContainer;
        [SerializeField] private float stackedCardSpacing;
        [SerializeField] private Sprite[] cardsDecals;

        [Header("Game Handlers")]
        [SerializeField] private GameHandlerLoadable[] loadableManagers;


        private Dictionary<string, GameHandlerLoadable> managerReferences;


        public static Transform getCardDragContainer => instance.cardDragContainer;
        public static float getStackedCardSpacing => instance.stackedCardSpacing;
        public static CardScript getCardTemplate => instance.cardTemplate;




        private void Awake()
        {
            if (instance)
            {
                Destroy(this);
                return;
            }

            instance = this;
            SetupManagerReferences();

# if PLATFORM_ANDROID
            Application.targetFrameRate = 60;
#endif
        }

        private void SetupManagerReferences()
        {
            managerReferences = new Dictionary<string, GameHandlerLoadable>();

            foreach (GameHandlerLoadable man in loadableManagers)
            {
                managerReferences.Add(man.GetType().FullName, man.Setup<GameHandlerLoadable>());
            }
        }


        public static T GetManagedReference<T>() where T : GameHandlerLoadable
        {
            string name = typeof(T).FullName;

            if (instance.managerReferences.ContainsKey(name))
                return (T)instance.managerReferences[name];

            return null;
        }

        public static bool TryGetManagedReference<T>(out T ofType) where T : GameHandlerLoadable
        {
            ofType = GetManagedReference<T>();
            return ofType;
        }




        public static Sprite GetCardDecal(CardData card) => GetCardDecal(card.id, card.suit);
        public static Sprite GetCardDecal(int id, CardSuit suit) => instance.cardsDecals[(((DeckGenerator.CARDSINDECK / 4) * (int)suit) + id) - 1];

    }
}