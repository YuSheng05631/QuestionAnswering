using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;  //工具 > NuGet > 主控台，輸入: PM> Install-Package Newtonsoft.Json

namespace QuestionAnswering
{
    class SaveData
    {
        //清空所有紀錄檔
        public void deleteAllSaveData()
        {
            Directory.Delete("SaveAnaphoraInfo", true);
            Directory.Delete("SavePLArticle", true);
            Directory.Delete("SaveInfobox", true);
            Directory.Delete("SaveThesaurus", true);
            
            Directory.CreateDirectory("SaveAnaphoraInfo");
            Directory.CreateDirectory("SavePLArticle");
            Directory.CreateDirectory("SaveInfobox");
            Directory.CreateDirectory("SaveThesaurus");

            File.Create("SaveInfobox\\((NoInfobox.txt");
            File.Create("SaveThesaurus\\((NoThesaurus.txt");
            return;
        }

        #region PLArticle
        //儲存PLArticle
        public void savePLArticle(List<List<PL>> PLArticle, string fileName)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("SavePLArticle\\" + fileName + ".txt"))
                {
                    //刷新紀錄檔
                }
                using (StreamWriter writer = new StreamWriter("SavePLArticle\\" + fileName + ".txt", true))
                {
                    foreach (List<PL> LPLList in PLArticle)
                    {
                        string saveData = JsonConvert.SerializeObject(LPLList);
                        writer.WriteLine(saveData);
                    }
                }
            }
            catch
            {

            }
        }
        //載入PLArticle
        public List<List<PL>> loadPLArticle(string fileName)
        {
            string saveData = "";
            List<List<PL>> PLArticle = new List<List<PL>>();
            try
            {
                using (StreamReader reader = new StreamReader("SavePLArticle\\" + fileName + ".txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        saveData = reader.ReadLine();
                        PLArticle.Add(JsonConvert.DeserializeObject<List<PL>>(saveData));
                    }
                }
            }
            catch
            {
                //Console.WriteLine("找不到 SavePLArticle\\" + fileName + ".txt 記錄檔。");
            }
            return PLArticle;
        }
        #endregion

        #region Infobox
        //儲存Infobox
        public void saveInfobox(Infobox infobox, string wikiTitle, string fileName)
        {
            try
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
            catch
            {

            }
        }
        //載入Infobox
        public Infobox loadInfobox(out string wikiTitle, string fileName)
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
            catch
            {
                //Console.WriteLine("找不到 SaveInfobox\\" + fileName + ".txt 記錄檔。");
            }
            return infobox;
        }
        //儲存NoInfobox
        public void saveNoInfobox(string word)
        {
            List<string> words = loadNoInfobox();   //先載入再加進新的一筆資料
            words.Add(word);
            using (StreamWriter writer = new StreamWriter("SaveInfobox\\((NoInfobox.txt"))
            {
                //刷新紀錄檔
            }
            using (StreamWriter writer = new StreamWriter("SaveInfobox\\((NoInfobox.txt", true))
            {
                string saveData = JsonConvert.SerializeObject(words);
                writer.WriteLine(saveData);
            }
        }
        //載入NoInfobox
        public List<string> loadNoInfobox()
        {
            List<string> words = new List<string>();
            using (StreamReader reader = new StreamReader("SaveInfobox\\((NoInfobox.txt"))
            {
                string saveData = reader.ReadLine();
                if (saveData == null) return words;
                words = JsonConvert.DeserializeObject<List<string>>(saveData);
            }
            return words;
        }
        #endregion

        #region Thesaurus
        //儲存Thesaurus
        public void savaThesaurus(List<string> synonymList, List<string> antonymList, string fileName)
        {
            try
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
            catch
            {

            }
        }
        //載入Thesaurus
        public void loadThesaurus(out List<string> synonymList, out List<string> antonymList, string fileName)
        {
            synonymList = new List<string>();
            antonymList = new List<string>();
            string saveData1, saveData2;
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
            catch
            {
                //Console.WriteLine("找不到 SaveThesaurus\\" + fileName + ".txt 記錄檔。");
            }
        }
        //儲存NoThesaurus
        public void saveNoThesaurus(string word)
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
        public List<string> loadNoThesaurus()
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
        #endregion

        #region AnaphoraInfo
        //儲存AnaphoraInfo
        public void saveAnaphoraInfo(AnaphoraInfo ai, string fileName)
        {
            try
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
            catch
            {

            }
        }
        //載入AnaphoraInfo
        public AnaphoraInfo loadAnaphoraInfo(string fileName)
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
            catch
            {
                //Console.WriteLine("找不到 SaveAnaphoraInfo\\" + fileName + ".txt 記錄檔。");
            }
            return ai;
        }
        #endregion

        #region WordNetResultList
        //儲存WordNetResultList
        public void saveWordNetResultList(List<WordNetResult> wnrList, string fileName)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("SaveWordNetResultList\\" + fileName + ".txt"))
                {
                    //刷新紀錄檔
                }
                using (StreamWriter writer = new StreamWriter("SaveWordNetResultList\\" + fileName + ".txt", true))
                {
                    string saveData = JsonConvert.SerializeObject(wnrList);
                    writer.WriteLine(saveData);
                }
            }
            catch
            {

            }
        }
        //載入WordNetResultList
        public List<WordNetResult> loadWordNetResultList(string fileName)
        {
            string saveData = "";
            List<WordNetResult> wnrList = null;
            try
            {
                using (StreamReader reader = new StreamReader("SaveWordNetResultList\\" + fileName + ".txt"))
                {
                    saveData = reader.ReadLine();
                    wnrList = JsonConvert.DeserializeObject<List<WordNetResult>>(saveData);
                }
            }
            catch
            {
                //Console.WriteLine("找不到 SaveWordNetResultList\\" + fileName + ".txt 記錄檔。");
            }
            return wnrList;
        }
        #endregion
    }
}
