namespace nexx.Saving
{
	using nexx.Individual;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Unity.VisualScripting;
	using UnityEngine;
	using UnityEngine.UIElements;

	public static class SaveManager
	{
		public static async Task Save<T>(string identifier, T d, bool compressAsHex) where T : class
		{
			string data = JsonUtility.ToJson(d, false);
			string vals = SaveCompressor.CompressData(ref data, compressAsHex);

			await SaveWriter.WriteDataToFile(identifier, vals);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="InputType"></typeparam>
		/// <typeparam name="LoadFormat"></typeparam>
		/// <param name="loadData"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static async Task<InputType> Load<LoadFormat, InputType>(LoadingDataHandler<LoadFormat> loadData, string fileName) where InputType : LoadingDataHandler<LoadFormat> where LoadFormat : class
        {
			return loadData.Unpack(await LoadRaw<LoadFormat>(fileName)) as InputType;
		}

		public static async Task<T> LoadRaw<T>(string fileName) where T : class
		{
            string readData = await SaveWriter.ReadDataFromFile(fileName);
            string data = SaveCompressor.UnCompressData(ref readData);

            return JsonUtility.FromJson<T>(data);
        }


		public static bool SaveExists(string mapName) => SaveWriter.FileExists(mapName);
		public static void RemoveSave(string mapName) => SaveWriter.RemoveSave(mapName);

    }





	/// <summary>
	/// Generic type provided is the type the data is saved in
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class LoadingDataHandler<T> where T : class
    {
        public abstract LoadingDataHandler<T> Unpack(T data);
	}


    [System.Serializable]
    public class SaveData_SubArray<T> : IEnumerable
    {
        public T[] Data;

        public SaveData_SubArray(int size)
        {
            Data = new T[size];
        }

        public void Write(T[] source)
        {
            Data = new T[source.Length];

            for (int i = 0; i < source.Length; i++)
                Data[i] = source[i];
        }

        public void WriteAt(int at, T of)
        {
            Data[at] = of;
        }

        public T ReadAt(int i) => Data[i];

        public int length => Data.Length;

        public IEnumerator GetEnumerator() => Data.GetEnumerator();
    }


    // -- solitaire 


    [System.Serializable]
    public class SaveData_Solitaire
    {
		public SaveData_Solitaire_Template baseData;
		public List<SaveData_Solitaire_Move> moves;

		public SaveData_Solitaire(CardDropPoint[] board, List<CardScript> hidden)
		{
            baseData = new SaveData_Solitaire_Template(board, hidden);
        }


		public SaveData_Solitaire_Move SaveMove(CardScript of, CardDropPoint newParent, string[] extraArgs = null)
		{
			if (moves == null) moves = new List<SaveData_Solitaire_Move>();
			SaveData_Solitaire_Move move = new SaveData_Solitaire_Move(of, newParent, extraArgs);
            moves.Add(move);

			nexx.Solitaire.SolitaireController.completeMove?.Invoke();
			return move;
        }

		public bool PopMove(out SaveData_Solitaire_Move move)
		{
			if(moves != null && moves.Count > 0)
			{
				move = moves[moves.Count - 1];
				moves.RemoveAt(moves.Count - 1);

				return true;
			}

			move = null;
			return false;
		}
    }

	[System.Serializable]
	public class SaveData_Solitaire_Template
	{
		public SaveData_SubArray<SaveData_Solitaire_Card>[] cardsOnBoard;
		public SaveData_SubArray<SaveData_Solitaire_Card> cardsInDeckHidden;

		public SaveData_Solitaire_Template(CardDropPoint[] board, List<CardScript> hidden)
		{
            cardsOnBoard = new SaveData_SubArray<SaveData_Solitaire_Card>[board.Length];

            for (int i = 0; i < board.Length; i++)
            {
				List<CardScript> cardChain = new List<CardScript>();
				CardScript curCard = board[i].getHarbouring ? board[i].getHarbouring as CardScript : null;

                while (curCard)
				{
					cardChain.Add(curCard);
                    curCard = curCard.getHarbouring ? curCard.getHarbouring as CardScript : null;
                }

                cardsOnBoard[i] = SaveData_Solitaire_Card.SerializeCardList(ref cardChain);
            }

            cardsInDeckHidden = SaveData_Solitaire_Card.SerializeCardList(ref hidden);
        }


    }


    [System.Serializable]
	public class SaveData_Solitaire_Card
	{
		public const char CARDSTACKIDENTIFIER = '#';


		public string flatVal;
		public bool isShown;

		public string childOf;
		public string ownerOf;

		public SaveData_Solitaire_Card(CardScript card) => SerializeCard(card);


        public void SerializeCard(CardScript card)
		{
			if (card)
			{
				isShown = card.getIsShown;
                flatVal = card.Serialize();

				if(card.getHarbouring) ownerOf = card.getHarbouring.Serialize();
				if(card.getParent) childOf = card.getParent.Serialize();
            }
		}

        public static SaveData_SubArray<SaveData_Solitaire_Card> SerializeCardArray(ref CardScript[] source)
        {
            SaveData_SubArray<SaveData_Solitaire_Card> dat = new SaveData_SubArray<SaveData_Solitaire_Card>(source.Length);
            for (int i = 0; i < dat.length; i++) dat.WriteAt(i, new SaveData_Solitaire_Card(source[i]));
			return dat;
        }

        public static SaveData_SubArray<SaveData_Solitaire_Card> SerializeCardList(ref List<CardScript> source)
        {
            SaveData_SubArray<SaveData_Solitaire_Card> dat = new SaveData_SubArray<SaveData_Solitaire_Card>(source.Count);
            for (int i = 0; i < dat.length; i++) dat.WriteAt(i, new SaveData_Solitaire_Card(source[i]));
            return dat;
        }
    }



	[System.Serializable]
	public class SaveData_Solitaire_Move
	{
		public string lastParent;
		public SaveData_Solitaire_Card card;
		public float atTime;

		/// <summary>
		/// u = unflip from,
		/// </summary>
		public string[] args;

		public SaveData_Solitaire_Move(CardScript card, CardDropPoint newParent, string[] extraArgs)
		{
			args = extraArgs;
			lastParent = card && card.getParent ? card.getParent.Serialize() : "";

			this.card = new SaveData_Solitaire_Card(card);
			this.card.childOf = newParent ? newParent.Serialize() : "";

			if (card && card.getParent && card.getParent is CardScript && lastParent[0] == SaveData_Solitaire_Card.CARDSTACKIDENTIFIER && !(card.getParent as CardScript).getIsShown)
			{
				if (args == null) args = new string[0];

				Array.Resize(ref args, args.Length + 1);
				args[args.Length - 1] = "u";
			}
		}

		public void SetTime(float to) => atTime = to;
    }
}