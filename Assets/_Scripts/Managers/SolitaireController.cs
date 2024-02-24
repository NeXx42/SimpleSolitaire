namespace nexx.Solitaire
{
    using nexx.Individual;
    using nexx.Manager;
    using nexx.Saving;

    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class SolitaireController : GameHandlerLoadable
    {
        public static UnityAction completeMove;
        public static float timer;

        private static int buildNum;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI moveText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Main")]
        [SerializeField] private Sprite resetDeckIMG;
        [SerializeField] private Image serveCardBTN;
        [SerializeField] private CardDropPoint inViewDeckContainer;
        [SerializeField] private Transform[] cardDisplayPoints;
        [SerializeField] private Transform emptyCardParent;

        [Header("Solving")]
        [SerializeField] private GameObject autoCompleteButton;
        [SerializeField] private CardDropPoint[] cardBoardContainers;
        [SerializeField] private CardDynamicSuitContainer[] suitContainers;

        [Header("Puased Menu")]
        [SerializeField] private GameObject pauseMenuMain;

        [Header("Completed Menu")]
        [SerializeField] private GameObject completeMenuMain;
        [SerializeField] private TextMeshProUGUI completeMenu_Move;
        [SerializeField] private TextMeshProUGUI completeMenu_Time;


        private CardData[] deck;
        private Sprite deckServeIMG;

        private List<CardScript> activeDeck = new List<CardScript>();
        private List<CardScript> inDeckView = new List<CardScript>();

        private CardScript[] existingCards;
        private Dictionary<string, CardDropPoint> serializedPorts = new Dictionary<string, CardDropPoint>();


        private SaveData_Solitaire saveDataChannel;

        private int moveCounter;
        private bool disableTimer = false;

        private EventSystem eventSystem;


        private void Start()
        {
            RunStartup();
        }


        private void RunStartup() 
        {
            //AdManager.LoadBannerAd();

            deckServeIMG = serveCardBTN.sprite;
            eventSystem = EventSystem.current;

            DeckGenerator.ServeDeck(ref deck, 1);
            DeckGenerator.ShuffleDeck(ref deck);

            existingCards = DeckGenerator.ConvertDeckToReal(GenericGameHandler.getCardTemplate, ref deck);

            serializedPorts = new Dictionary<string, CardDropPoint>();
            foreach (CardScript c in existingCards) serializedPorts.Add(c.Serialize(), c);

            disableTimer = true;

            completeMenuMain.gameObject.SetActive(false);
            TogglePauseMenu(false);

            FindPorts();

            completeMove = () => { moveCounter++; moveText.text = moveCounter.ToString(); };

            moveCounter = -1;
            completeMove?.Invoke();

            buildNum++;

            Setup();
            CheckForComplete();
        }



        private void FindPorts()
        {
            SerializableGameobject[] objs = FindObjectsOfType<SerializableGameobject>(true);

            foreach (SerializableGameobject o in objs)
                serializedPorts.Add(o.getIdentity, o.GetComponent<CardDropPoint>());
        }





        private async void Setup()
        {
            int thisBuildIndex = buildNum;

            timer = 0;
            eventSystem.enabled = false;

            await LoadTemplate(thisBuildIndex);
            await Task.Delay(250);


            if(buildNum == thisBuildIndex)
            {
                if (saveDataChannel.moves != null)
                    foreach (SaveData_Solitaire_Move move in saveDataChannel.moves)
                    {
                        if (buildNum != thisBuildIndex) break;

                        RedoMove(move);

                        int time = 5;
                        float startTime = timer;

                        for (int i = 0; i <= time; i++)
                        {
                            timer = Mathf.Lerp(startTime, move.atTime, (float)i / time);
                            await Task.Delay(1);
                        }
                    }

                disableTimer = false;
                eventSystem.enabled = true;
            }

            Debug.Log("Loaded");
           // AdManager.HideBannerAd();
        }

        private async Task LoadTemplate(int thisBuildIndex)
        {
            if (SaveManager.SaveExists("Solitaire"))
            {
                Debug.Log("Loading");
                saveDataChannel = await SaveManager.LoadRaw<SaveData_Solitaire>("Solitaire");

                foreach (CardScript c in existingCards)
                {
                    c.Reset();

                    c.transform.SetParent(emptyCardParent);
                    c.gameObject.SetActive(false);
                }

                foreach (CardDropPoint p in cardBoardContainers) p.Reset();
                foreach (CardDropPoint p in suitContainers) p.Reset();

                inDeckView = new List<CardScript>();
                activeDeck = new List<CardScript>();


                foreach (SaveData_SubArray<SaveData_Solitaire_Card> chain in saveDataChannel.baseData.cardsOnBoard) LoadChain(chain);
                foreach (SaveData_Solitaire_Card card in saveDataChannel.baseData.cardsInDeckHidden) activeDeck.Add(GetCardFromSaveData(card));

                RedrawActiveDeck();
            }
            else
            {
                await BuildTemplateFromScratch(thisBuildIndex);
                disableTimer = false;
            }
        }


        private async Task BuildTemplateFromScratch(int thisBuildIndex)
        {
            int total = 0;
            for (int x = 0; x < 7; x++)
            {
                CardScript endOfCardStack = null;

                for (int y = 0; y <= x; y++, total++)
                {
                    if (buildNum != thisBuildIndex) return;

                    existingCards[total].SetPosition(inViewDeckContainer.getStackPoint, 0);

                    if (endOfCardStack == null)
                        cardBoardContainers[x].StackCard(existingCards[total], false);
                    else
                        endOfCardStack.StackCard(existingCards[total], false);

                    endOfCardStack = existingCards[total];
                    await Task.Delay(250);
                }

                if (endOfCardStack) endOfCardStack.SetShowStatus(true);
            }

            for (int i = total; i < existingCards.Length; i++)
            {
                existingCards[i].gameObject.SetActive(false);
                existingCards[i].Reposition(emptyCardParent);

                activeDeck.Add(existingCards[i]);
            }

            saveDataChannel = new SaveData_Solitaire(cardBoardContainers, activeDeck);
        }


        public string FloatToTime(float f) 
        {
            float time = Mathf.RoundToInt(f);
            float mins = Mathf.Floor(time / 60);

            return $"{mins}:{(timer - (mins * 60)).ToString("00")}";
        }


        private void Update()
        {
            timeText.text = FloatToTime(timer);
            timer += !disableTimer ? Time.deltaTime : 0;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                WriteSave();
                Debug.Log(saveDataChannel.moves.Count);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SaveManager.RemoveSave("solitaire");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (Input.GetKeyDown(KeyCode.Q))
                LoadSnapshot();
        }



        public void ServeNewCard(bool ignoreSave = false)
        {
            if (activeDeck.Count == 0)
            {
                serveCardBTN.sprite = resetDeckIMG;

                activeDeck = new List<CardScript>(inDeckView);
                inDeckView = new List<CardScript>();

                foreach (CardScript card in activeDeck)
                {
                    card.gameObject.SetActive(false);
                    card.Reposition(emptyCardParent);
                }

                if (!ignoreSave) CreateSnapShot(null, null, new string[] { "q" });
                return;
            }

            serveCardBTN.sprite = deckServeIMG;

            activeDeck[0].transform.position = serveCardBTN.transform.position;
            activeDeck[0].gameObject.SetActive(true);
            activeDeck[0].SetShowStatus(true);

            inDeckView.Add(activeDeck[0]);
            activeDeck.RemoveAt(0);

            RedrawActiveDeck();
            if (!ignoreSave && activeDeck.Count > 0) CreateSnapShot(inDeckView[inDeckView.Count - 1], null, new string[] { "a" });
        }

        private void RemoveCardFromActiveDeck(CardScript card)
        {
            card.onCardParentChange = null;

            card.BlockStacking(false);
            inDeckView.Remove(card);

            RedrawActiveDeck();
        }

        private void RedrawActiveDeck()
        {
            inViewDeckContainer.ClearChild(inViewDeckContainer.getHarbouring);

            for (int i = 0; i < inDeckView.Count; i++)
            {
                inDeckView[i].BlockStacking(true);
                inDeckView[i].SetCanDragTo(false);
                inDeckView[i].onCardParentChange = null;

                if (i < inDeckView.Count - 3)
                {
                    inDeckView[i].gameObject.SetActive(false);
                }
                else
                {
                    inDeckView[i].gameObject.SetActive(true);

                    if((inDeckView.Count - 1) == i)
                    {
                        inDeckView[i].SetCanDragTo(true);
                        inViewDeckContainer.StackCard(inDeckView[i], false);

                        inDeckView[i].onCardParentChange += RemoveCardFromActiveDeck;
                    }
                    else
                    {
                        inDeckView[i].Reposition(cardDisplayPoints[(inDeckView.Count - 1) - i]);
                    }
                }
            }
        }


        private bool GetDidWin()
        {
            foreach (CardDynamicSuitContainer suitCol in suitContainers)
            {
                if (!suitCol.getFrontCard || suitCol.getFrontCard.getData.id != 13)
                {
                    return false;
                }
            }

            return true;
        }

        public void CheckForComplete()
        {
            if (GetDidWin())
            {
                SaveManager.RemoveSave("Solitaire");

                disableTimer = true;
                eventSystem.enabled = false;

                completeMenuMain.SetActive(true);
                completeMenu_Move.text = $"Moves - {moveCounter}";
                completeMenu_Time.text = $"Moves - {FloatToTime(timer)}";

                eventSystem.enabled = true;
            }
            else
            {
                autoCompleteButton.SetActive(false);

                if (activeDeck.Count == 0 && inDeckView.Count == 0)
                {
                    foreach (CardDropPoint c in cardBoardContainers)
                    {
                        if (c.getHarbouring && !((CardScript)c.getHarbouring).getIsShown)
                            return;
                    }

                    autoCompleteButton.SetActive(true);
                }
            }
        }

        public async void Solve()
        {
            eventSystem.enabled = false;
            autoCompleteButton.SetActive(false);
            List<CardScript> cards = new List<CardScript>();

            for (int i = 0; i < cardBoardContainers.Length; i++)
            {
                CardDropPoint cur = cardBoardContainers[i];

                while (cur && cur.getHarbouring)
                {
                    cur = cur.getHarbouring;

                    if (cur is CardScript)
                        cards.Add(cur as CardScript);
                }
            }

            List<CardScript> toserach = cards.OrderBy(x => x.getData.id).ToList();
            int limit = toserach.Count();

            for(int i = 0; i < limit; i++)
            {
                if (toserach.Count == 0)
                    break;

                CardScript toRemove = null;

                foreach (CardScript card in cards)
                {
                    TryAutoPlace(card, true, out bool wasSuccess);
                    if (wasSuccess)
                    {
                        await System.Threading.Tasks.Task.Delay(100);
                        toRemove = card;
                        break;
                    }
                }

                if (toRemove) toserach.Remove(toRemove);
            }

            CheckForComplete();
        }


        public void TryAutoPlace(CardScript card, bool onlyInsertIntoSuits, out bool b)
        {
            bool tryPlaceIntobody = false;
            b = false;

            if (!card.getIsShown) return;

            foreach (CardDynamicSuitContainer container in suitContainers)
            {
                if (card.getParent == container)
                {
                    tryPlaceIntobody = true;
                    break;
                }
            }

            if (!tryPlaceIntobody)
            {
                if(card.getData.id == 1)
                {
                    foreach(CardDynamicSuitContainer container in suitContainers)
                        if(container.getFrontCard == null)
                        {
                            container.HandleOnDrop(card);
                            b = true;
                            return;
                        }

                    // should never reach this
                    return;
                }
                else
                {
                    if (!card.getHarbouring)
                    {
                        foreach (CardDynamicSuitContainer container in suitContainers)
                            if (container.CanStack(container, card))
                            {
                                container.HandleOnDrop(card);
                                b = true;
                                return;
                            }
                    }
                }
            }

            if (!onlyInsertIntoSuits)
            {
                List<CardDropPoint> bodyTails = new List<CardDropPoint>();

                foreach(CardDropPoint c in cardBoardContainers)
                {
                    CardDropPoint cur = c;

                    while (cur.getHarbouring && cur != card)
                        cur = cur.getHarbouring;

                    if (cur)
                        bodyTails.Add(cur);
                }

                foreach(CardDropPoint tail in bodyTails)
                {
                    if(tail.CanStack(tail, card))
                    {
                        tail.StackCard(card);
                        b = true;
                        break;
                    }
                }
            }
        }


        // -- redoing / undoing moves




        private void RedoMove(SaveData_Solitaire_Move move)
        {
            completeMove?.Invoke();
            if(move.card.flatVal != null && serializedPorts.ContainsKey(move.card.flatVal)) serializedPorts[move.card.flatVal].ClearStacker();

            if (move.args != null)
            {
                if(serializedPorts.ContainsKey(move.lastParent))
                    serializedPorts[move.lastParent].SetShowStatus(true);

                foreach (string str in move.args)
                {
                    switch (str[0])
                    {
                        case 'a': // add to deck, reverse so from deck -> in view
                            ServeNewCard(true);
                            return;

                        case 'q':
                            ServeNewCard(true);
                            return;
                    }
                }

                LoadStacker(move.card.childOf, move.card.flatVal);
            }



        }

        private void LoadStacker(string desired, string card)
        {
            CardScript c = serializedPorts[card] as CardScript;

            if (serializedPorts.ContainsKey(desired) && c)
            {
                activeDeck.Remove(c);
                c.gameObject.SetActive(true);

                switch (serializedPorts[desired])
                {
                    case CardDynamicSuitContainer:
                        serializedPorts[desired].HandleOnDrop(c, false);
                        break;

                    default:
                        serializedPorts[desired].StackCard(c, false);
                        break;
                }
            }
        }


        public void LoadSnapshot()
        {
            if(saveDataChannel.PopMove(out SaveData_Solitaire_Move move))
            {
                if (move.card.flatVal != null && move.card.flatVal == "#47") Debug.Break();

                if(move.card.flatVal != null && serializedPorts.ContainsKey(move.card.flatVal)) serializedPorts[move.card.flatVal].ClearStacker();

                if (move.args != null)
                {
                    foreach(string str in move.args)
                    {
                        switch (str[0])
                        {
                            case 'u':
                                serializedPorts[move.lastParent].SetShowStatus(false);
                                break;

                            case 'r':
                                inDeckView.Add(serializedPorts[move.card.flatVal] as CardScript);
                                return;

                            case 'a':
                                activeDeck.Insert(0, serializedPorts[move.card.flatVal] as CardScript);

                                (serializedPorts[move.card.flatVal] as CardScript).gameObject.SetActive(false);
                                inDeckView.Remove((serializedPorts[move.card.flatVal] as CardScript));
                                RedrawActiveDeck();
                                return;

                            case 'q':

                                serveCardBTN.sprite = deckServeIMG;

                                inDeckView = new List<CardScript>(activeDeck);
                                activeDeck = new List<CardScript>();

                                RedrawActiveDeck();
                                return;
                        }
                    }
                }

                if (serializedPorts.ContainsKey(move.lastParent))
                {
                    switch (serializedPorts[move.lastParent])
                    {
                        case CardScript:
                            serializedPorts[move.lastParent].StackCard(serializedPorts[move.card.flatVal] as CardScript, false);
                            break;

                        default:
                            serializedPorts[move.lastParent].HandleOnDrop(serializedPorts[move.card.flatVal] as CardScript, false);
                            break;
                    }
                }
            }
        }


        private CardScript GetCardFromSaveData(SaveData_Solitaire_Card from)
        {
            (serializedPorts[from.flatVal] as CardScript).GenerateCardData(new CardData(from));
            serializedPorts[from.flatVal].SetShowStatus(from.isShown);

            return serializedPorts[from.flatVal] as CardScript;
        }


        private void LoadChain(SaveData_SubArray<SaveData_Solitaire_Card> cards)
        {
            CardScript last = null;
            for (int i = 0; i < cards.length; i++)
            {
                CardScript card = GetCardFromSaveData(cards.ReadAt(i));
                card.gameObject.SetActive(true);

                CardDropPoint point = last != null ? last : serializedPorts[cards.ReadAt(i).childOf].GetComponent<CardDropPoint>();

                point.StackCard(card, false);
                last = card;
            }
        }



        public void CreateSnapShot(CardScript card, CardDropPoint newParent, string[] args)
        {
            SaveData_Solitaire_Move m = saveDataChannel.SaveMove(card, newParent, args);
            m.SetTime(timer);
        }

        public async Task WriteSave()
        {
            Debug.Log("Saving");
            await SaveManager.Save("Solitaire", saveDataChannel, true);
            Debug.Log("Saved");
        }


        public void OnApplicationQuit()
        {
            if (saveDataChannel != null && saveDataChannel.moves != null && saveDataChannel.moves.Count > 0) WriteSave();
        }


        public void TogglePauseMenu(bool to)
        {
            pauseMenuMain.SetActive(to);
            Time.timeScale = to ? 0 : 1;
        }


        public async void RestartGame()
        {
            buildNum++;

            await Task.Delay(300); // try wait for everything to end

            SaveManager.RemoveSave("Solitiare");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public async void ReturnToMainMenu(bool writeSave)
        {
            if (writeSave)
            {
                await WriteSave();
            }
            else
            {
                SaveManager.RemoveSave("Solitaire");
            }

            buildNum++;
            SceneManager.LoadScene(0);
        }
    }
}