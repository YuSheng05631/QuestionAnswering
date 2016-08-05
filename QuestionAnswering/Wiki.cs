using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace QuestionAnswering
{
    class Wiki
    {
        //搜尋Google取得第一個wiki連結
        private static string googleWiki(string keyword)
        {
            string url = @"http://www.google.com.tw/search?hl=zh-TW&q=" + keyword;
            string allWebData = "";
            WebClient client = new WebClient();
            using (Stream data = client.OpenRead(url))
            {
                using (StreamReader reader = new StreamReader(data, Encoding.GetEncoding("UTF-8")))
                {
                    allWebData = reader.ReadToEnd();
                }
            }
            MatchCollection matches = Regex.Matches(allWebData, @"<h3 class=""r""><a.+?href=[\""'](?<href>.+?)[\""'].+?></h3>");

            //擷取出所有連結
            List<string> resultUrl = new List<string>();
            foreach (Match m in matches)
            {
                string cut = m.Value.ToString();
                int first = 0, last = 0;
                first = cut.IndexOf("<a href=\"/url?q=") + 16;
                if (first != -1 + 16)
                {
                    last = cut.IndexOf("&", first);
                    cut = cut.Substring(first, last - first);
                    resultUrl.Add(cut);
                }
            }

            //挑選出第一個wiki連結
            bool hasWikiResult = false;
            foreach (string strUrl in resultUrl)
            {
                if (strUrl.Contains("https://en.wikipedia.org"))
                {
                    if (strUrl.Contains("List_of_")) continue;  //濾掉此類連結
                    url = strUrl;
                    url = url.Replace("%25", "%");              //%25在URL Encoding就等於%，搜尋時需先替換
                    hasWikiResult = true;
                    break;
                }
                else if (strUrl.Contains("http://en.wikipedia.org"))
                {
                    if (strUrl.Contains("List_of_")) continue;  //濾掉此類連結
                    url = strUrl;
                    url = url.Insert(24, "wiki/");              //搜尋到的是http而不是https，需修改網址
                    url = url.Replace("%3Ftitle%3D", "");       //去掉%3Ftitle%3D
                    hasWikiResult = true;
                    break;
                }
            }
            if (!hasWikiResult) url = "";   //如果搜尋結果中沒有wiki連結
            return url;
        }
        //取得wiki上<p>標籤的所有句子
        private static List<string> getWikiData(string url)
        {
            List<string> wikiData = new List<string>();
            WebClient client = new WebClient();

            //取得所有網頁內容
            string allWebData = "";
            using (Stream data = client.OpenRead(url))
            {
                using (StreamReader reader = new StreamReader(data, Encoding.GetEncoding("UTF-8")))
                {
                    allWebData = reader.ReadToEnd();
                }
            }

            //取得所有<p>標籤的句子
            int first = 0, last = 0, firstL = 0, lastL = 0;
            string cut = "";
            while (first != -1)
            {
                first = allWebData.IndexOf("<p>", last);
                if (first != -1)    //-1代表沒找到該詞彙，便不做處理
                {
                    last = allWebData.IndexOf("</p>", first);
                    cut = allWebData.Substring(first + 3, last - first - 3);
                    //去掉<標籤>
                    firstL = 0;
                    lastL = 0;
                    while (firstL != -1 && lastL != -1)
                    {
                        firstL = cut.IndexOf("<");
                        lastL = cut.IndexOf(">");
                        if (firstL != -1 && lastL != -1)
                        {
                            if (firstL < lastL) //避免因編碼問題而發生錯誤
                            {
                                cut = cut.Remove(firstL, lastL - firstL + 1);
                            }
                            else
                            {
                                Console.WriteLine("*因編碼問題，去除<標籤>時發生錯誤：" + cut + "\n");
                                break;
                            }
                        }
                    }
                    //去掉[標籤]
                    firstL = 0;
                    lastL = 0;
                    while (firstL != -1 && lastL != -1)
                    {
                        firstL = cut.IndexOf("[");
                        lastL = cut.IndexOf("]");
                        if (firstL != -1 && lastL != -1)
                        {
                            if (firstL < lastL) //避免因編碼問題而發生錯誤(ex.2001_A20_C2)
                            {
                                cut = cut.Remove(firstL, lastL - firstL + 1);
                            }
                            else
                            {
                                Console.WriteLine("*因編碼問題，去除[標籤]時發生錯誤：" + cut);
                                break;
                            }
                        }
                    }
                    //以句點分隔段落
                    firstL = 0;
                    lastL = 0;
                    while (firstL != -1 && lastL != -1)
                    {
                        firstL = 0;
                        lastL = cut.IndexOf(". ");
                        if (firstL != -1 && lastL != -1)    //此段落擁有兩個句子以上
                        {
                            wikiData.Add(cut.Substring(firstL, lastL - firstL + 1));   //List加進此段落的第一個句子
                            cut = cut.Remove(firstL, lastL - firstL + 2);               //cut刪除此段落的第一個句子
                        }
                        else     //已經是單一個句子
                        {
                            if (cut.Length > 1)     //長度小於1的字串不需要加進List
                            {
                                wikiData.Add(cut);
                            }
                        }
                    }
                }
            }

            //使用Stemmer讓動名詞轉為原型
            /*IStemmer stemmer = new EnglishStemmer();
            List<string> wikiDataTemp = wikiData;
            wikiData = new List<string>();
            char[] c = { ' ', ',', '.', '?', '!', '"', '(', ')', '[', ']', ';', ':' };    //拿來斷句的字元
            foreach (string data in wikiDataTemp)
            {
                string sentence = "";
                string[] sAry = data.Split(c, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in sAry)
                {
                    sentence += stemmer.Stem(word) + " ";
                }
                wikiData.Add(sentence);
            }*/

            return wikiData;
        }
        //取得wiki的標題
        private static string getWikiTitle(string url)
        {
            int first = url.IndexOf("/wiki/") + 6;
            string cut = url.Substring(first);
            return cut;
        }
        //取得wiki所有句子的POSTree，並儲存下來
        private static List<ROOT> getWikiPOSTree(List<string> wikiData, string wikiTitle)
        {
            List<ROOT> rootList = new List<ROOT>();
            foreach (string data in wikiData) rootList.Add(POSTree.getPOSTree(data));
            SaveData.savePOSTree(rootList, wikiTitle);
            return rootList;
        }

        //main
        public static List<ROOT> getWikiPOSTree(string keyword)
        {
            //搜尋Google取得第一個wiki連結
            string url = googleWiki(keyword);
            if (url == "")      //若搜尋不到wiki，將關鍵字後面加上" wiki"再搜尋一次
            {
                url = googleWiki(keyword + " wiki");
                if (url == "")  //還是找不到
                {
                    Console.WriteLine("找不到wiki。");
                    return new List<ROOT>();
                }
            }

            //取得wiki的標題
            string wikiTitle = getWikiTitle(url);

            //載入POSTree(List)
            List<ROOT> rootList = SaveData.loadPOSTree(wikiTitle);
            if (rootList.Count == 0)    //若沒有記錄檔
            {
                //取得wiki上<p>標籤的所有句子
                List<string> wikiData = getWikiData(url);

                //取得wiki所有句子的POSTree，並儲存下來
                rootList = getWikiPOSTree(wikiData, wikiTitle);
            }

            return rootList;
        }

    }
}
