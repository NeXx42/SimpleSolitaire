namespace nexx.Saving
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using System.Text;
    using UnityEngine;

	public static class SaveCompressor
	{
		public const int BINARYSIZE = 2;
		private static string[] hexLetters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s" };

		public static string CompressData(ref string data, bool asHex)
        {
			string finalData = asHex ? "1" : "0";

            if (asHex)
            {
				string binary = ConvertStringToBinary(ref data);
				string hex = ConvertBinaryToHex(ref binary);
				finalData += RunLengthEncoding(ref hex);
			}
            else
            {
				string binary = ConvertStringToBinary(ref data);
				finalData += RunLengthEncoding(ref binary);
			}

			return finalData;
        }

		public static string UnCompressData(ref string data)
        {
			bool asHex = data[0] == '1' ? true : false;
			data = data.Remove(0, 1);

            if (asHex)
            {
				string Uncompressed = RunLengthDecoding(ref data);
				List<int> decimals = ConvertHexToDecimals(ref Uncompressed);
				return ConvertDecimalsToText(ref decimals);
			}
            else
            {
				string Uncompressed = RunLengthDecoding(ref data);
				return ConvertBinaryToString(ref Uncompressed);
			}
        }


        #region binary

		// to

        private static string ConvertStringToBinary(ref string data)
        {
			string newData = "";

			foreach (char c in data)
				newData += ConvertCharToBinary(c);

			return newData;
        }

		private static string ConvertCharToBinary(char c)
        {
			int counter = Encoding.ASCII.GetBytes(c.ToString())[0];
			string binary = "";

			for(int x = (BINARYSIZE * 4) - 1; x >= 0 ; x--)
            {
				int pow = Mathf.RoundToInt(Mathf.Pow(2, (float)x));
				if (counter - pow >= 0)
				{
					binary += "1";
					counter -= pow;
				}
				else
					binary += 0;
            }

			return binary;
        }



		// from

		private static string ConvertBinaryToString(ref string binary)
        {
			string reconstruction = "";
			for (int i = 0; i < binary.Length; i += 8)
            {
				reconstruction += ConvertDecimalToText(BinaryToNumber(binary.Substring(i, 8)));
            }

			return reconstruction;
        }

		private static int BinaryToNumber(string binarySet)
		{
			int front = BinarySetToNumber(binarySet.Substring(0, 4)) * 16;
			int end = BinarySetToNumber(binarySet.Substring(4, 4));

			return front + end;
		}

		private static int BinarySetToNumber(string binarySet)
		{
			int num = 0;

			int total = 4;
			for (int x = total - 1; x >= 0; x--)
			{
				if (binarySet[(total - 1) - x] == '1')
					num += Mathf.RoundToInt(Mathf.Pow(2, (float)x));
			}

			return num;
		}

		private static string ConvertDecimalsToText(ref List<int> decimals)
        {
			string reconstructed = "";

			foreach (int i in decimals)
				reconstructed += ConvertDecimalToText(i);

			return reconstructed;
		}

		private static string ConvertDecimalToText(int i) => Encoding.ASCII.GetString(new byte[] { (byte)i });




		#endregion

		#region hex

		// to

		private static string ConvertBinaryToHex(ref string binary)
        {
			string hex = "";
			for(int i = 0; i < binary.Length; i += 8)
            {
				string data = "";
				for (int x = 0; x < 8; x++)
					data += binary[i + x];

				hex += NumToHex(BinarySetToNumber(data.Substring(0, 4)));
				hex += NumToHex(BinarySetToNumber(data.Substring(4, 4)));
			}

			return hex;
        }

	
		private static string NumToHex(int num)
        {	
			if(num >= 10)
				return hexLetters[num - 10];

			return num.ToString();
        }


		// from


		private static List<int> ConvertHexToDecimals(ref string hex)
        {
			List<int> nums = new List<int>(); 

			for(int i = 0; i < hex.Length; i += 2)
            {
				int firstPart = HexToNum(hex[i]) * 16;
				int secondPart = HexToNum(hex[i + 1]);

				nums.Add(firstPart + secondPart);
			}

			return nums;
        }

		private static int HexToNum(char hex)
        {
			if(int.TryParse(hex.ToString(), out int i))
            {
				return i;
            }
            else
            {
				for (int x = 0; x < hexLetters.Length; x++)
					if (hexLetters[x].ToCharArray()[0] == hex)
						return 10 + x;
            }

			return -1;
        }


        #endregion

        #region Run Length


        private static string RunLengthEncoding(ref string binary)
        {
			string newData = "";

			int index = -1;
			List<KeyValuePair<int, char>> data = new List<KeyValuePair<int, char>>();

			foreach(char c in binary)
            {
				if(index == -1)
                {
					data.Add(new KeyValuePair<int, char>(1, c));
					index++;
                }
                else
                {
					if (data[index].Value == c)
                    {
						data[index] = new KeyValuePair<int, char>(data[index].Key + 1, data[index].Value);
                    }
                    else
                    {
						data.Add(new KeyValuePair<int, char>(1, c));
						index++;
					}
                }
            }

			foreach (KeyValuePair<int, char> vals in data) // letter then number
            {
				newData += NumToHex(HexToNum(vals.Value) + 10) + (vals.Key == 1 ? "" : vals.Key.ToString());
            }

			return newData;
        }

		private static string RunLengthDecoding(ref string data)
        {
			data = System.Text.RegularExpressions.Regex.Replace(data, @"\t|\n|\r", "");
			string construction = "";

			int index = -1;
			List<KeyValuePair<int, char>> unwrappedData = new List<KeyValuePair<int, char>>();
			string currentCounter = "";

			foreach (char c in data)
            {
				if (index == -1)
                {
					index = 0;
					unwrappedData.Add(new KeyValuePair<int, char>(0, c));

					continue;
                }

				if(int.TryParse(c.ToString(), out int i))
                {
					currentCounter += i;
				}
                else
                {
                    if(currentCounter != "")
						unwrappedData[index] = new KeyValuePair<int, char>(int.Parse(currentCounter), unwrappedData[index].Value);
					else
						unwrappedData[index] = new KeyValuePair<int, char>(1, unwrappedData[index].Value);

					index++;
					unwrappedData.Add(new KeyValuePair<int, char>(0, c));
					currentCounter = "";
				}
            }

			if (currentCounter != "")
				unwrappedData[index] = new KeyValuePair<int, char>(int.Parse(currentCounter), unwrappedData[index].Value);
			else
				unwrappedData[index] = new KeyValuePair<int, char>(1, unwrappedData[index].Value);

			foreach (KeyValuePair<int, char> q in unwrappedData)
            {
				for (int i = 0; i < q.Key; i++)
					construction += NumToHex(HexToNum(q.Value) - 10);
            }

			return construction;
        }

        #endregion

	}
}
