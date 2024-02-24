namespace nexx.Saving
{
	using System.Collections;
	using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;

	public static class SaveWriter
	{
        public const string SAVELOCATION = "/GameSaves/";
        public const string SAVETYPE = ".txt";

        public static string getSaveLocation => Application.persistentDataPath + SAVELOCATION;


        private static string getLocationTyped(string name) => getSaveLocation + name + SAVETYPE;


        public async static Task WriteDataToFile(string fileName, string data)
        {
            if (!Directory.Exists(getSaveLocation))
                Directory.CreateDirectory(getSaveLocation);

            using (StreamWriter sw = new StreamWriter(getLocationTyped(fileName)))
            {
                await sw.WriteLineAsync(data);
            }
        }

        public static async Task<string> ReadDataFromFile(string fileName)
        {
            if (!FileExists(fileName))
                return "";

            using (StreamReader sr = new StreamReader(getLocationTyped(fileName)))
            {
                return await sr.ReadToEndAsync();
            }
        }

        public static bool FileExists(string fileName) => File.Exists(getLocationTyped(fileName));
        public static void RemoveSave(string fileName) 
        {
            if (FileExists(fileName))
                File.Delete(getLocationTyped(fileName));
        }

    }
}