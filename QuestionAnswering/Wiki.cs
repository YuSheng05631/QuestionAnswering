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
    //wiki的Infobox
    public class Infobox
    {
        public string infobox, birthDate, deathDate, parents, father, mother, children;
        public Infobox()
        {
            infobox = "";
            birthDate = "";
            deathDate = "";
            parents = "";
            father = "";
            mother = "";
            children = "";
        }
        //是否為人
        public bool isHuman()
        {
            if (infobox == "person" || infobox == "people") return true;
            if (birthDate != "" || deathDate != "" || parents != "" || 
                father != "" || mother != "" || children != "") return true;
            return false;
        }
        //是否為單數
        public bool isSingular()
        {
            if (infobox == "people") return false;
            return true;
        }
    }
    class Wiki
    {
        //取得網頁原始碼
        public static string getAllWebData(string url)
        {
            WebClient client = new WebClient();
            string allWebData = "";
            try
            {
                using (Stream data = client.OpenRead(url))
                {
                    using (StreamReader reader = new StreamReader(data, Encoding.GetEncoding("UTF-8")))
                    {
                        allWebData = reader.ReadToEnd();
                    }
                }
            }
            catch
            {

            }
            return allWebData;
        }
        //(無使用)檢查wiki頁面是否存在
        private static bool existPage(string url, string keyword)
        {
            string allWebData = getAllWebData(url);
            if (allWebData.IndexOf("title=\"Category: Disambiguation pages") != -1) return false;
            if (allWebData.IndexOf(keyword + "</a>\" does not exist.") != -1) return false;
            return true;
        }

        //搜尋Google取得wiki連結
        private static string googleWiki(string keyword)
        {
            string url = @"http://www.google.com.tw/search?hl=zh-TW&q=" + keyword;
            string allWebData = getAllWebData(url); //取得網頁原始碼
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

            //挑選出wiki連結
            List<string> wikiUrl = new List<string>();
            foreach (string strUrl in resultUrl)
            {
                if (strUrl.Contains("https://en.wikipedia.org"))
                {
                    if (strUrl.Contains("List_of_")) continue;  //濾掉此類連結
                    url = strUrl;
                    url = url.Replace("%25", "%");              //%25在URL Encoding就等於%，搜尋時需先替換
                    wikiUrl.Add(url);
                }
                else if (strUrl.Contains("http://en.wikipedia.org"))
                {
                    if (strUrl.Contains("List_of_")) continue;  //濾掉此類連結
                    url = strUrl;
                    url = url.Insert(24, "wiki/");              //搜尋到的是http而不是https，需修改網址
                    url = url.Replace("%3Ftitle%3D", "");       //去掉%3Ftitle%3D
                    wikiUrl.Add(url);
                }
            }

            //優先選擇wiki標題和keywords完全相符的url
            keyword = keyword.ToLower();
            foreach (string strUrl in wikiUrl)
            {
                string wikiTitle = getWikiTitle(strUrl);     //取得wiki的標題
                wikiTitle = wikiTitle.Replace("_", " ").ToLower();
                if (wikiTitle == keyword) return strUrl;
            }

            if (wikiUrl.Count != 0) return wikiUrl[0];
            else return "";
        }
        //取得wiki上<p>標籤的所有句子
        private static List<string> getWikiData(string url)
        {
            List<string> wikiData = new List<string>();
            string allWebData = getAllWebData(url); //取得網頁原始碼

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
            List<ROOT> rootList = POSTree.getROOTList(wikiData);
            SaveData.savePOSTree(rootList, wikiTitle);
            return rootList;
        }

        //main
        //取得wiki所有句子的POSTree，並儲存下來
        public static List<ROOT> getWikiPOSTree(string keyword)
        {
            //搜尋Google取得wiki連結
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
        
        //取得wiki的Infobox
        //wikiTitle: 用以判斷單複數。e.g. Tigris可以找到wiki頁面，代表是單數
        public static Infobox getInfobox(string keyword, out string wikiTitle)
        {
            wikiTitle = "";

            //載入Infobox
            Infobox infobox = SaveData.loadInfobox(out wikiTitle, keyword);
            if (infobox != null) return infobox;

            //搜尋Google取得wiki連結
            string url = googleWiki(keyword);
            if (url == "")      //若搜尋不到wiki，將關鍵字後面加上" wiki"再搜尋一次
            {
                url = googleWiki(keyword + " wiki");
                if (url == "")  //還是找不到
                {
                    Console.WriteLine("找不到wiki。");
                    return null;
                }
            }

            //取得wiki的標題
            wikiTitle = getWikiTitle(url);

            //組合Edit頁面網址
            url = "https://en.wikipedia.org/w/index.php?title=" + wikiTitle + "&action=edit";

            //取得網頁原始碼
            string allWebData = getAllWebData(url);

            //取得Infobox區段
            string infoboxSec = getInfoboxSec(allWebData);

            //從infoboxSec取得Infobox資訊
            infobox = getInfoboxFromSec(infoboxSec);

            //儲存Infobox
            if (infobox != null) SaveData.saveInfobox(infobox, wikiTitle, keyword);

            //印出Infobox
            //printInfobox(infobox);

            return infobox;
        }
        //取得Infobox區段
        private static string getInfoboxSec(string allWebData)
        {
            //取出Infobox區段
            int firstInfobox = allWebData.IndexOf("{{Infobox");
            if (firstInfobox == -1) firstInfobox = allWebData.IndexOf("{{\nInfobox");
            if (firstInfobox == -1) return null;
            //找到結尾
            int first = allWebData.IndexOf("{{", firstInfobox + 2);
            int last = allWebData.IndexOf("}}", firstInfobox + 2);
            while (first < last && first != -1)
            {
                first = allWebData.IndexOf("{{", first + 2);
                last = allWebData.IndexOf("}}", last + 2);
            }
            return allWebData.Substring(firstInfobox, last - firstInfobox + 2);
        }
        //從infoboxSec取得Infobox資訊
        private static Infobox getInfoboxFromSec(string infoboxSec)
        {
            if (infoboxSec == null) return null;
            infoboxSec = infoboxSec.ToLower();
            Infobox infobox = new Infobox();

            infobox.infobox = getInfoboxItem(infoboxSec, "infobox");
            infobox.birthDate = getInfoboxItem(infoboxSec, "birth_date");
            infobox.deathDate = getInfoboxItem(infoboxSec, "death_date");
            infobox.parents = getInfoboxItem(infoboxSec, "parents");
            infobox.father = getInfoboxItem(infoboxSec, "father");
            infobox.mother = getInfoboxItem(infoboxSec, "mother");
            infobox.children = getInfoboxItem(infoboxSec, "children");
            return infobox;
        }
        //從infoboxSec取得某項資訊
        private static string getInfoboxItem(string infoboxSec, string item)
        {
            int first = 0, last = 0;
            first = infoboxSec.IndexOf(item);
            if (first != -1)
            {
                first = infoboxSec.IndexOf(" ", first + 2) + 1;
                last = infoboxSec.IndexOf("\n", first);
                return infoboxSec.Substring(first, last - first);
            }
            else return "";
        }
        //印出Infobox
        private static void printInfobox(Infobox infobox)
        {
            Console.WriteLine("infobox: " + infobox.infobox);
            Console.WriteLine("birthDate: " + infobox.birthDate);
            Console.WriteLine("deathDate: " + infobox.deathDate);
            Console.WriteLine("parents: " + infobox.parents);
            Console.WriteLine("father: " + infobox.father);
            Console.WriteLine("mother: " + infobox.mother);
            Console.WriteLine("children: " + infobox.children);
        }

        //取得wiki或Category頁面的所有Category
        private static List<string> getCategoryList(string allWebData)
        {
            List<string> categoryList = new List<string>();
            int first = 0, last = 0, end = 0;
            first = allWebData.IndexOf("title=\"Help:Category");
            if (first != -1)
            {
                end = allWebData.IndexOf("</ul>", first);   //目錄區塊的結尾位置
                first = allWebData.IndexOf("/wiki/Category:", first);
                while (first != -1)
                {
                    if (first > end) break;     //到達結尾
                    first += "/wiki/Category:".Length;
                    last = allWebData.IndexOf("\" ", first);
                    categoryList.Add(allWebData.Substring(first, last - first));
                    first = allWebData.IndexOf("/wiki/Category:", last);
                }
            }
            return categoryList;
        }
        //取得Category頁面的Subcategory
        private static List<string> getSubcategoryList(string allWebData)
        {
            List<string> subcategoryList = new List<string>();
            int first = 0, last = 0;
            first = allWebData.IndexOf("►");
            while (first != -1)
            {
                first = allWebData.IndexOf("/wiki/Category:", first);
                first += "/wiki/Category:".Length;
                last = allWebData.IndexOf("\">", first);
                subcategoryList.Add(allWebData.Substring(first, last - first));
                first = allWebData.IndexOf("►", last);
            }
            return subcategoryList;
        }
        //根據wikiTitle從categoryList中選擇最相關的目錄
        private static string getMostRelevantCategory(List<string> categoryList, string wikiTitle)
        {
            foreach (string category in categoryList)
                if(category.IndexOf(wikiTitle) != -1)   //目錄包含了Title
                    return category;
            if (categoryList.Count != 0) return categoryList[0];    //回傳第一個
            else return "";
        }

        //取得由下到上的目錄路線
        private static List<string> getCategoryRoute(string keyword)
        {
            string url = googleWiki(keyword);       //搜尋Google取得wiki連結
            string allWebData = getAllWebData(url); //取得網頁原始碼
            string wikiTitle = getWikiTitle(url);   //取得wiki的標題

            //取得wiki或Category頁面的所有Category
            List<string> categoryList = getCategoryList(allWebData);

            //根據wikiTitle從categoryList中選擇最相關的目錄
            string mrCategory = getMostRelevantCategory(categoryList, wikiTitle);

            //向上搜尋
            List<string> categoryRoute = new List<string>();
            while (categoryList.Count != 0)
            {
                if (mrCategory != "")   //只有第一次需要選擇最相關的目錄
                {
                    url = "https://en.wikipedia.org/wiki/Category:" + mrCategory;
                    categoryRoute.Add(mrCategory);
                    mrCategory = "";
                }
                else
                {
                    url = "https://en.wikipedia.org/wiki/Category:" + categoryList[0];  //選擇第一個
                    categoryRoute.Add(categoryList[0]);
                }
                allWebData = getAllWebData(url);    //取得網頁原始碼
                categoryList = getCategoryList(allWebData);

                if (categoryRoute.Count > 100)
                {
                    Console.WriteLine("categoryRoute 可能有迴圈。");
                    break;
                }
            }
            return categoryRoute;
        }
        //取得由下到上的目錄路線，並找到兩個目錄路線的交叉點
        public static void getCross(string keyword1, string keyword2, out List<string> cr1, out List<string> cr2, out int ci1, out int ci2)
        {
            cr1 = Wiki.getCategoryRoute(keyword1);  //取得由下到上的目錄路線
            cr2 = Wiki.getCategoryRoute(keyword2);  //取得由下到上的目錄路線
            ci1 = -1;
            ci2 = -1;
            for (int i = 0; i < cr1.Count; i++)
            {
                int index = cr2.IndexOf(cr1[i]);
                if (index != -1)
                {
                    ci1 = i;
                    ci2 = index;
                    break;
                }
            }
        }
    }
}
