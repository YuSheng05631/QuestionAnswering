﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace QuestionAnswering
{
    class Thesaurus
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
            if (pageNumList.Count == 0) return 1;
            else return pageNumList.Max();
        }
        //取得同義詞與反義詞(經過Stem處理)
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
                if (isAnt) antonymList.Add(Stem.getStem(cut));  //經過Stem處理
                else synonymList.Add(Stem.getStem(cut));
                first = allWebData.IndexOf(indexStr, first);
            }
        }
        
        //查詢該詞彙是否有某個同義詞
        public static bool hasSynonym(string word, string synonym)
        {
            List<string> synonymList, antonymList;
            getThesaurus(word, out synonymList, out antonymList);
            if (synonymList.IndexOf(Stem.getStem(synonym)) != -1) return true;
            else return false;
        }
        //查詢該詞彙是否有某個反義詞
        public static bool hasAntonym(string word, string antonym)
        {
            List<string> synonymList, antonymList;
            getThesaurus(word, out synonymList, out antonymList);
            if (antonymList.IndexOf(Stem.getStem(antonym)) != -1) return true;
            else return false;
        }

        //同義詞轉換，將r2的詞彙轉換成r1的詞彙，回傳轉換完成的r2(Start)
        public static void transformSynonym(ROOT r1, ROOT r2)
        {
            transformSynonymTraversal1(r1.S, r2);
        }
        //同義詞轉換，將r2的詞彙轉換成r1的詞彙，回傳轉換完成的r2(Traversal for r1)
        private static void transformSynonymTraversal1(S s, ROOT r2)
        {
            if (s.WH != null)
            {
                foreach (PL pl in s.WH)
                {
                    foreach (WordAndPOS wap in pl.words)
                        transformSynonymTraversal2(r2.S, wap.word, "WH");
                    if (pl.next != null) transformSynonymTraversal1(pl.next, r2);
                }
            }
            if (s.SQ != null)
            {
                foreach (PL pl in s.SQ)
                {
                    foreach (WordAndPOS wap in pl.words)
                        transformSynonymTraversal2(r2.S, wap.word, "SQ");
                    if (pl.next != null) transformSynonymTraversal1(pl.next, r2);
                }
            }
            if (s.NP != null)
            {
                foreach (PL pl in s.NP)
                {
                    foreach (WordAndPOS wap in pl.words)
                        transformSynonymTraversal2(r2.S, wap.word, "NP");
                    if (pl.next != null) transformSynonymTraversal1(pl.next, r2);
                }
            }
            if (s.VP != null)
            {
                foreach (PL pl in s.VP)
                {
                    foreach (WordAndPOS wap in pl.words)
                        transformSynonymTraversal2(r2.S, wap.word, "VP");
                    if (pl.next != null) transformSynonymTraversal1(pl.next, r2);
                }
            }
            if (s.PP != null)
            {
                foreach (PL pl in s.PP)
                {
                    foreach (WordAndPOS wap in pl.words)
                        transformSynonymTraversal2(r2.S, wap.word, "PP");
                    if (pl.next != null) transformSynonymTraversal1(pl.next, r2);
                }
            }
            if (s.ADJP != null)
            {
                foreach (PL pl in s.ADJP)
                {
                    foreach (WordAndPOS wap in pl.words)
                        transformSynonymTraversal2(r2.S, wap.word, "ADJP");
                    if (pl.next != null) transformSynonymTraversal1(pl.next, r2);
                }
            }
            if (s.ADVP != null)
            {
                foreach (PL pl in s.ADVP)
                {
                    foreach (WordAndPOS wap in pl.words)
                        transformSynonymTraversal2(r2.S, wap.word, "ADVP");
                    if (pl.next != null) transformSynonymTraversal1(pl.next, r2);
                }
            }
            if (s.Ss != null)
            {
                foreach (PL pl in s.Ss)
                {
                    foreach (WordAndPOS wap in pl.words)
                        transformSynonymTraversal2(r2.S, wap.word, "Ss");
                    if (pl.next != null) transformSynonymTraversal1(pl.next, r2);
                }
            }
        }
        //同義詞轉換，將r2的詞彙轉換成r1的詞彙，回傳轉換完成的r2(Traversal for r2)
        private static void transformSynonymTraversal2(S s, string word, string pos)
        {
            if (s.WH != null)
            {
                foreach (PL pl in s.WH)
                {
                    if (pos == "WH")
                        foreach (WordAndPOS wap in pl.words)
                            if (Thesaurus.hasSynonym(word, wap.word)) wap.word = word;
                    if (pl.next != null) transformSynonymTraversal2(pl.next, word, pos);
                }
            }
            if (s.SQ != null)
            {
                foreach (PL pl in s.SQ)
                {
                    if (pos == "SQ")
                        foreach (WordAndPOS wap in pl.words)
                            if (Thesaurus.hasSynonym(word, wap.word)) wap.word = word;
                    if (pl.next != null) transformSynonymTraversal2(pl.next, word, pos);
                }
            }
            if (s.NP != null)
            {
                foreach (PL pl in s.NP)
                {
                    if (pos == "NP")
                        foreach (WordAndPOS wap in pl.words)
                            if (Thesaurus.hasSynonym(word, wap.word)) wap.word = word;
                    if (pl.next != null) transformSynonymTraversal2(pl.next, word, pos);
                }
            }
            if (s.VP != null)
            {
                foreach (PL pl in s.VP)
                {
                    if (pos == "VP")
                        foreach (WordAndPOS wap in pl.words)
                            if (Thesaurus.hasSynonym(word, wap.word)) wap.word = word;
                    if (pl.next != null) transformSynonymTraversal2(pl.next, word, pos);
                }
            }
            if (s.PP != null)
            {
                foreach (PL pl in s.PP)
                {
                    if (pos == "PP")
                        foreach (WordAndPOS wap in pl.words)
                            if (Thesaurus.hasSynonym(word, wap.word)) wap.word = word;
                    if (pl.next != null) transformSynonymTraversal2(pl.next, word, pos);
                }
            }
            if (s.ADJP != null)
            {
                foreach (PL pl in s.ADJP)
                {
                    if (pos == "ADJP")
                        foreach (WordAndPOS wap in pl.words)
                            if (Thesaurus.hasSynonym(word, wap.word)) wap.word = word;
                    if (pl.next != null) transformSynonymTraversal2(pl.next, word, pos);
                }
            }
            if (s.ADVP != null)
            {
                foreach (PL pl in s.ADVP)
                {
                    if (pos == "ADVP")
                        foreach (WordAndPOS wap in pl.words)
                            if (Thesaurus.hasSynonym(word, wap.word)) wap.word = word;
                    if (pl.next != null) transformSynonymTraversal2(pl.next, word, pos);
                }
            }
            if (s.Ss != null)
            {
                foreach (PL pl in s.Ss)
                {
                    if (pos == "Ss")
                        foreach (WordAndPOS wap in pl.words)
                            if (Thesaurus.hasSynonym(word, wap.word)) wap.word = word;
                    if (pl.next != null) transformSynonymTraversal2(pl.next, word, pos);
                }
            }
        }

        //是否在NoThesaurus檔案裡
        private static bool isInNoThesaurus(string word)
        {
            List<string> words = SaveData.loadNoThesaurus();
            int index = words.IndexOf(word);
            if (index != -1) return true;
            else return false;
        }
        
        //main
        //取得同義詞與反義詞(經過Stem處理)
        public static void getThesaurus(string word, out List<string> synonymList, out List<string> antonymList)
        {
            synonymList = new List<string>();
            antonymList = new List<string>();
            string allWebData = "";

            //載入Thesaurus
            if (isInNoThesaurus(word))
            {
                Console.WriteLine(word + " 已記錄在 ((NoThesaurus.txt 檔裡。");
                return;
            }
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
                    //儲存NoThesaurus
                    SaveData.saveNoThesaurus(word);
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
