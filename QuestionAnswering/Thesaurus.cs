using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace QuestionAnswering
{
    public class Thesaurus
    {
        //取得Thesaurus網頁原始碼
        private static string getAllWebData(string word, int pageNum)
        {
            string url = "http://www.thesaurus.com/browse/" + word + "/" + pageNum;
            string allWebData = "";
            WebClient client = new WebClient();
            using (Stream data = client.OpenRead(url))
            {
                using (StreamReader reader = new StreamReader(data, Encoding.GetEncoding("UTF-8")))
                {
                    allWebData = reader.ReadToEnd();
                }
            }
            return allWebData;
        }
        //取得總頁數
        private static int getPageNum(string word, string allWebData)
        {
            List<int> pageNumList = new List<int>();
            string indexStr = "<a href=\"http://www.thesaurus.com/browse/" + word + "/";
            string cut = "";
            int first = allWebData.IndexOf(indexStr);
            int last = 0;
            while (first != -1)
            {
                first += indexStr.Length;
                last = allWebData.IndexOf("\"", first);
                cut = allWebData.Substring(first, last - first);
                pageNumList.Add(Convert.ToInt32(cut));          //將所有頁數存起來，之後return對大的頁數
                first = allWebData.IndexOf(indexStr, first);
            }
            return pageNumList.Max();
        }
        //取得同義詞與反義詞
        private static void getSynonymListAndAntonymList(string allWebData, out List<string> synonymList, out List<string> antonymList)
        {
            synonymList = new List<string>();
            antonymList = new List<string>();

            //抓取原始碼中反義詞的範圍
            List<int> secStart = new List<int>();
            List<int> secEnd = new List<int>();
            string indexStr = "<section class=\"container-info antonyms\" >";
            int first = allWebData.IndexOf(indexStr);
            int last = 0;
            while(first != -1)
            {
                last = allWebData.IndexOf("</section>", first);
                secStart.Add(first);
                secEnd.Add(last);
                first = allWebData.IndexOf(indexStr, last);
            }

            //取得同義詞與反義詞
            string cut = "";
            bool isAnt = false;
            indexStr = "<li><a href=\"http://www.thesaurus.com/browse/";
            first = allWebData.IndexOf(indexStr);
            while (first != -1)
            {
                isAnt = false;
                first = allWebData.IndexOf(">", first) + 1;
                if (allWebData[first] == '<') continue;
                last = allWebData.IndexOf("<", first);
                cut = allWebData.Substring(first, last - first);
                for (int i = 0; i < secStart.Count; i++)
                {
                    if (first > secStart[i] && first < secEnd[i]) isAnt = true; //在反義詞的範圍內
                }
                if (isAnt) antonymList.Add(cut);
                else synonymList.Add(cut);
                first = allWebData.IndexOf(indexStr, first);
            }
        }
        
        //main
        public static void getThesaurus(string word, out List<string> synonymList, out List<string> antonymList)
        {
            synonymList = new List<string>();
            antonymList = new List<string>();
            string allWebData = "";

            //載入Thesaurus
            SaveData.loadThesaurus(out synonymList, out antonymList, word);
            if (synonymList.Count == 0 && antonymList.Count == 0)   //若沒有記錄檔
            {
                //取得Thesaurus網頁原始碼(第1頁)
                try
                {
                    allWebData = getAllWebData(word, 1);
                }
                catch (WebException)
                {
                    Console.WriteLine("找不到 " + word + " 的同義詞頁面。");
                    return;
                }

                //取得總頁數
                int pageNum = getPageNum(word, allWebData);

                for (int i = 1; i <= pageNum; i++)
                {
                    //取得Thesaurus網頁原始碼(第1頁到最後一頁)
                    allWebData = getAllWebData(word, i);

                    //取得同義詞與反義詞
                    List<string> tempList1, tempList2;
                    getSynonymListAndAntonymList(allWebData, out tempList1, out tempList2);
                    synonymList.AddRange(tempList1);
                    antonymList.AddRange(tempList2);
                }

                synonymList.Sort();   //排序
                synonymList = synonymList.Distinct().ToList();  //去重複
                antonymList.Sort();   //排序
                antonymList = antonymList.Distinct().ToList();  //去重複

                //儲存Thesaurus
                SaveData.savaThesaurus(synonymList, antonymList, word);
            }
        }
    }
}
