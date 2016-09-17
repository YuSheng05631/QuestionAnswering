using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;  //PM> Install-Package Newtonsoft.Json

namespace QuestionAnswering
{
    class SaveData
    {
        //儲存POSTree(List)
        public static void savePOSTree(List<ROOT> rootList, string fileName)
        {
            using (StreamWriter writer = new StreamWriter("SavePOSTree\\" + fileName + ".txt"))
            {
                //刷新紀錄檔
            }
            using (StreamWriter writer = new StreamWriter("SavePOSTree\\" + fileName + ".txt", true))
            {
                foreach (ROOT root in rootList)
                {
                    string saveData = JsonConvert.SerializeObject(root);
                    writer.WriteLine(saveData);
                }
            }
        }
        //載入POSTree(List)
        public static List<ROOT> loadPOSTree(string fileName)
        {
            string saveData = "";
            List<ROOT> rootList = new List<ROOT>();
            try
            {
                using (StreamReader reader = new StreamReader("SavePOSTree\\" + fileName + ".txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        saveData = reader.ReadLine();
                        rootList.Add(JsonConvert.DeserializeObject<ROOT>(saveData));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //Console.WriteLine("找不到 SavePOSTree\\" + fileName + ".txt 記錄檔。");
            }
            return rootList;
        }

        //儲存Thesaurus
        public static void savaThesaurus(List<string> synonymList, List<string> antonymList, string fileName)
        {
            using (StreamWriter writer = new StreamWriter("SaveThesaurus\\" + fileName + ".txt"))
            {
                //刷新紀錄檔
            }
            using (StreamWriter writer = new StreamWriter("SaveThesaurus\\" + fileName + ".txt", true))
            {
                string saveData1 = JsonConvert.SerializeObject(synonymList);
                string saveData2 = JsonConvert.SerializeObject(antonymList);
                writer.WriteLine(saveData1);
                writer.WriteLine(saveData2);
            }
        }
        //載入Thesaurus
        public static void loadThesaurus(out List<string> synonymList, out List<string> antonymList, string fileName)
        {
            synonymList = new List<string>();
            antonymList = new List<string>();
            string saveData1, saveData2;
            List<ROOT> rootList = new List<ROOT>();
            try
            {
                using (StreamReader reader = new StreamReader("SaveThesaurus\\" + fileName + ".txt"))
                {
                    saveData1 = reader.ReadLine();
                    saveData2 = reader.ReadLine();
                    synonymList = JsonConvert.DeserializeObject<List<string>>(saveData1);
                    antonymList = JsonConvert.DeserializeObject<List<string>>(saveData2);
                }
            }
            catch (FileNotFoundException)
            {
                //Console.WriteLine("找不到 SaveThesaurus\\" + fileName + ".txt 記錄檔。");
            }
        }
        //儲存NoThesaurus
        public static void saveNoThesaurus(string word)
        {
            List<string> words = loadNoThesaurus(); //先載入再加進新的一筆資料
            words.Add(word);
            using (StreamWriter writer = new StreamWriter("SaveThesaurus\\((NoThesaurus.txt"))
            {
                //刷新紀錄檔
            }
            using (StreamWriter writer = new StreamWriter("SaveThesaurus\\((NoThesaurus.txt", true))
            {
                string saveData = JsonConvert.SerializeObject(words);
                writer.WriteLine(saveData);
            }
        }
        //載入NoThesaurus
        public static List<string> loadNoThesaurus()
        {
            List<string> words = new List<string>();
            using (StreamReader reader = new StreamReader("SaveThesaurus\\((NoThesaurus.txt"))
            {
                string saveData = reader.ReadLine();
                if (saveData == null) return words;
                words = JsonConvert.DeserializeObject<List<string>>(saveData);
            }
            return words;
        }

        //儲存AnaphoraInfo
        public static void saveAnaphoraInfo(AnaphoraInfo ai, string fileName)
        {
            using (StreamWriter writer = new StreamWriter("SaveAnaphoraInfo\\" + fileName + ".txt"))
            {
                //刷新紀錄檔
            }
            using (StreamWriter writer = new StreamWriter("SaveAnaphoraInfo\\" + fileName + ".txt", true))
            {
                string saveData = JsonConvert.SerializeObject(ai);
                writer.WriteLine(saveData);
            }
        }
        //載入AnaphoraInfo
        public static AnaphoraInfo loadAnaphoraInfo(string fileName)
        {
            string saveData = "";
            AnaphoraInfo ai = null;
            try
            {
                using (StreamReader reader = new StreamReader("SaveAnaphoraInfo\\" + fileName + ".txt"))
                {
                    saveData = reader.ReadLine();
                    ai = JsonConvert.DeserializeObject<AnaphoraInfo>(saveData);
                }
            }
            catch (FileNotFoundException)
            {
                //Console.WriteLine("找不到 SaveAnaphoraInfo\\" + fileName + ".txt 記錄檔。");
            }
            return ai;
        }

        //儲存Infobox
        public static void saveInfobox(Infobox infobox, string wikiTitle, string fileName)
        {
            using (StreamWriter writer = new StreamWriter("SaveInfobox\\" + fileName + ".txt"))
            {
                //刷新紀錄檔
            }
            using (StreamWriter writer = new StreamWriter("SaveInfobox\\" + fileName + ".txt", true))
            {
                string saveData = JsonConvert.SerializeObject(infobox);
                writer.WriteLine(saveData);
                saveData = JsonConvert.SerializeObject(wikiTitle);
                writer.WriteLine(saveData);
            }
        }
        //載入Infobox
        public static Infobox loadInfobox(out string wikiTitle, string fileName)
        {
            wikiTitle = "";
            string saveData = "";
            Infobox infobox = null;
            try
            {
                using (StreamReader reader = new StreamReader("SaveInfobox\\" + fileName + ".txt"))
                {
                    saveData = reader.ReadLine();
                    infobox = JsonConvert.DeserializeObject<Infobox>(saveData);
                    saveData = reader.ReadLine();
                    wikiTitle = JsonConvert.DeserializeObject<string>(saveData);
                }
            }
            catch (FileNotFoundException)
            {
                //Console.WriteLine("找不到 SaveInfobox\\" + fileName + ".txt 記錄檔。");
            }
            return infobox;
        }
    }
}
